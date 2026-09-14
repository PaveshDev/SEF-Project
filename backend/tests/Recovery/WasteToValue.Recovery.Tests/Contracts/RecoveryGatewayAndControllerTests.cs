using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Filters;
using WasteToValue.Api.Modules.Recovery.Controllers;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Contracts;

public sealed class RecoveryGatewayAndControllerTests
{
    [Fact]
    public async Task Confirmed_assessment_is_accepted_and_cancellation_propagates()
    {
        var fixture = new Fixture();
        var result = await fixture.Assessments.GetCurrentAsync(Guid.NewGuid(), default);

        Assert.Equal(GatewayOutcome.Success, result.Outcome);
        Assert.NotNull(RecoveryRequestValidator.Require(result));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => fixture.Assessments.GetCurrentAsync(Guid.NewGuid(), cancellation.Token));
    }

    [Fact]
    public void Gateway_failure_outcomes_are_safe_and_structured()
    {
        foreach (var outcome in new[] { GatewayOutcome.NotFound, GatewayOutcome.Invalid, GatewayOutcome.Stale, GatewayOutcome.Unavailable })
        {
            var result = GatewayResult<AssessmentSummary>.Failure(outcome, "provider_code", "Safe failure");
            Assert.Null(result.Value);
            var exception = Assert.Throws<RecoveryException>(() => RecoveryRequestValidator.Require(result));
            Assert.Equal("provider_code", exception.Code);
            Assert.Equal(outcome == GatewayOutcome.NotFound ? 404 : outcome == GatewayOutcome.Stale ? 409 : outcome == GatewayOutcome.Unavailable ? 503 : 400, exception.Status);
        }
    }

    [Fact]
    public async Task Unavailable_matching_and_pickup_return_safe_states()
    {
        var integrations = new UnavailableRecoveryIntegrations();
        var matching = await ((IMatchingGateway)integrations).RevalidateAsync(Guid.NewGuid(), 1, "token", default);
        var pickup = await ((IPickupPlanningGateway)integrations).RevalidateAsync(Guid.NewGuid(), 1, "token", default);

        Assert.Equal(GatewayOutcome.Unavailable, matching.Outcome);
        Assert.Equal(GatewayOutcome.Unavailable, pickup.Outcome);
        Assert.Null(matching.Value);
        Assert.Null(pickup.Value);
    }

    [Fact]
    public void Recovery_exception_filter_returns_structured_problem_details()
    {
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-id";
        context.Request.Path = "/api/recovery/cases";
        var actionContext = new ActionContext(context, new(), new(), new ModelStateDictionary());
        var exceptionContext = new ExceptionContext(actionContext, Array.Empty<IFilterMetadata>())
        {
            Exception = RecoveryException.Conflict("stale_version", "Reload required.")
        };

        new RecoveryExceptionFilter().OnException(exceptionContext);

        var result = Assert.IsType<ObjectResult>(exceptionContext.Result);
        var details = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal("stale_version", details.Extensions["code"]);
        Assert.Equal("trace-id", details.Extensions["traceId"]);
    }

    [Fact]
    public void Controllers_expose_response_dtos_instead_of_entities()
    {
        var caseMethods = typeof(RecoveryCasesController).GetMethods().Where(method => method.DeclaringType == typeof(RecoveryCasesController));
        Assert.DoesNotContain(caseMethods, method => method.ReturnType.Name.Contains("RecoveryCase"));
        Assert.Contains(caseMethods, method => method.Name == nameof(RecoveryCasesController.Get) && method.ReturnType == typeof(Task<RecoveryCaseResponse>));
    }
}