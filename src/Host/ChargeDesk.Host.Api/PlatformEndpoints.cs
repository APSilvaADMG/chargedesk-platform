// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Endpoints Fase 1 (paridade) + Fase 2 Estacionamento.

using ChargeDesk.BuildingBlocks.Domain;
using ChargeDesk.BuildingBlocks.Time;
using ChargeDesk.Cadastros.Domain;
using ChargeDesk.Core.Domain;
using ChargeDesk.Financeiro.Application;
using ChargeDesk.Financeiro.Domain;
using ChargeDesk.Infrastructure.Persistence;
using ChargeDesk.Infrastructure.Seed;
using ChargeDesk.Operacao.Application;
using ChargeDesk.Operacao.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChargeDesk.Host.Api;

public static class PlatformEndpoints
{
    public static void MapPlatformApi(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/health", () => Results.Ok(new
        {
            produto = "ChargeDesk Platform",
            status = "ok",
            horarioOperacional = HorarioOperacional.Agora(),
            utc = DateTime.UtcNow
        }));

        MapAuth(api);
        MapCadastros(api);
        MapEquipamentos(api);
        MapCaixa(api);
        MapAtendimentos(api);
    }

    private static void MapAuth(RouteGroupBuilder api)
    {
        api.MapPost("/auth/login", async (LoginRequest req, PlatformDbContext db) =>
        {
            var login = (req.Login ?? "").Trim().ToLowerInvariant();
            var user = await db.Usuarios.FirstOrDefaultAsync(u =>
                u.Login.ToLower() == login && u.Status == EntityStatus.Ativo);
            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Senha ?? "", user.SenhaHash))
                return Results.Unauthorized();

            user.UltimoAcesso = HorarioOperacional.Agora();
            await db.SaveChangesAsync();
            return Results.Ok(new
            {
                user.Id,
                user.Nome,
                user.Login,
                user.EmpresaId,
                user.UnidadeId,
                user.Admin
            });
        });
    }

    private static void MapCadastros(RouteGroupBuilder api)
    {
        api.MapGet("/clientes", async (PlatformDbContext db, Guid? empresaId) =>
        {
            var eid = empresaId ?? DbSeed.EmpresaDemoId;
            var lista = await db.Clientes
                .Where(c => c.EmpresaId == eid && c.Status == EntityStatus.Ativo)
                .OrderBy(c => c.Nome)
                .Select(c => new { c.Id, c.Nome, c.Telefone, c.Email, c.CpfCnpj })
                .ToListAsync();
            return Results.Ok(lista);
        });

        api.MapPost("/clientes", async (ClienteCreateRequest req, PlatformDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Nome))
                return Results.BadRequest("Informe o nome do cliente.");
            if (string.IsNullOrWhiteSpace(req.Telefone))
                return Results.BadRequest("Informe o telefone do cliente.");

            var c = new Cliente
            {
                EmpresaId = req.EmpresaId == Guid.Empty ? DbSeed.EmpresaDemoId : req.EmpresaId,
                Nome = req.Nome.Trim(),
                Telefone = req.Telefone.Trim(),
                Email = req.Email?.Trim(),
                CpfCnpj = req.CpfCnpj?.Trim(),
                Observacoes = req.Observacoes?.Trim(),
                CriadoEm = HorarioOperacional.Agora()
            };
            db.Clientes.Add(c);
            await db.SaveChangesAsync();
            return Results.Created($"/api/clientes/{c.Id}", new { c.Id, c.Nome, c.Telefone });
        });

        api.MapGet("/veiculos", async (PlatformDbContext db, Guid? empresaId) =>
        {
            var eid = empresaId ?? DbSeed.EmpresaDemoId;
            var lista = await (
                from v in db.Veiculos
                join c in db.Clientes on v.ClienteId equals c.Id
                where v.EmpresaId == eid && v.Status == EntityStatus.Ativo
                orderby v.Placa
                select new
                {
                    v.Id,
                    v.ClienteId,
                    ClienteNome = c.Nome,
                    ClienteTelefone = c.Telefone,
                    v.Placa,
                    v.Marca,
                    v.Modelo,
                    v.Ano,
                    v.Cor,
                    v.Conector,
                    v.Observacoes
                }).ToListAsync();
            return Results.Ok(lista);
        });

        api.MapPost("/veiculos", async (VeiculoCreateRequest req, PlatformDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Placa))
                return Results.BadRequest("Informe a placa.");
            var placa = req.Placa.Trim().ToUpperInvariant();
            var eid = req.EmpresaId == Guid.Empty ? DbSeed.EmpresaDemoId : req.EmpresaId;
            if (await db.Veiculos.AnyAsync(v => v.EmpresaId == eid && v.Placa == placa))
                return Results.BadRequest("Placa já cadastrada.");
            if (!await db.Clientes.AnyAsync(c => c.Id == req.ClienteId && c.EmpresaId == eid))
                return Results.BadRequest("Cliente não encontrado.");

            var v = new Veiculo
            {
                EmpresaId = eid,
                ClienteId = req.ClienteId,
                Placa = placa,
                Marca = req.Marca?.Trim(),
                Modelo = req.Modelo?.Trim(),
                Ano = req.Ano,
                Cor = req.Cor?.Trim(),
                Conector = req.Conector?.Trim(),
                Observacoes = req.Observacoes?.Trim(),
                CriadoEm = HorarioOperacional.Agora()
            };
            db.Veiculos.Add(v);
            await db.SaveChangesAsync();
            return Results.Created($"/api/veiculos/{v.Id}", new { v.Id, v.Placa, v.ClienteId });
        });
    }

    private static void MapEquipamentos(RouteGroupBuilder api)
    {
        api.MapGet("/equipamentos", async (PlatformDbContext db, Guid? empresaId, EquipamentoTipo? tipo) =>
        {
            var eid = empresaId ?? DbSeed.EmpresaDemoId;
            var q = db.Equipamentos.Where(e => e.EmpresaId == eid && e.Status == EntityStatus.Ativo);
            if (tipo.HasValue)
                q = q.Where(e => e.Tipo == tipo);
            var lista = await q.OrderBy(e => e.Nome)
                .Select(e => new { e.Id, e.Nome, e.Tipo, e.UnidadeId, e.Descricao })
                .ToListAsync();
            return Results.Ok(lista);
        });

        api.MapGet("/equipamentos/disponiveis", async (PlatformDbContext db, Guid? empresaId, EquipamentoTipo? tipo) =>
        {
            var eid = empresaId ?? DbSeed.EmpresaDemoId;
            var tipoFiltro = tipo ?? EquipamentoTipo.Carregador;

            HashSet<Guid> ocupados;
            if (tipoFiltro == EquipamentoTipo.Vaga)
            {
                ocupados = (await db.AtendimentosEstacionamento
                    .Join(db.Atendimentos, ae => ae.AtendimentoId, a => a.Id, (ae, a) => new { ae.EquipamentoId, a.StatusAtendimento, a.EmpresaId })
                    .Where(x => x.EmpresaId == eid
                        && x.StatusAtendimento == AtendimentoStatus.EmExecucao
                        && x.EquipamentoId != null)
                    .Select(x => x.EquipamentoId!.Value)
                    .Distinct()
                    .ToListAsync()).ToHashSet();
            }
            else
            {
                ocupados = (await db.AtendimentosCarregamento
                    .Join(db.Atendimentos, ac => ac.AtendimentoId, a => a.Id, (ac, a) => new { ac.EquipamentoId, a.StatusAtendimento, a.EmpresaId })
                    .Where(x => x.EmpresaId == eid && x.StatusAtendimento == AtendimentoStatus.EmExecucao)
                    .Select(x => x.EquipamentoId)
                    .Distinct()
                    .ToListAsync()).ToHashSet();
            }

            var candidatos = await db.Equipamentos
                .Where(e => e.EmpresaId == eid
                    && e.Status == EntityStatus.Ativo
                    && e.Tipo == tipoFiltro)
                .OrderBy(e => e.Nome)
                .ToListAsync();

            var lista = candidatos
                .Where(e => AtendimentoValidacaoService.EquipamentoDisponivel(true, ocupados.Contains(e.Id)))
                .Select(e => new { e.Id, e.Nome, e.UnidadeId, e.Tipo })
                .ToList();
            return Results.Ok(lista);
        });
    }

    private static void MapCaixa(RouteGroupBuilder api)
    {
        api.MapGet("/caixa/atual", async (PlatformDbContext db, Guid? empresaId, Guid? unidadeId) =>
        {
            var eid = empresaId ?? DbSeed.EmpresaDemoId;
            var uid = unidadeId ?? DbSeed.UnidadeDemoId;
            var caixa = await db.Caixas
                .Where(c => c.EmpresaId == eid && c.UnidadeId == uid && c.StatusCaixa == StatusCaixa.Aberto)
                .OrderByDescending(c => c.DataAbertura)
                .Select(c => new { c.Id, c.Numero, c.DataAbertura, c.ValorInicial, c.OperadorId })
                .FirstOrDefaultAsync();
            return Results.Ok(caixa);
        });

        api.MapGet("/caixa/{id:guid}/recebimentos", async (Guid id, PlatformDbContext db) =>
        {
            var caixa = await db.Caixas.FindAsync(id);
            if (caixa is null) return Results.NotFound();

            var lista = await (
                from r in db.Recebimentos
                join a in db.Atendimentos on r.AtendimentoId equals a.Id into aj
                from a in aj.DefaultIfEmpty()
                where r.CaixaId == id
                orderby r.DataHora descending
                select new
                {
                    r.Id,
                    r.Forma,
                    r.Valor,
                    r.DataHora,
                    r.AtendimentoId,
                    Ticket = a != null ? a.Ticket : null,
                    Tipo = a != null ? (AtendimentoTipo?)a.Tipo : null
                }).ToListAsync();
            return Results.Ok(lista);
        });

        api.MapGet("/caixa/historico", async (PlatformDbContext db, Guid? empresaId, Guid? unidadeId) =>
        {
            var eid = empresaId ?? DbSeed.EmpresaDemoId;
            var uid = unidadeId ?? DbSeed.UnidadeDemoId;
            var lista = await db.Caixas
                .Where(c => c.EmpresaId == eid && c.UnidadeId == uid)
                .OrderByDescending(c => c.DataAbertura)
                .Take(30)
                .Select(c => new
                {
                    c.Id,
                    c.Numero,
                    c.DataAbertura,
                    c.DataFechamento,
                    c.ValorInicial,
                    c.ValorInformado,
                    c.StatusCaixa,
                    c.ObservacoesFechamento
                })
                .ToListAsync();
            return Results.Ok(lista);
        });

        api.MapPost("/caixa/abrir", async (CaixaAbrirRequest req, PlatformDbContext db) =>
        {
            var eid = req.EmpresaId == Guid.Empty ? DbSeed.EmpresaDemoId : req.EmpresaId;
            var uid = req.UnidadeId == Guid.Empty ? DbSeed.UnidadeDemoId : req.UnidadeId;
            if (await db.Caixas.AnyAsync(c => c.EmpresaId == eid && c.UnidadeId == uid && c.StatusCaixa == StatusCaixa.Aberto))
                return Results.BadRequest("Já existe um caixa aberto nesta unidade.");

            var numero = (await db.Caixas.CountAsync(c => c.EmpresaId == eid && c.UnidadeId == uid)) + 1;
            var caixa = new Caixa
            {
                EmpresaId = eid,
                UnidadeId = uid,
                OperadorId = req.OperadorId == Guid.Empty ? DbSeed.AdminId : req.OperadorId,
                Numero = numero,
                DataAbertura = HorarioOperacional.Agora(),
                ValorInicial = req.ValorInicial,
                StatusCaixa = StatusCaixa.Aberto,
                CriadoEm = HorarioOperacional.Agora()
            };
            db.Caixas.Add(caixa);
            await db.SaveChangesAsync();
            return Results.Created($"/api/caixa/{caixa.Id}", new { caixa.Id, caixa.Numero });
        });

        api.MapPost("/caixa/{id:guid}/fechar", async (Guid id, CaixaFecharRequest req, PlatformDbContext db) =>
        {
            var caixa = await db.Caixas.FindAsync(id);
            if (caixa is null) return Results.NotFound();
            if (caixa.StatusCaixa != StatusCaixa.Aberto)
                return Results.BadRequest("Caixa já está fechado.");

            var ativos = await db.Atendimentos.CountAsync(a =>
                a.EmpresaId == caixa.EmpresaId
                && a.UnidadeId == caixa.UnidadeId
                && a.StatusAtendimento == AtendimentoStatus.EmExecucao);
            var erro = AtendimentoValidacaoService.ValidarFechamentoSemAtendimentosAtivos(ativos);
            if (erro is not null) return Results.BadRequest(erro);

            caixa.DataFechamento = HorarioOperacional.Agora();
            caixa.ValorInformado = req.ValorInformado;
            caixa.ObservacoesFechamento = req.Observacoes?.Trim();
            caixa.StatusCaixa = StatusCaixa.Fechado;
            caixa.AtualizadoEm = HorarioOperacional.Agora();
            await db.SaveChangesAsync();
            return Results.Ok(new { caixa.Id, caixa.StatusCaixa });
        });
    }

    private static void MapAtendimentos(RouteGroupBuilder api)
    {
        api.MapGet("/atendimentos/proximo-ticket", async (PlatformDbContext db, Guid? empresaId) =>
        {
            var eid = empresaId ?? DbSeed.EmpresaDemoId;
            var max = await db.Atendimentos
                .Where(a => a.EmpresaId == eid && a.Ticket != null)
                .MaxAsync(a => (int?)a.Ticket) ?? 0;
            return Results.Ok(new { ticket = max + 1 });
        });

        api.MapGet("/atendimentos", async (PlatformDbContext db, Guid? empresaId, AtendimentoStatus? status) =>
        {
            var eid = empresaId ?? DbSeed.EmpresaDemoId;
            var q =
                from a in db.Atendimentos
                join c in db.Clientes on a.ClienteId equals c.Id
                join v in db.Veiculos on a.VeiculoId equals v.Id into vj
                from v in vj.DefaultIfEmpty()
                join ac in db.AtendimentosCarregamento on a.Id equals ac.AtendimentoId into acj
                from ac in acj.DefaultIfEmpty()
                join ae in db.AtendimentosEstacionamento on a.Id equals ae.AtendimentoId into aej
                from ae in aej.DefaultIfEmpty()
                where a.EmpresaId == eid
                select new { a, c, v, ac, ae };

            if (status.HasValue)
                q = q.Where(x => x.a.StatusAtendimento == status);

            var agora = HorarioOperacional.Agora();
            var lista = await q.OrderByDescending(x => x.a.AbertoEm).Take(200).ToListAsync();
            var result = lista.Select(x =>
            {
                var (minutos, valor) = ResolverTempoValor(x.a, x.ac, x.ae, agora);
                return new
                {
                    x.a.Id,
                    x.a.Ticket,
                    x.a.Tipo,
                    x.a.StatusAtendimento,
                    x.a.ClienteId,
                    ClienteNome = x.c.Nome,
                    x.a.VeiculoId,
                    Placa = x.v != null ? x.v.Placa : null,
                    Recurso = x.a.Tipo == AtendimentoTipo.Estacionamento
                        ? (x.ae?.Vaga ?? "")
                        : "",
                    x.a.AbertoEm,
                    x.a.EncerradoEm,
                    minutos,
                    tempo = minutos.HasValue ? CobrancaService.FormatarTempo(minutos.Value) : null,
                    valor
                };
            });
            return Results.Ok(result);
        });

        api.MapPost("/atendimentos/carregamento", async (AtendimentoCarregamentoCreateRequest req, PlatformDbContext db) =>
        {
            var eid = req.EmpresaId == Guid.Empty ? DbSeed.EmpresaDemoId : req.EmpresaId;
            var uid = req.UnidadeId == Guid.Empty ? DbSeed.UnidadeDemoId : req.UnidadeId;

            var caixaAberto = await db.Caixas.AnyAsync(c =>
                c.EmpresaId == eid && c.UnidadeId == uid && c.StatusCaixa == StatusCaixa.Aberto);
            var erroCaixa = AtendimentoValidacaoService.ValidarCaixaAbertoParaIniciar(caixaAberto);
            if (erroCaixa is not null) return Results.BadRequest(erroCaixa);

            var ticket = req.Ticket > 0
                ? req.Ticket
                : (await db.Atendimentos.Where(a => a.EmpresaId == eid && a.Ticket != null).MaxAsync(a => (int?)a.Ticket) ?? 0) + 1;
            var duplicado = await db.Atendimentos.AnyAsync(a => a.EmpresaId == eid && a.Ticket == ticket);
            var erroTicket = AtendimentoValidacaoService.ValidarTicket(ticket, duplicado);
            if (erroTicket is not null) return Results.BadRequest(erroTicket);

            var veiculo = await db.Veiculos.FirstOrDefaultAsync(v =>
                v.Id == req.VeiculoId && v.EmpresaId == eid && v.Status == EntityStatus.Ativo);
            if (veiculo is null) return Results.BadRequest("Veículo não encontrado ou inativo.");
            var clienteId = AtendimentoClienteResolver.ResolverClienteId(veiculo.ClienteId);

            var equipamento = await db.Equipamentos.FirstOrDefaultAsync(e =>
                e.Id == req.EquipamentoId && e.EmpresaId == eid && e.Tipo == EquipamentoTipo.Carregador);
            var ocupado = await db.AtendimentosCarregamento
                .Join(db.Atendimentos, ac => ac.AtendimentoId, a => a.Id, (ac, a) => new { ac, a })
                .AnyAsync(x => x.ac.EquipamentoId == req.EquipamentoId
                    && x.a.StatusAtendimento == AtendimentoStatus.EmExecucao);
            var erroEq = AtendimentoValidacaoService.ValidarEquipamento(
                equipamento is not null && equipamento.Status == EntityStatus.Ativo, ocupado);
            if (erroEq is not null) return Results.BadRequest(erroEq);

            var inicio = req.HoraInicial ?? HorarioOperacional.Agora();
            var erroHorario = AtendimentoValidacaoService.ValidarHorarios(inicio, req.HoraFinal);
            if (erroHorario is not null) return Results.BadRequest(erroHorario);

            var atendimento = new Atendimento
            {
                EmpresaId = eid,
                UnidadeId = uid,
                ClienteId = clienteId,
                VeiculoId = veiculo.Id,
                Tipo = AtendimentoTipo.Carregamento,
                StatusAtendimento = AtendimentoStatus.EmExecucao,
                Origem = AtendimentoOrigem.Manual,
                Ticket = ticket,
                AbertoEm = inicio,
                EncerradoEm = req.HoraFinal,
                OperadorId = req.OperadorId == Guid.Empty ? DbSeed.AdminId : req.OperadorId,
                Observacoes = req.Observacoes?.Trim(),
                CriadoEm = HorarioOperacional.Agora()
            };

            if (req.HoraFinal.HasValue)
            {
                var min = CobrancaService.CalcularMinutos(inicio, req.HoraFinal.Value);
                var valor = CobrancaService.CalcularValor(min);
                atendimento.StatusAtendimento = AtendimentoStatus.AguardandoPagamento;
                db.Atendimentos.Add(atendimento);
                db.AtendimentosCarregamento.Add(new AtendimentoCarregamento
                {
                    AtendimentoId = atendimento.Id,
                    EquipamentoId = req.EquipamentoId,
                    TempoMinutos = min,
                    ValorCalculado = valor
                });
            }
            else
            {
                db.Atendimentos.Add(atendimento);
                db.AtendimentosCarregamento.Add(new AtendimentoCarregamento
                {
                    AtendimentoId = atendimento.Id,
                    EquipamentoId = req.EquipamentoId
                });
            }

            await db.SaveChangesAsync();
            return Results.Created($"/api/atendimentos/{atendimento.Id}", new
            {
                atendimento.Id,
                atendimento.Ticket,
                atendimento.ClienteId,
                atendimento.VeiculoId,
                atendimento.StatusAtendimento,
                equipamentoId = req.EquipamentoId
            });
        });

        api.MapPost("/atendimentos/estacionamento", async (AtendimentoEstacionamentoCreateRequest req, PlatformDbContext db) =>
        {
            var eid = req.EmpresaId == Guid.Empty ? DbSeed.EmpresaDemoId : req.EmpresaId;
            var uid = req.UnidadeId == Guid.Empty ? DbSeed.UnidadeDemoId : req.UnidadeId;

            var caixaAberto = await db.Caixas.AnyAsync(c =>
                c.EmpresaId == eid && c.UnidadeId == uid && c.StatusCaixa == StatusCaixa.Aberto);
            var erroCaixa = AtendimentoValidacaoService.ValidarCaixaAbertoParaIniciar(caixaAberto);
            if (erroCaixa is not null) return Results.BadRequest(erroCaixa);

            var ticket = req.Ticket > 0
                ? req.Ticket
                : (await db.Atendimentos.Where(a => a.EmpresaId == eid && a.Ticket != null).MaxAsync(a => (int?)a.Ticket) ?? 0) + 1;
            var duplicado = await db.Atendimentos.AnyAsync(a => a.EmpresaId == eid && a.Ticket == ticket);
            var erroTicket = AtendimentoValidacaoService.ValidarTicket(ticket, duplicado);
            if (erroTicket is not null) return Results.BadRequest(erroTicket);

            var veiculo = await db.Veiculos.FirstOrDefaultAsync(v =>
                v.Id == req.VeiculoId && v.EmpresaId == eid && v.Status == EntityStatus.Ativo);
            if (veiculo is null) return Results.BadRequest("Veículo não encontrado ou inativo.");
            var clienteId = AtendimentoClienteResolver.ResolverClienteId(veiculo.ClienteId);

            var vaga = await db.Equipamentos.FirstOrDefaultAsync(e =>
                e.Id == req.EquipamentoId && e.EmpresaId == eid && e.Tipo == EquipamentoTipo.Vaga);
            var ocupado = await db.AtendimentosEstacionamento
                .Join(db.Atendimentos, ae => ae.AtendimentoId, a => a.Id, (ae, a) => new { ae, a })
                .AnyAsync(x => x.ae.EquipamentoId == req.EquipamentoId
                    && x.a.StatusAtendimento == AtendimentoStatus.EmExecucao);
            var erroVaga = AtendimentoValidacaoService.ValidarVaga(
                vaga is not null && vaga.Status == EntityStatus.Ativo, ocupado);
            if (erroVaga is not null) return Results.BadRequest(erroVaga);

            var inicio = req.HoraInicial ?? HorarioOperacional.Agora();
            var atendimento = new Atendimento
            {
                EmpresaId = eid,
                UnidadeId = uid,
                ClienteId = clienteId,
                VeiculoId = veiculo.Id,
                Tipo = AtendimentoTipo.Estacionamento,
                StatusAtendimento = AtendimentoStatus.EmExecucao,
                Origem = AtendimentoOrigem.Manual,
                Ticket = ticket,
                AbertoEm = inicio,
                OperadorId = req.OperadorId == Guid.Empty ? DbSeed.AdminId : req.OperadorId,
                Observacoes = req.Observacoes?.Trim(),
                CriadoEm = HorarioOperacional.Agora()
            };
            db.Atendimentos.Add(atendimento);
            db.AtendimentosEstacionamento.Add(new AtendimentoEstacionamento
            {
                AtendimentoId = atendimento.Id,
                EquipamentoId = req.EquipamentoId,
                Vaga = vaga!.Nome
            });
            await db.SaveChangesAsync();
            return Results.Created($"/api/atendimentos/{atendimento.Id}", new
            {
                atendimento.Id,
                atendimento.Ticket,
                atendimento.ClienteId,
                atendimento.VeiculoId,
                atendimento.StatusAtendimento,
                equipamentoId = req.EquipamentoId,
                vaga = vaga.Nome
            });
        });

        api.MapPost("/atendimentos/{id:guid}/finalizar", async (Guid id, FinalizarRequest req, PlatformDbContext db) =>
        {
            var a = await db.Atendimentos.FindAsync(id);
            if (a is null) return Results.NotFound();
            if (a.StatusAtendimento != AtendimentoStatus.EmExecucao)
                return Results.BadRequest("Atendimento não está em execução.");

            var fim = req.HoraFinal ?? HorarioOperacional.Agora();
            var erroH = AtendimentoValidacaoService.ValidarHorarios(a.AbertoEm, fim);
            if (erroH is not null) return Results.BadRequest(erroH);

            var min = CobrancaService.CalcularMinutos(a.AbertoEm, fim);
            var tarifa = TarifaDoTipo(a.Tipo);
            var valor = CobrancaService.CalcularValor(min, tarifa);
            a.EncerradoEm = fim;
            a.StatusAtendimento = AtendimentoStatus.AguardandoPagamento;
            a.AtualizadoEm = HorarioOperacional.Agora();

            if (a.Tipo == AtendimentoTipo.Estacionamento)
            {
                var ext = await db.AtendimentosEstacionamento.FindAsync(id);
                if (ext is not null)
                {
                    ext.TempoMinutos = min;
                    ext.ValorCalculado = valor;
                }
            }
            else
            {
                var ext = await db.AtendimentosCarregamento.FindAsync(id);
                if (ext is not null)
                {
                    ext.TempoMinutos = min;
                    ext.ValorCalculado = valor;
                }
            }

            await db.SaveChangesAsync();
            return Results.Ok(new
            {
                a.Id,
                a.StatusAtendimento,
                minutos = min,
                tempo = CobrancaService.FormatarTempo(min),
                valor
            });
        });

        api.MapGet("/atendimentos/{id:guid}", async (Guid id, PlatformDbContext db) =>
        {
            var a = await db.Atendimentos.FindAsync(id);
            if (a is null) return Results.NotFound();
            var ac = await db.AtendimentosCarregamento.FindAsync(id);
            var ae = await db.AtendimentosEstacionamento.FindAsync(id);
            var cliente = await db.Clientes.FindAsync(a.ClienteId);
            var veiculo = a.VeiculoId.HasValue ? await db.Veiculos.FindAsync(a.VeiculoId.Value) : null;
            Guid? eqId = a.Tipo == AtendimentoTipo.Estacionamento ? ae?.EquipamentoId : ac?.EquipamentoId;
            var equipamento = eqId.HasValue ? await db.Equipamentos.FindAsync(eqId.Value) : null;

            var (minutos, valor) = ResolverTempoValor(a, ac, ae, HorarioOperacional.Agora());

            return Results.Ok(new
            {
                a.Id,
                a.Ticket,
                a.Tipo,
                a.StatusAtendimento,
                a.ClienteId,
                ClienteNome = cliente?.Nome,
                ClienteTelefone = cliente?.Telefone,
                a.VeiculoId,
                Placa = veiculo?.Placa,
                Marca = veiculo?.Marca,
                Modelo = veiculo?.Modelo,
                EquipamentoId = eqId,
                EquipamentoNome = equipamento?.Nome,
                Vaga = ae?.Vaga,
                a.AbertoEm,
                a.EncerradoEm,
                minutos,
                tempo = minutos.HasValue ? CobrancaService.FormatarTempo(minutos.Value) : null,
                valor,
                a.Observacoes
            });
        });

        api.MapGet("/atendimentos/{id:guid}/ticket", async (Guid id, PlatformDbContext db) =>
        {
            var a = await db.Atendimentos.FindAsync(id);
            if (a is null) return Results.NotFound();
            if (a.Ticket is null) return Results.BadRequest("Atendimento sem número de ticket.");

            var cliente = await db.Clientes.FindAsync(a.ClienteId);
            var veiculo = a.VeiculoId.HasValue ? await db.Veiculos.FindAsync(a.VeiculoId.Value) : null;
            string recurso;
            TarifaConfig tarifa;
            string titulo;
            if (a.Tipo == AtendimentoTipo.Estacionamento)
            {
                var ae = await db.AtendimentosEstacionamento.FindAsync(id);
                recurso = ae?.Vaga ?? "Vaga";
                tarifa = CobrancaService.EstacionamentoPadrao;
                titulo = "ChargeDesk — Estacionamento";
            }
            else
            {
                var ac = await db.AtendimentosCarregamento.FindAsync(id);
                var eq = ac is not null ? await db.Equipamentos.FindAsync(ac.EquipamentoId) : null;
                recurso = eq?.Nome ?? "Carregador";
                tarifa = CobrancaService.Padrao;
                titulo = "ChargeDesk — Carregamento";
            }

            var html = TicketHtmlService.GerarInicio(
                a.Ticket.Value,
                titulo,
                cliente?.Nome ?? "—",
                veiculo?.Placa ?? "—",
                recurso,
                a.AbertoEm,
                tarifa);
            return Results.Content(html, "text/html; charset=utf-8");
        });

        api.MapPost("/atendimentos/{id:guid}/pagar", async (Guid id, PagarRequest req, PlatformDbContext db) =>
        {
            var a = await db.Atendimentos.FindAsync(id);
            if (a is null) return Results.NotFound();
            if (a.StatusAtendimento == AtendimentoStatus.Finalizado)
                return Results.BadRequest("Atendimento já está pago/finalizado.");
            if (a.StatusAtendimento != AtendimentoStatus.AguardandoPagamento)
                return Results.BadRequest("Atendimento precisa estar aguardando pagamento.");

            var erroForma = RecebimentoValidacaoService.ValidarCortesia(req.Forma, req.MotivoCortesia);
            if (erroForma is not null) return Results.BadRequest(erroForma);

            var caixa = await db.Caixas
                .Where(c => c.EmpresaId == a.EmpresaId && c.UnidadeId == a.UnidadeId && c.StatusCaixa == StatusCaixa.Aberto)
                .OrderByDescending(c => c.DataAbertura)
                .FirstOrDefaultAsync();
            if (caixa is null)
                return Results.BadRequest("Abra o caixa antes de registrar o pagamento.");

            decimal valorSessao = 0m;
            if (a.Tipo == AtendimentoTipo.Estacionamento)
            {
                var ext = await db.AtendimentosEstacionamento.FindAsync(id);
                valorSessao = ext?.ValorCalculado ?? 0m;
            }
            else
            {
                var ext = await db.AtendimentosCarregamento.FindAsync(id);
                valorSessao = ext?.ValorCalculado ?? 0m;
            }

            var valorReceber = RecebimentoValidacaoService.ValorRecebimentoPagamento(req.Forma, valorSessao);

            var recebimento = new Recebimento
            {
                EmpresaId = a.EmpresaId,
                CaixaId = caixa.Id,
                AtendimentoId = a.Id,
                Forma = req.Forma,
                Valor = valorReceber,
                DataHora = HorarioOperacional.Agora(),
                CriadoEm = HorarioOperacional.Agora()
            };
            db.Recebimentos.Add(recebimento);

            var erroFin = AtendimentoValidacaoService.ValidarTransicao(
                a.StatusAtendimento, AtendimentoStatus.Finalizado);
            if (erroFin is not null) return Results.BadRequest(erroFin);

            a.StatusAtendimento = AtendimentoStatus.Finalizado;
            a.AtualizadoEm = HorarioOperacional.Agora();
            if (!string.IsNullOrWhiteSpace(req.MotivoCortesia))
                a.Observacoes = string.IsNullOrWhiteSpace(a.Observacoes)
                    ? $"Cortesia: {req.MotivoCortesia.Trim()}"
                    : $"{a.Observacoes} | Cortesia: {req.MotivoCortesia.Trim()}";

            await db.SaveChangesAsync();
            return Results.Ok(new
            {
                a.Id,
                a.StatusAtendimento,
                recebimentoId = recebimento.Id,
                valor = valorReceber,
                forma = req.Forma
            });
        });
    }

    private static TarifaConfig TarifaDoTipo(AtendimentoTipo tipo)
        => tipo == AtendimentoTipo.Estacionamento
            ? CobrancaService.EstacionamentoPadrao
            : CobrancaService.Padrao;

    private static (int? minutos, decimal? valor) ResolverTempoValor(
        Atendimento a,
        AtendimentoCarregamento? ac,
        AtendimentoEstacionamento? ae,
        DateTime agora)
    {
        int? minutos;
        decimal? valor;
        var tarifa = TarifaDoTipo(a.Tipo);

        if (a.Tipo == AtendimentoTipo.Estacionamento)
        {
            minutos = ae?.TempoMinutos;
            valor = ae?.ValorCalculado;
        }
        else
        {
            minutos = ac?.TempoMinutos;
            valor = ac?.ValorCalculado;
        }

        if (a.StatusAtendimento == AtendimentoStatus.EmExecucao)
        {
            minutos = CobrancaService.CalcularMinutos(a.AbertoEm, agora);
            valor = CobrancaService.CalcularValor(minutos.Value, tarifa);
        }

        return (minutos, valor);
    }
}

