using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ModelBaseAPI.Data;
using ModelBaseAPI.Interfaces.Repository;
using ModelBaseAPI.Interfaces.Service;
using ModelBaseAPI.Middleware;
using ModelBaseAPI.Repositories;
using ModelBaseAPI.Services;
using ModelBaseAPI.Utilities;
using ModelBaseAspireApp.ServiceDefaults;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Polly;
using Polly.Retry;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Data;
using System.Reflection;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllersWithViews();

// Add Cache Memory

builder.Services.AddMemoryCache();

// Add Health Check: 

builder.Services.AddHealthChecks().AddCheck<CustomHealthCheck>("Database");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insert the JWT token in the format: Bearer {token}"
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
    options.OperationFilter<IdentityTagRenameFilter>();
});

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Base API Model",
        Version = "v1",
        Description = "A simple CRUD operation for testing purposes with JWT authentication, RabbitMQ messenger and user identity"
    });
});

// Add request limit

builder.Services.AddRateLimiter(_ =>
{
    _.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 10;
        options.QueueLimit = 2;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.Window = TimeSpan.FromMinutes(1);
    })
    .AddTokenBucketLimiter("tokenBucket", options =>
    {
        options.TokenLimit = 20;
        options.TokensPerPeriod = 5;
        options.QueueLimit = 5;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
    })
    .AddPolicy("ìpPolicy", context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            QueueLimit = 2,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            Window = TimeSpan.FromMinutes(1)
        });
    })
    .RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add Resilience Pipeline

builder.Services.AddResiliencePipeline("default", x =>
{
    x.AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
        Delay = TimeSpan.FromSeconds(2),
        MaxRetryAttempts = 2,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true
    })
    .AddTimeout(TimeSpan.FromSeconds(25));
});

// Add Connection String

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});

// Add EmployeeRepository to dependence injection

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddTransient<RabbitMQService>();

// Add Authorization and Admin authorization role

builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
builder.Services.AddIdentityApiEndpoints<IdentityUser>().AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

// Add CORS Policy

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "LocalPolicy",
        policy =>
        {
            policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:44381"
            // Your custom url
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        }
    );
});

// Add OpenTelemetry

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddSource("MinhaAPI");
    });

// Add custom execption handler

builder.Services.AddExceptionHandler<ProblemExceptionHandler>();

// Add custom error handler

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity!.Id);
    };
});

// Add Serilog

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var app = builder.Build();

app.MapDefaultEndpoints();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("api/").MapIdentityApi<IdentityUser>();

// Middlewares

app.UseMiddleware<ExecutionLoggingMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();
app.UseMiddleware<XssProtectionMiddleware>();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("LocalPolicy");

app.MapControllers();

app.UseRateLimiter();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.Run();