using Dapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Npgsql;

namespace grpc_service.Services;

public class SensorService(IConfiguration config) : grpc_service.SensorService.SensorServiceBase
{
    private NpgsqlConnection Connect() => new(config.GetConnectionString("DefaultConnection"));

    // ── Scenario A: High-Frequency Ingestion ─────────────────────────────────

    public override async Task<IngestResponse> IngestReading(IngestRequest request, ServerCallContext context)
    {
        const string sql = """
            INSERT INTO sensor_readings
                (timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location)
            VALUES
                (@Timestamp, @DeviceId, @Temperature, @Humidity, @Pressure, @Light, @Sound, @Motion, @Battery, @Location)
            RETURNING id
            """;

        await using var db = Connect();
        var id = await db.ExecuteScalarAsync<long>(sql, new
        {
            Timestamp   = request.Timestamp?.ToDateTime() ?? DateTime.UtcNow,
            DeviceId    = request.DeviceId,
            Temperature = request.Temperature,
            Humidity    = request.Humidity,
            Pressure    = request.Pressure,
            Light       = request.Light,
            Sound       = request.Sound,
            Motion      = request.Motion,
            Battery     = request.Battery,
            Location    = request.Location
        });

        return new IngestResponse { Id = id };
    }

    // ── Scenario B: Selective Monitoring ─────────────────────────────────────

    public override async Task<GetReadingsResponse> GetReadings(GetReadingsRequest request, ServerCallContext context)
    {
        const string sql = """
            SELECT id, timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location
            FROM sensor_readings
            WHERE (@DeviceId = '' OR device_id = @DeviceId)
              AND (@From = '' OR timestamp >= @From::timestamptz)
              AND (@To   = '' OR timestamp <= @To::timestamptz)
            ORDER BY timestamp DESC
            LIMIT @Limit
            """;

        await using var db = Connect();
        var rows = await db.QueryAsync(sql, new
        {
            DeviceId = request.DeviceId,
            From     = request.From,
            To       = request.To,
            Limit    = request.Limit > 0 ? request.Limit : 100
        });

        var response = new GetReadingsResponse();
        foreach (var r in rows)
            response.Readings.Add(MapReading(r));

        return response;
    }

    // ── Scenario C: Heavy Querying ────────────────────────────────────────────

    public override async Task<AggregateResponse> GetAggregates(AggregateRequest request, ServerCallContext context)
    {
        const string sql = """
            SELECT hour, device_id, location,
                   avg_temperature, avg_humidity, avg_pressure,
                   avg_light, avg_sound, total_motion_events,
                   min_battery, reading_count
            FROM hourly_aggregates
            WHERE (@DeviceId = '' OR device_id = @DeviceId)
              AND (@From = '' OR hour >= @From::timestamptz)
              AND (@To   = '' OR hour <= @To::timestamptz)
            ORDER BY hour DESC
            LIMIT 1000
            """;

        await using var db = Connect();
        var rows = await db.QueryAsync(sql, new
        {
            DeviceId = request.DeviceId,
            From     = request.From,
            To       = request.To
        });

        var response = new AggregateResponse();
        foreach (var r in rows)
        {
            response.Aggregates.Add(new HourlyAggregate
            {
                Hour              = ((DateTime)r.hour).ToString("o"),
                DeviceId          = r.device_id  ?? "",
                Location          = r.location   ?? "",
                AvgTemperature    = (double)(r.avg_temperature   ?? 0),
                AvgHumidity       = (double)(r.avg_humidity      ?? 0),
                AvgPressure       = (double)(r.avg_pressure      ?? 0),
                AvgLight          = (double)(r.avg_light         ?? 0),
                AvgSound          = (double)(r.avg_sound         ?? 0),
                TotalMotionEvents = (long)(r.total_motion_events ?? 0),
                MinBattery        = (double)(r.min_battery       ?? 0),
                ReadingCount      = (long)(r.reading_count       ?? 0)
            });
        }

        return response;
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static SensorReading MapReading(dynamic r) => new()
    {
        Id          = (long)r.id,
        Timestamp   = Timestamp.FromDateTime(DateTime.SpecifyKind((DateTime)r.timestamp, DateTimeKind.Utc)),
        DeviceId    = r.device_id  ?? "",
        Temperature = r.temperature is null ? null : (double?)r.temperature,
        Humidity    = r.humidity    is null ? null : (double?)r.humidity,
        Pressure    = r.pressure    is null ? null : (double?)r.pressure,
        Light       = r.light       is null ? null : (int?)r.light,
        Sound       = r.sound       is null ? null : (int?)r.sound,
        Motion      = (int)(r.motion ?? 0),
        Battery     = r.battery     is null ? null : (double?)r.battery,
        Location    = r.location    ?? ""
    };
}
