namespace LogiTracker.Application.Tests.Services;

using LogiTracker.Application.DTOs;
using LogiTracker.Application.Services;
using LogiTracker.Application.Services.Implementations;
using LogiTracker.Domain.Entities;
using Moq;

public class PaginationTests
{
    private readonly Mock<IDeliveryRepository> _repo = new();
    private readonly DeliveryService _service;

    public PaginationTests()
    {
        _service = new DeliveryService(
            new Mock<IRepository<Vehicle>>().Object,
            new Mock<IRepository<Driver>>().Object,
            new Mock<IRepository<Cargo>>().Object,
            _repo.Object);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(1, 9999)]
    public void GetPaged_ComParametrosInvalidos_DeveLancarArgumentExceptionENaoConsultar(int page, int pageSize)
    {
        Assert.Throws<ArgumentException>(() => _service.GetPaged(page, pageSize));

        _repo.Verify(r => r.GetPaged(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetPaged_ComIntervaloValido_DeveMontarEnvelopeComTotais()
    {
        IReadOnlyList<DeliveryResponse> itens = new List<DeliveryResponse>();
        _repo.Setup(r => r.GetPaged(2, 2)).Returns((itens, 5));

        var result = _service.GetPaged(2, 2);

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPrevious);
        Assert.True(result.HasNext);
    }
}