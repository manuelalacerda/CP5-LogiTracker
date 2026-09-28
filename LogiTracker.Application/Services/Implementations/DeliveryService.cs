using LogiTracker.Application.DTOs;
using LogiTracker.Domain.Entities;

namespace LogiTracker.Application.Services.Implementations;

/// <inheritdoc cref="IDeliveryService" />
public sealed class DeliveryService(
    IRepository<Vehicle> vehicleRepository,
    IRepository<Driver> driverRepository,
    IRepository<Cargo> cargoRepository,
    IDeliveryRepository deliveryRepository) : IDeliveryService
{
    /// <inheritdoc />
    public async Task<DeliveryResponse> CreateAsync(DeliveryRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (!await vehicleRepository.ExistsByIdAsync(request.VehicleId))
            throw new KeyNotFoundException("Veículo não encontrado.");

        if (!await driverRepository.ExistsByIdAsync(request.DriverId))
            throw new KeyNotFoundException("Motorista não encontrado.");

        if (!await cargoRepository.ExistsByIdAsync(request.CargoId))
            throw new KeyNotFoundException("Carga não encontrada.");

        // Só chega aqui (e só persiste) se todas as dependências existirem.
        return deliveryRepository.Create(request);
    }
    
    /// <inheritdoc />
    public IReadOnlyList<DeliveryResponse> GetAll() => deliveryRepository.GetAll();

    /// <inheritdoc />
    public PagedResponse<DeliveryResponse> GetPaged(int page, int pageSize)
    {
        PaginationRules.Validate(page, pageSize);

        var (items, total) = deliveryRepository.GetPaged(page, pageSize);

        return PagedResponse<DeliveryResponse>.Create(page, pageSize, total, items);
    }
}
