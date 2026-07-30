// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Cliente da sessão/atendimento sempre herdado do veículo (paridade ChargeDesk).

namespace ChargeDesk.Operacao.Application;

public static class AtendimentoClienteResolver
{
    /// <summary>
    /// Impede divergência: o ClienteId informado na request nunca prevalece sobre o dono do veículo.
    /// </summary>
    public static Guid ResolverClienteId(Guid veiculoClienteId) => veiculoClienteId;
}
