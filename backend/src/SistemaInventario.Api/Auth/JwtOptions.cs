namespace SistemaInventario.Api.Auth;

/// <summary>Opciones JWT leídas de la sección "Jwt" de la configuración (clave, emisor, audiencia y duración).</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    // Duración de la sesión: 8 horas por defecto cuando la sección no la especifica
    public int ExpirationMinutes { get; set; } = 480;
}
