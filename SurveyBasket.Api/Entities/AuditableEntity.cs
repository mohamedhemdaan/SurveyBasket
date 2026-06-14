namespace SurveyBasket.Api.Entities
{
    public class AuditableEntity
    {
        public string CreatedById { get; set; } = string.Empty; //FK
        public string? UpdatedById { get; set; }  //FK

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow; //Default Value
        public DateTime? UpdatedOn { get; set; }


        public ApplicationUser CreatedBy { get; set; } = default!;//Create Relationship with user
        public ApplicationUser? UpdatedBy { get; set; }           //Update Relationship with user
    }
}
