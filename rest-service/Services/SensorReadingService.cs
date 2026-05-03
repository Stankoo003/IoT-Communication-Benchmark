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

    // Scenario A - svi podaci (paginacija)
    public async Task<IEnumerable<SensorReading>> GetAllAsync(int page = 1, int pageSize = 100)
    {
        using var conn = _connectionFactory.CreateConnection();
        var offset = (page - 1) * pageSize;
        return await conn.QueryAsync<SensorReading>(
            @"SELECT id, timestamp, device_id AS DeviceId, temperature, humidity,
                     pressure, light, sound, motion, battery, location
              FROM sensor_readings
              ORDER BY timestamp DESC
              LIMIT @PageSize OFFSET @Offset",
            new { PageSize = pageSize, Offset = offset });
    }

    // Scenario A - upis novog merenja
    public async Task<long> InsertAsync(SensorReading reading)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            @"INSERT INTO sensor_readings
                (timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location)
              VALUES
                (@Timestamp, @DeviceId, @Temperature, @Humidity, @Pressure, @Light, @Sound, @Motion, @Battery, @Location)
              RETURNING id",
            reading);
    }

    // Scenario B - selective monitoring (samo temperature i humidity)
    public async Task<IEnumerable<SensorReadingSelectiveDto>> GetSelectiveAsync(string deviceId)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<SensorReadingSelectiveDto>(
            @"SELECT timestamp, device_id AS DeviceId, temperature, humidity
              FROM sensor_readings
              WHERE device_id = @DeviceId
              ORDER BY timestamp DESC
              LIMIT 100",
            new { DeviceId = deviceId });
    }

    // Scenario C - heavy querying (agregacije po satu)
    public async Task<IEnumerable<HourlyAggregateDto>> GetHourlyAggregatesAsync(
        DateTime from, DateTime to, string? deviceId = null)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = deviceId != null
            ? @"SELECT hour, device_id AS DeviceId, location, avg_temperature AS AvgTemperature,
                       avg_humidity AS AvgHumidity, avg_pressure AS AvgPressure,
                       avg_light AS AvgLight, avg_sound AS AvgSound,
                       total_motion_events AS TotalMotionEvents,
                       min_battery AS MinBattery, reading_count AS ReadingCount
                FROM hourly_aggregates
                WHERE hour BETWEEN @From AND @To AND device_id = @DeviceId
                ORDER BY hour DESC"
            : @"SELECT hour, device_id AS DeviceId, location, avg_temperature AS AvgTemperature,
                       avg_humidity AS AvgHumidity, avg_pressure AS AvgPressure,
                       avg_light AS AvgLight, avg_sound AS AvgSound,
                       total_motion_events AS TotalMotionEvents,
                       min_battery AS MinBattery, reading_count AS ReadingCount
                FROM hourly_aggregates
                WHERE hour BETWEEN @From AND @To
                ORDER BY hour DESC";

        return await conn.QueryAsync<HourlyAggregateDto>(sql,
            new { From = from, To = to, DeviceId = deviceId });
    }

    // Poslednje ocitavanje po uredjaju
    public async Task<IEnumerable<SensorReading>> GetLatestReadingsAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<SensorReading>(
            @"SELECT id, timestamp, device_id AS DeviceId, temperature, humidity,
                     pressure, light, sound, motion, battery, location
              FROM latest_readings
              ORDER BY device_id");
    }
}
