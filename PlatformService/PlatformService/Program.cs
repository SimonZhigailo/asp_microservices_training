using PlatformService.Data;
using Microsoft.EntityFrameworkCore;
using PlatformService.SyncDataServices.http;
using PlatformService.AsyncDataServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about  OpenAPI at https://aka.ms/aspnet/openapi


builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

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

builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();

builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();

builder.Services.AddControllers();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

Console.WriteLine($"--> Command Service Endpoint {builder.Configuration["CommandService"]}");

var app = builder.Build();

// Configure the HTTP request pipeline.

PrepDbo.PrepPopulation(app, builder.Environment.IsProduction());

// app.UseHsts();

// app.UseHttpsRedirection();

// app.UseCors("AllowSpecificOrigin");

app.UseRouting();

app.UseEndpoints (endpoints => {
    endpoints.MapControllers ();
});

app.Run();
