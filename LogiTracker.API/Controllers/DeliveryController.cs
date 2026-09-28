using Asp.Versioning;
using LogiTracker.Application.DTOs;
using LogiTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LogiTracker.API.Controllers;

/// 
/// Controller responsável pelas operações de entregas na API.
///
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IDeliveryService _deliveryService;
    private readonly ILogger _logger;

    public DeliveryController(
        IDeliveryRepository deliveryRepository,
        IDeliveryService deliveryService,
        ILogger<DeliveryController> logger)
    {
        _deliveryRepository = deliveryRepository;
        _deliveryService = deliveryService;
        _logger = logger;
    }
    
    /// 
    /// [DEPRECADA] Lista todas as entregas (array, sem paginação). Use a v2.0.
    /// 
    /// Retorna todas as entregas cadastradas.
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IReadOnlyList<DeliveryResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        return Ok(_deliveryService.GetAll());
    }
    
    /// 
    /// Lista as entregas de forma paginada (v2.0).
    /// 
    /// Número da página, inteiro maior ou igual a 1 (padrão 1).
    /// Itens por página, de 1 a 100 (padrão 20).
    /// Retorna o envelope paginado. Página além do total devolve items vazio.
    /// page ou pageSize fora da faixa permitida.
    [HttpGet("paged")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(PagedResponse<DeliveryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationRules.DefaultPageSize)
    {
        return Ok(_deliveryService.GetPaged(page, pageSize));
    }
    
    /// 
    /// Busca uma entrega pelo identificador único.
    /// 
    /// Identificador único da entrega.
    /// Retorna a entrega encontrada.
    /// Entrega não encontrada.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var delivery = _deliveryRepository.GetById(id);

        if (delivery is null)
            throw new KeyNotFoundException("Entrega não encontrada.");

        return Ok(delivery);
    }
    
    /// 
    /// Cria uma nova entrega. Limitado a 10 requisições por minuto por IP.
    /// 
    /// Dados da entrega.
    /// Entrega criada com sucesso.
    /// Dados inválidos.
    /// Recurso não encontrado.
    /// Limite de requisições excedido (veja o header Retry-After).
    [HttpPost]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create([FromBody] DeliveryRequest request)
    {
        var traceId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "Iniciando criação de entrega. VehicleId: {VehicleId}, DriverId: {DriverId}, CargoId: {CargoId}, TraceId: {TraceId}",
            request.VehicleId,
            request.DriverId,
            request.CargoId,
            traceId);

        var delivery = await _deliveryService.CreateAsync(request);

        _logger.LogInformation(
            "Entrega criada com sucesso. DeliveryId: {DeliveryId}, TraceId: {TraceId}",
            delivery.Id,
            traceId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = delivery.Id },
            delivery
        );
    }
    
    /// 
    /// Remove uma entrega pelo identificador único.
    /// 
    /// Identificador único da entrega.
    /// Entrega removida com sucesso.
    /// Entrega não encontrada.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var deleted = _deliveryRepository.Delete(id);

        if (!deleted)
            throw new KeyNotFoundException("Entrega não encontrada.");

        return NoContent();
    }
}