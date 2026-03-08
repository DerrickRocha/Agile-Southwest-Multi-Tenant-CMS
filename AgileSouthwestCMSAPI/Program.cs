using System.Text.Json;
using AgileSouthwestCMSAPI.Api.Middleware;
using AgileSouthwestCMSAPI.Application.DTOs.Tenants;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Application.Services;
using AgileSouthwestCMSAPI.Domain.ValueObjects;
using AgileSouthwestCMSAPI.Infrastructure.Configuration;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Configuration
// --------------------------------------------------

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured");

// --------------------------------------------------
// Services
// --------------------------------------------------

builder.Services.AddLogging();

builder.Services.AddDbContext<CmsDbContext>(options =>
{
    options.UseMySQL(connectionString, sql =>
    {
        sql.EnableRetryOnFailure();
    });
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy
                .WithOrigins(
                    "http://localhost:3000",
                    "http://localhost:5173"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy
                .WithOrigins("https://agilesouthwest.com")
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_R0b1zUu0r";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Authority,
            ValidateAudience = true,
            RoleClaimType = "cognito:groups",   
            ValidAudience = builder.Configuration["Cognito:Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantAdmin",
        policy => policy.Requirements.Add(new TenantAdminRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, TenantAdminHandler>();
// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.Window = TimeSpan.FromSeconds(10);
        limiter.PermitLimit = 100;
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CmsDbContext>("mysql");

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = new[] { "application/json" };
});

// Forwarded Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// AWS / Cognito
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonCognitoIdentityProvider>();
builder.Services.Configure<CognitoSettings>(
    builder.Configuration.GetSection("Cognito"));

builder.Services.AddSingleton<ICognitoService, CognitoService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ICmsUserContext, CmsUserContext>();

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantsService, TenantsService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// --------------------------------------------------
// App
// --------------------------------------------------

var app = builder.Build();

app.UseForwardedHeaders();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("Default");

app.UseRouting();

// 3️⃣ Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    context.Response.Headers.TryAdd(
        "Content-Security-Policy",
        "default-src 'none'; frame-ancestors 'none';"
    );

    await next();
});

app.UseRateLimiter();

app.UseMiddleware<IpAllowListMiddleware>();

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("api");

app.MapHealthChecks("/health")
   .AllowAnonymous();

app.Run();