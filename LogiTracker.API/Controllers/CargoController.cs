using Asp.Versioning;
using LogiTracker.Application.DTOs;
using LogiTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogiTracker.API.Controllers;

/// <summary>
/// Controller responsável pelas operações de cargas na API.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiVersionNeutral]
public class CargoController : ControllerBase
{
    private readonly ICargoRepository _cargoRepository;

    public CargoController(ICargoRepository cargoRepository)
    {
        _cargoRepository = cargoRepository;
    }

    /// <summary>
    /// Lista todas as cargas cadastradas.
    /// </summary>
    /// <response code="200">
    /// Retorna todas as cargas cadastradas.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var cargos = _cargoRepository.GetAll();

        return Ok(cargos);
    }

    /// <summary>
    /// Busca uma carga pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único da carga.</param>
    /// <response code="200">
    /// Retorna a carga encontrada.
    /// </response>
    /// <response code="404">
    /// Carga não encontrada.
    /// </response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var cargo = _cargoRepository.GetById(id);

        if (cargo is null)
            throw new KeyNotFoundException("Carga não encontrada.");

        return Ok(cargo);
    }

    /// <summary>
    /// Cria uma nova carga.
    /// </summary>
    /// <param name="request">Dados da carga.</param>
    /// <response code="201">
    /// Carga criada com sucesso.
    /// </response>
    /// <response code="400">
    /// Dados inválidos.
    /// </response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CargoRequest request)
    {
        var cargo = _cargoRepository.Create(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = cargo.Id },
            cargo
        );
    }

    /// <summary>
    /// Remove uma carga pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único da carga.</param>
    /// <response code="204">
    /// Carga removida com sucesso.
    /// </response>
    /// <response code="404">
    /// Carga não encontrada.
    /// </response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var deleted = _cargoRepository.Delete(id);

        if (!deleted)
            throw new KeyNotFoundException("Carga não encontrada.");

        return NoContent();
    }
}