using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Partners.Entities;

namespace WasteToValue.Api.Modules.Partners.Configurations;

public class AcceptanceRuleConfiguration : IEntityTypeConfiguration<AcceptanceRule>
{
    public void Configure(EntityTypeBuilder<AcceptanceRule> builder)
    {
        builder.ToTable("acceptance_rules");

        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ar => ar.PartnerId)
            .HasColumnName("partner_id");

        builder.Property(ar => ar.CategoryId)
            .HasColumnName("category_id");

        builder.Property(ar => ar.RouteType)
            .HasColumnName("route_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(ar => ar.MinimumCondition)
            .HasColumnName("minimum_condition")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ar => ar.Restrictions)
            .HasColumnName("restrictions")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}")
            .IsRequired();

        builder.Property(ar => ar.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(ar => ar.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(ar => ar.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        builder.HasIndex(ar => new { ar.PartnerId, ar.CategoryId, ar.RouteType })
            .IsUnique()
            .HasDatabaseName("uq_acceptance_rule");

        builder.HasOne(ar => ar.Partner)
            .WithMany(p => p.AcceptanceRules)
            .HasForeignKey(ar => ar.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
