using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Items;
using WasteToValue.Api.Modules.Recovery;
using WasteToValue.Api.Modules.Partners;
using WasteToValue.Api.Modules.Collections;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

// Configuration is checked only when a future operation resolves the context.
// The health controller does not resolve it or verify database connectivity.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Database configuration is missing. Set ConnectionStrings__DefaultConnection " +
            "or the ConnectionStrings:DefaultConnection .NET user secret.");
    }

    options.UseNpgsql(connectionString);
});

builder.Services.AddItemsModule();
builder.Services.AddRecoveryModule();
builder.Services.AddPartnersModule();
builder.Services.AddCollectionsModule();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options => options.AddPolicy("DevelopmentClients", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod()));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevelopmentClients");
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.MapControllers();
app.Run();
