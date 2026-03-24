using Amazon.Runtime;
using Amazon.S3;
using FluentValidation;
using FluentValidation.AspNetCore;
using LivestreamApp.API.Hubs;
using LivestreamApp.API.HostedServices;
using LivestreamApp.API.Infrastructure;
using LivestreamApp.API.Infrastructure.Options;
using LivestreamApp.API.Infrastructure.Repositories;
using LivestreamApp.API.Infrastructure.Services;
using LivestreamApp.Auth.Options;
using LivestreamApp.Auth.Repositories;
using LivestreamApp.Auth.Services;
using LivestreamApp.DirectChat.Repositories;
using LivestreamApp.DirectChat.Services;
using LivestreamApp.Livestream.Repositories;
using LivestreamApp.Livestream.Services;
using LivestreamApp.Profiles.Repositories;
using LivestreamApp.Profiles.Services;
using LivestreamApp.RoomChat.Services;
using LivestreamApp.Shared.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

namespace LivestreamApp.API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers EF Core DbContext with PostgreSQL.</summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("PostgreSQL"),
                npgsql => npgsql.EnableRetryOnFailure(3)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IHostProfileRepository, HostProfileRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();

        return services;
    }

    /// <summary>Registers Redis cache service.</summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration config)
    {
        var redisConn = config.GetConnectionString("Redis")!;
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));
        services.AddScoped<ICacheService, RedisCacheService>();
        return services;
    }

    /// <summary>Registers Auth domain services.</summary>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));
        services.Configure<LineOptions>(config.GetSection(LineOptions.SectionName));

        services.AddScoped<IAuthService, AuthService>();

        // LineOAuthService requires a typed HttpClient
        services.AddHttpClient<ILineOAuthService, LineOAuthService>();

        // JWT Bearer authentication
        var jwtSection = config.GetSection(JwtOptions.SectionName);
        var secretKey = jwtSection["SecretKey"]!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                // Read JWT from httpOnly cookie
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        ctx.Token = ctx.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>Registers Profile domain services.</summary>
    public static IServiceCollection AddProfileServices(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IHostVerificationService, HostVerificationService>();
        return services;
    }

    /// <summary>
    /// Registers stub implementations for external services (email, SMS).
    /// Storage is registered separately via AddStorageService.
    /// </summary>
    public static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        // TODO: replace stubs with real providers (SES, Twilio) — Refs: NFR-U1-INFRA
        services.AddScoped<IEmailService, StubEmailService>();
        services.AddScoped<ISmsService, StubSmsService>();
        return services;
    }

    /// <summary>
    /// Registers S3StorageService backed by LocalStack (dev) or real AWS S3 (prod).
    /// Reads config from S3Options section. Falls back to StubStorageService if S3 section is absent.
    /// </summary>
    public static IServiceCollection AddStorageService(this IServiceCollection services, IConfiguration config)
    {
        var s3Section = config.GetSection(S3Options.SectionName);
        if (!s3Section.Exists())
        {
            // No S3 config — use stub (safe fallback for environments without LocalStack)
            services.AddScoped<IStorageService, StubStorageService>();
            return services;
        }

        services.Configure<S3Options>(s3Section);
        var s3Options = s3Section.Get<S3Options>()!;

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(s3Options.Region),
            };

            if (!string.IsNullOrEmpty(s3Options.ServiceUrl))
            {
                // LocalStack: force path-style, point to local endpoint, and disable HTTPS
                s3Config.ServiceURL = s3Options.ServiceUrl;
                s3Config.ForcePathStyle = true;
                s3Config.UseHttp = true;
            }

            // LocalStack accepts any credentials — use dummy values in dev
            var credentials = new BasicAWSCredentials("test", "test");
            return new AmazonS3Client(credentials, s3Config);
        });

        services.AddScoped<IStorageService, S3StorageService>();
        return services;
    }

    /// <summary>Registers FluentValidation validators from this assembly.</summary>
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }

    /// <summary>Registers CORS policy allowing frontend origins with credentials.</summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000", "http://localhost:3001"];

        services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()); // Required for httpOnly cookie flow
        });

        return services;
    }

    /// <summary>Registers health checks for PostgreSQL and Redis.</summary>
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration config)
    {
        services.AddHealthChecks()
            .AddNpgSql(config.GetConnectionString("PostgreSQL")!,
                name: "postgresql",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                tags: ["ready"])
            .AddRedis(config.GetConnectionString("Redis")!,
                name: "redis",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                tags: ["ready"]);

        return services;
    }

    /// <summary>Registers SignalR with Redis backplane.</summary>
    public static IServiceCollection AddSignalRServices(this IServiceCollection services, IConfiguration config)
    {
        var redisConn = config.GetConnectionString("Redis")!;

        services.AddSignalR(options =>
        {
            options.KeepAliveInterval = TimeSpan.FromSeconds(
                config.GetValue("SignalR:KeepAliveIntervalSeconds", 15));
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(
                config.GetValue("SignalR:ClientTimeoutIntervalSeconds", 30));
        })
        .AddStackExchangeRedis(redisConn, options =>
        {
            options.Configuration.ChannelPrefix = RedisChannel.Literal("signalr");
        });

        // Connection tracker — singleton per ECS task
        services.AddSingleton<IConnectionTracker, ConnectionTracker>();

        return services;
    }

    /// <summary>Registers Unit 2 — Livestream, RoomChat, DirectChat services.</summary>
    public static IServiceCollection AddLivestreamServices(this IServiceCollection services)
    {
        // Livestream module
        services.AddScoped<ILivestreamService, LivestreamService>();
        services.AddScoped<IPrivateCallService, PrivateCallService>();
        services.AddScoped<IAgoraTokenService, AgoraTokenService>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IViewerCountService, ViewerCountService>();

        // Repositories — Livestream
        services.AddScoped<ILivestreamRoomRepository, LivestreamRoomRepository>();
        services.AddScoped<IViewerSessionRepository, ViewerSessionRepository>();
        services.AddScoped<ICallSessionRepository, CallSessionRepository>();
        services.AddScoped<IBillingTickRepository, BillingTickRepository>();

        // Coin wallet
        services.AddScoped<ICoinWalletService, CoinWalletService>();

        // RoomChat module
        services.AddScoped<IRoomChatService, RoomChatService>();
        services.AddSingleton<IChatRateLimitService, ChatRateLimitService>();

        // DirectChat module
        services.AddScoped<IDirectChatService, DirectChatService>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IDirectMessageRepository, DirectMessageRepository>();
        services.AddScoped<IBlockRepository, BlockRepository>();

        // Hosted services
        services.AddHostedService<PartitionMaintenanceService>();
        services.AddHostedService<ViewerCountBroadcastService>();
        services.AddHostedService<MetricsPublisherService>();

        return services;
    }
}
