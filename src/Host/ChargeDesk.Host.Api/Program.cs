// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Host API unificado da ChargeDesk Platform (modular monolith).

using ChargeDesk.BuildingBlocks.Time;

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TZ")))
    Environment.SetEnvironmentVariable("TZ", "America/Sao_Paulo");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapGet("/api/health", () => Results.Ok(new
{
    produto = "ChargeDesk Platform",
    status = "ok",
    horarioOperacional = HorarioOperacional.Agora(),
    utc = DateTime.UtcNow
}));

app.Run();
