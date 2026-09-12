using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Partners.Entities;

namespace WasteToValue.Api.Modules.Partners.Configurations;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("partners");

        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.PartnerType)
            .HasColumnName("partner_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.VerificationStatus)
            .HasColumnName("verification_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(VerificationStatus.Pending)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.ServiceArea)
            .HasColumnName("service_area")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.ContactEmail)
            .HasColumnName("contact_email")
            .HasMaxLength(320);

        builder.Property(p => p.ContactPhone)
            .HasColumnName("contact_phone")
            .HasMaxLength(40);

        builder.Property(p => p.Capacity)
            .HasColumnName("capacity")
            .HasDefaultValue(0);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        builder.Property(p => p.Version)
            .HasColumnName("version")
            .HasDefaultValue(1)
            .IsConcurrencyToken();
    }
}
