namespace rest_service.Models;

public class HourlyAggregate
{
    public DateTime Hour { get; set; }
    public string DeviceId { get; set; } = "";
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
