using SistemaInventario.Api.Auth;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Infrastructure.Services;
using SistemaInventario.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace SistemaInventario.Tests.Services;

/// <summary>Pruebas de <see cref="AuthService"/> con el generador JWT real de la API.</summary>
public class AuthServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_db.Db, new JwtTokenGenerator(Options.Create(JwtOptionsDePrueba())));
    }

    public void Dispose() => _db.Dispose();

    /// <summary>Configuración JWT mínima pero válida: la clave debe medir al menos 32 bytes para HMAC-SHA256.</summary>
    private static JwtOptions JwtOptionsDePrueba() => new()
    {
        Issuer = "sistema-inventario-tests",
        Audience = "frontend-tests",
        SecretKey = "clave-secreta-de-prueba-de-al-menos-treinta-y-dos-bytes",
        ExpirationMinutes = 60
    };

    private async Task<(int rolId, int userId)> CrearUsuarioAsync(string contrasena = "Admin123!", bool activo = true)
    {
        var rol = await Seed.RolAsync(_db.Db);
        var usuario = await Seed.UsuarioAsync(_db.Db, rol.Id, "admin", contrasena, activo);
        return (rol.Id, usuario.Id);
    }

    [Fact]
    public async Task LoginAsync_CredencialesCorrectas_DevuelveTokenPerfilYCaducidad()
    {
        await CrearUsuarioAsync("Admin123!");

        var respuesta = await _service.LoginAsync(new LoginRequest("admin", "Admin123!"));

        Assert.False(string.IsNullOrWhiteSpace(respuesta.Token));
        Assert.True(respuesta.ExpiresAt > DateTime.UtcNow);
        Assert.Equal("admin", respuesta.User.Username);
        Assert.Equal("Admin", respuesta.User.Role);
        Assert.DoesNotContain("Admin123!", respuesta.Token, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoginAsync_ContrasenaIncorrecta_LanzaAppException401()
    {
        await CrearUsuarioAsync("Admin123!");

        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.LoginAsync(new LoginRequest("admin", "otra-contraseña")));

        Assert.Equal(401, excepcion.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_UsuarioInexistente_DaElMismoMensajeQueLaContrasenaMala()
    {
        // El mensaje no puede filtrar si la cuenta existe: sería un vector de enumeración de usuarios.
        await CrearUsuarioAsync();

        var porUsuario = await Assert.ThrowsAsync<AppException>(
            () => _service.LoginAsync(new LoginRequest("no-existe", "Admin123!")));
        var porContrasena = await Assert.ThrowsAsync<AppException>(
            () => _service.LoginAsync(new LoginRequest("admin", "mala")));

        Assert.Equal(porContrasena.Message, porUsuario.Message);
        Assert.Equal(401, porUsuario.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_UsuarioInactivo_LanzaAppException403()
    {
        await CrearUsuarioAsync(activo: false);

        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.LoginAsync(new LoginRequest("admin", "Admin123!")));

        Assert.Equal(403, excepcion.StatusCode);
        Assert.Equal("La cuenta está deshabilitada. Contacte al administrador.", excepcion.Message);
    }

    [Fact]
    public async Task LoginAsync_NoAjustaMayusculasEnElNombreDeUsuario()
    {
        await CrearUsuarioAsync();

        // El servicio guarda y busca el nombre en minúsculas, así que "ADMIN" no debe entrar.
        await Assert.ThrowsAsync<AppException>(() => _service.LoginAsync(new LoginRequest("ADMIN", "Admin123!")));
    }

    [Fact]
    public async Task GetMeAsync_DevuelveElPerfilConSuRol()
    {
        var (_, userId) = await CrearUsuarioAsync();

        var perfil = await _service.GetMeAsync(userId);

        Assert.Equal("admin", perfil.Username);
        Assert.Equal("Admin", perfil.Role);
        Assert.True(perfil.IsActive);
    }

    [Fact]
    public async Task GetMeAsync_UsuarioEliminado_LanzaNotFound()
    {
        var (_, userId) = await CrearUsuarioAsync();
        await _db.Db.Usuarios.Where(u => u.Id == userId).ExecuteDeleteAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetMeAsync(userId));
    }
}
