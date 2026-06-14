using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SurveyBasket.Api.Extensions;
using System.Reflection;
using System.Security.Claims;

namespace SurveyBasket.Api.Persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : IdentityDbContext<ApplicationUser>(options)
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public DbSet<Poll> Polls { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<VoteAnswer> VoteAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

           var cascadeFks =   modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys())
                                                     .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

            foreach (var fk in cascadeFks)
                fk.DeleteBehavior = DeleteBehavior.Restrict;



            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // variable refere all entries that inherit AuditableEntity
            var entries = ChangeTracker.Entries<AuditableEntity>();
            foreach (var entryEntry in entries)
            {
                var currentUserId = _httpContextAccessor.HttpContext?.User.GetUserId();

                if (entryEntry.State == EntityState.Added)
                {
                    entryEntry.Property(x => x.CreatedById).CurrentValue = currentUserId!;
                }
                else if (entryEntry.State == EntityState.Modified)
                {
                    entryEntry.Property(x => x.UpdatedById).CurrentValue = currentUserId;
                    entryEntry.Property(x => x.UpdatedOn).CurrentValue = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
