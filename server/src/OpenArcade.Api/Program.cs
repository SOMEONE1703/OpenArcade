using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using DnsClient;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using OpenArcade.Api.Auth;
using OpenArcade.Api.Data;
using OpenArcade.Api.Endpoints;
using OpenArcade.Api.Jobs;
using OpenArcade.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is not set.");

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(o => o
    .UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention());

builder.Services.AddKeycloakAuth(builder.Configuration, builder.Environment);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUser>();

builder.Services.AddHttpClient<EmbedChecker>(c => c.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddHttpClient<DomainVerifier>(c => c.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddSingleton<ILookupClient>(new LookupClient());
builder.Services.AddSingleton<ProfanityFilter>();

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // 5 comments per minute per signed-in user.
    o.AddPolicy(CommentEndpoints.PostRateLimit, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.User.FindFirst("sub")?.Value ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddHangfire(c => c
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DevSeed.RunAsync(db, app.Configuration);

    // Dashboard is open in development only; production needs an admin-only authorization filter.
    app.UseHangfireDashboard("/hangfire", new DashboardOptions { Authorization = [] });
}

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" })).WithTags("Health");
app.MapGameEndpoints();
app.MapCommentEndpoints();
app.MapAccountEndpoints();
app.MapPublisherEndpoints();
app.MapAdminEndpoints();

CatalogJobs.Schedule(app.Services.GetRequiredService<IRecurringJobManager>());

app.Run();
