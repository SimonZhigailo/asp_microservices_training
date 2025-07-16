using PlatformService.Data;
using Microsoft.EntityFrameworkCore;
using PlatformService.SyncDataServices;
using PlatformService.AsyncDataServices;
using PlatformService.SyncDataServices.Grpc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PlatformService.SyncDataServices.http;

var builder = WebApplication.CreateBuilder(args);

// 👇 Настройка HTTPS на 8080 и HTTP на 5085
builder.WebHost.ConfigureKestrel(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Локально запускаем HTTPS для gRPC
        options.ListenLocalhost(8080, listenOptions =>
        {
            listenOptions.UseHttps();
            listenOptions.Protocols = HttpProtocols.Http2;
        });

        options.ListenLocalhost(5085, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        });
    }
    else
    {
        // В Kubernetes HTTPS не нужен — Ingress это обрабатывает
        options.ListenAnyIP(80, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2; // gRPC
        });

        options.ListenAnyIP(81, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1AndHttp2; // обычный API
        });
    }
});

// Настройка конфигурации
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Настройка БД
if (builder.Environment.IsProduction())
{
    Console.WriteLine("Использую SqlServer DB");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConnection")));
}
else
{
    Console.WriteLine("Использую inMem DB");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseInMemoryDatabase("InMem"));
}

// Регистрация зависимостей
builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();
// builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();

builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

Console.WriteLine($"--> Command Service Endpoint {builder.Configuration["CommandService"]}");

var app = builder.Build();

// Засев базы
PrepDbo.PrepPopulation(app, builder.Environment.IsProduction());

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGrpcService<GrpcPlatformService>();

    endpoints.MapGet("/protos/platforms.proto", async context =>
    {
        await context.Response.WriteAsync(System.IO.File.ReadAllText("/protos/platforms.proto"));
    });
});

app.Run();