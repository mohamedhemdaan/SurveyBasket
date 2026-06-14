using SurveyBasket.Api.Contracts.Answers;
using SurveyBasket.Api.Contracts.Questions;
using SurveyBasket.Api.Errors;

namespace SurveyBasket.Api.Services
{
    public class QuestionService(ApplicationDbContext dbContext) : IQuestionService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int pollId, CancellationToken cancellationToken = default)
        {
            var pollIsExists = await _dbContext.Polls.AnyAsync(p => p.Id == pollId, cancellationToken: cancellationToken);
            if (!pollIsExists)
                return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);



            var question = await _dbContext.Questions
                .Where(q => q.PollId == pollId)
                .Include(q => q.Answers)
                ///.Select(q => new QuestionResponse(
                ///    q.Id,
                ///    q.Content,
                ///    q.Answers.Select(a => new AnswerResponse(a.Id, a.Content))
                ///))
                .ProjectToType<QuestionResponse>()
                .AsNoTracking()
                .ToListAsync();


            return Result.Success<IEnumerable<QuestionResponse>>(question);


        }


        public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int pollId, string userId, CancellationToken cancellationToken = default)
        {
            //this method to  Get Questions Of particular poll that user will vote on them


            // check  if user voted before on this poll
            var hasVote = await _dbContext.Votes.AnyAsync(v => v.UserId == userId && v.PollId == pollId, cancellationToken: cancellationToken);
            if (hasVote)
                return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.DublicatedVote);

            var pollIsExists = await _dbContext.Polls.
                AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                                                              && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken: cancellationToken);
            if (!pollIsExists)
                return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);


            #region Get Question As QuestionReponse and Include Answers that are isActive Only
            //Method1 :
            var questions = await _dbContext.Questions
                   .Where(q => q.PollId == pollId && q.IsActive)
                   //ProjectToType :  will (1.select specefic column based on QuestionReponse , 2. include be default ,3. mapping to QuesionResponse)
                   // it will include all Answers by default , writing Include() will be ignored  ,
                   // if you want a  different behaviour like including  Answers that are isActive only ,
                   // you should first tell mapster by adding configurations in MappingConfigurations 
                   .ProjectToType<QuestionResponse>()
                   .AsNoTracking()
                   .ToListAsync(cancellationToken: cancellationToken);

            ///Method2 :
            //var questions = await _dbContext.Questions
            //    .Where(q => q.PollId == pollId && q.IsActive)
            //    /*.Include(q => q.Answers)*/ // i don't need to write include explicitly here  to get Answers  ,
            //                                 // EF Core will include Answers implicitly because we are writing q.Answers in "Select" below 
            //    .Select(q => new QuestionResponse(
            //        q.Id,
            //        q.Content,
            //        q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
            //    ))
            //    .AsNoTracking()
            //    .ToListAsync(cancellationToken: cancellationToken);
            #endregion

            return Result.Success<IEnumerable<QuestionResponse>>(questions);
        }

        public async Task<Result<QuestionResponse>> GetAsync(int pollId, int id, CancellationToken cancellationToken = default)
        {
            var question = await _dbContext.Questions
                  .Where(q => q.Id == id && q.PollId == pollId)
                  .Include(q => q.Answers)
                  ///.Select(q => new QuestionResponse(
                  ///    q.Id,
                  ///    q.Content,
                  ///    q.Answers.Select(a => new AnswerResponse(a.Id,a.Content))
                  ///    ))
                  .ProjectToType<QuestionResponse>()
                  .AsNoTracking()
                  .SingleOrDefaultAsync();

            if (question is null)
                return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

            return Result.Success(question);
        }


        public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionsRequest request, CancellationToken cancellationToken = default)
        {
            //Add Question in poll

            var pollIsExists = await _dbContext.Polls.AnyAsync(p => p.Id == pollId);

            if (!pollIsExists)
                return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

            var questionIsExistsInThisPoll = await _dbContext.Questions.AnyAsync(Q => Q.Content == request.Content && Q.PollId == pollId, cancellationToken);

            if (questionIsExistsInThisPoll)
                return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

            // here now:  Question is ready to add after mapping

            var question = request.Adapt<Question>();
            question.PollId = pollId;

            //request.Answers.ForEach(Answer => question.Answers.Add(new Answer { Content = Answer }));


            await _dbContext.Questions.AddAsync(question, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(question.Adapt<QuestionResponse>());
        }
        public async Task<Result> UpdateAsync(int pollId, int id, QuestionsRequest request, CancellationToken cancellation = default)
        {

            //1- (for particular poll)  , to avoid  that 2 deffernet records in questions table have the same content
            //(like edit question 1 to question 2 , and you already have a nother question named question 2)

            var questionIsExistsinThisPollAfterEditContent = await _dbContext.Questions.AnyAsync(q =>
            q.PollId == pollId &&
            q.Content == request.Content &&
            q.Id != id );

            if (questionIsExistsinThisPollAfterEditContent) // true => error becuase will have e.g Question2 and another record also named Question2
                return Result.Failure(QuestionErrors.DuplicatedQuestionContent);



            //2- get this specific  record (question "is belongs specific poll" ) to edit his Content or Answers

            var question = await _dbContext.Questions.Include(q => q.Answers).SingleOrDefaultAsync(q => q.Id == id && q.PollId == pollId);

            if (question is null)
                return Result.Failure(QuestionErrors.QuestionNotFound);


            //3- Edit Content
            question.Content = request.Content;


            //4 Edit Answers : here we have 3 scenarios

            //4.1 Answers coming from user : have old +  New Answers 
            /// here adding new answers => steps
            /// get old answers which are in db
            /// request.Answers have "new + old"
            /// remove old , and now you have new answers to add 
            
            var currentAnswers = question.Answers.Select(a => a.Content).ToList();

            var newAnswers =  request.Answers.Except(currentAnswers).ToList();

            newAnswers.ForEach(answer =>
            {
                question.Answers.Add(new Answer() { Content = answer });
            });


            //4.2 Answers coming from user : have No Old Answers , soft delete them
            //4.3 Answers coming from user : have  Old Answers , No Action , isActive = true (this default)

            question.Answers.ToList().ForEach(answer =>
            {
                answer.IsActive = request.Answers.Contains(answer.Content);
            });

            await _dbContext.SaveChangesAsync(cancellation);

            return Result.Success();

    
        }

        public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
        {
            var question = await _dbContext.Questions
                .SingleOrDefaultAsync(q => q.Id == id && q.PollId == pollId, cancellationToken: cancellationToken);

            if (question is null)
                return Result.Failure(QuestionErrors.QuestionNotFound);

            question.IsActive = !question.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

       
    }
}
