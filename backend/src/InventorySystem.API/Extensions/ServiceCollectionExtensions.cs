using System.Text;
using InventorySystem.API.Filters;
using InventorySystem.API.Reports;
using InventorySystem.API.Services;
using InventorySystem.Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace InventorySystem.API.Extensions;

/// <summary>
/// Registro de los servicios propios de la capa HTTP: controladores, validación,
/// Swagger, CORS, JWT y exportadores de reportes.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllersWithValidation();
        services.AddSwaggerDocumentation();
        services.AddCurrentUser();
        services.AddFrontendCors(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddReportExporters();

        return services;
    }

    private static void AddControllersWithValidation(this IServiceCollection services)
    {
        services.AddScoped<ValidationActionFilter>();

        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationActionFilter>();
        }).AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    }

    private static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Inventory System API",
                Version = "v1",
                Description = "API de gestión de inventario: productos, almacenes, movimientos, reportes y usuarios."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingrese su token JWT: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    Array.Empty<string>()
                }
            });
        });
    }

    private static void AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
    }

    private static void AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? new[] { "http://localhost:5173", "http://127.0.0.1:5173" };

        services.AddCors(options => options.AddPolicy("Frontend", policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta la configuración Jwt:Key.");

        // AddIdentity (capa Infrastructure) registra el esquema de cookies como
        // desafío por defecto: se sobreescribe para usar únicamente JWT.
        services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        services
            .AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "InventorySystem",
                    ValidAudience = configuration["Jwt:Audience"] ?? "InventorySystem",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();
    }

    /// <summary>
    /// Estrategias de exportación de reportes. Cada formato es una clase
    /// distinta: registrar aquí una implementación nueva es todo lo necesario
    /// para que la API soporte ese formato (Principio Abierto/Cerrado).
    /// </summary>
    private static void AddReportExporters(this IServiceCollection services)
    {
        services.AddSingleton<IReportExporter, JsonReportExporter>();
        services.AddSingleton<IReportExporter, ExcelReportExporter>();
        services.AddSingleton<IReportExporter, PdfReportExporter>();
        services.AddSingleton<IReportExporterSelector, ReportExporterSelector>();
    }
}
