// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Garante tabelas novas em SQLite sem migration formal (MVP).

using ChargeDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChargeDesk.Infrastructure.Seed;

public static class SchemaEnsure
{
    public static async Task ApplyAsync(PlatformDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AtendimentosEstacionamento" (
                "AtendimentoId" TEXT NOT NULL CONSTRAINT "PK_AtendimentosEstacionamento" PRIMARY KEY,
                "EquipamentoId" TEXT NULL,
                "Vaga" TEXT NULL,
                "TempoMinutos" INTEGER NULL,
                "ValorCalculado" TEXT NULL
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "LegacyIdMaps" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_LegacyIdMaps" PRIMARY KEY,
                "Tabela" TEXT NOT NULL,
                "LegacyId" INTEGER NOT NULL,
                "NovoId" TEXT NOT NULL
            );
            """);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_LegacyIdMaps_Tabela_LegacyId"
            ON "LegacyIdMaps" ("Tabela", "LegacyId");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AgendaReservas" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AgendaReservas" PRIMARY KEY,
                "EmpresaId" TEXT NOT NULL,
                "Status" INTEGER NOT NULL,
                "CriadoEm" TEXT NOT NULL,
                "CriadoPor" TEXT NULL,
                "AtualizadoEm" TEXT NULL,
                "AtualizadoPor" TEXT NULL,
                "ExcluidoEm" TEXT NULL,
                "ExcluidoPor" TEXT NULL,
                "Versao" INTEGER NOT NULL,
                "UnidadeId" TEXT NOT NULL,
                "ClienteId" TEXT NOT NULL,
                "VeiculoId" TEXT NULL,
                "TipoServico" INTEGER NOT NULL,
                "InicioPrevisto" TEXT NOT NULL,
                "FimPrevisto" TEXT NULL,
                "StatusAgenda" INTEGER NOT NULL,
                "AtendimentoId" TEXT NULL,
                "Observacoes" TEXT NULL
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "OrdensServico" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_OrdensServico" PRIMARY KEY,
                "EmpresaId" TEXT NOT NULL,
                "Status" INTEGER NOT NULL,
                "CriadoEm" TEXT NOT NULL,
                "CriadoPor" TEXT NULL,
                "AtualizadoEm" TEXT NULL,
                "AtualizadoPor" TEXT NULL,
                "ExcluidoEm" TEXT NULL,
                "ExcluidoPor" TEXT NULL,
                "Versao" INTEGER NOT NULL,
                "UnidadeId" TEXT NOT NULL,
                "AtendimentoId" TEXT NOT NULL,
                "ClienteId" TEXT NOT NULL,
                "VeiculoId" TEXT NULL,
                "Numero" INTEGER NOT NULL,
                "StatusOs" INTEGER NOT NULL,
                "Diagnostico" TEXT NULL,
                "Observacoes" TEXT NULL,
                "AbertaEm" TEXT NOT NULL,
                "EncerradaEm" TEXT NULL
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "OrdemServicoItens" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_OrdemServicoItens" PRIMARY KEY,
                "OrdemServicoId" TEXT NOT NULL,
                "Descricao" TEXT NOT NULL,
                "Tipo" INTEGER NOT NULL,
                "Quantidade" TEXT NOT NULL,
                "ValorUnitario" TEXT NOT NULL,
                "Concluido" INTEGER NOT NULL
            );
            """);
    }
}
