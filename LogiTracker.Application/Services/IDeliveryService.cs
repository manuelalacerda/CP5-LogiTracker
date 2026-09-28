using LogiTracker.Application.DTOs;

namespace LogiTracker.Application.Services;

/// <summary>
/// Orquestra o caso de uso de criação de entregas, validando as dependências
/// (veículo, motorista e carga) antes de delegar a persistência ao
/// <see cref="IDeliveryRepository"/>.
/// </summary>
public interface IDeliveryService
{
    /// <summary>
    /// Cria uma entrega após validar que o veículo, o motorista e a carga informados existem.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// Lançada quando o veículo, o motorista ou a carga informados não existem.
    /// Nesse caso, nenhuma entrega é persistida.
    /// </exception>
    Task<DeliveryResponse> CreateAsync(DeliveryRequest request);
    
    /// <summary>Lista todas as entregas (contrato v1, sem paginação).</summary>
    IReadOnlyList<DeliveryResponse> GetAll();

    /// <summary>Lista uma página de entregas (contrato v2).</summary>
    /// <exception cref="ArgumentException">Quando page &lt; 1 ou pageSize fora de 1–100.</exception>
    PagedResponse<DeliveryResponse> GetPaged(int page, int pageSize);
}
