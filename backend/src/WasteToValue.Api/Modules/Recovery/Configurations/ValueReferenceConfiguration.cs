using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;

namespace WasteToValue.Api.Modules.Recovery.Configurations;

public sealed class ValueReferenceConfiguration : IEntityTypeConfiguration<ValueReference>
{
    public void Configure(EntityTypeBuilder<ValueReference> builder)
    {
        RecoveryMapping.Common(builder, "value_references");
        RecoveryMapping.Enum<ValueReference, ConditionGrade>(builder, nameof(ValueReference.Condition), "condition_grade", 20);
        RecoveryMapping.Enum<ValueReference, RecoveryRoute>(builder, nameof(ValueReference.Route), "route_type");
        RecoveryMapping.Currency(builder);
        RecoveryMapping.Money(builder, nameof(ValueReference.ValueLow), nameof(ValueReference.ValueHigh));
        builder.Property(x => x.SourceName).HasMaxLength(200);
        builder.Property(x => x.SourceReference).HasMaxLength(1000);
        builder.HasIndex(x => new { x.CategoryId, x.Condition, x.Route, x.Currency, x.ObservedAt });
        builder.ToTable(t => t.HasCheckConstraint("ck_value_references_bounds", "value_low <= value_high"));
    }
}
