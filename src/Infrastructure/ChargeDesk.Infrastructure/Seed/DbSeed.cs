// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Seed inicial (empresa demo, admin, unidade, carregador).

using ChargeDesk.BuildingBlocks.Domain;
using ChargeDesk.BuildingBlocks.Time;
using ChargeDesk.Cadastros.Domain;
using ChargeDesk.Core.Domain;
using ChargeDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChargeDesk.Infrastructure.Seed;

public static class DbSeed
{
    public static readonly Guid EmpresaDemoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid UnidadeDemoId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid AdminId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static async Task InitializeAsync(PlatformDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Empresas.AnyAsync()) return;

        var agora = HorarioOperacional.Agora();
        var empresa = new Empresa
        {
            Id = EmpresaDemoId,
            EmpresaId = EmpresaDemoId,
            Nome = "ChargeDesk Demo",
            CriadoEm = agora,
            Status = EntityStatus.Ativo
        };
        var unidade = new Unidade
        {
            Id = UnidadeDemoId,
            EmpresaId = EmpresaDemoId,
            Nome = "Unidade Central",
            Codigo = "U01",
            CriadoEm = agora
        };
        var admin = new Usuario
        {
            Id = AdminId,
            EmpresaId = EmpresaDemoId,
            UnidadeId = UnidadeDemoId,
            Nome = "Administrador",
            Login = "admin",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Admin = true,
            CriadoEm = agora
        };
        var licenca = new EmpresaLicenca
        {
            EmpresaId = EmpresaDemoId,
            Modulo = "Carregamento",
            CriadoEm = agora
        };
        var eq = new Equipamento
        {
            EmpresaId = EmpresaDemoId,
            UnidadeId = UnidadeDemoId,
            Tipo = EquipamentoTipo.Carregador,
            Nome = "Ponto 01",
            CriadoEm = agora
        };

        db.Empresas.Add(empresa);
        db.Unidades.Add(unidade);
        db.Usuarios.Add(admin);
        db.Licencas.Add(licenca);
        db.Equipamentos.Add(eq);
        await db.SaveChangesAsync();
    }
}
