using System.Text;
using CodeSync.ReviewHub.Consumers;
using CodeSync.ReviewHub.Data;
using CodeSync.ReviewHub.Repositories;
using CodeSync.ReviewHub.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ReviewHubDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("ReviewHubDb")));

// ── N-Layer DI wiring ─────────────────────────────────────────────────────────
builder.Services.AddScoped<ISnapshotRepository, SnapshotRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ISnapshotService, SnapshotServiceImpl>();
builder.Services.AddScoped<ICommentService, CommentServiceImpl>();

// ── HttpClient ────────────────────────────────────────────────────────────────
builder.Services.AddHttpClient();

// ── JWT ───────────────────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret must be set");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer           = false,
            ValidateAudience         = false,
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── SAGA — MassTransit In-Memory ──────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FileCreatedConsumer>();
    x.AddConsumer<FileDeletedConsumer>();
    x.AddConsumer<ProjectDeletedConsumer>();

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opt =>
    opt.AddPolicy("CodeSyncCors", policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// ── Controllers + Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "CodeSync — ReviewHub Service",
        Version     = "v1",
        Description = "Manages snapshots, diffs, branches and code review comments"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter: Bearer {your JWT token}"
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
            []
        }
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReviewHub Service v1"));
}

app.UseHttpsRedirection();
app.UseCors("CodeSyncCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReviewHubDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();