using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Api.Modules.Items.Configurations;

public class ItemConditionAnswerConfiguration : IEntityTypeConfiguration<ItemConditionAnswer>
{
    public void Configure(EntityTypeBuilder<ItemConditionAnswer> builder)
    {
        builder.ToTable("ItemConditionAnswers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.QuestionText).HasMaxLength(500);
        builder.Property(x => x.Answer).HasMaxLength(2000);
        
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => new { x.ItemId, x.QuestionCode }).IsUnique();
    }
}
