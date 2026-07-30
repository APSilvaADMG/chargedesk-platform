// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Host API — JWT, SPA, seed e importação.

using System.Text;
using ChargeDesk.BuildingBlocks.Time;
using ChargeDesk.Host.Api;
using ChargeDesk.Host.Api.Auth;
using ChargeDesk.Infrastructure.Import;
using ChargeDesk.Infrastructure.Persistence;
using ChargeDesk.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TZ")))
    Environment.SetEnvironmentVariable("TZ", "America/Sao_Paulo");

var builder = WebApplication.CreateBuilder(args);

var dbPath = Environment.GetEnvironmentVariable("DB_PATH");
if (string.IsNullOrWhiteSpace(dbPath))
    dbPath = Path.Combine(builder.Environment.ContentRootPath, "platform.db");
var dataDir = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrWhiteSpace(dataDir))
    Directory.CreateDirectory(dataDir);

builder.Services.AddDbContext<PlatformDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<LegacySqliteImportService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddOpenApi();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var secret = builder.Configuration["Auth:SecretKey"]
    ?? "ChargeDesk-Platform-Dev-Secret-Key-Min32Chars!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Auth:Issuer"] ?? "ChargeDesk.Platform",
            ValidAudience = builder.Configuration["Auth:Audience"] ?? "ChargeDesk.Platform",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(AuthPolicies.Autenticado, p => p.RequireAuthenticatedUser());
    opt.AddPolicy(AuthPolicies.Admin, p => p.RequireRole("Admin"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    await DbSeed.InitializeAsync(db);
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapPlatformApi();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
