using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SurveyBasket.Api.Persistence.EntitiesConfigurations
{
    public class PollConfigurtion : IEntityTypeConfiguration<Poll>
    {
        public void Configure(EntityTypeBuilder<Poll> builder)
        {
            builder.HasIndex(P => P.Title).IsUnique();
            builder.Property(P => P.Title).HasMaxLength(100);
            builder.Property(P => P.Summary).HasMaxLength(1500);

            //builder.Property(P => P.CreatedOn).HasDefaultValue(DateTime.UtcNow);
            //builder.Property(P => P.CreatedOn).HasDefaultValueSql("GETDATE");   
        }
    }
}
