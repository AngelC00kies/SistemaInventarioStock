using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;

namespace SistemaInventario.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserDto> GetMeAsync(int userId, CancellationToken ct = default);
}

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<UserDto> GetAsync(int id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<List<RoleDto>> GetRolesAsync(CancellationToken ct = default);
}
