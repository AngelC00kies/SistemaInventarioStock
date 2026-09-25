using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Auth;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenGenerator _tokens;

    public AuthService(AppDbContext db, IJwtTokenGenerator tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _db.Usuarios
            .Include(u => u.Rol)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.Username, ct);

        if (user is null || !PasswordHasher.Verify(request.Password, user.ContrasenaHash))
            throw new AppException("Usuario o contraseña incorrectos.", 401);

        if (!user.Activo)
            throw new AppException("La cuenta está deshabilitada. Contacte al administrador.", 403);

        var (token, expires) = _tokens.Generate(user);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expires,
            User = Mapping.ToDto(user)
        };
    }

    public async Task<UserDto> GetMeAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.Usuarios
            .Include(u => u.Rol)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException("Usuario no encontrado.");

        return Mapping.ToDto(user);
    }
}
