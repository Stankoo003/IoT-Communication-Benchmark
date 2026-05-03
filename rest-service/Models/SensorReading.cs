namespace rest_service.Models;

public class SensorReading
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public decimal? Temperature { get; set; }  // °C
    public decimal? Humidity { get; set; }      // %
    public decimal? Pressure { get; set; }      // hPa
    public int? Light { get; set; }             // lux
    public int? Sound { get; set; }             // dB
    public short? Motion { get; set; }          // 0 or 1
    public decimal? Battery { get; set; }       // %
    public string? Location { get; set; }
}

// DTO za Scenario B - Selective Monitoring (samo 2 polja)
public class SensorReadingSelectiveDto
{
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
}

// DTO za Scenario C - Hourly aggregates
public class HourlyAggregateDto
{
    public DateTime Hour { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? AvgTemperature { get; set; }
    public decimal? AvgHumidity { get; set; }
    public decimal? AvgPressure { get; set; }
    public decimal? AvgLight { get; set; }
    public decimal? AvgSound { get; set; }
    public long? TotalMotionEvents { get; set; }
    public decimal? MinBattery { get; set; }
    public long ReadingCount { get; set; }
}
