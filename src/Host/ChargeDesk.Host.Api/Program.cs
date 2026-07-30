// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Host API unificado da ChargeDesk Platform (modular monolith).

using ChargeDesk.BuildingBlocks.Time;
using ChargeDesk.Host.Api;
using ChargeDesk.Infrastructure.Persistence;
using ChargeDesk.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddOpenApi();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    await DbSeed.InitializeAsync(db);
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.MapPlatformApi();

app.Run();

public partial class Program;
