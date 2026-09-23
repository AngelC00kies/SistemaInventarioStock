using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;

namespace InventorySystem.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
}

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAsync(PagedQuery query, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(string id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(string id, UpdateUserRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRolesAsync(CancellationToken ct = default);
}
