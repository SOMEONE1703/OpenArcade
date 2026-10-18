using DnsClient;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;
using OpenArcade.Domain.Users;
using OpenArcade.Infrastructure.Checks;
using OpenArcade.Infrastructure.Persistence;
using OpenArcade.Infrastructure.Persistence.Repositories;

namespace OpenArcade.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers persistence, repositories, external checks and background jobs.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is not set.");

        services.AddDbContext<AppDbContext>(o => o
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPublisherRepository, PublisherRepository>();
        services.AddScoped<IPublisherRequestRepository, PublisherRequestRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPlayRepository, PlayRepository>();
        services.AddScoped<IGameCheckRepository, GameCheckRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IContentFilter, ProfanityFilter>();
        services.AddSingleton<ILookupClient>(new LookupClient());
        services.AddHttpClient<IEmbedChecker, EmbedChecker>(c => c.Timeout = TimeSpan.FromSeconds(15));
        services.AddHttpClient<IDomainVerifier, DomainVerifier>(c => c.Timeout = TimeSpan.FromSeconds(10));

        services.AddHangfire(c => c
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(connectionString)));
        services.AddHangfireServer();

        return services;
    }
}

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
