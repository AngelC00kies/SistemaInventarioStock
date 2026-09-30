using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SistemaInventario.Core.Entities;
using SistemaInventario.Core.Interfaces;

namespace SistemaInventario.Api.Auth;

/// <summary>Firma los tokens JWT de acceso a partir de la sección "Jwt" de la configuración.</summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options) => _options = options.Value;

    /// <summary>Genera el token del usuario y su fecha de expiración en UTC.</summary>
    public (string Token, DateTime ExpiresAt) Generate(Usuario user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        // El rol viaja embebido en el token para que las políticas de autorización
        // se resuelvan en cada petición sin volver a consultar la base de datos.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.NombreUsuario),
            new("fullName", user.NombreCompleto),
            new(ClaimTypes.Role, user.Rol.Nombre),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
