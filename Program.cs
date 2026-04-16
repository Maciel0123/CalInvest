using Options;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<InvestmentCalculatorService>();
builder.Services.Configure<GroqOptions>(
    builder.Configuration.GetSection("Groq"));

builder.Services.AddHttpClient<IGroqAiService, GroqAiService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.Run();