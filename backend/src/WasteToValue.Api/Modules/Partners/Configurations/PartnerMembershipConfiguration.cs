using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Partners.Entities;

namespace WasteToValue.Api.Modules.Partners.Configurations;

public class PartnerMembershipConfiguration : IEntityTypeConfiguration<PartnerMembership>
{
    public void Configure(EntityTypeBuilder<PartnerMembership> builder)
    {
        builder.ToTable("partner_memberships");

        builder.HasKey(pm => new { pm.PartnerId, pm.UserId });

        builder.Property(pm => pm.PartnerId)
            .HasColumnName("partner_id");

        builder.Property(pm => pm.UserId)
            .HasColumnName("user_id");

        builder.Property(pm => pm.MembershipRole)
            .HasColumnName("membership_role")
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(MembershipRole.Representative)
            .IsRequired();

        builder.Property(pm => pm.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(pm => pm.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.HasOne(pm => pm.Partner)
            .WithMany(p => p.Memberships)
            .HasForeignKey(pm => pm.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
