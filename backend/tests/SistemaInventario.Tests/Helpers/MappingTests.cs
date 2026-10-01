using SistemaInventario.Core.Entities;
using SistemaInventario.Infrastructure.Data;
using Xunit;

namespace SistemaInventario.Tests.Helpers;

/// <summary>Pruebas de <see cref="Mapping"/>: estado derivado del stock y conversión entidad → DTO.</summary>
public class MappingTests
{
    [Theory]
    [InlineData(0, 5, "empty")]
    [InlineData(-3, 5, "empty")]
    [InlineData(6, 5, "ok")]
    [InlineData(5, 5, "low")]
    [InlineData(3, 5, "low")]
    [InlineData(2, 5, "critical")]
    [InlineData(1, 5, "critical")]
    public void ProductStatus_DerivaElEstadoDelStockTotal(int total, int minimo, string esperado)
    {
        Assert.Equal(esperado, Mapping.ProductStatus(total, minimo));
    }

    [Fact]
    public void ProductStatus_ConMinimoCero_LaMitadEsUnoYPorEsoNoHayUmbralNulo()
    {
        // minStock = 0 no debería dar "critical" con 1 unidad: el Math.Max(1, …) evita ese umbral.
        Assert.Equal("ok", Mapping.ProductStatus(1, 0));
        Assert.Equal("empty", Mapping.ProductStatus(0, 0));
    }

    [Fact]
    public void ProductStatus_ConMinimoUno_LaUnidadQueLlegaAlMinimoEsCritica()
    {
        // 1 <= max(1, 1/2 = 0) → 1 <= 1, así que al mínimo con minStock=1 el estado es crítico.
        Assert.Equal("critical", Mapping.ProductStatus(1, 1));
        Assert.Equal("ok", Mapping.ProductStatus(2, 1));
    }

    [Fact]
    public void ToDto_Producto_SumaLosAlmacenesYTraduceElEstado()
    {
        var categoria = new Categoria { Nombre = "Electrónica" };
        var producto = new Producto
        {
            Id = 7,
            Codigo = "P-007",
            Nombre = "Cable HDMI",
            Categoria = categoria,
            CategoriaId = 1,
            StockMinimo = 10,
            NivelesStock = new List<NivelStock>
            {
                // 4 + 1 = 5, justo el mínimo: el estado pasa a "critical" al caer a la mitad.
                new() { AlmacenId = 1, Cantidad = 4 },
                new() { AlmacenId = 2, Cantidad = 1 }
            }
        };

        var dto = Mapping.ToDto(producto);

        Assert.Equal(5, dto.TotalStock);
        Assert.Equal("critical", dto.Status);
        Assert.Equal("Electrónica", dto.CategoryName);
        Assert.Equal(2, dto.StockByWarehouse.Count);
    }

    [Fact]
    public void ToDto_Producto_ConAlmacenConcreto_FiltraElDesglosePeroNoElTotal()
    {
        var producto = new Producto
        {
            Codigo = "P-008",
            Nombre = "Teclado",
            Categoria = new Categoria { Nombre = "Periféricos" },
            CategoriaId = 1,
            StockMinimo = 5,
            NivelesStock = new List<NivelStock>
            {
                new() { AlmacenId = 1, Cantidad = 2 },
                new() { AlmacenId = 2, Cantidad = 100 }
            }
        };

        var dto = Mapping.ToDto(producto, warehouseId: 1);

        // El desglose se queda con el almacén pedido…
        Assert.Single(dto.StockByWarehouse);
        Assert.Equal(1, dto.StockByWarehouse[0].WarehouseId);
        // …pero TotalStock y Status siguen mirando la red completa (2 + 100 > 5).
        Assert.Equal(102, dto.TotalStock);
        Assert.Equal("ok", dto.Status);
    }

    [Fact]
    public void ToDto_Categoria_CuentaSoloLosProductosActivos()
    {
        var categoria = new Categoria
        {
            Nombre = "Oficina",
            Productos = new List<Producto>
            {
                new() { Activo = true },
                new() { Activo = true },
                new() { Activo = false }
            }
        };

        Assert.Equal(2, Mapping.ToDto(categoria).ProductCount);
    }

    [Fact]
    public void ToDto_Almacen_CuentaProductosConExistenciasYSumaUnidades()
    {
        var almacen = new Almacen
        {
            Codigo = "ALM-01",
            Nombre = "Principal",
            NivelesStock = new List<NivelStock>
            {
                new() { Cantidad = 0 },
                new() { Cantidad = 5 },
                new() { Cantidad = 12 }
            }
        };

        var dto = Mapping.ToDto(almacen);

        // Los niveles con 0 unidades no cuentan como producto guardado ahí.
        Assert.Equal(2, dto.ProductCount);
        Assert.Equal(17, dto.TotalUnits);
    }

    [Fact]
    public void ToDto_Movimiento_CopiaLosCamposResueltos()
    {
        var movimiento = new Movimiento
        {
            Id = 3,
            Fecha = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            Tipo = Core.Enums.MovementType.Salida,
            Motivo = "Venta mostrador",
            Cantidad = 2,
            ProductoId = 9,
            AlmacenId = 4,
            UsuarioId = 5,
            DocumentoReferencia = "FAC-001",
            StockResultante = 8,
            PrecioUnitario = 1500m,
            Producto = new Producto { Codigo = "P-009", Nombre = "Ratón" },
            Almacen = new Almacen { Codigo = "ALM-01", Nombre = "Principal" },
            Usuario = new Usuario { NombreCompleto = "Ana Pérez" }
        };

        var dto = Mapping.ToDto(movimiento);

        Assert.Equal("Salida", dto.Type.ToString());
        Assert.Equal("P-009", dto.ProductCode);
        Assert.Equal("Principal", dto.WarehouseName);
        Assert.Equal("Ana Pérez", dto.UserName);
        Assert.Equal(8, dto.StockAfter);
        // Total es calculado en memoria: 2 × 1500.
        Assert.Equal(3000m, dto.Total);
    }
}
