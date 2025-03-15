using door.UI.Components;
using Microsoft.EntityFrameworkCore;

// �v���W�F�N�g�Q��
using door.Infrastructure.SQLite;
using door.Domain.Entities;
using door.Infrastructure;
using door.Domain.Repositories;
using door.Infrastructure.Services;
// using Blazored.Toast;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext and services
builder.Services.AddDbContext<DoorDbContext>();
//builder.Services.AddScoped<ICameraNotification, CameraNotificationService>();
//builder.Services.AddSingleton<IDiscordNotificationService, DiscordNotificationService>();
builder.Services.AddScoped<IDiscordNotificationService, DiscordNotificationService>();
builder.Services.AddSingleton<DiscordNotificationService>();
builder.Services.AddSingleton<DataEntrySQLiteService>();


builder.Services.AddSignalR();

// SQLite �̐ڑ�������� appsettings.json ����擾����
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext �� DI �ɓo�^
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
