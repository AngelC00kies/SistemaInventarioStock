using SistemaInventario.Core.Entities;

namespace SistemaInventario.Core.Interfaces;

/// <summary>Firma el JWT de un usuario (claims de id, usuario y rol).</summary>
public interface IJwtTokenGenerator
{
    /// <summary>Devuelve el token y su caducidad en UTC para que el frontend cierre sesión al expirar.</summary>
    (string Token, DateTime ExpiresAt) Generate(Usuario user);
}
