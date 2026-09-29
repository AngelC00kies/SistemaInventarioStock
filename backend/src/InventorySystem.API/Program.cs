using InventorySystem.API.Extensions;
using InventorySystem.Application;
using InventorySystem.Infrastructure;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Licencia QuestPDF (comunidad, uso gratuito) ──
QuestPDF.Settings.License = LicenseType.Community;

// ── Capas de la aplicación + servicios de la API ──
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// ── Base de datos: migraciones + seed ──
await app.InitializeDatabaseAsync();

// ── Pipeline HTTP ──
app.UseInventoryPipeline();

app.Run();
