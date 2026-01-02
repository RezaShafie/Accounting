using Accounting.ApiService.Endpoints;
using Accounting.ApiService.Extensions;
using Accounting.ApiService.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWebServices(builder.Configuration);

builder.EnrichInfrastructureDatabase();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapVoucherEndpoints();
app.MapGrpcService<VoucherGrpcService>();

app.MapHealthChecks("/health/details", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();