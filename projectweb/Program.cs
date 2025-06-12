using projectweb;
using projectweb.Components;
using projectweb.Repositories;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using System.Globalization;

var culture = new CultureInfo("nl-NL");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<TafelRepository>();
builder.Services.AddSingleton<BestellingRepository>();
builder.Services.AddScoped<RondeRepository>();
builder.Services.AddScoped<OrderRegelRepository>();
builder.Services.AddScoped<SectieRepository>();
builder.Services.AddScoped<CategorieRepository>();
builder.Services.AddSingleton<BestellingStateService>();




builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();



// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DbConnectionProvider>();

var app = builder.Build();


CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("nl-NL");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("nl-NL");


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
