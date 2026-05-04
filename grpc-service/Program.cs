using grpc_service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<SensorService>();
app.MapGet("/", () => "gRPC IoT Sensor Service is running. Use a gRPC client to connect.");

app.Run();
