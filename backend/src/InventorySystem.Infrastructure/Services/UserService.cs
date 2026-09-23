using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Exceptions;
using InventorySystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Services;

public class UserService : IUserService
{
    public static readonly string[] AllowedRoles = { "Admin", "Usuario", "Auditor" };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;

    public UserService(UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<UserDto>> GetAsync(PagedQuery query, CancellationToken ct = default)
    {
        var q = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(u => u.UserName!.Contains(term) ||
                             u.FullName.Contains(term) ||
                             (u.Email != null && u.Email.Contains(term)));
        }

        var total = await q.CountAsync(ct);

        var users = await q
            .OrderBy(u => u.UserName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var items = new List<UserDto>();
        foreach (var u in users)
            items.Add(await MapAsync(u));

        return new PagedResult<UserDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<UserDto> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new NotFoundException($"Usuario {id} no encontrado.");
        return await MapAsync(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        if (!AllowedRoles.Contains(request.Role))
            throw new AppException("Rol inválido. Use Admin, Usuario o Auditor.");

        if (await _userManager.FindByNameAsync(request.UserName) is not null)
            throw new AppException($"El usuario '{request.UserName}' ya existe.");

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            throw new AppException($"El email '{request.Email}' ya está registrado.");

        var user = new ApplicationUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new AppException(string.Join(" ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, request.Role);
        return await MapAsync(user);
    }

    public async Task<UserDto> UpdateAsync(string id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new NotFoundException($"Usuario {id} no encontrado.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (await _userManager.Users.AnyAsync(u => u.Email == request.Email && u.Id != id, ct))
                throw new AppException($"El email '{request.Email}' ya está registrado.");
            user.Email = request.Email.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName.Trim();

        if (request.IsActive.HasValue)
        {
            // Un usuario no puede desactivarse a sí mismo
            if (request.IsActive == false && user.Id == _currentUser.UserId)
                throw new AppException("No puede desactivar su propia cuenta.");

            user.IsActive = request.IsActive.Value;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new AppException(string.Join(" ", updateResult.Errors.Select(e => e.Description)));

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!AllowedRoles.Contains(request.Role))
                throw new AppException("Rol inválido. Use Admin, Usuario o Auditor.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, request.Role);
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!result.Succeeded)
                throw new AppException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        return await GetByIdAsync(id, ct);
    }

    public Task<IReadOnlyList<string>> GetRolesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<string>>(AllowedRoles.ToList());

    private async Task<UserDto> MapAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? "Usuario",
            IsActive = user.IsActive
        };
    }
}
