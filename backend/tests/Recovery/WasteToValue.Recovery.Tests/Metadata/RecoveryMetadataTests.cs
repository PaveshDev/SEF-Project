using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Recovery.Entities;
using Xunit;

namespace WasteToValue.Recovery.Tests.Metadata;

public sealed class RecoveryMetadataTests
{
    [Fact]
    public void Recovery_entities_have_expected_tables_and_constraints()
    {
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql().Options);

        var expectedTables = new Dictionary<Type, string>
        {
            [typeof(RecoveryCase)] = "recovery_cases",
            [typeof(RecoveryOption)] = "recovery_options",
            [typeof(RecoveryProposal)] = "recovery_proposals",
            [typeof(ProposalDecision)] = "proposal_decisions",
            [typeof(ValueReference)] = "value_references"
        };

        foreach (var expected in expectedTables)
        {
            var entity = context.Model.FindEntityType(expected.Key)!;
            Assert.Equal(expected.Value, entity.GetTableName());
            Assert.NotNull(entity.FindProperty("Id"));
            if (expected.Key != typeof(ProposalDecision))
            {
                Assert.NotNull(entity.FindProperty("CreatedAt"));
                Assert.NotNull(entity.FindProperty("UpdatedAt"));
            }
        }

        var cases = context.Model.FindEntityType(typeof(RecoveryCase))!;
        Assert.True(cases.FindProperty("Version")!.IsConcurrencyToken);
        Assert.Contains(cases.GetIndexes(), index => index.IsUnique && index.GetFilter()!.Contains("'COMPLETED'"));
        Assert.Contains(cases.GetIndexes(), index => index.Properties.Select(property => property.Name).SequenceEqual(new[] { "OwnerId", "Status" }));

        var options = context.Model.FindEntityType(typeof(RecoveryOption))!;
        Assert.Equal(DeleteBehavior.Restrict, options.GetForeignKeys().Single().DeleteBehavior);
        Assert.Contains(options.GetIndexes(), index => index.Properties.Select(property => property.Name).SequenceEqual(new[] { "RecoveryCaseId", "Status" }));

        var proposals = context.Model.FindEntityType(typeof(RecoveryProposal))!;
        Assert.Equal(DeleteBehavior.Restrict, proposals.GetForeignKeys().First().DeleteBehavior);
        Assert.Contains(proposals.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[] { "RecoveryCaseId", "Revision" }));

        var decisions = context.Model.FindEntityType(typeof(ProposalDecision))!;
        Assert.True(decisions.GetForeignKeys().Single().Properties.Select(property => property.Name).SequenceEqual(new[] { "RecoveryProposalId", "ProposalRevision" }));
        Assert.All(new[] { "ValueLow", "ValueHigh", "EstimatedValueLow", "EstimatedValueHigh", "EstimatedRepairCost", "EstimatedPickupCost", "EstimatedNetValue", "EstimatedShortfall", "MaximumPickupCost" }, propertyName =>
        {
            var property = context.Model.GetEntityTypes().Select(entity => entity.FindProperty(propertyName)).FirstOrDefault(property => property is not null);
            if (property is not null) Assert.Equal((12, 2), (property.GetPrecision(), property.GetScale()));
        });
    }

    [Fact]
    public void Required_json_and_currency_columns_are_configured()
    {
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql().Options);

        foreach (var type in new[] { typeof(RecoveryOption), typeof(RecoveryProposal) })
        {
            var entity = context.Model.FindEntityType(type)!;
            Assert.Contains(entity.GetProperties(), property => property.IsNullable == false && property.GetColumnType() == "jsonb");
        }

        foreach (var type in new[] { typeof(RecoveryCase), typeof(RecoveryOption), typeof(ValueReference) })
            Assert.Equal("character(3)", context.Model.FindEntityType(type)!.FindProperty("Currency")!.GetColumnType());
    }
}