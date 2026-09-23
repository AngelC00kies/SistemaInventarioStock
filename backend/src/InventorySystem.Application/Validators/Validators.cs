using FluentValidation;
using InventorySystem.Application.Dtos;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.Validators;

public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class SupplierRequestValidator : AbstractValidator<SupplierRequest>
{
    public SupplierRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(150);
        RuleFor(x => x.ContactName).MaximumLength(150);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Email inválido.");
        RuleFor(x => x.Address).MaximumLength(300);
    }
}

public class WarehouseRequestValidator : AbstractValidator<WarehouseRequest>
{
    public WarehouseRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("El código es obligatorio.").MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(100);
        RuleFor(x => x.Address).MaximumLength(300);
    }
}

public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("El código es obligatorio.").MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Debe seleccionar una categoría.");
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0).WithMessage("El precio de compra no puede ser negativo.");
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0).WithMessage("El precio de venta no puede ser negativo.");
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(x => x.PurchasePrice)
            .When(x => x.SalePrice > 0)
            .WithMessage("El precio de venta no puede ser menor que el precio de compra.");
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo.");
        RuleFor(x => x.UnitOfMeasure).IsInEnum();
    }
}

public class MovementRequestValidator : AbstractValidator<MovementRequest>
{
    public MovementRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Debe seleccionar un producto.");
        RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("Debe seleccionar un almacén.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("El motivo es obligatorio.").MaximumLength(300);
        RuleFor(x => x.DocumentReference).MaximumLength(50);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("El usuario es obligatorio.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FullName).NotEmpty().WithMessage("El nombre completo es obligatorio.").MaximumLength(150);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("Debe contener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("Debe contener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("Debe contener al menos un número.");
        RuleFor(x => x.Role).Must(r => r is "Admin" or "Usuario" or "Auditor")
            .WithMessage("Rol inválido. Use Admin, Usuario o Auditor.");
    }
}
