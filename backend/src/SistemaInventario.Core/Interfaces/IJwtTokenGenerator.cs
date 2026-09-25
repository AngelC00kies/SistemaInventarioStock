using SistemaInventario.Core.Entities;

namespace SistemaInventario.Core.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(Usuario user);
}
