using Dapper;
using rest_service.Models;

namespace rest_service.Services;

public class SensorReadingService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SensorReadingService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Scenario A – svi podaci sa paginacijom
    public async Task<IEnumerable<SensorReading>> GetAllAsync(int page = 1, int pageSize = 100)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<SensorReading>(
            @"SELECT id, timestamp, device_id, temperature, humidity,
                     pressure, light, sound, motion, battery, location
              FROM sensor_readings
              ORDER BY timestamp DESC
              LIMIT @PageSize OFFSET @Offset",
            new { PageSize = pageSize, Offset = (page - 1) * pageSize });
    }

    // Scenario A – upis novog merenja
    public async Task<long> InsertAsync(SensorReadingInput input)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            @"INSERT INTO sensor_readings
                (timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location)
              VALUES
                (NOW(), @DeviceId, @Temperature, @Humidity, @Pressure, @Light, @Sound, @Motion, @Battery, @Location)
              RETURNING id",
            input);
    }

    // Scenario B – selective monitoring (samo temperature i humidity)
    public async Task<IEnumerable<SensorReadingSelectiveDto>> GetSelectiveAsync(string deviceId)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<SensorReadingSelectiveDto>(
            @"SELECT timestamp, device_id, temperature, humidity
              FROM sensor_readings
              WHERE device_id = @DeviceId
              ORDER BY timestamp DESC
              LIMIT 100",
            new { DeviceId = deviceId });
    }

    // Scenario C – heavy querying (agregacije po satu)
    public async Task<IEnumerable<HourlyAggregate>> GetHourlyAggregatesAsync(
        DateTime from, DateTime to, string? deviceId = null)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<HourlyAggregate>(
            @"SELECT hour, device_id, location, avg_temperature, avg_humidity, avg_pressure,
                     avg_light, avg_sound, total_motion_events, min_battery, reading_count
              FROM hourly_aggregates
              WHERE hour BETWEEN @From AND @To
                AND (@DeviceId IS NULL OR device_id = @DeviceId)
              ORDER BY hour DESC",
            new { From = from, To = to, DeviceId = deviceId });
    }

    // Poslednje ocitavanje po uredjaju
    public async Task<IEnumerable<SensorReading>> GetLatestReadingsAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<SensorReading>(
            @"SELECT id, timestamp, device_id, temperature, humidity,
                     pressure, light, sound, motion, battery, location
              FROM latest_readings
              ORDER BY device_id");
    }
}
