using Microsoft.AspNetCore.Mvc;
using rest_service.Models;
using rest_service.Services;

namespace rest_service.Controllers;

[ApiController]
[Route("api/readings")]
[Produces("application/json")]
public class SensorReadingsController : ControllerBase
{
    private readonly SensorReadingService _service;

    public SensorReadingsController(SensorReadingService service)
    {
        _service = service;
    }

    // Scenario A – lista ocitavanja sa paginacijom
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SensorReading>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var readings = await _service.GetAllAsync(page, pageSize);
        return Ok(readings);
    }

    // Scenario A – upis novog ocitavanja (High-Frequency Ingestion)
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    public async Task<IActionResult> Insert([FromBody] SensorReadingInput input)
    {
        var id = await _service.InsertAsync(input);
        return CreatedAtAction(nameof(GetAll), new { id }, new { id });
    }

    // Scenario B – Selective Monitoring: samo temperature i humidity za uredjaj
    [HttpGet("selective/{deviceId}")]
    [ProducesResponseType(typeof(IEnumerable<SensorReadingSelectiveDto>), 200)]
    public async Task<IActionResult> GetSelective(string deviceId)
    {
        var readings = await _service.GetSelectiveAsync(deviceId);
        return Ok(readings);
    }

    // Poslednje ocitavanje po uredjaju
    [HttpGet("latest")]
    [ProducesResponseType(typeof(IEnumerable<SensorReading>), 200)]
    public async Task<IActionResult> GetLatest()
    {
        var readings = await _service.GetLatestReadingsAsync();
        return Ok(readings);
    }

    // Scenario C – Heavy Querying: agregacije po satu za vremenski opseg
    [HttpGet("aggregates")]
    [ProducesResponseType(typeof(IEnumerable<HourlyAggregate>), 200)]
    public async Task<IActionResult> GetAggregates(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? deviceId = null)
    {
        var aggregates = await _service.GetHourlyAggregatesAsync(from, to, deviceId);
        return Ok(aggregates);
    }
}
