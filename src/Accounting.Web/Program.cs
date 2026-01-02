using Accounting.Web.Components;
using Accounting.Web.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddMudServices();

builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddHttpClient<IVoucherService, VoucherService>(client =>
{
    client.BaseAddress = new Uri("http://apiservice");
    client.Timeout = TimeSpan.FromSeconds(9999);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();


app.Run();
