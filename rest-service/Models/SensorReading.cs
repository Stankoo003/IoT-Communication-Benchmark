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

public class SensorReadingInput
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
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
