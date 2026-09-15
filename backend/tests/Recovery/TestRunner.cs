using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Recovery;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;

// Build the relational model without a connection string or opening a database.
using (var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql().Options))
{
    var recoveryTypes = context.Model.GetEntityTypes()
        .Where(e => e.ClrType.Namespace == "WasteToValue.Api.Modules.Recovery.Entities").ToArray();
    Check.That(recoveryTypes.Length == 5, "The five Recovery entities are mapped.");
    var caseType = context.Model.FindEntityType(typeof(RecoveryCase))!;
    Check.That(caseType.GetTableName() == "recovery_cases", "Existing table name preserved.");
    Check.That(caseType.FindProperty("Version")!.IsConcurrencyToken, "Version is an EF concurrency token.");
    Check.That(caseType.GetIndexes().Any(i => i.IsUnique && i.GetFilter()!.Contains("'COMPLETED'")), "Active-case partial unique index exists.");
    var decision = context.Model.FindEntityType(typeof(ProposalDecision))!;
    Check.That(decision.GetForeignKeys().Single().Properties.Count == 2, "Decision references exact proposal ID/revision.");
    Check.That(recoveryTypes.All(e => new[] { "RecoveryCase", "RecoveryOption", "RecoveryProposal", "ProposalDecision", "ValueReference" }
        .Contains(e.ClrType.Name)), "Recovery owns no duplicated foreign module entities.");
}
var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
services.AddRecoveryModule();
using (var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true }))
using (var scope = provider.CreateScope())
{
    var gateway = scope.ServiceProvider.GetRequiredService<IAssessmentGateway>();
    Check.That((await gateway.GetCurrentAsync(Guid.NewGuid(), default)).Outcome == GatewayOutcome.Unavailable, "Absent upstream returns structured unavailable.");
    await Check.ErrorAsync(async () => await scope.ServiceProvider.GetRequiredService<IRecoveryActorAccessor>().GetAsync(default), "identity_unavailable");
    scope.ServiceProvider.GetRequiredService<IRecoveryPlanningService>();
    scope.ServiceProvider.GetRequiredService<IProposalDecisionService>();
    Check.That(true, "Recovery DI graph resolves without database credentials.");
}
Check.Finish();
