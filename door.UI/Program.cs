using door.UI.Components;
using Microsoft.EntityFrameworkCore;

// プロジェクト参照
using door.Infrastructure.SQLite;
using door.Domain.Entities;
using door.Infrastructure;
using door.Domain.Repositories;
using door.Infrastructure.Services;

// using Blazored.Toast;


var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:7275");
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext and services
builder.Services.AddDbContext<DoorDbContext>();
builder.Services.AddScoped<DataEntrySQLiteService>();

// インターフェース利用
builder.Services.AddScoped<IDataEntryService, DataEntrySQLiteService>();
builder.Services.AddScoped<INotificationService, DiscordNotificationService>();


builder.Services.AddSingleton<DiscordNotificationService>();

//アプリ終了まで状態を維持
//builder.Services.AddSingleton<StateChangedEvent>();



builder.Services.AddSignalR();

// SQLite の接続文字列を appsettings.json から取得する
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext を DI に登録
builder.Services.AddDbContext<DoorDbContext>(options =>
    options.UseSqlite(connectionString));

// Add HttpClient for API calls (if needed)
builder.Services.AddHttpClient();

// Add controllers for API routes
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use routing before antiforgery
app.UseRouting();

// Antiforgery token validation should be applied after routing
app.UseAntiforgery();

// Map API controllers
app.MapControllers(); // Add this to map API routes

// Map Razor components (Blazor Server)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();



app.Run();
