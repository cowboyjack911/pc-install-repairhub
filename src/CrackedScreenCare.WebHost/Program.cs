using CrackedScreenCare.Core.Interfaces;
using CrackedScreenCare.Infrastructure.Data;
using CrackedScreenCare.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL with Npgsql
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=crackedscreencare;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    });
});

// Configure ASP.NET Core Identity with PostgreSQL
builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Register repository implementations (enforcing modular boundaries)
builder.Services.AddScoped<ITicketingRepository, TicketingRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

// Add controllers and API support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => new
{
    Application = "CrackedScreenCare - Modular Monolith PSA Platform",
    Version = "1.0.0",
    Architecture = "ASP.NET Core 9 Modular Monolith",
    Database = "PostgreSQL with Npgsql",
    Features = new[]
    {
        "Asset-Centric Repair Ticketing",
        "PostgreSQL with Schema Separation",
        "ASP.NET Core Identity",
        "Repository Pattern for Module Boundaries"
    }
});

app.Run();
