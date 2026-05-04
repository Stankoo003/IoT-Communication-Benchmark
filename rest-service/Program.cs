using Dapper;
using Npgsql;
using rest_service.Models;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IoT REST Service", Version = "v1" });
});

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connStr));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// ── Scenario A: High-Frequency Ingestion ─────────────────────────────────────

app.MapPost("/api/readings", async (SensorReadingInput input, NpgsqlConnection db) =>
{
    const string sql = """
        INSERT INTO sensor_readings
            (timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location)
        VALUES
            (@Timestamp, @DeviceId, @Temperature, @Humidity, @Pressure, @Light, @Sound, @Motion, @Battery, @Location)
        RETURNING id
        """;

    var id = await db.ExecuteScalarAsync<long>(sql, input);
    return Results.Created($"/api/readings/{id}", new { id });
})
.WithName("IngestReading")
.WithTags("Scenario A – Ingestion");

// ── Scenario B: Selective Monitoring ─────────────────────────────────────────

app.MapGet("/api/readings", async (
    NpgsqlConnection db,
    string? deviceId,
    DateTime? from,
    DateTime? to,
    int limit = 100) =>
{
    const string sql = """
        SELECT id, timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location
        FROM sensor_readings
        WHERE (@DeviceId IS NULL OR device_id = @DeviceId)
          AND (@From::timestamptz IS NULL OR timestamp >= @From::timestamptz)
          AND (@To::timestamptz   IS NULL OR timestamp <= @To::timestamptz)
        ORDER BY timestamp DESC
        LIMIT @Limit
        """;

    var rows = await db.QueryAsync<SensorReading>(sql, new { DeviceId = deviceId, From = from, To = to, Limit = limit });
    return Results.Ok(rows);
})
.WithName("GetReadings")
.WithTags("Scenario B – Selective Monitoring");

app.MapGet("/api/readings/{deviceId}/latest", async (string deviceId, NpgsqlConnection db) =>
{
    const string sql = """
        SELECT id, timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location
        FROM latest_readings
        WHERE device_id = @DeviceId
        """;

    var row = await db.QueryFirstOrDefaultAsync<SensorReading>(sql, new { DeviceId = deviceId });
    return row is null ? Results.NotFound() : Results.Ok(row);
})
.WithName("GetLatestReading")
.WithTags("Scenario B – Selective Monitoring");

app.MapGet("/api/devices", async (NpgsqlConnection db) =>
{
    var rows = await db.QueryAsync("SELECT device_id, location, created_at FROM devices ORDER BY device_id");
    return Results.Ok(rows);
})
.WithName("GetDevices")
.WithTags("Devices");

// ── Scenario C: Heavy Querying ────────────────────────────────────────────────

app.MapGet("/api/aggregates", async (
    NpgsqlConnection db,
    string? deviceId,
    DateTime? from,
    DateTime? to) =>
{
    const string sql = """
        SELECT hour, device_id, location,
               avg_temperature, avg_humidity, avg_pressure,
               avg_light, avg_sound, total_motion_events,
               min_battery, reading_count
        FROM hourly_aggregates
        WHERE (@DeviceId IS NULL OR device_id = @DeviceId)
          AND (@From::timestamptz IS NULL OR hour >= @From::timestamptz)
          AND (@To::timestamptz   IS NULL OR hour <= @To::timestamptz)
        ORDER BY hour DESC
        LIMIT 1000
        """;

    var rows = await db.QueryAsync<HourlyAggregate>(sql, new { DeviceId = deviceId, From = from, To = to });
    return Results.Ok(rows);
})
.WithName("GetAggregates")
.WithTags("Scenario C – Heavy Querying");

app.Run();
