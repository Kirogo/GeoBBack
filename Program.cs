// GeoBack/Program.cs
using geoback.Data;
using geoback.Services;
using geoback.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "GeoBuild API", 
        Version = "v1" 
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

// MySQL Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        )
    ));

// Register services
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// HTTP client for core banking
builder.Services.AddHttpClient("CoreBanking", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CoreBanking:BaseUrl"] ?? "https://core-banking.ncba.co.ke/api");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// JWT Authentication - Configure with consistent settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? 
                 builder.Configuration["Jwt:Key"] ?? 
                 "ThisIsASecretKeyForDevelopmentOnly12345!MakeSureItIsLongEnough";

// IMPORTANT: These must match exactly what your AuthController uses
var issuer = "geoback";  // Must match AuthController
var audience = "GeoBuildClient"; // Changed from 'geofront' to match AuthController

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Important for SignalR - allows token to be read from query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"✅ Token validated for: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RMOnly", policy => policy.RequireRole("RM"));
    options.AddPolicy("QSOnly", policy => policy.RequireRole("QS"));
    options.AddPolicy("RMOrQS", policy => policy.RequireRole("RM", "QS"));
});

// CORS for React frontend (Vite ports)
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", 
                "http://localhost:3000", 
                "http://localhost:5000",
                "https://localhost:5001"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add SignalR for real-time notifications
builder.Services.AddSignalR();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GeoBuild API V1");
    });
}
else
{
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Comment out if not using HTTPS in development

// Serve static files for uploads
app.UseStaticFiles();

// Add custom middleware to serve uploaded files
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/rmChecklist/photos") ||
        context.Request.Path.StartsWithSegments("/api/rmChecklist/documents"))
    {
        // Let the controller handle it
        await next();
    }
    else
    {
        await next();
    }
});

app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();

// Map health checks
app.MapHealthChecks("/health");

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<NotificationHub>("/hub/notificationHub");

// Test database connection on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        if (dbContext.Database.CanConnect())
        {
            logger.LogInformation("✓ Successfully connected to MySQL database");
            
            // Apply migrations automatically in development
            if (app.Environment.IsDevelopment())
            {
                // dbContext.Database.Migrate(); // Uncomment if you want auto-migration
                logger.LogInformation("Database is ready");
            }
        }
        else
        {
            logger.LogError("✗ Failed to connect to MySQL database");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "✗ Database connection error: {Message}", ex.Message);
    }
}

// Log application startup
var logger2 = app.Services.GetRequiredService<ILogger<Program>>();
logger2.LogInformation("✅ Application started successfully");
logger2.LogInformation("🌍 Environment: {Environment}", app.Environment.EnvironmentName);
logger2.LogInformation("🔗 API URLs: {Urls}", string.Join(", ", app.Urls));
logger2.LogInformation("🔑 JWT Audience: {Audience}", audience);
logger2.LogInformation("🔑 JWT Issuer: {Issuer}", issuer);

app.Run();