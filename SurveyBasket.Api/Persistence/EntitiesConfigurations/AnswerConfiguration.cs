using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SurveyBasket.Api.Persistence.EntitiesConfigurations
{
    public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.Property(A => A.Content).HasMaxLength(1000);
            builder.HasIndex(A => new { A.Content, A.QuestionId }).IsUnique();

        }
    }
}
