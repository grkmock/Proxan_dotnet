using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
// using Microsoft.OpenApi.Models; // Hata devam ederse burayı yorum satırı yapabilirsin
using ProxanReservation.Data;
using ProxanReservation.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL DateTime uyumluluk ayarı
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// 1. Veritabanı Yapılandırması
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, LogLevel.Information));

// 2. JWT Authentication Yapılandırması
var key = Encoding.ASCII.GetBytes("Proxan_Reservation_Super_Secret_Key_2026");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
        SignatureValidator = delegate (string token, TokenValidationParameters parameters)
        {
            if (token == "proxan-admin-secret-2026-token")
            {
                return new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(
                    "{\"alg\":\"HS256\",\"typ\":\"JWT\"}", 
                    "{\"unique_name\":\"admin\",\"role\":\"Admin\"}");
            }
            return null; 
        }
    };
});

builder.Services.AddScoped<ReservationService>();
builder.Services.AddHostedService<ExpiredReservationWorker>();

bool enableAuth = builder.Configuration.GetValue<bool>("AuthConfig:EnableAuthorize");

builder.Services.AddControllers(options =>
{
    if (enableAuth)
    {
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
    }
});

builder.Services.AddEndpointsApiExplorer();

// --- SWAGGER AUTHORIZE BUTONU (Full Namespaces) ---
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Proxan Reservation API", 
        Version = "v1" 
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Sabit token: proxan-admin-secret-2026-token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Proxan API v1");
    });
}

app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    if (authHeader == "Bearer proxan-admin-secret-2026-token")
    {
        var claims = new[] { 
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "admin"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin") 
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "StaticAdmin");
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);
    }
    await next();
});

app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
app.Run();