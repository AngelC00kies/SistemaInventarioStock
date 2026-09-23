namespace InventorySystem.Application.Common;

/// <summary>Usuario autenticado actual (implementado en la capa API).</summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
