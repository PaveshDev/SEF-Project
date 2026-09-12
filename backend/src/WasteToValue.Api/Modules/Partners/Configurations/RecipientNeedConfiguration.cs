using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Partners.Entities;

namespace WasteToValue.Api.Modules.Partners.Configurations;

public class RecipientNeedConfiguration : IEntityTypeConfiguration<RecipientNeed>
{
    public void Configure(EntityTypeBuilder<RecipientNeed> builder)
    {
        builder.ToTable("recipient_needs");

        builder.HasKey(rn => rn.Id);

        builder.Property(rn => rn.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(rn => rn.PartnerId)
            .HasColumnName("partner_id");

        builder.Property(rn => rn.CategoryId)
            .HasColumnName("category_id");

        builder.Property(rn => rn.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(rn => rn.QuantityRequired)
            .HasColumnName("quantity_required")
            .IsRequired();

        builder.Property(rn => rn.QuantityFulfilled)
            .HasColumnName("quantity_fulfilled")
            .HasDefaultValue(0);

        builder.Property(rn => rn.Deadline)
            .HasColumnName("deadline");

        builder.Property(rn => rn.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(NeedStatus.Open)
            .IsRequired();

        builder.Property(rn => rn.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(rn => rn.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        builder.Property(rn => rn.Version)
            .HasColumnName("version")
            .HasDefaultValue(1)
            .IsConcurrencyToken();

        builder.HasOne(rn => rn.Partner)
            .WithMany(p => p.RecipientNeeds)
            .HasForeignKey(rn => rn.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
