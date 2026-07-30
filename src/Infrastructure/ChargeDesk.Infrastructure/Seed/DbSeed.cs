// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Seed inicial (empresa demo, admin, carregador, vagas e licenças).

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
        await SchemaEnsure.ApplyAsync(db);

        if (!await db.Empresas.AnyAsync())
        {
            var agora = HorarioOperacional.Agora();
            db.Empresas.Add(new Empresa
            {
                Id = EmpresaDemoId,
                EmpresaId = EmpresaDemoId,
                Nome = "ChargeDesk Demo",
                CriadoEm = agora,
                Status = EntityStatus.Ativo
            });
            db.Unidades.Add(new Unidade
            {
                Id = UnidadeDemoId,
                EmpresaId = EmpresaDemoId,
                Nome = "Unidade Central",
                Codigo = "U01",
                CriadoEm = agora
            });
            db.Usuarios.Add(new Usuario
            {
                Id = AdminId,
                EmpresaId = EmpresaDemoId,
                UnidadeId = UnidadeDemoId,
                Nome = "Administrador",
                Login = "admin",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Admin = true,
                CriadoEm = agora
            });
            db.Licencas.Add(new EmpresaLicenca
            {
                EmpresaId = EmpresaDemoId,
                Modulo = "Carregamento",
                CriadoEm = agora
            });
            db.Equipamentos.Add(new Equipamento
            {
                EmpresaId = EmpresaDemoId,
                UnidadeId = UnidadeDemoId,
                Tipo = EquipamentoTipo.Carregador,
                Nome = "Ponto 01",
                CriadoEm = agora
            });
            await db.SaveChangesAsync();
        }

        await EnsureModulosDemoAsync(db);
    }

    private static async Task EnsureModulosDemoAsync(PlatformDbContext db)
    {
        var agora = HorarioOperacional.Agora();
        var mudou = false;

        if (!await db.Licencas.AnyAsync(l => l.EmpresaId == EmpresaDemoId && l.Modulo == "Estacionamento"))
        {
            db.Licencas.Add(new EmpresaLicenca
            {
                EmpresaId = EmpresaDemoId,
                Modulo = "Estacionamento",
                CriadoEm = agora
            });
            mudou = true;
        }

        if (!await db.Equipamentos.AnyAsync(e =>
                e.EmpresaId == EmpresaDemoId && e.Tipo == EquipamentoTipo.Vaga && e.Status == EntityStatus.Ativo))
        {
            db.Equipamentos.Add(new Equipamento
            {
                EmpresaId = EmpresaDemoId,
                UnidadeId = UnidadeDemoId,
                Tipo = EquipamentoTipo.Vaga,
                Nome = "Vaga 01",
                CriadoEm = agora
            });
            db.Equipamentos.Add(new Equipamento
            {
                EmpresaId = EmpresaDemoId,
                UnidadeId = UnidadeDemoId,
                Tipo = EquipamentoTipo.Vaga,
                Nome = "Vaga 02",
                CriadoEm = agora
            });
            mudou = true;
        }

        if (mudou)
            await db.SaveChangesAsync();
    }
}