public record LoginRequest(string? Login, string? Senha);
public record ClienteCreateRequest(Guid EmpresaId, string Nome, string? Telefone, string? Email, string? CpfCnpj, string? Observacoes);
public record VeiculoCreateRequest(Guid EmpresaId, Guid ClienteId, string Placa, string? Marca, string? Modelo, int? Ano, string? Cor, string? Conector, string? Observacoes);
public record CaixaAbrirRequest(Guid EmpresaId, Guid UnidadeId, Guid OperadorId, decimal ValorInicial);
public record CaixaFecharRequest(decimal ValorInformado, string? Observacoes);
public record AtendimentoCarregamentoCreateRequest(
    Guid EmpresaId, Guid UnidadeId, Guid OperadorId, Guid VeiculoId, Guid EquipamentoId,
    int Ticket, DateTime? HoraInicial, DateTime? HoraFinal, string? Observacoes);
public record AtendimentoEstacionamentoCreateRequest(
    Guid EmpresaId, Guid UnidadeId, Guid OperadorId, Guid VeiculoId, Guid EquipamentoId,
    int Ticket, DateTime? HoraInicial, string? Observacoes);
public record FinalizarRequest(DateTime? HoraFinal);
public record PagarRequest(FormaPagamento Forma, string? MotivoCortesia = null);
