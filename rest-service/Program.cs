using Npgsql;
using rest_service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "IoT Benchmark - REST API",
        Version = "v1",
        Description = "REST servis za komparativnu analizu IoT komunikacionih protokola"
    });
});

// PostgreSQL konekcija
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=iot_benchmark;Username=iot_user;Password=iot_password";

builder.Services.AddScoped<IDbConnectionFactory>(_ =>
    new NpgsqlConnectionFactory(connectionString));

builder.Services.AddScoped<SensorReadingService>();

var app = builder.Build();

// Swagger uvek dostupan
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IoT REST API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
