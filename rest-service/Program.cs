using Dapper;
using rest_service.Services;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IoT REST Service", Version = "v1" });
});

var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(connStr));
builder.Services.AddScoped<SensorReadingService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
