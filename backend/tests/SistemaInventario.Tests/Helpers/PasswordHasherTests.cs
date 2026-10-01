using SistemaInventario.Infrastructure.Auth;
using Xunit;

namespace SistemaInventario.Tests.Helpers;

/// <summary>Pruebas del hash PBKDF2 con el que se guarda la contraseña de los usuarios.</summary>
public class PasswordHasherTests
{
    [Fact]
    public void Hash_DevuelveElFormatoIteracionesSalClave()
    {
        var hash = PasswordHasher.Hash("Admin123!");

        var parts = hash.Split('.', 3);
        Assert.Equal(3, parts.Length);
        // 100.000 iteraciones fijadas en la clase: subirlas exige poder verificar los hashes viejos.
        Assert.Equal("100000", parts[0]);
        // Sal de 16 bytes y clave de 32, ambas en Base64.
        Assert.Equal(24, parts[1].Length);
        Assert.Equal(44, parts[2].Length);
    }

    [Fact]
    public void Hash_NoGuardaLaContrasenaEnClaro()
    {
        var hash = PasswordHasher.Hash("Admin123!");

        Assert.DoesNotContain("Admin123!", hash, StringComparison.Ordinal);
    }

    [Fact]
    public void Hash_DosVeces_DaResultadosDistintos()
    {
        // La sal es aleatoria por usuario: la misma contraseña no debe producir el mismo hash.
        var primero = PasswordHasher.Hash("Admin123!");
        var segundo = PasswordHasher.Hash("Admin123!");

        Assert.NotEqual(primero, segundo);
    }

    [Fact]
    public void Verify_ContrasenaCorrecta_DevuelveTrue()
    {
        var hash = PasswordHasher.Hash("Admin123!");

        Assert.True(PasswordHasher.Verify("Admin123!", hash));
    }

    [Fact]
    public void Verify_ContrasenaIncorrecta_DevuelveFalse()
    {
        var hash = PasswordHasher.Hash("Admin123!");

        Assert.False(PasswordHasher.Verify("admin123!", hash));
        Assert.False(PasswordHasher.Verify("", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("sin-puntos")]
    [InlineData("100000.solamente-dos")]
    [InlineData("iteraciones-no-numericas.dHJpcw==.YWFh")]
    public void Verify_HashConFormatoInvalido_DevuelveFalseEnLanzarError(string hash)
    {
        // Verify solo tolera la estructura rota (faltan separadores o las iteraciones no son un número).
        // La sal y la clave siempre las genera Hash, así que no hace falta sanear Base64 corrupto.
        Assert.False(PasswordHasher.Verify("Admin123!", hash));
    }
}
