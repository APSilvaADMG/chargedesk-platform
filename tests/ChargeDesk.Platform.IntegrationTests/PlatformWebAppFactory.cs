// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Factory de testes de integração (SQLite temporário + JWT).

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChargeDesk.Platform.IntegrationTests;

public class PlatformWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"cdp-it-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("DB_PATH", _dbPath);
        builder.UseSetting("Auth:SecretKey", "ChargeDesk-Platform-Dev-Secret-Key-Min32Chars!");
        builder.UseEnvironment("Development");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* ignore */ }
    }
}
