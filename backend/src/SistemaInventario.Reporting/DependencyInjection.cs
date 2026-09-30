using Microsoft.Extensions.DependencyInjection;
using SistemaInventario.Core.Interfaces;

namespace SistemaInventario.Reporting;

/// <summary>Registra el generador de reportes y fija la licencia de QuestPDF usada para el PDF.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddReporting(this IServiceCollection services)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        services.AddScoped<IReportService, ReportService>();
        return services;
    }
}
