using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Tests.Infrastructure;

/// <summary>Proveedor de datos que usa cada base de prueba.</summary>
public enum TestProvider
{
    /// <summary>
    /// SQLite en memoria: un proveedor relacional de verdad. Valida que las consultas LINQ se
    /// traduzcan a SQL y soporta las transacciones que usa <c>MovementService</c>.
    /// </summary>
    Sqlite,

    /// <summary>
    /// Proveedor in-memory de EF Core. Solo lo necesita <c>DashboardService</c>: SQLite no sabe
    /// aplicar <c>SUM</c> a una columna decimal ("SQLite cannot apply aggregate operator 'Sum'
    /// on expressions of type 'decimal'"), y ese servicio agrega importes en dinero.
    /// Al ser de solo lectura no echa en falta la transaccionalidad que este proveedor no ofrece.
    /// </summary>
    InMemory
}

/// <summary>
/// Base de datos de prueba: una por cada test, con el esquema creado desde el modelo EF,
/// sin tocar migraciones ni el SQL Server de desarrollo.
///
/// Por defecto es SQLite en memoria; la conexión se mantiene abierta a propósito porque
/// SQLite borra la base al cerrar la última conexión.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly TestProvider _provider;
    private readonly SqliteConnection? _connection;
    private readonly InMemoryDatabaseRoot? _root;
    private readonly string _nombre = Guid.NewGuid().ToString();

    public AppDbContext Db { get; }

    public TestDatabase(TestProvider provider = TestProvider.Sqlite)
    {
        _provider = provider;

        if (provider == TestProvider.Sqlite)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }
        else
        {
            // Raíz propia: así esta base no comparte datos con ninguna otra del mismo test run.
            _root = new InMemoryDatabaseRoot();
        }

        Db = NewContext();
        Db.Database.EnsureCreated();
    }

    /// <summary>Abre un contexto nuevo sobre la misma base, para simular un pedido HTTP distinto.</summary>
    public AppDbContext NewContext() => _provider switch
    {
        // El operador acota el nullable: el proveedor Sqlite siempre abre su conexión en el ctor.
        TestProvider.Sqlite => new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection!).Options),
        _ => new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_nombre, _root).Options)
    };

    public void Dispose()
    {
        Db.Dispose();
        _connection?.Dispose();
    }
}
