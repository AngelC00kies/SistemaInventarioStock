using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SistemaInventario.Api.Auth;
using SistemaInventario.Core.Entities;
using SistemaInventario.Core.Interfaces;
using Microsoft.Extensions.Options;
using Xunit;

namespace SistemaInventario.Tests.Auth;

/// <summary>Pruebas de <see cref="JwtTokenGenerator"/>: claims, caducidad y firma del token.</summary>
public class JwtTokenGeneratorTests
{
    private const string Clave =
        "clave-secreta-de-prueba-de-al-menos-treinta-y-dos-bytes";

    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests() => _generator = new JwtTokenGenerator(Options.Create(new JwtOptions
    {
        Issuer = "sistema-inventario-tests",
        Audience = "frontend-tests",
        SecretKey = Clave,
        ExpirationMinutes = 30
    }));

    private static Usuario UsuarioDePrueba() => new()
    {
        Id = 42,
        NombreUsuario = "ana.perez",
        NombreCompleto = "Ana Pérez",
        Rol = new Rol { Id = 1, Nombre = "Admin" }
    };

    [Fact]
    public void Generate_DevuelveUnTokenNoVacioYUnaCaducidadFutura()
    {
        var (token, expiresAt) = _generator.Generate(UsuarioDePrueba());

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAt > DateTime.UtcNow);
        // 30 minutos de duración: el token no puede caducar ni antes ni mucho después.
        Assert.True(expiresAt <= DateTime.UtcNow.AddMinutes(31));
    }

    [Fact]
    public void Generate_EmbebeElIdElUsuarioYElRol()
    {
        var (token, _) = _generator.Generate(UsuarioDePrueba());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var valores = jwt.Claims.Select(c => c.Value).ToList();

        // El rol viaja embebido para resolver las políticas sin volver a la base de datos.
        Assert.Contains("Admin", valores);
        Assert.Contains("42", valores);
        Assert.Contains("ana.perez", valores);
        Assert.Contains("Ana Pérez", valores);
        Assert.Contains(jwt.Claims, c => c.Value == "Admin" && c.Type.EndsWith("role", StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_DejaElEmisorYLaAudienciaDeLaConfiguracion()
    {
        var (token, _) = _generator.Generate(UsuarioDePrueba());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("sistema-inventario-tests", jwt.Issuer);
        Assert.Equal("frontend-tests", jwt.Audiences.Single());
    }

    [Fact]
    public void Generate_DosLlamadas_DanTokensDistintos()
    {
        // El "jti" (id del token) es aleatorio: dos emisiones nunca deben coincidir.
        var primero = _generator.Generate(UsuarioDePrueba());
        var segundo = _generator.Generate(UsuarioDePrueba());

        Assert.NotEqual(primero.Token, segundo.Token);
        Assert.NotEqual(primero.ExpiresAt, segundo.ExpiresAt);
    }

    [Fact]
    public void Generate_LaFirmaSeValidaConLaMismaClave()
    {
        var (token, _) = _generator.Generate(UsuarioDePrueba());

        var parametros = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "sistema-inventario-tests",
            ValidAudience = "frontend-tests",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Clave)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, parametros, out var tokenValidado);

        Assert.NotNull(tokenValidado);
        Assert.Contains(principal.Claims, c => c.Value == "Admin");
    }

    [Fact]
    public void Generate_LaClaveErronea_NoValidaElToken()
    {
        var (token, _) = _generator.Generate(UsuarioDePrueba());

        var parametros = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidIssuer = "sistema-inventario-tests",
            ValidAudience = "frontend-tests",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("otra-clave-completamente-distinta-1234")),
            // Sin validar emisor/audiencia para que el fallo venga solo de la firma.
            ValidateIssuer = false,
            ValidateAudience = false
        };

        Assert.ThrowsAny<SecurityTokenException>(
            () => new JwtSecurityTokenHandler().ValidateToken(token, parametros, out _));
    }
}
