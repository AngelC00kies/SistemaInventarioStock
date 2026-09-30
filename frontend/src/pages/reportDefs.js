/**
 * Definiciones de los reportes de la vista "Reportes" (vista previa en tabla + exportación PDF/Excel).
 * Cada clave de `reportDefs` describe un reporte con:
 *   - title / description / icon: metadatos que se muestran en la tarjeta de selección.
 *   - filters: códigos de filtros que la vista renderiza y reenvía a `load()`
 *     ('warehouse', 'category', 'supplier', 'type', 'from', 'to', 'search'); [] = sin filtros.
 *   - adminOnly: si está, el reporte solo se lista para el rol Admin.
 *   - load(f): llama al endpoint indicado en cada caso y devuelve { columns, rows, summary },
 *     donde cada celda es { text, cls?, badge? } (badge la pinta con el componente de estado).
 */
import { getCategories, getSuppliers, getWarehouses } from '@/api/catalogs'
import { getProducts } from '@/api/products'
import { getMovements, getUsers } from '@/api/operations'

// Formateadores de presentación con locale es-CL: moneda CLP sin decimales, número y fecha/hora.
const money = (v) =>
  new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP', maximumFractionDigits: 0 }).format(v || 0)

const num = (v) => new Intl.NumberFormat('es-CL').format(Number(v) || 0)

