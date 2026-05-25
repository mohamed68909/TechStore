using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using TechStore.DataAccess.Data;
using TechStore.DataAccess.Implementation;
using TechStore.DataAccess.DbInitializer;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;
using TechStore.Services.Implementation;
using TechStore.Services.Interfaces;
using TechStore.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using TechStore.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Validate critical configuration at startup — fail fast rather than silently
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured. Set it in appsettings or environment variables.");

// ── Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.Configure<ImageUploadSettings>(options =>
{
    options.WebRootPath = builder.Environment.ContentRootPath;
});

builder.Services.AddOpenApi();

// ── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity + Lockout (FIX 4) ──────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // FIX 4: Enable lockout to prevent brute-force password attacks.
    // After 5 failed attempts the account is locked for 15 minutes.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── JWT Authentication ──────────────────────────────────────────────────────
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ── Stripe ──────────────────────────────────────────────────────────────────
builder.Services.Configure<StripeData>(builder.Configuration.GetSection("stripe"));

// ── Dependency Injection ────────────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, TechStore.Services.Implementation.ProductService>();
builder.Services.AddScoped<IOrderService, TechStore.Services.Implementation.OrderService>();
builder.Services.AddScoped<ITokenService, TechStore.Services.Implementation.TokenService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IOTPService, OTPService>();
builder.Services.AddSingleton<IEmailSender, TechStore.Utilities.EmailSender>();

// ── CORS (FIX 2) ────────────────────────────────────────────────────────────
// Replaced AllowAnyOrigin with a configuration-driven allowlist.
// Origins are read from appsettings.json "Cors:AllowedOrigins" array.
// In development, localhost is also permitted automatically.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("TechStorePolicy", policy =>
    {
        var origins = allowedOrigins.ToList();

        // Always allow localhost in development for easier local frontend work
        if (builder.Environment.IsDevelopment())
        {
            origins.Add("http://localhost:3000");
            origins.Add("https://localhost:3000");
            origins.Add("http://localhost:5173");  // Vite dev server default
            origins.Add("https://localhost:7000");
        }

        policy
            .WithOrigins(origins.ToArray())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Safe because we restrict origins explicitly
    });
});

// ── Rate Limiting (FIX 8) ───────────────────────────────────────────────────
// Protect auth endpoints from brute-force and OTP enumeration attacks.
// Fixed window: max 5 requests per IP per minute for the "auth" policy.
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // No queuing — reject immediately over limit
    });

    // Return 429 Too Many Requests instead of 503
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── Exception Handling ──────────────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── Memory Cache (for DashboardService FIX 10) ─────────────────────────────
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// FIX 2: Apply the restricted CORS policy (must come before auth middleware)
app.UseCors("TechStorePolicy");

// FIX 8: Apply rate limiting middleware
app.UseRateLimiter();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("stripe:Secretkey").Get<string>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// FIX 11: Renamed SeeDB → SeedDb
await SeedDb();
app.Run();

async Task SeedDb()
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await db.InitializeAsync();
}
