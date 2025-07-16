using CommandService.AsyncDataServices;
using CommandService.Data;
using CommandService.EventProcessing;
using CommandService.SyncDataServices.Grpc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
builder.Services.AddScoped<ICommandRepo, CommandRepo>();
builder.Services.AddControllers();

builder.Services.AddHostedService<MessageBusSubscriber>();
 
builder.Services.AddSingleton<IEventProcessor, EventProcessor>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IPlatformDataClient, PlatformDataClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// app.UseHsts();

// app.UseHttpsRedirection();

// app.UseCors("AllowSpecificOrigin");

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

PrepDb.PrepPopulation(app);

app.Run();
