using Microsoft.EntityFrameworkCore;
using PRN232_BE;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using PRN232_BE.BackgroundJobs;
using PRN232_BE.Hubs;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<CloneEbayDb1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDB")));

// Read allowed origins from configuration
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    // Policy dùng cho regular API requests (no credentials needed) - development friendly
    options.AddPolicy("AllowApi", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              // For API endpoints we can allow any origin in dev (change in production)
              .AllowAnyOrigin();
    });

    // Policy dành riêng cho SignalR (needs credentials) - must list explicit origins
    options.AddPolicy("SignalR", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowCredentials();
        }
        else
        {
            // Dev fallback: if no explicit origins provided, allow echo origin to satisfy AllowCredentials
            policy.SetIsOriginAllowed(origin => true)
                  .AllowCredentials();
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<AutoReleaseOrdersService>();

// SignalR
builder.Services.AddSignalR();

// JWT key guard
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("Configuration 'Jwt:Key' is missing or empty. Please set Jwt:Key in appsettings.json.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // allow access_token in query string for SignalR
                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Swagger + JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ebay Clone API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Optional DB connectivity check (log only)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CloneEbayDb1Context>();
        var canConnect = await db.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogError("Cannot connect to database. ConnectionString: {cs}", builder.Configuration.GetConnectionString("MyDB"));
            logger.LogError("DB unavailable at startup — app will continue to run for debugging. Fix DB and restart.");
        }
        else
        {
            logger.LogInformation("Database connection successful.");
        }
    }
    catch (Exception ex)
    {
        var loggerEx = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        loggerEx.LogError(ex, "Database connectivity check failed at startup (exception). App will continue to run.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

// Use API CORS globally (allows frontend login requests)
app.UseCors("AllowApi");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR with SignalR-specific CORS
app.MapHub<ChatHub>("/chathub").RequireCors("SignalR");

app.Run();