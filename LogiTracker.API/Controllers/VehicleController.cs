using Asp.Versioning;
using LogiTracker.Application.DTOs;
using LogiTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogiTracker.API.Controllers;

/// <summary>
/// Controller responsável pelas operações de veículos na API.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiVersionNeutral]
public class VehicleController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleController(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    /// <summary>
    /// Lista todos os veículos cadastrados.
    /// </summary>
    /// <response code="200">
    /// Retorna todos os veículos cadastrados.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var vehicles = _vehicleRepository.GetAll();

        return Ok(vehicles);
    }

    /// <summary>
    /// Busca um veículo pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do veículo.</param>
    /// <response code="200">
    /// Retorna o veículo encontrado.
    /// </response>
    /// <response code="404">
    /// Veículo não encontrado.
    /// </response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var vehicle = _vehicleRepository.GetById(id);

        if (vehicle is null)
            throw new KeyNotFoundException("Veículo não encontrado.");

        return Ok(vehicle);
    }

    /// <summary>
    /// Cria um novo veículo.
    /// </summary>
    /// <param name="request">Dados do veículo.</param>
    /// <response code="201">
    /// Veículo criado com sucesso.
    /// </response>
    /// <response code="400">
    /// Dados inválidos.
    /// </response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] VehicleRequest request)
    {
        var vehicle = _vehicleRepository.Create(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = vehicle.Id },
            vehicle
        );
    }

    /// <summary>
    /// Remove um veículo pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do veículo.</param>
    /// <response code="204">
    /// Veículo removido com sucesso.
    /// </response>
    /// <response code="404">
    /// Veículo não encontrado.
    /// </response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var deleted = _vehicleRepository.Delete(id);

        if (!deleted)
            throw new KeyNotFoundException("Veículo não encontrado.");

        return NoContent();
    }
}