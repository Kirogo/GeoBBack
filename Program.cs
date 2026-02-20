//GeoBack/Program.cs
using geoback.Data;
using geoback.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MySQL Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Register services
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// HTTP client for core banking
builder.Services.AddHttpClient("CoreBanking", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CoreBanking:BaseUrl"] ?? "https://core-banking.ncba.co.ke/api");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// CORS for React frontend (Vite ports)
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add authentication if needed (commented out for now)
// builder.Services.AddAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ReactApp");
// app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Test database connection on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        if (dbContext.Database.CanConnect())
        {
            Console.WriteLine("✓ Successfully connected to MySQL database");
        }
        else
        {
            Console.WriteLine("✗ Failed to connect to MySQL database");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Database connection error: {ex.Message}");
    }
}

app.Run();