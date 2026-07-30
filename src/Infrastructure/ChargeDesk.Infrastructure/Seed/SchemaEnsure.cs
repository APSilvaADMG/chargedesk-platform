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
    }
}
