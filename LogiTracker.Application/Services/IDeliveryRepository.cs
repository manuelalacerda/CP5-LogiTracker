using LogiTracker.Application.DTOs;

namespace LogiTracker.Application.Services;

/// <summary>
/// Define as operações de repositório para o domínio de entregas.
/// </summary>
public interface IDeliveryRepository
{
    /// <summary>
    /// Retorna todas as entregas cadastradas.
    /// </summary>
    IReadOnlyList<DeliveryResponse> GetAll();

    /// <summary>
    /// Busca uma entrega pelo identificador único.
    /// </summary>
    DeliveryResponse? GetById(Guid id);

    /// <summary>
    /// Cria uma nova entrega a partir dos dados informados.
    /// </summary>
    DeliveryResponse Create(DeliveryRequest request);

    /// <summary>
    /// Remove uma entrega pelo identificador único.
    /// </summary>
    bool Delete(Guid id);
    
    /// <summary>
    /// Retorna uma página de entregas (cortada no banco) e o total de registros.
    /// </summary>
    (IReadOnlyList<DeliveryResponse> Items, int TotalItems) GetPaged(int page, int pageSize);
    
}
