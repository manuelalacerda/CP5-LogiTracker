using Asp.Versioning;
using LogiTracker.Application.DTOs;
using LogiTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogiTracker.API.Controllers;

/// <summary>
/// Controller responsável pelas operações de transportadoras na API.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiVersionNeutral]
public class CarrierController : ControllerBase
{
    private readonly ICarrierRepository _carrierRepository;

    public CarrierController(ICarrierRepository carrierRepository)
    {
        _carrierRepository = carrierRepository;
    }

    /// <summary>
    /// Lista todas as transportadoras cadastradas.
    /// </summary>
    /// <response code="200">
    /// Retorna todas as transportadoras cadastradas.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var carriers = _carrierRepository.GetAll();

        return Ok(carriers);
    }

    /// <summary>
    /// Busca uma transportadora pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único da transportadora.</param>
    /// <response code="200">
    /// Retorna a transportadora encontrada.
    /// </response>
    /// <response code="404">
    /// Transportadora não encontrada.
    /// </response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var carrier = _carrierRepository.GetById(id);

        if (carrier is null)
            throw new KeyNotFoundException("Transportadora não encontrada.");

        return Ok(carrier);
    }

    /// <summary>
    /// Cria uma nova transportadora.
    /// </summary>
    /// <param name="request">Dados da transportadora.</param>
    /// <response code="201">
    /// Transportadora criada com sucesso.
    /// </response>
    /// <response code="400">
    /// Dados inválidos.
    /// </response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CarrierRequest request)
    {
        var carrier = _carrierRepository.Create(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = carrier.Id },
            carrier
        );
    }

    /// <summary>
    /// Remove uma transportadora pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único da transportadora.</param>
    /// <response code="204">
    /// Transportadora removida com sucesso.
    /// </response>
    /// <response code="404">
    /// Transportadora não encontrada.
    /// </response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var deleted = _carrierRepository.Delete(id);

        if (!deleted)
            throw new KeyNotFoundException("Transportadora não encontrada.");

        return NoContent();
    }
}