using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Controllers;

// Scoped to Recovery controllers; shared middleware is left untouched.
public sealed class RecoveryExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException && context.HttpContext.RequestAborted.IsCancellationRequested) return;
        var exception = context.Exception;
        var database = exception is DbUpdateException update ? update.InnerException : exception;
        var (status, code, message) = exception switch
        {
            RecoveryException error => (error.Status, error.Code, error.Message),
            DbUpdateConcurrencyException => (409, "stale_version", "The resource changed. Reload before retrying."),
            _ when database is PostgresException { SqlState: "23505" } => (409, "duplicate_operation", "An active case, revision, or decision already exists."),
            _ when database is PostgresException { SqlState: "23503" or "23514" } => (409, "invalid_relationship", "The operation conflicts with the current schema or related data."),
            _ when database is NpgsqlException || exception is TimeoutException => (503, "database_unavailable", "Recovery persistence is unavailable."),
            _ => (500, "recovery_error", "The Recovery operation could not be completed.")
        };
        var details = new ProblemDetails { Status = status, Title = message, Instance = context.HttpContext.Request.Path };
        details.Extensions["code"] = code;
        details.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.Result = new ObjectResult(details) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
