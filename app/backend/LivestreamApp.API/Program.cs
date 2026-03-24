using LivestreamApp.API.Extensions;
using LivestreamApp.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LivestreamApp API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Cookie,
        Name = "access_token",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
});

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddCachingServices(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddProfileServices();
builder.Services.AddExternalServices();
builder.Services.AddStorageService(builder.Configuration);
builder.Services.AddValidation();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddHealthChecks(builder.Configuration);
builder.Services.AddSignalRServices(builder.Configuration);
builder.Services.AddLivestreamServices();

var app = builder.Build();

// Auto-migrate on startup in non-production environments
if (!app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Middleware pipeline
app.UseCustomMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks();

// SignalR hubs — Unit 2: Livestream Engine
app.MapHub<LivestreamApp.API.Hubs.LivestreamHub>("/hubs/livestream");
app.MapHub<LivestreamApp.API.Hubs.ChatHub>("/hubs/chat");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
