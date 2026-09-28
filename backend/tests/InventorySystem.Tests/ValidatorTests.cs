using FluentValidation.TestHelper;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Validators;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Tests;

public class ValidatorTests
{
    // ─────────────── Productos ───────────────

    [Fact]
    public void Product_Directriz_Valida()
    {
        var v = new ProductRequestValidator();
        var model = new ProductRequest
        {
            Code = "P-001",
            Name = "Producto",
            CategoryId = 1,
            PurchasePrice = 100,
            SalePrice = 150,
            MinimumStock = 5,
            UnitOfMeasure = UnitOfMeasure.Unidad,
        };

        var result = v.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "El código es obligatorio.")]
    [InlineData("   ", "El código es obligatorio.")]
    public void Product_Code_Obligatorio(string code, string expected)
    {
        var v = new ProductRequestValidator();
        var model = ValidProduct();
        model.Code = code;

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage(expected);
    }

    [Fact]
    public void Product_Name_Requerido()
    {
        var v = new ProductRequestValidator();
        var model = ValidProduct();
        model.Name = "";

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Product_CategoryId_DebSerPositiva()
    {
        var v = new ProductRequestValidator();
        var model = ValidProduct();
        model.CategoryId = 0;

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Product_Precios_Negativos_Invalidos()
    {
        var v = new ProductRequestValidator();
        var model = ValidProduct();
        model.PurchasePrice = -1;
        model.SalePrice = -5;

        var result = v.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PurchasePrice);
        result.ShouldHaveValidationErrorFor(x => x.SalePrice);
    }

    [Fact]
    public void Product_Venta_MenorQueCompra_Invalido()
    {
        var v = new ProductRequestValidator();
        var model = ValidProduct();
        model.PurchasePrice = 200;
        model.SalePrice = 100;

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.SalePrice);
    }

    [Fact]
    public void Product_StockMinimo_Negativo_Invalido()
    {
        var v = new ProductRequestValidator();
        var model = ValidProduct();
        model.MinimumStock = -3;

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.MinimumStock);
    }

    // ─────────────── Movimientos ───────────────

    [Fact]
    public void Movement_Valido_NoTieneErrores()
    {
        var v = new MovementRequestValidator();
        var model = new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = 1,
            WarehouseId = 1,
            Quantity = 10,
            Reason = "Compra a proveedor",
        };

        v.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Movement_Cantidad_NoPositiva_Invalida(int quantity)
    {
        var v = new MovementRequestValidator();
        var model = ValidMovement();
        model.Quantity = quantity;

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("La cantidad debe ser mayor que cero.");
    }

    [Fact]
    public void Movement_SinProducto_Invalido()
    {
        var v = new MovementRequestValidator();
        var model = ValidMovement();
        model.ProductId = 0;

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Movement_SinMotivo_Invalido()
    {
        var v = new MovementRequestValidator();
        var model = ValidMovement();
        model.Reason = "";

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Reason);
    }

    // ─────────────── Usuarios ───────────────

    [Fact]
    public void CreateUser_Directriz_Valida()
    {
        var v = new CreateUserRequestValidator();
        var model = new CreateUserRequest
        {
            UserName = "juan",
            Email = "juan@mail.com",
            FullName = "Juan Pérez",
            Password = "Abcdef12",
            Role = "Usuario",
        };

        v.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("abc", "Debe contener al menos una mayúscula.")]
    [InlineData("abcdef12", "Debe contener al menos una mayúscula.")]
    [InlineData("ABCDEFG12", "Debe contener al menos una minúscula.")]
    [InlineData("Abcdefgh", "Debe contener al menos un número.")]
    [InlineData("Ab1", "La contraseña debe tener al menos 8 caracteres.")]
    public void CreateUser_Password_DebCumplirPolitica(string password, string expected)
    {
        var v = new CreateUserRequestValidator();
        var model = new CreateUserRequest
        {
            UserName = "juan",
            Email = "juan@mail.com",
            FullName = "Juan Pérez",
            Password = password,
            Role = "Usuario",
        };

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(expected);
    }

    [Fact]
    public void CreateUser_RolInvalido_Rechazado()
    {
        var v = new CreateUserRequestValidator();
        var model = new CreateUserRequest
        {
            UserName = "juan",
            Email = "juan@mail.com",
            FullName = "Juan Pérez",
            Password = "Abcdef12",
            Role = "SuperUsuario",
        };

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Role);
    }

    // ─────────────── Catálogos ───────────────

    [Fact]
    public void Category_SinNombre_Invalida()
    {
        var v = new CategoryRequestValidator();
        var model = new CategoryRequest { Name = "" };

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Supplier_EmailInvalido_Rechazado()
    {
        var v = new SupplierRequestValidator();
        var model = new SupplierRequest { Name = "Proveedor", Email = "no-es-email" };

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Supplier_SinNombre_Invalido()
    {
        var v = new SupplierRequestValidator();
        var model = new SupplierRequest { Name = "" };

        v.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Warehouse_RequiereCodigoYNombre()
    {
        var v = new WarehouseRequestValidator();
        var model = new WarehouseRequest { Code = "", Name = "" };

        var result = v.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Code);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    // ─────────────── Login ───────────────

    [Fact]
    public void Login_RequiereCredenciales()
    {
        var v = new LoginRequestValidator();
        var model = new LoginRequest("", "");

        var result = v.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    // ─────────────── helpers ───────────────

    private static ProductRequest ValidProduct() => new()
    {
        Code = "P-001",
        Name = "Producto",
        CategoryId = 1,
        PurchasePrice = 100,
        SalePrice = 150,
        MinimumStock = 5,
        UnitOfMeasure = UnitOfMeasure.Unidad,
    };

    private static MovementRequest ValidMovement() => new()
    {
        Type = MovementType.Entrada,
        ProductId = 1,
        WarehouseId = 1,
        Quantity = 10,
        Reason = "Motivo",
    };
}
