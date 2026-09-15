using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Api.Modules.Items.Configurations;

public class AssessmentClarificationConfiguration : IEntityTypeConfiguration<AssessmentClarification>
{
    public void Configure(EntityTypeBuilder<AssessmentClarification> builder)
    {
        builder.ToTable("AssessmentClarifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionCode).HasMaxLength(100);
        builder.Property(x => x.Question).HasMaxLength(1000);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.Answer).HasMaxLength(2000);

        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.AssessmentId);
    }
}
