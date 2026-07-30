// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Importação SQLite do ChargeDesk legado → schema Platform.

using System.Data;
using ChargeDesk.BuildingBlocks.Domain;
using ChargeDesk.BuildingBlocks.Time;
using ChargeDesk.Cadastros.Domain;
using ChargeDesk.Core.Domain;
using ChargeDesk.Financeiro.Domain;
using ChargeDesk.Infrastructure.Persistence;
using ChargeDesk.Infrastructure.Seed;
using ChargeDesk.Operacao.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChargeDesk.Infrastructure.Import;

public record LegacyImportResult(
    int Usuarios,
    int Clientes,
    int Veiculos,
    int Equipamentos,
    int Caixas,
    int Atendimentos,
    int Recebimentos,
    string Mensagem);

public class LegacySqliteImportService(PlatformDbContext db)
{
    public async Task<LegacyImportResult> ImportarAsync(
        string caminhoDbLegado,
        Guid? empresaId = null,
        Guid? unidadeId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(caminhoDbLegado) || !File.Exists(caminhoDbLegado))
            throw new FileNotFoundException("Arquivo SQLite legado não encontrado.", caminhoDbLegado);

        var eid = empresaId ?? DbSeed.EmpresaDemoId;
        var uid = unidadeId ?? DbSeed.UnidadeDemoId;
        var agora = HorarioOperacional.Agora();

        await using var src = new SqliteConnection($"Data Source={caminhoDbLegado}");
        await src.OpenAsync(ct);

        var map = await CarregarMapaAsync(ct);

        var nUsuarios = await ImportUsuariosAsync(src, eid, uid, agora, map, ct);
        var nClientes = await ImportClientesAsync(src, eid, agora, map, ct);
        var nVeiculos = await ImportVeiculosAsync(src, eid, agora, map, ct);
        var nEq = await ImportPontosAsync(src, eid, uid, agora, map, ct);
        var nCaixas = await ImportCaixasAsync(src, eid, uid, agora, map, ct);
        var nAt = await ImportSessoesAsync(src, eid, uid, agora, map, ct);
        var nRec = await ImportRecebimentosAsync(src, eid, agora, map, ct);

        await db.SaveChangesAsync(ct);

