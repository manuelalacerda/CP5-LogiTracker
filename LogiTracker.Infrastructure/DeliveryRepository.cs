using LogiTracker.Application.DTOs;
using LogiTracker.Application.Services;
using LogiTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogiTracker.Infrastructure;

/// <summary>
/// Repositório para operações de persistência e consulta de entregas.
/// </summary>
public sealed class DeliveryRepository(ApplicationDbContext context) : IDeliveryRepository
{
    /// <inheritdoc />
    public IReadOnlyList<DeliveryResponse> GetAll()
    {
        return context.Deliveries
            .OrderBy(d => d.OrderDate)
            .Select(DeliveryResponse.FromDomain)
            .ToList();
    }

    /// <inheritdoc />
    public (IReadOnlyList<DeliveryResponse> Items, int TotalItems) GetPaged(int page, int pageSize)
    {
        var query = context.Deliveries
            .AsNoTracking()
            .OrderBy(d => d.CreatedAt)
            .ThenBy(d => d.Id);

        var total = query.Count();

        var skip = (long)(page - 1) * pageSize;
        if (skip >= total)
            return (Array.Empty<DeliveryResponse>(), total);

        var items = query
            .Skip((int)skip)
            .Take(pageSize)
            .Select(d => new DeliveryResponse(d.Id, d.Status, d.OrderDate, d.VehicleId, d.DriverId, d.CargoId))
            .ToList();

        return (items, total);
    }

    /// <inheritdoc />
    public DeliveryResponse? GetById(Guid id)
    {
        var delivery = context.Deliveries
            .FirstOrDefault(d => d.Id == id);

        return delivery is null ? null : DeliveryResponse.FromDomain(delivery);
    }

    /// <inheritdoc />
    public DeliveryResponse Create(DeliveryRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var delivery = request.ToDomain();

        context.Deliveries.Add(delivery);
        context.SaveChanges();

        return DeliveryResponse.FromDomain(delivery);
    }

    /// <inheritdoc />
    public bool Delete(Guid id)
    {
        var delivery = context.Deliveries.FirstOrDefault(d => d.Id == id);
        if (delivery is null)
            return false;

        context.Deliveries.Remove(delivery);
        context.SaveChanges();

        return true;
    }
}