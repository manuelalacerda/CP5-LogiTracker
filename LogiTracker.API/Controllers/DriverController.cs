using Asp.Versioning;
using LogiTracker.Application.DTOs;
using LogiTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogiTracker.API.Controllers;

/// <summary>
/// Controller responsável pelas operações de motoristas na API.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiVersionNeutral]
public class DriverController : ControllerBase
{
    private readonly IDriverRepository _driverRepository;

    public DriverController(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    /// <summary>
    /// Lista todos os motoristas cadastrados.
    /// </summary>
    /// <response code="200">
    /// Retorna todos os motoristas cadastrados.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var drivers = _driverRepository.GetAll();

        return Ok(drivers);
    }

    /// <summary>
    /// Busca um motorista pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do motorista.</param>
    /// <response code="200">
    /// Retorna o motorista encontrado.
    /// </response>
    /// <response code="404">
    /// Motorista não encontrado.
    /// </response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var driver = _driverRepository.GetById(id);

        if (driver is null)
            throw new KeyNotFoundException("Motorista não encontrado.");

        return Ok(driver);
    }

    /// <summary>
    /// Cria um novo motorista.
    /// </summary>
    /// <param name="request">Dados do motorista.</param>
    /// <response code="201">
    /// Motorista criado com sucesso.
    /// </response>
    /// <response code="400">
    /// Dados inválidos.
    /// </response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] DriverRequest request)
    {
        var driver = _driverRepository.Create(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = driver.Id },
            driver
        );
    }

    /// <summary>
    /// Remove um motorista pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do motorista.</param>
    /// <response code="204">
    /// Motorista removido com sucesso.
    /// </response>
    /// <response code="404">
    /// Motorista não encontrado.
    /// </response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var deleted = _driverRepository.Delete(id);

        if (!deleted)
            throw new KeyNotFoundException("Motorista não encontrado.");

        return NoContent();
    }
}