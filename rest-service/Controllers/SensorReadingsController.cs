using Microsoft.AspNetCore.Mvc;
using rest_service.Models;
using rest_service.Services;

namespace rest_service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SensorReadingsController : ControllerBase
{
    private readonly SensorReadingService _service;
    private readonly ILogger<SensorReadingsController> _logger;

    public SensorReadingsController(SensorReadingService service,
        ILogger<SensorReadingsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Scenario A - Dohvata listu senzorskih ocitavanja (paginacija)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SensorReading>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var readings = await _service.GetAllAsync(page, pageSize);
        return Ok(readings);
    }

    /// <summary>
    /// Scenario A - Upisuje novo senzorsko ocitavanje (High-Frequency Ingestion)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    public async Task<IActionResult> Insert([FromBody] SensorReading reading)
    {
        reading.Timestamp = DateTime.UtcNow;
        var id = await _service.InsertAsync(reading);
        return CreatedAtAction(nameof(GetAll), new { id }, new { id, message = "Reading saved" });
    }

    /// <summary>
    /// Scenario B - Selective Monitoring: vraca samo temperature i humidity za uredjaj
    /// </summary>
    [HttpGet("selective/{deviceId}")]
    [ProducesResponseType(typeof(IEnumerable<SensorReadingSelectiveDto>), 200)]
    public async Task<IActionResult> GetSelective(string deviceId)
    {
        var readings = await _service.GetSelectiveAsync(deviceId);
        return Ok(readings);
    }

    /// <summary>
    /// Scenario C - Heavy Querying: agregacije po satu za vremenski opseg
    /// </summary>
    [HttpGet("aggregates")]
    [ProducesResponseType(typeof(IEnumerable<HourlyAggregateDto>), 200)]
    public async Task<IActionResult> GetAggregates(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? deviceId = null)
    {
        var aggregates = await _service.GetHourlyAggregatesAsync(from, to, deviceId);
        return Ok(aggregates);
    }

    /// <summary>
    /// Vraca poslednje ocitavanje za svaki uredjaj
    /// </summary>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(IEnumerable<SensorReading>), 200)]
    public async Task<IActionResult> GetLatest()
    {
        var readings = await _service.GetLatestReadingsAsync();
        return Ok(readings);
    }
}
