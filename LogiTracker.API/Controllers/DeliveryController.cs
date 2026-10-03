using Asp.Versioning;
using LogiTracker.Application.DTOs;
using LogiTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LogiTracker.API.Controllers;

/// <summary>
/// Controller responsável pelas operações de entregas na API.
/// </summary>
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IDeliveryService _deliveryService;
    private readonly ILogger<DeliveryController> _logger;

    public DeliveryController(
        IDeliveryRepository deliveryRepository,
        IDeliveryService deliveryService,
        ILogger<DeliveryController> logger)
    {
        _deliveryRepository = deliveryRepository;
        _deliveryService = deliveryService;
        _logger = logger;
    }

    /// <summary>
    /// [DEPRECADA] Lista todas as entregas (array, sem paginação). Use a v2.0.
    /// </summary>
    /// <response code="200">Retorna todas as entregas cadastradas.</response>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IReadOnlyList<DeliveryResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        return Ok(_deliveryService.GetAll());
    }

    /// <summary>
    /// Lista as entregas de forma paginada (v2.0).
    /// </summary>
    /// <param name="page">Número da página, inteiro maior ou igual a 1 (padrão 1).</param>
    /// <param name="pageSize">Itens por página, de 1 a 100 (padrão 20).</param>
    /// <response code="200">Retorna o envelope paginado. Página além do total devolve items vazio.</response>
    /// <response code="400">page ou pageSize fora da faixa permitida.</response>
    [HttpGet]                                   // <-- faltava
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(PagedResponse<DeliveryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationRules.DefaultPageSize)
    {
        return Ok(_deliveryService.GetPaged(page, pageSize));
    }

    /// <summary>
    /// Busca uma entrega pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único da entrega.</param>
    /// <response code="200">Retorna a entrega encontrada.</response>
    /// <response code="404">Entrega não encontrada.</response>
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

    /// <summary>
    /// Cria uma nova entrega. Limitado a 10 requisições por minuto por IP.
    /// </summary>
    /// <param name="request">Dados da entrega.</param>
    /// <response code="201">Entrega criada com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Recurso não encontrado.</response>
    /// <response code="429">Limite de requisições excedido (veja o header Retry-After).</response>
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

    /// <summary>
    /// Remove uma entrega pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único da entrega.</param>
    /// <response code="204">Entrega removida com sucesso.</response>
    /// <response code="404">Entrega não encontrada.</response>
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