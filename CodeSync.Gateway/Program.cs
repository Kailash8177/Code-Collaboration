using System.Text;
using AspNetCoreRateLimit;
using CodeSync.Gateway.Middleware;
using CodeSync.Gateway.Transforms;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ════════════════════════════════════════════════════════════════════════════
// 1. CONFIGURATION
// ════════════════════════════════════════════════════════════════════════════
var config    = builder.Configuration;
var jwtKey    = config["Jwt:Secret"]!;
var jwtIssuer = config["Jwt:Issuer"]!;
var jwtAud    = config["Jwt:Audience"]!;

// ════════════════════════════════════════════════════════════════════════════
// 2. JWT AUTHENTICATION
// ════════════════════════════════════════════════════════════════════════════
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;   // HTTP for local dev
        options.SaveToken            = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer           = true,
            ValidIssuer              = jwtIssuer,
            ValidateAudience         = true,
            ValidAudience            = jwtAud,
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero
        };

        // Forward JWT errors as JSON instead of redirect
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode  = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    """{"error":"Unauthorized","message":"A valid JWT token is required."}""");
            },
            OnForbidden = ctx =>
            {
                ctx.Response.StatusCode  = 403;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    """{"error":"Forbidden","message":"You do not have permission to access this resource."}""");
            }
        };
    });

// ════════════════════════════════════════════════════════════════════════════
// 3. AUTHORIZATION POLICIES
// ════════════════════════════════════════════════════════════════════════════
builder.Services.AddAuthorization(options =>
{
    // Used by authenticated routes
    options.AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser());

    // Used by auth routes (register, login) — no token needed
    options.AddPolicy("Public", policy =>
        policy.RequireAssertion(_ => true));

    // Admin-only routes (for future use)
    options.AddPolicy("admin-only", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("role", "ADMIN"));

    // Fallback: require authenticated
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.FallbackPolicy = null; // YARP routes handle their own policy
});

// ════════════════════════════════════════════════════════════════════════════
// 4. CORS  — allow Angular dev server + production domain
// ════════════════════════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("CodeSyncCors", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",     // Angular dev
                "http://localhost:3000"      // optional React dev
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();            // needed for SignalR
    });
});

// ════════════════════════════════════════════════════════════════════════════
// 5. RATE LIMITING  (AspNetCoreRateLimit)
// ════════════════════════════════════════════════════════════════════════════
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(config.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ════════════════════════════════════════════════════════════════════════════
// 6. YARP REVERSE PROXY
// ════════════════════════════════════════════════════════════════════════════
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(config.GetSection("ReverseProxy"))
    .AddTransforms<GatewayTransformProvider>();   // Custom transform provider

// ════════════════════════════════════════════════════════════════════════════
// 7. SWAGGER  — gateway-level docs
// ════════════════════════════════════════════════════════════════════════════
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "CodeSync API Gateway (YARP)",
        Version     = "v1",
        Description =
            "Single entry point for all CodeSync microservices via YARP reverse proxy.\n\n" +
            "**Route Map:**\n" +
            "- `/gateway/auth/**`          → Auth Service :7001\n" +
            "- `/gateway/projects/**`      → Project Service :7002\n" +
            "- `/gateway/files/**`         → File Service :7003\n" +
            "- `/gateway/sessions/**`      → Collab Service :7004\n" +
            "- `/gateway/executions/**`    → Execution Service :7005\n" +
            "- `/gateway/snapshots/**`     → ReviewHub Service :7006\n" +
            "- `/gateway/comments/**`      → ReviewHub Service :7006\n" +
            "- `/gateway/notifications/**` → Notification Service :7007"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Paste your JWT token here. IMPORTANT: Do NOT include 'Bearer ' prefix, just paste the token string."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ════════════════════════════════════════════════════════════════════════════
// 8. HEALTH CHECKS
// ════════════════════════════════════════════════════════════════════════════
builder.Services
    .AddHealthChecks()
    .AddUrlGroup(new Uri("http://localhost:7001/health"), name: "auth-service",        tags: ["services"])
    .AddUrlGroup(new Uri("http://localhost:7002/health"), name: "project-service",     tags: ["services"])
    .AddUrlGroup(new Uri("http://localhost:7003/health"), name: "file-service",        tags: ["services"])
    .AddUrlGroup(new Uri("http://localhost:7004/health"), name: "collab-service",      tags: ["services"])
    .AddUrlGroup(new Uri("http://localhost:7005/health"), name: "execution-service",   tags: ["services"])
    .AddUrlGroup(new Uri("http://localhost:7006/health"), name: "reviewhub-service",   tags: ["services"])
    .AddUrlGroup(new Uri("http://localhost:7007/health"), name: "notification-service",tags: ["services"]);

// ════════════════════════════════════════════════════════════════════════════
// BUILD
// ════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// ════════════════════════════════════════════════════════════════════════════
// 9. MIDDLEWARE PIPELINE  — ORDER MATTERS
// ════════════════════════════════════════════════════════════════════════════

// 9a. Rate limiting — first, so abusive requests are dropped immediately
app.UseIpRateLimiting();

// 9b. CORS — before auth so preflight OPTIONS requests are handled
app.UseCors("CodeSyncCors");

// 9c. Request logging middleware (custom)
app.UseMiddleware<RequestLoggingMiddleware>();

// 9d. Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeSync Gateway v1");
    c.RoutePrefix       = "swagger";
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
});

// 9e. Health checks — own endpoint, no auth needed
app.MapHealthChecks("/health");

// 9f. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 9g. Gateway info controller
app.MapControllers();



app.UseWebSockets();

// 9h. YARP — must be last; proxies all matched routes
app.MapReverseProxy(pipeline =>
{
    // Custom middleware inside the YARP proxy pipeline
    pipeline.Use(async (ctx, next) =>
    {
        // Forward the original host to downstream services
        if (ctx.Request.Headers.TryGetValue("X-Forwarded-Host", out var fwdHost))
        {
            ctx.Request.Headers["X-Original-Host"] = fwdHost;
        }
        else
        {
            ctx.Request.Headers["X-Original-Host"] = ctx.Request.Host.Value;
        }

        // Always stamp the gateway name
        ctx.Request.Headers["X-Gateway"] = "CodeSync-YARP-Gateway";

        await next();
    });
});

app.Run();