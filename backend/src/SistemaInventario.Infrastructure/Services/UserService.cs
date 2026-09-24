using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Auth;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public async Task<PagedResult<UserDto>> GetAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.Users.Include(u => u.Role).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(term) ||
                u.FullName.ToLower().Contains(term) ||
                (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => Mapping.ToDto(u))
            .ToListAsync(ct);

        return new PagedResult<UserDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<UserDto> GetAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Role).AsNoTracking()
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

        var username = request.Username.Trim().ToLower();
        if (await _db.Users.AnyAsync(u => u.Username == username, ct))
            throw new AppException("Ya existe un usuario con ese nombre.");

        if (!await _db.Roles.AnyAsync(r => r.Id == request.RoleId, ct))
            throw new AppException("El rol seleccionado no es válido.");

        var user = new Core.Entities.User
        {
            Username = username,
            PasswordHash = PasswordHasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            RoleId = request.RoleId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(user.Id, ct);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException("Usuario no encontrado.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new AppException("El nombre completo es obligatorio.");
        if (!await _db.Roles.AnyAsync(r => r.Id == request.RoleId, ct))
            throw new AppException("El rol seleccionado no es válido.");

        var currentAdmins = await _db.Users.CountAsync(u => u.Role.Name == "Admin" && u.IsActive, ct);
        if (user.RoleId != request.RoleId || !request.IsActive)
        {
            if (user.Role.Name == "Admin" && (currentAdmins <= 1))
                throw new AppException("No es posible retirar el último administrador activo del sistema.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        user.RoleId = request.RoleId;
        user.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 6)
                throw new AppException("La contraseña debe tener al menos 6 caracteres.");
            user.PasswordHash = PasswordHasher.Hash(request.Password);
        }

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException("Usuario no encontrado.");

        if (user.Role.Name == "Admin")
            throw new AppException("No se puede eliminar un usuario administrador. Deshabilítelo en su lugar.");

        if (await _db.Movements.AnyAsync(m => m.UserId == id, ct))
        {
            user.IsActive = false;
            user.Username = $"{user.Username}.baja.{DateTime.UtcNow.Ticks}";
        }
        else
        {
            _db.Users.Remove(user);
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<RoleDto>> GetRolesAsync(CancellationToken ct = default) =>
        await _db.Roles.AsNoTracking().OrderBy(r => r.Id)
            .Select(r => new RoleDto { Id = r.Id, Name = r.Name, Description = r.Description })
            .ToListAsync(ct);
}
