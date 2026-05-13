namespace rest_service.Models;

public class SensorReading
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; set; } = "";
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public decimal? Pressure { get; set; }
    public int? Light { get; set; }
    public int? Sound { get; set; }
    public short? Motion { get; set; }
    public decimal? Battery { get; set; }
    public string? Location { get; set; }
}

public record SensorReadingInput(
    string DeviceId,
    decimal? Temperature,
    decimal? Humidity,
    decimal? Pressure,
    int? Light,
    int? Sound,
    short? Motion,
    decimal? Battery,
    string? Location
);

public record SensorReadingSelectiveDto(
    DateTime Timestamp,
    string DeviceId,
    decimal? Temperature,
    decimal? Humidity
);
