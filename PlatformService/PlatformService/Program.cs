using PlatformService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about  OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<AppDbContext>(opt => 
    opt.UseInMemoryDatabase("InMem"));

builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();

builder.Services.AddControllers();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());



var app = builder.Build();

// Configure the HTTP request pipeline.

PrepDbo.PrepPopulation(app);

// app.UseHsts();

// app.UseHttpsRedirection();

// app.UseCors("AllowSpecificOrigin");

app.UseRouting();

app.UseEndpoints (endpoints => {
    endpoints.MapControllers ();
});

app.Run();
