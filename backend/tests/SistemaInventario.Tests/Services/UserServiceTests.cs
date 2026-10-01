using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Infrastructure.Auth;
using SistemaInventario.Infrastructure.Services;
using SistemaInventario.Tests.Infrastructure;
using Xunit;

namespace SistemaInventario.Tests.Services;

/// <summary>Pruebas de <see cref="UserService"/>: alta de cuentas, cambio de contraseña y protección del último admin.</summary>
public class UserServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly UserService _service;

    public UserServiceTests() => _service = new UserService(_db.Db);

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateAsync_GuardaElHashYNuncaLaContrasenaEnClaro()
    {
        var rol = await Seed.RolAsync(_db.Db);

        var usuario = await _service.CreateAsync(new CreateUserRequest
        {
            Username = "  Ana.Perez ",
            Password = "Secreta123",
            FullName = "  Ana Pérez  ",
            Email = " ana@correo.cl ",
            RoleId = rol.Id
        });

        Assert.True(usuario.Id > 0);

        var guardado = await _db.Db.Usuarios.SingleAsync(u => u.Id == usuario.Id);
        // El nombre se guarda en minúsculas y los textos van recortados.
        Assert.Equal("ana.perez", guardado.NombreUsuario);
        Assert.Equal("Ana Pérez", guardado.NombreCompleto);
        Assert.Equal("ana@correo.cl", guardado.Correo);

        Assert.NotEqual("Secreta123", guardado.ContrasenaHash);
        Assert.True(PasswordHasher.Verify("Secreta123", guardado.ContrasenaHash));
    }

    [Fact]
    public async Task CreateAsync_DatosObligatoriosAusentes_LanzaAppException()
    {
        var rol = await Seed.RolAsync(_db.Db);

        var sinNombre = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new CreateUserRequest
        {
            Username = "   ",
            Password = "123456",
            FullName = "Sin nombre",
            RoleId = rol.Id
        }));
        Assert.Equal("El nombre de usuario es obligatorio.", sinNombre.Message);

        var contrasenaCorta = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new CreateUserRequest
        {
            Username = "corto",
            Password = "12345",
            FullName = "Nombre Corto",
            RoleId = rol.Id
        }));
        Assert.Equal("La contraseña debe tener al menos 6 caracteres.", contrasenaCorta.Message);

        var sinNombreCompleto = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new CreateUserRequest
        {
            Username = "sincompleto",
            Password = "123456",
            FullName = "  ",
            RoleId = rol.Id
        }));
        Assert.Equal("El nombre completo es obligatorio.", sinNombreCompleto.Message);
    }

    [Fact]
    public async Task CreateAsync_UsuarioRepetidoAunEnDistintaCaja_LanzaAppException()
    {
        var rol = await Seed.RolAsync(_db.Db);
        await Seed.UsuarioAsync(_db.Db, rol.Id, "admin");

        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new CreateUserRequest
        {
            Username = "ADMIN",
            Password = "123456",
            FullName = "Otra Persona",
            RoleId = rol.Id
        }));

        Assert.Equal("Ya existe un usuario con ese nombre.", excepcion.Message);
    }

    [Fact]
    public async Task CreateAsync_RolInexistente_LanzaAppException()
    {
        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new CreateUserRequest
        {
            Username = "sinrol",
            Password = "123456",
            FullName = "Sin Rol",
            RoleId = 999
        }));

        Assert.Equal("El rol seleccionado no es válido.", excepcion.Message);
    }

    [Fact]
    public async Task UpdateAsync_SinPasswordConservaElHashActual()
    {
        var rol = await Seed.RolAsync(_db.Db);
        var usuario = await Seed.UsuarioAsync(_db.Db, rol.Id, "operador", contrasena: "Original123");
        var hashAntes = usuario.ContrasenaHash;

        await _service.UpdateAsync(usuario.Id, new UpdateUserRequest
        {
            FullName = "Nombre Nuevo",
            Email = "nuevo@correo.cl",
            RoleId = rol.Id,
            IsActive = true
        });

        var guardado = await _db.Db.Usuarios.SingleAsync(u => u.Id == usuario.Id);
        Assert.Equal(hashAntes, guardado.ContrasenaHash);
        Assert.Equal("Nombre Nuevo", guardado.NombreCompleto);
        Assert.True(PasswordHasher.Verify("Original123", guardado.ContrasenaHash));
    }

    [Fact]
    public async Task UpdateAsync_ConPasswordNuevaLaReemplaza()
    {
        var rol = await Seed.RolAsync(_db.Db);
        var usuario = await Seed.UsuarioAsync(_db.Db, rol.Id, "operador", contrasena: "Original123");

        await _service.UpdateAsync(usuario.Id, new UpdateUserRequest
        {
            Password = "NuevaClave9",
            FullName = "Operador Uno",
            RoleId = rol.Id,
            IsActive = true
        });

        var guardado = await _db.Db.Usuarios.SingleAsync(u => u.Id == usuario.Id);
        Assert.True(PasswordHasher.Verify("NuevaClave9", guardado.ContrasenaHash));
        Assert.False(PasswordHasher.Verify("Original123", guardado.ContrasenaHash));
    }

    [Fact]
    public async Task UpdateAsync_ConPasswordCorta_LanzaAppException()
    {
        var rol = await Seed.RolAsync(_db.Db);
        var usuario = await Seed.UsuarioAsync(_db.Db, rol.Id, "operador");

        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.UpdateAsync(usuario.Id, new UpdateUserRequest
        {
            Password = "12345",
            FullName = "Operador Uno",
            RoleId = rol.Id,
            IsActive = true
        }));

        Assert.Equal("La contraseña debe tener al menos 6 caracteres.", excepcion.Message);
    }

    [Fact]
    public async Task UpdateAsync_NoSePuedeRetirarElRolDelUltimoAdminActivo()
    {
        var adminRol = await Seed.RolAsync(_db.Db, "Admin");
        var usuarioRol = await Seed.RolAsync(_db.Db, "Usuario");
        var admin = await Seed.UsuarioAsync(_db.Db, adminRol.Id, "admin");

        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.UpdateAsync(admin.Id, new UpdateUserRequest
        {
            FullName = "Administrador",
            RoleId = usuarioRol.Id,
            IsActive = true
        }));

        Assert.Equal("No es posible retirar el último administrador activo del sistema.", excepcion.Message);
        // El usuario sigue tal cual estaba.
        var sinCambios = await _db.Db.Usuarios.SingleAsync(u => u.Id == admin.Id);
        Assert.Equal(adminRol.Id, sinCambios.RolId);
        Assert.True(sinCambios.Activo);
    }

    [Fact]
    public async Task UpdateAsync_NoSePuedeDesactivarElUltimoAdminActivo()
    {
        var adminRol = await Seed.RolAsync(_db.Db, "Admin");
        var admin = await Seed.UsuarioAsync(_db.Db, adminRol.Id, "admin");

        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.UpdateAsync(admin.Id, new UpdateUserRequest
        {
            FullName = "Administrador",
            RoleId = adminRol.Id,
            IsActive = false
        }));

        Assert.Equal("No es posible retirar el último administrador activo del sistema.", excepcion.Message);
    }

    [Fact]
    public async Task UpdateAsync_SiHayOtroAdminActivo_SiSePuedeDesactivar()
    {
        var adminRol = await Seed.RolAsync(_db.Db, "Admin");
        var admin = await Seed.UsuarioAsync(_db.Db, adminRol.Id, "admin");
        await Seed.UsuarioAsync(_db.Db, adminRol.Id, "admin2");

        var actualizado = await _service.UpdateAsync(admin.Id, new UpdateUserRequest
        {
            FullName = "Administrador Principal",
            RoleId = adminRol.Id,
            IsActive = false
        });

        Assert.False(actualizado.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_EnUnContextoNuevo_TambienProtegeAlUltimoAdmin()
    {
        // Como en una petición HTTP real: el servicio trabaja con un contexto recién abierto,
        // donde la entidad Rol del usuario todavía no está materializada.
        var adminRol = await Seed.RolAsync(_db.Db, "Admin");
        var admin = await Seed.UsuarioAsync(_db.Db, adminRol.Id, "admin");
        var servicioNuevaSesion = new UserService(_db.NewContext());

        var excepcion = await Assert.ThrowsAsync<AppException>(() => servicioNuevaSesion.UpdateAsync(admin.Id, new UpdateUserRequest
        {
            FullName = "Administrador",
            RoleId = adminRol.Id,
            IsActive = false
        }));

        Assert.Equal("No es posible retirar el último administrador activo del sistema.", excepcion.Message);
    }

    [Fact]
    public async Task UpdateAsync_IdInexistente_LanzaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(999, new UpdateUserRequest
        {
            FullName = "Nadie",
            RoleId = 1,
            IsActive = true
        }));
    }

    [Fact]
    public async Task DeleteAsync_NuncaBorraUnAdministrador()
    {
        var adminRol = await Seed.RolAsync(_db.Db, "Admin");
        var admin = await Seed.UsuarioAsync(_db.Db, adminRol.Id, "admin");

        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.DeleteAsync(admin.Id));

        Assert.Contains("administrador", excepcion.Message);
        Assert.True(await _db.Db.Usuarios.AnyAsync(u => u.Id == admin.Id));
    }

    [Fact]
    public async Task DeleteAsync_SinHistorial_BorraFisicamente()
    {
        var rol = await Seed.RolAsync(_db.Db, "Usuario");
        var usuario = await Seed.UsuarioAsync(_db.Db, rol.Id, "invitado");

        await _service.DeleteAsync(usuario.Id);

        Assert.False(await _db.Db.Usuarios.AnyAsync(u => u.Id == usuario.Id));
    }

    [Fact]
    public async Task DeleteAsync_ConMovimientos_SoloLoDesactivaYLiberaElNombre()
    {
        var rol = await Seed.RolAsync(_db.Db, "Usuario");
        var usuario = await Seed.UsuarioAsync(_db.Db, rol.Id, "operador");
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var almacen = await Seed.AlmacenAsync(_db.Db);
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id);
        await Seed.MovimientoAsync(_db.Db, producto.Id, almacen.Id, usuario.Id);

        await _service.DeleteAsync(usuario.Id);

        var superviviente = await _db.Db.Usuarios.SingleAsync(u => u.Id == usuario.Id);
        Assert.False(superviviente.Activo);
        // El nombre de usuario queda liberado con sufijo para poder crear otro con ese nombre.
        Assert.NotEqual("operador", superviviente.NombreUsuario);
        Assert.StartsWith("operador.baja.", superviviente.NombreUsuario);
    }

    [Fact]
    public async Task GetAsync_ConBusqueda_FiltraPorNombreNombreCompletoYCorreo()
    {
        var rol = await Seed.RolAsync(_db.Db);
        await Seed.UsuarioAsync(_db.Db, rol.Id, "ana", contrasena: null);
        await _service.CreateAsync(new CreateUserRequest
        {
            Username = "luis",
            Password = "123456",
            FullName = "Luis Contreras",
            Email = "luis@correo.cl",
            RoleId = rol.Id
        });

        Assert.Single((await _service.GetAsync("ANA", 1, 50)).Items);
        Assert.Single((await _service.GetAsync("contreras", 1, 50)).Items);
        Assert.Single((await _service.GetAsync("luis@correo", 1, 50)).Items);
        Assert.Equal(2, (await _service.GetAsync(null, 1, 50)).Total);
    }

    [Fact]
    public async Task GetAsync_AcotaLaPaginacionEntre1Y200()
    {
        var rol = await Seed.RolAsync(_db.Db);
        await Seed.UsuarioAsync(_db.Db, rol.Id);

        Assert.Equal(200, (await _service.GetAsync(null, page: 1, pageSize: 9999)).PageSize);
        Assert.Equal(1, (await _service.GetAsync(null, page: 0, pageSize: -7)).Page);
        Assert.Equal(1, (await _service.GetAsync(null, page: 0, pageSize: -7)).PageSize);
    }

    [Fact]
    public async Task GetRolesAsync_DevuelveLosRolesOrdenados()
    {
        await Seed.RolAsync(_db.Db, "Usuario", "Operador de inventario");
        await Seed.RolAsync(_db.Db, "Auditor", "Solo lectura");

        var roles = await _service.GetRolesAsync();

        Assert.Equal(2, roles.Count);
        Assert.Equal(new[] { "Usuario", "Auditor" }, roles.Select(r => r.Name));
        Assert.Equal("Operador de inventario", roles[0].Description);
    }
}
