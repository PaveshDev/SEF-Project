using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Api.Modules.Items.Configurations;

public class ItemAssessmentConfiguration : IEntityTypeConfiguration<ItemAssessment>
{
    public void Configure(EntityTypeBuilder<ItemAssessment> builder)
    {
        builder.ToTable("ItemAssessments", t => 
        {
            t.HasCheckConstraint("CK_Assessment_Confidence", "\"Confidence\" >= 0 AND \"Confidence\" <= 1");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SuggestedCategory).HasMaxLength(100);
        builder.Property(x => x.ConditionGrade).HasMaxLength(50);
        builder.Property(x => x.ConditionSummary).HasMaxLength(2000);
        builder.Property(x => x.VisibleObservations).HasMaxLength(2000);
        builder.Property(x => x.OwnerReportedFunctionality).HasMaxLength(2000);
        builder.Property(x => x.MissingInformation).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(x => new { x.ItemId, x.Version }).IsUnique();

        builder.HasMany(x => x.Evidences)
            .WithOne(x => x.Assessment)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Clarifications)
            .WithOne(x => x.Assessment)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.SetNull); // SetNull allows clarifications to stay if assessment is deleted, or Cascade if they must be deleted. Since it references both Item and Assessment, Cascade might cause multiple cascade paths. Usually Item cascade handles it. Set to Restrict or SetNull.
    }
}
