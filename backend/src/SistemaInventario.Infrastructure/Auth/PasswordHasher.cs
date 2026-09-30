using System.Security.Cryptography;

namespace SistemaInventario.Infrastructure.Auth;

/// <summary>
/// Hash de contraseñas con PBKDF2 (SHA256, 100.000 iteraciones) y sal aleatoria por usuario.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    /// <summary>Genera una sal aleatoria nueva y devuelve el hash en formato "iteraciones.sal.clave".</summary>
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        // La sal y las iteraciones se guardan junto a la clave: permiten verificar hoy el hash y subir el costo de derivación sin romper contraseñas ya guardadas.
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    /// <summary>Reconstruye la clave desde la sal almacenada y la compara con la contraseña indicada.</summary>
    public static bool Verify(string password, string hash)
    {
        var parts = hash.Split('.', 3);
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], out var iterations)) return false;
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        // Comparación en tiempo fijo: evita deducir el hash mediendo el tiempo de respuesta.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
