using Api.Application.DependencyInjection;
using Api.Infrastructure.DependencyInjection;
using Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.InitializeDatabaseAsync();

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();
