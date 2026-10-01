using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Auth;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

/// <summary>Gestión de usuarios y roles, con protección del último administrador activo y bajas lógicas.</summary>
public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public async Task<PagedResult<UserDto>> GetAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.Usuarios.Include(u => u.Rol).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.NombreUsuario.ToLower().Contains(term) ||
                u.NombreCompleto.ToLower().Contains(term) ||
                (u.Correo != null && u.Correo.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.NombreUsuario)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => Mapping.ToDto(u))
            .ToListAsync(ct);

        return new PagedResult<UserDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<UserDto> GetAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Usuarios.Include(u => u.Rol).AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException("Usuario no encontrado.");
        return Mapping.ToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new AppException("El nombre de usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new AppException("La contraseña debe tener al menos 6 caracteres.");
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new AppException("El nombre completo es obligatorio.");

        // El nombre se guarda en minúsculas y la unicidad se comprueba en ese formato.
        var username = request.Username.Trim().ToLower();
        if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == username, ct))
            throw new AppException("Ya existe un usuario con ese nombre.");

        if (!await _db.Roles.AnyAsync(r => r.Id == request.RoleId, ct))
            throw new AppException("El rol seleccionado no es válido.");

        var user = new Core.Entities.Usuario
        {
            NombreUsuario = username,
            ContrasenaHash = PasswordHasher.Hash(request.Password),
            NombreCompleto = request.FullName.Trim(),
            Correo = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            RolId = request.RoleId
        };

        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(user.Id, ct);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        // El Rol hay que materializarlo: sin el Include, user.Rol queda a null y la comprobación
        // del último administrador de más abajo petaría con NullReferenceException (500) en vez
        // de devolver el mensaje de negocio (400). Es lo mismo que ya hace DeleteAsync.
        var user = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException("Usuario no encontrado.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new AppException("El nombre completo es obligatorio.");
        if (!await _db.Roles.AnyAsync(r => r.Id == request.RoleId, ct))
            throw new AppException("El rol seleccionado no es válido.");

        // Regla de permisos: nunca se puede retirar el rol ni desactivar al último administrador activo, o la aplicación quedaría sin acceso.
        var currentAdmins = await _db.Usuarios.CountAsync(u => u.Rol.Nombre == "Admin" && u.Activo, ct);
        if (user.RolId != request.RoleId || !request.IsActive)
        {
            if (user.Rol.Nombre == "Admin" && (currentAdmins <= 1))
                throw new AppException("No es posible retirar el último administrador activo del sistema.");
        }

        user.NombreCompleto = request.FullName.Trim();
        user.Correo = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        user.RolId = request.RoleId;
        user.Activo = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 6)
                throw new AppException("La contraseña debe tener al menos 6 caracteres.");
            user.ContrasenaHash = PasswordHasher.Hash(request.Password);
        }

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    /// <summary>Elimina el usuario o lo da de baja cuando su historial impide el borrado físico.</summary>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException("Usuario no encontrado.");

        if (user.Rol.Nombre == "Admin")
            throw new AppException("No se puede eliminar un usuario administrador. Deshabilítelo en su lugar.");

        // Baja lógica: se desactiva y se libera el nombre de usuario con un sufijo para poder crear otro con ese nombre sin alterar el histórico.
        if (await _db.Movimientos.AnyAsync(m => m.UsuarioId == id, ct))
        {
            user.Activo = false;
            user.NombreUsuario = $"{user.NombreUsuario}.baja.{DateTime.UtcNow.Ticks}";
        }
        else
        {
            _db.Usuarios.Remove(user);
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<RoleDto>> GetRolesAsync(CancellationToken ct = default) =>
        await _db.Roles.AsNoTracking().OrderBy(r => r.Id)
            .Select(r => new RoleDto { Id = r.Id, Name = r.Nombre, Description = r.Descripcion })
            .ToListAsync(ct);
}
