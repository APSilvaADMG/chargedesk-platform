// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Emissão e validação de JWT (auth Platform).

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChargeDesk.Core.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ChargeDesk.Host.Api.Auth;

public class JwtTokenService(IConfiguration config)
{
    public const string ClaimEmpresa = "empresa_id";
    public const string ClaimUnidade = "unidade_id";
    public const string ClaimAdmin = "admin";

    public string Emitir(Usuario user, out DateTime expiraEm)
    {
        var secret = config["Auth:SecretKey"]
            ?? throw new InvalidOperationException("Auth:SecretKey não configurada.");
        var issuer = config["Auth:Issuer"] ?? "ChargeDesk.Platform";
        var audience = config["Auth:Audience"] ?? "ChargeDesk.Platform";
        var horas = int.TryParse(config["Auth:ExpiresHours"], out var h) ? h : 12;

        expiraEm = DateTime.UtcNow.AddHours(horas);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Login),
            new(ClaimTypes.Name, user.Nome),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimEmpresa, user.EmpresaId.ToString()),
            new(ClaimAdmin, user.Admin ? "true" : "false")
        };
        if (user.UnidadeId.HasValue)
            claims.Add(new Claim(ClaimUnidade, user.UnidadeId.Value.ToString()));
        if (user.Admin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class AuthPolicies
{
    public const string Autenticado = "Autenticado";
    public const string Admin = "Admin";
}
