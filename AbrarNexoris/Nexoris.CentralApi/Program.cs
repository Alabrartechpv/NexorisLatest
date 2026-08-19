using Nexoris.CentralApi.Middleware;
using Nexoris.CentralApi.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<ICentralSyncService, CentralSyncService>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexoris Central API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();

// Custom Branch Authentication Middleware
app.UseMiddleware<BranchAuthMiddleware>();

app.MapControllers();

app.Run();
