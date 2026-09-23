using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Exceptions;
using InventorySystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InventorySystem.Infrastructure.Services;

public interface ITokenService
{
    (string Token, DateTime Expiration) GenerateToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config) => _config = config;

    public (string Token, DateTime Expiration) GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var key = _config["Jwt:Key"] ?? throw new AppException("Falta la configuración Jwt:Key.", 500);
        var issuer = _config["Jwt:Issuer"] ?? "InventorySystem";
        var audience = _config["Jwt:Audience"] ?? "InventorySystem";
        var minutes = int.TryParse(_config["Jwt:ExpirationMinutes"], out var m) ? m : 480;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("fullName", user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddMinutes(minutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByNameAsync(request.UserName)
                   ?? await _userManager.FindByEmailAsync(request.UserName);

        if (user is null || !user.IsActive)
            throw new AppException("Usuario o contraseña incorrectos.", 401);

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new AppException("Usuario o contraseña incorrectos.", 401);

        return await BuildResponseAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new AppException("Refresh token inválido.", 401);

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct)
            ?? throw new AppException("Sesión expirada. Inicie sesión nuevamente.", 401);

        if (!user.IsActive || user.RefreshTokenExpiry is null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new AppException("Sesión expirada. Inicie sesión nuevamente.", 401);

        return await BuildResponseAsync(user, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);

        if (user is null)
            return;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);
    }

    private async Task<AuthResponse> BuildResponseAsync(ApplicationUser user, CancellationToken ct)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiration) = _tokenService.GenerateToken(user, roles);

        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = user.RefreshToken,
            Expiration = expiration,
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "Usuario",
                IsActive = user.IsActive
            }
        };
    }
}
