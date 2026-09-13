using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Api.Modules.Items.Configurations;

public class AssessmentEvidenceConfiguration : IEntityTypeConfiguration<AssessmentEvidence>
{
    public void Configure(EntityTypeBuilder<AssessmentEvidence> builder)
    {
        builder.ToTable("AssessmentEvidences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Observation).HasMaxLength(1000);
        builder.Property(x => x.EvidenceType).HasMaxLength(100);

        builder.HasOne(x => x.Photo)
            .WithMany()
            .HasForeignKey(x => x.PhotoId)
            .OnDelete(DeleteBehavior.SetNull); // If photo is deleted, keep evidence with null photo or delete? SetNull is safe.
            
        builder.HasIndex(x => x.AssessmentId);
    }
}
