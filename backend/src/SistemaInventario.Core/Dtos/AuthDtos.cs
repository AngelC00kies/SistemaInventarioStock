namespace SistemaInventario.Core.Dtos;

/// <summary>Credenciales de login en texto plano: viajan por HTTPS y se comparan contra el hash guardado.</summary>
public record LoginRequest(string Username, string Password);

/// <summary>Resultado del login: el token para las peticiones siguientes y el usuario ya resuelto.</summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    /// <summary>Caducidad del token en UTC; al llegar, el frontend debe pedir login de nuevo.</summary>
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

/// <summary>Usuario con el nombre de su rol ya resuelto; es lo que devuelven el login y /auth/me.</summary>
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int RoleId { get; set; }
    /// <summary>Nombre del rol asignado ("Admin", "Usuario", "Auditor"...), redundante con RoleId para pintarlo sin otra llamada.</summary>
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Rol asignable a un usuario.</summary>
public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>Alta de usuario; la contraseña es obligatoria y debe tener al menos 6 caracteres.</summary>
public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int RoleId { get; set; }
}

/// <summary>Edición de usuario; el nombre de usuario no se puede cambiar.</summary>
public class UpdateUserRequest
{
    /// <summary>Null o vacío = se conserva la contraseña actual; si viene, se exigen al menos 6 caracteres.</summary>
    public string? Password { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
}
