using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;

namespace SistemaInventario.Core.Interfaces;

/// <summary>Autenticación: compara la contraseña con el hash almacenado y resuelve el usuario del token actual.</summary>
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    /// <summary>Perfil del usuario identificado por el token; lanza NotFoundException si la cuenta ya no existe.</summary>
    Task<UserDto> GetMeAsync(int userId, CancellationToken ct = default);
}

/// <summary>Alta y mantenimiento de usuarios del backoffice, incluido el cambio de contraseña y el listado de roles.</summary>
public interface IUserService
{
    Task<PagedResult<UserDto>> GetAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<UserDto> GetAsync(int id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<List<RoleDto>> GetRolesAsync(CancellationToken ct = default);
}
