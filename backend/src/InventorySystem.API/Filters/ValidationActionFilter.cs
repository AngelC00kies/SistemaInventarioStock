using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InventorySystem.API.Filters;

/// <summary>
/// Ejecuta automáticamente los validadores FluentValidation registrados en el contenedor
/// para cada argumento de modelo de la petición. Las fallas se envían al middleware global,
/// que responde HTTP 400 con el detalle de errores por campo.
/// </summary>
public class ValidationActionFilter : IAsyncActionFilter
{
    private static readonly MethodInfo ValidateCoreMethod =
        typeof(ValidationActionFilter)
            .GetMethod(nameof(ValidateCore), BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly IServiceProvider _services;

    public ValidationActionFilter(IServiceProvider services) => _services = services;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var type = argument.GetType();
            if (type.IsPrimitive || type == typeof(string)) continue;

            var task = (Task)ValidateCoreMethod
                .MakeGenericMethod(type)
                .Invoke(null, new object[] { _services, argument })!;

            await task;
        }

        await next();
    }

    private static async Task ValidateCore<T>(IServiceProvider services, object instance)
    {
        var validator = services.GetService<IValidator<T>>();
        if (validator is null) return;

        var result = await validator.ValidateAsync((T)instance);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);
    }
}
