// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: DbContext unificado da Platform (modular monolith).

using ChargeDesk.BuildingBlocks.Domain;
using ChargeDesk.Cadastros.Domain;
using ChargeDesk.Core.Domain;
using ChargeDesk.Financeiro.Domain;
using ChargeDesk.Operacao.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChargeDesk.Infrastructure.Persistence;

public class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : DbContext(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Unidade> Unidades => Set<Unidade>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<EmpresaLicenca> Licencas => Set<EmpresaLicenca>();
    public DbSet<AuditoriaRegistro> Auditorias => Set<AuditoriaRegistro>();

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Equipamento> Equipamentos => Set<Equipamento>();

    public DbSet<Atendimento> Atendimentos => Set<Atendimento>();
    public DbSet<AtendimentoCarregamento> AtendimentosCarregamento => Set<AtendimentoCarregamento>();
    public DbSet<AtendimentoEstacionamento> AtendimentosEstacionamento => Set<AtendimentoEstacionamento>();

    public DbSet<Caixa> Caixas => Set<Caixa>();
    public DbSet<Recebimento> Recebimentos => Set<Recebimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>().HasIndex(x => x.Nome);
        modelBuilder.Entity<Usuario>().HasIndex(x => new { x.EmpresaId, x.Login }).IsUnique();
        modelBuilder.Entity<Cliente>().HasIndex(x => new { x.EmpresaId, x.Nome });
        modelBuilder.Entity<Veiculo>().HasIndex(x => new { x.EmpresaId, x.Placa }).IsUnique();
        modelBuilder.Entity<Equipamento>().HasIndex(x => new { x.EmpresaId, x.UnidadeId, x.Nome });
        modelBuilder.Entity<Atendimento>().HasIndex(x => new { x.EmpresaId, x.Ticket });
        modelBuilder.Entity<Atendimento>().HasIndex(x => new { x.EmpresaId, x.StatusAtendimento });
        modelBuilder.Entity<AtendimentoCarregamento>().HasKey(x => x.AtendimentoId);
        modelBuilder.Entity<AtendimentoEstacionamento>().HasKey(x => x.AtendimentoId);
        modelBuilder.Entity<Caixa>().HasIndex(x => new { x.EmpresaId, x.UnidadeId, x.StatusCaixa });

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(EntityBase).IsAssignableFrom(entity.ClrType))
                modelBuilder.Entity(entity.ClrType).Property(nameof(EntityBase.Versao)).IsConcurrencyToken();
        }
    }
}
