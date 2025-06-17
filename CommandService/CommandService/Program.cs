
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about  OpenAPI at https://aka.ms/aspnet/openapi


builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

// app.UseHsts();

// app.UseHttpsRedirection();

// app.UseCors("AllowSpecificOrigin");

app.UseRouting();

app.UseEndpoints (endpoints => {
    endpoints.MapControllers ();
});

app.Run();