        return new LegacyImportResult(
            nUsuarios, nClientes, nVeiculos, nEq, nCaixas, nAt, nRec,
            "Importação concluída (idempotente por LegacyIdMap).");
    }

    private async Task<Dictionary<(string, int), Guid>> CarregarMapaAsync(CancellationToken ct)
    {
        var rows = await db.LegacyIdMaps.AsNoTracking().ToListAsync(ct);
        return rows.ToDictionary(x => (x.Tabela, x.LegacyId), x => x.NovoId);
    }

    private async Task<Guid> ResolverOuCriarMapAsync(
        Dictionary<(string, int), Guid> map,
        string tabela,
        int legacyId,
        Func<Guid> factory,
        CancellationToken ct)
    {
        if (map.TryGetValue((tabela, legacyId), out var existing))
            return existing;

        var novo = factory();
        db.LegacyIdMaps.Add(new LegacyIdMap { Tabela = tabela, LegacyId = legacyId, NovoId = novo });
        map[(tabela, legacyId)] = novo;
        await Task.CompletedTask;
        return novo;
    }

    private async Task<int> ImportUsuariosAsync(
        SqliteConnection src, Guid eid, Guid uid, DateTime agora,
        Dictionary<(string, int), Guid> map, CancellationToken ct)
    {
        var n = 0;
        await using var cmd = src.CreateCommand();
        cmd.CommandText = "SELECT Id, Nome, Login, SenhaHash, Ativo, Admin, UltimoAcesso, CriadoEm FROM Usuarios";
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var legacyId = r.GetInt32(0);
            var login = r.GetString(2);
            if (string.Equals(login, "admin", StringComparison.OrdinalIgnoreCase)
                && await db.Usuarios.AnyAsync(u => u.Login == "admin" && u.EmpresaId == eid, ct))
            {
                var admin = await db.Usuarios.FirstAsync(u => u.Login == "admin" && u.EmpresaId == eid, ct);
                await ResolverOuCriarMapAsync(map, "Usuarios", legacyId, () => admin.Id, ct);
                continue;
            }

            if (map.ContainsKey(("Usuarios", legacyId)))
                continue;

            var id = await ResolverOuCriarMapAsync(map, "Usuarios", legacyId, Guid.NewGuid, ct);
            if (await db.Usuarios.AnyAsync(u => u.Id == id, ct))
                continue;

            db.Usuarios.Add(new Usuario
            {
                Id = id,
                EmpresaId = eid,
                UnidadeId = uid,
                Nome = r.GetString(1),
                Login = login,
                SenhaHash = r.GetString(3),
                Status = r.GetBoolean(4) ? EntityStatus.Ativo : EntityStatus.Inativo,
                Admin = r.GetBoolean(5),
                UltimoAcesso = r.IsDBNull(6) ? null : r.GetDateTime(6),
                CriadoEm = r.IsDBNull(7) ? agora : r.GetDateTime(7)
            });
            n++;
        }
        return n;
    }

    private async Task<int> ImportClientesAsync(
        SqliteConnection src, Guid eid, DateTime agora,
        Dictionary<(string, int), Guid> map, CancellationToken ct)
    {
        var n = 0;
        await using var cmd = src.CreateCommand();
        cmd.CommandText = "SELECT Id, Nome, Cpf, Telefone, Email, Observacoes, Ativo, CriadoEm FROM Clientes";
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var legacyId = r.GetInt32(0);
            if (map.ContainsKey(("Clientes", legacyId)))
                continue;
            var id = await ResolverOuCriarMapAsync(map, "Clientes", legacyId, Guid.NewGuid, ct);
            db.Clientes.Add(new Cliente
            {
                Id = id,
                EmpresaId = eid,
                Nome = r.GetString(1),
                CpfCnpj = r.IsDBNull(2) ? null : r.GetString(2),
                Telefone = r.IsDBNull(3) ? null : r.GetString(3),
                Email = r.IsDBNull(4) ? null : r.GetString(4),
                Observacoes = r.IsDBNull(5) ? null : r.GetString(5),
                Status = r.GetBoolean(6) ? EntityStatus.Ativo : EntityStatus.Inativo,
                CriadoEm = r.IsDBNull(7) ? agora : r.GetDateTime(7)
            });
            n++;
        }
        return n;
    }

    private async Task<int> ImportVeiculosAsync(
        SqliteConnection src, Guid eid, DateTime agora,
        Dictionary<(string, int), Guid> map, CancellationToken ct)
    {
        var n = 0;
        await using var cmd = src.CreateCommand();
        cmd.CommandText = "SELECT Id, ClienteId, Placa, Marca, Modelo, Ano, Cor, Conector, Observacoes, Ativo FROM Veiculos";
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var legacyId = r.GetInt32(0);
            if (map.ContainsKey(("Veiculos", legacyId)))
                continue;
            var clienteLegacy = r.GetInt32(1);
            if (!map.TryGetValue(("Clientes", clienteLegacy), out var clienteId))
                continue;

            var id = await ResolverOuCriarMapAsync(map, "Veiculos", legacyId, Guid.NewGuid, ct);
            var placa = r.GetString(2).Trim().ToUpperInvariant();
            if (await db.Veiculos.AnyAsync(v => v.EmpresaId == eid && v.Placa == placa, ct))
                continue;

            db.Veiculos.Add(new Veiculo
            {
                Id = id,
                EmpresaId = eid,
                ClienteId = clienteId,
                Placa = placa,
                Marca = r.IsDBNull(3) ? null : r.GetString(3),
                Modelo = r.IsDBNull(4) ? null : r.GetString(4),
                Ano = r.IsDBNull(5) ? null : r.GetInt32(5),
                Cor = r.IsDBNull(6) ? null : r.GetString(6),
                Conector = r.IsDBNull(7) ? null : r.GetString(7),
                Observacoes = r.IsDBNull(8) ? null : r.GetString(8),
                Status = r.GetBoolean(9) ? EntityStatus.Ativo : EntityStatus.Inativo,
                CriadoEm = agora
            });
            n++;
        }
        return n;
    }

    private async Task<int> ImportPontosAsync(
        SqliteConnection src, Guid eid, Guid uid, DateTime agora,
        Dictionary<(string, int), Guid> map, CancellationToken ct)
    {
        var n = 0;
        await using var cmd = src.CreateCommand();
        cmd.CommandText = "SELECT Id, Nome, Descricao, Ativo FROM PontosCarregamento";
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var legacyId = r.GetInt32(0);
            if (map.ContainsKey(("PontosCarregamento", legacyId)))
                continue;
            var nome = r.GetString(1);
            var existente = await db.Equipamentos.FirstOrDefaultAsync(e =>
                e.EmpresaId == eid && e.Tipo == EquipamentoTipo.Carregador && e.Nome == nome, ct);
            if (existente is not null)
            {
                await ResolverOuCriarMapAsync(map, "PontosCarregamento", legacyId, () => existente.Id, ct);
                continue;
            }

            var id = await ResolverOuCriarMapAsync(map, "PontosCarregamento", legacyId, Guid.NewGuid, ct);
            db.Equipamentos.Add(new Equipamento
            {
                Id = id,
                EmpresaId = eid,
                UnidadeId = uid,
                Tipo = EquipamentoTipo.Carregador,
                Nome = nome,
                Descricao = r.IsDBNull(2) ? null : r.GetString(2),
                Status = r.GetBoolean(3) ? EntityStatus.Ativo : EntityStatus.Inativo,
                CriadoEm = agora
            });
            n++;
        }
        return n;
    }

    private async Task<int> ImportCaixasAsync(
        SqliteConnection src, Guid eid, Guid uid, DateTime agora,
        Dictionary<(string, int), Guid> map, CancellationToken ct)
    {
        var n = 0;
        await using var cmd = src.CreateCommand();
        cmd.CommandText = @"SELECT Id, Numero, OperadorId, DataAbertura, ValorInicial, DataFechamento,
            ValorInformado, ObservacoesFechamento, Status FROM Caixas";
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var legacyId = r.GetInt32(0);
            if (map.ContainsKey(("Caixas", legacyId)))
                continue;
            var opLegacy = r.GetInt32(2);
            var opId = map.TryGetValue(("Usuarios", opLegacy), out var ou) ? ou : DbSeed.AdminId;
            var id = await ResolverOuCriarMapAsync(map, "Caixas", legacyId, Guid.NewGuid, ct);
            var status = r.GetInt32(8);
            db.Caixas.Add(new Caixa
            {
                Id = id,
                EmpresaId = eid,
                UnidadeId = uid,
                OperadorId = opId,
                Numero = r.GetInt32(1),
                DataAbertura = r.GetDateTime(3),
                ValorInicial = r.GetDecimal(4),
                DataFechamento = r.IsDBNull(5) ? null : r.GetDateTime(5),
                ValorInformado = r.IsDBNull(6) ? null : r.GetDecimal(6),
                ObservacoesFechamento = r.IsDBNull(7) ? null : r.GetString(7),
                StatusCaixa = status == 1 ? StatusCaixa.Aberto : StatusCaixa.Fechado,
                CriadoEm = agora
            });
            n++;
        }
        return n;
    }

    private async Task<int> ImportSessoesAsync(
        SqliteConnection src, Guid eid, Guid uid, DateTime agora,
        Dictionary<(string, int), Guid> map, CancellationToken ct)
    {
        var n = 0;
        await using var cmd = src.CreateCommand();
        cmd.CommandText = @"SELECT Id, Ticket, ClienteId, VeiculoId, PontoCarregamentoId,
            HoraInicial, HoraFinal, TempoTotalMinutos, ValorCalculado, OperadorId, Status, Origem, Observacoes
            FROM Sessoes";
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var legacyId = r.GetInt32(0);
            if (map.ContainsKey(("Sessoes", legacyId)))
                continue;
            if (!map.TryGetValue(("Clientes", r.GetInt32(2)), out var clienteId))
                continue;
            if (!map.TryGetValue(("Veiculos", r.GetInt32(3)), out var veiculoId))
                continue;
            if (!map.TryGetValue(("PontosCarregamento", r.GetInt32(4)), out var eqId))
                continue;

            var opId = map.TryGetValue(("Usuarios", r.GetInt32(9)), out var o) ? o : DbSeed.AdminId;
            var statusLegado = r.GetInt32(10);
            var status = statusLegado switch
            {
                1 => AtendimentoStatus.EmExecucao,
                2 => AtendimentoStatus.AguardandoPagamento,
                3 => AtendimentoStatus.Finalizado,
                4 => AtendimentoStatus.Cancelado,
                _ => AtendimentoStatus.EmExecucao
            };
            var origem = r.GetInt32(11) == 2 ? AtendimentoOrigem.Integracao : AtendimentoOrigem.Manual;
            var id = await ResolverOuCriarMapAsync(map, "Sessoes", legacyId, Guid.NewGuid, ct);

            db.Atendimentos.Add(new Atendimento
            {
                Id = id,
                EmpresaId = eid,
                UnidadeId = uid,
                ClienteId = clienteId,
                VeiculoId = veiculoId,
                Tipo = AtendimentoTipo.Carregamento,
                StatusAtendimento = status,
                Origem = origem,
                Ticket = r.GetInt32(1),
                AbertoEm = r.GetDateTime(5),
                EncerradoEm = r.IsDBNull(6) ? null : r.GetDateTime(6),
                OperadorId = opId,
                Observacoes = r.IsDBNull(12) ? null : r.GetString(12),
                CriadoEm = agora
            });
            db.AtendimentosCarregamento.Add(new AtendimentoCarregamento
            {
                AtendimentoId = id,
                EquipamentoId = eqId,
                TempoMinutos = r.IsDBNull(7) ? null : r.GetInt32(7),
                ValorCalculado = r.IsDBNull(8) ? null : r.GetDecimal(8)
            });
            n++;
        }
        return n;
    }

    private async Task<int> ImportRecebimentosAsync(
        SqliteConnection src, Guid eid, DateTime agora,
        Dictionary<(string, int), Guid> map, CancellationToken ct)
    {
        var n = 0;
        await using var cmd = src.CreateCommand();
        cmd.CommandText = @"SELECT Id, CaixaId, Tipo, FormaPagamento, Valor, SessaoCarregamentoId, DataHora
            FROM CaixaMovimentacoes WHERE Tipo = 1 AND SessaoCarregamentoId IS NOT NULL";
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var legacyId = r.GetInt32(0);
            if (map.ContainsKey(("CaixaMovimentacoes", legacyId)))
                continue;
            if (!map.TryGetValue(("Caixas", r.GetInt32(1)), out var caixaId))
                continue;
            if (!map.TryGetValue(("Sessoes", r.GetInt32(5)), out var atId))
                continue;

            var forma = r.IsDBNull(3) ? FormaPagamento.Pix : (FormaPagamento)r.GetInt32(3);
            var id = await ResolverOuCriarMapAsync(map, "CaixaMovimentacoes", legacyId, Guid.NewGuid, ct);
            db.Recebimentos.Add(new Recebimento
            {
                Id = id,
                EmpresaId = eid,
                CaixaId = caixaId,
                AtendimentoId = atId,
                Forma = forma,
                Valor = r.GetDecimal(4),
                DataHora = r.GetDateTime(6),
                CriadoEm = agora
            });
            n++;
        }
        return n;
    }
}
