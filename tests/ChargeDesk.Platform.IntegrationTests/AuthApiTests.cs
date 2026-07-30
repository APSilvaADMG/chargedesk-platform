// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Testes de integração API — auth JWT, health e fluxo básico.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChargeDesk.Platform.IntegrationTests;

public class AuthApiTests : IClassFixture<PlatformWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(PlatformWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_PermiteAnonimo()
    {
        var res = await _client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Clientes_SemToken_Retorna401()
    {
        var res = await _client.GetAsync("/api/clientes");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Login_Admin_RetornaTokenEListaClientes()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { login = "admin", senha = "admin123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var doc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var clientes = await _client.GetAsync("/api/clientes");
        Assert.Equal(HttpStatusCode.OK, clientes.StatusCode);
    }

    [Fact]
    public async Task Login_SenhaInvalida_Unauthorized()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { login = "admin", senha = "errada" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }
}