const date = (v) =>
  new Date(v).toLocaleString('es-CL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })

/** Define cada reporte disponible: metadatos, filtros y cómo construir la vista previa. */
export const reportDefs = {
  stock: {
    title: 'Stock actual',
    description: 'Existencias por producto y almacén con valorización.',
    icon: 'Boxes',
    filters: ['warehouse', 'category', 'search'],
    // GET /api/products paginado a 200; la valorización multiplica stock total por precio de venta.
    async load(f) {
      const data = await getProducts({
        warehouseId: f.warehouseId ?? undefined,
        categoryId: f.categoryId ?? undefined,
        search: f.search || undefined,
        pageSize: 200,
      })
      return {
        columns: ['Código', 'Producto', 'Categoría', 'Stock', 'Mínimo', 'Valor stock', 'Estado'],
        rows: data.items.map((p) => [
          { text: p.code, cls: 'font-mono text-[12.5px] text-ink-500' },
          { text: p.name, cls: 'font-medium text-ink-800' },
          { text: p.categoryName },
          { text: num(p.totalStock), cls: 'text-right font-semibold tabular-nums' },
          { text: num(p.minStock), cls: 'text-right text-ink-500' },
          { text: money(p.totalStock * p.salePrice), cls: 'text-right tabular-nums' },
          { text: p.status, badge: true },
        ]),
        summary: `${data.total} SKU · ${money(data.items.reduce((a, p) => a + p.totalStock * p.salePrice, 0))} en stock`,
      }
    },
  },

  'critical-stock': {
    title: 'Stock crítico',
    description: 'Productos que igualan o bajan del stock mínimo configurado.',
    icon: 'TriangleAlert',
    filters: ['warehouse', 'search'],
    // onlyLowStock: true lo resuelve el backend (stock <= mínimo): sin ese prefiltro habría que
    // descargar el catálogo completo y descartar en cliente casi todos los productos.
    async load(f) {
      const data = await getProducts({
        warehouseId: f.warehouseId ?? undefined,
        search: f.search || undefined,
        onlyLowStock: true,
        pageSize: 200,
      })
      return {
        columns: ['Código', 'Producto', 'Categoría', 'Stock', 'Mínimo', 'Faltante', 'Estado'],
        rows: data.items.map((p) => [
          { text: p.code, cls: 'font-mono text-[12.5px] text-ink-500' },
          { text: p.name, cls: 'font-medium text-ink-800' },
          { text: p.categoryName },
          { text: num(p.totalStock), cls: 'text-right font-semibold tabular-nums text-rose-600' },
          { text: num(p.minStock), cls: 'text-right text-ink-500' },
          { text: num(Math.max(0, p.minStock - p.totalStock)), cls: 'text-right tabular-nums text-ink-700' },
          { text: p.status, badge: true },
        ]),
        summary: `${data.total} productos requieren reposición`,
      }
    },
  },

  movements: {
    title: 'Movimientos',
    description: 'Historial de entradas y salidas filtrable por fecha, usuario y tipo.',
    icon: 'ArrowLeftRight',
    filters: ['warehouse', 'type', 'from', 'to', 'search'],
    // GET /api/movements: entradas y salidas se separan solo para agregar el resumen final.
    async load(f) {
      const data = await getMovements({
        warehouseId: f.warehouseId ?? undefined,
        type: f.type || undefined,
        from: f.from || undefined,
        to: f.to || undefined,
        search: f.search || undefined,
        pageSize: 200,
      })
      const entries = data.items.filter((m) => m.type === 'Entrada')
      const exits = data.items.filter((m) => m.type === 'Salida')
      return {
        columns: ['Fecha', 'Tipo', 'Producto', 'Almacén', 'Cantidad', 'Total', 'Usuario', 'Documento'],
        rows: data.items.map((m) => [
          { text: date(m.date), cls: 'whitespace-nowrap text-[13px] text-ink-500' },
          { text: m.type, badge: true },
          { text: m.productName, cls: 'font-medium text-ink-800' },
          { text: m.warehouseName },
          {
            text: `${m.type === 'Entrada' ? '+' : '−'}${num(m.quantity)}`,
            cls: `text-right font-semibold tabular-nums ${m.type === 'Entrada' ? 'text-emerald-600' : 'text-rose-600'}`,
          },
          { text: money(m.total), cls: 'text-right tabular-nums text-ink-600' },
          { text: m.userName, cls: 'text-ink-500' },
          { text: m.documentReference || '—', cls: 'text-[13px] text-brand-600' },
        ]),
        summary: `${entries.reduce((a, m) => a + m.quantity, 0)} unidades ingresadas · ${exits.reduce((a, m) => a + m.quantity, 0)} retiradas`,
      }
    },
  },

  products: {
    title: 'Catálogo de productos',
    description: 'Listado completo con precios, categorías y proveedores.',
    icon: 'Package',
    filters: ['category', 'supplier', 'search'],
    // GET /api/products con includeInactive: true para mostrar también los productos dados de baja.
    async load(f) {
      const data = await getProducts({
        categoryId: f.categoryId ?? undefined,
        supplierId: f.supplierId ?? undefined,
        search: f.search || undefined,
        pageSize: 200,
        includeInactive: true,
      })
      return {
        columns: ['Código', 'Producto', 'Categoría', 'Proveedor', 'P. compra', 'P. venta', 'Estado'],
        rows: data.items.map((p) => [
          { text: p.code, cls: 'font-mono text-[12.5px] text-ink-500' },
          { text: p.name, cls: 'font-medium text-ink-800' },
          { text: p.categoryName },
          { text: p.supplierName || '—', cls: 'text-ink-500' },
          { text: money(p.purchasePrice), cls: 'text-right tabular-nums' },
          { text: money(p.salePrice), cls: 'text-right tabular-nums font-medium' },
          { text: p.isActive ? p.status : 'inactive', badge: true },
        ]),
        summary: `${data.total} productos en el catálogo`,
      }
    },
  },

  categories: {
    title: 'Categorías',
    description: 'Categorías de productos y cantidad asociada.',
    icon: 'Tags',
    filters: [],
    // GET /api/categories: array plano sin paginación, por eso aquí no se accede a data.items.
    async load() {
      const data = await getCategories({ includeInactive: true })
      return {
        columns: ['Categoría', 'Descripción', 'Productos activos', 'Estado'],
        rows: data.map((c) => [
          { text: c.name, cls: 'font-medium text-ink-800' },
          { text: c.description || '—', cls: 'text-ink-500' },
          { text: num(c.productCount), cls: 'text-right tabular-nums' },
          { text: c.isActive ? 'active' : 'inactive', badge: true },
        ]),
        summary: `${data.length} categorías registradas`,
      }
    },
  },

  suppliers: {
    title: 'Proveedores',
    description: 'Directorio de proveedores y sus datos de contacto.',
    icon: 'Building2',
    filters: ['search'],
    // GET /api/suppliers con búsqueda server-side; incluye inactivos (este reporte no expone ese filtro).
    async load(f) {
      const data = await getSuppliers({ includeInactive: true, search: f.search || undefined })
      return {
        columns: ['Proveedor', 'Contacto', 'Teléfono', 'Correo', 'Productos', 'Estado'],
        rows: data.map((s) => [
          { text: s.name, cls: 'font-medium text-ink-800' },
          { text: s.contactName || '—' },
          { text: s.phone || '—', cls: 'whitespace-nowrap' },
          { text: s.email || '—', cls: 'text-brand-600' },
          { text: num(s.productCount), cls: 'text-right tabular-nums' },
          { text: s.isActive ? 'active' : 'inactive', badge: true },
        ]),
        summary: `${data.length} proveedores registrados`,
      }
    },
  },

  users: {
    title: 'Usuarios',
    description: 'Cuentas de acceso, roles y estado (solo administradores).',
    icon: 'Users',
    adminOnly: true,
    filters: [],
    // adminOnly lo oculta en Reports.vue a los no administradores; el endpoint /users también rechaza el rol.
    async load() {
      const data = await getUsers({ pageSize: 200 })
      return {
        columns: ['Usuario', 'Nombre', 'Correo', 'Rol', 'Estado', 'Creado'],
        rows: data.items.map((u) => [
          { text: u.username, cls: 'font-medium text-ink-800' },
          { text: u.fullName },
          { text: u.email || '—', cls: 'text-ink-500' },
          { text: u.role, cls: 'text-ink-600' },
          { text: u.isActive ? 'active' : 'inactive', badge: true },
          { text: new Date(u.createdAt).toLocaleDateString('es-CL'), cls: 'text-ink-500' },
        ]),
        summary: `${data.total} usuarios con acceso al sistema`,
      }
    },
  },
}

// Catálogos base en paralelo (son independientes entre sí); se cargan una sola vez al montar
// Reports.vue para rellenar los <select> de filtros sin repetir la petición en cada reporte.
export async function loadCatalogOptions() {
  const [warehouses, categories, suppliers] = await Promise.all([
    getWarehouses(),
    getCategories(),
    getSuppliers(),
  ])
  return { warehouses, categories, suppliers }
}
