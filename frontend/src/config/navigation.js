/**
 * Registro único de pantallas.
 *
 * Cada pantalla se declara UNA sola vez con todo lo que necesita:
 *   - ruta y componente  → router/index.js construye las rutas a partir de aquí
 *   - roles              → el guard del router aplica el permiso
 *   - section/icon/label → MainLayout.vue dibuja el menú lateral
 *   - title              → el topbar muestra el título
 *
 * Para crear una pantalla nueva: añade una entrada aquí. No hay que tocar
 * ni el router ni el layout (Principio Abierto/Cerrado).
 *
 * Campos opcionales:
 *   - section : si existe, el ítem aparece en el menú lateral bajo esa sección
 *   - badge   : 'alerts' muestra el contador de alertas pendientes
 *   - fallback: a dónde redirigir si el rol no tiene acceso
 *   - public  : pantalla accesible sin sesión
 */
import { EVERYONE, WRITERS, ADMINS } from './roles.js'

export const screens = [
  // ── Públicas ──
  {
    path: '/login',
    name: 'login',
    layout: 'bare',
    public: true,
    component: () => import('../pages/Login.vue'),
  },

  // ── General ──
  {
    path: '/dashboard',
    name: 'dashboard',
    layout: 'app',
    section: 'General',
    icon: '📊',
    label: 'Dashboard',
    title: 'Dashboard',
    roles: EVERYONE,
    component: () => import('../pages/Dashboard.vue'),
  },
  {
    path: '/movements',
    name: 'movements',
    layout: 'app',
    section: 'General',
    icon: '🔄',
    label: 'Movimientos',
    title: 'Movimientos',
    roles: EVERYONE,
    component: () => import('../pages/Movements.vue'),
  },
  {
    path: '/movements/new',
    name: 'movement-new',
    layout: 'app',
    title: 'Registrar movimiento',
    roles: WRITERS,
    fallback: '/movements',
    component: () => import('../pages/MovementForm.vue'),
  },
  {
    path: '/notifications',
    name: 'notifications',
    layout: 'app',
    section: 'General',
    icon: '🔔',
    label: 'Alertas',
    title: 'Alertas de stock',
    badge: 'alerts',
    roles: EVERYONE,
    component: () => import('../pages/Notifications.vue'),
  },

  // ── Inventario ──
  {
    path: '/products',
    name: 'products',
    layout: 'app',
    section: 'Inventario',
    icon: '🏷️',
    label: 'Productos',
    title: 'Productos',
    roles: EVERYONE,
    component: () => import('../pages/Products.vue'),
  },
  {
    path: '/products/new',
    name: 'product-new',
    layout: 'app',
    title: 'Nuevo producto',
    roles: WRITERS,
    fallback: '/products',
    component: () => import('../pages/ProductForm.vue'),
  },
  {
    path: '/products/:id',
    name: 'product-edit',
    layout: 'app',
    title: 'Editar producto',
    roles: WRITERS,
    fallback: '/products',
    component: () => import('../pages/ProductForm.vue'),
  },
  {
    path: '/categories',
    name: 'categories',
    layout: 'app',
    section: 'Inventario',
    icon: '📁',
    label: 'Categorías',
    title: 'Categorías',
    roles: EVERYONE,
    component: () => import('../pages/Categories.vue'),
  },
  {
    path: '/suppliers',
    name: 'suppliers',
    layout: 'app',
    section: 'Inventario',
    icon: '🚚',
    label: 'Proveedores',
    title: 'Proveedores',
    roles: EVERYONE,
    component: () => import('../pages/Suppliers.vue'),
  },
  {
    path: '/warehouses',
    name: 'warehouses',
    layout: 'app',
    section: 'Inventario',
    icon: '🏬',
    label: 'Almacenes',
    title: 'Almacenes',
    roles: EVERYONE,
    component: () => import('../pages/Warehouses.vue'),
  },

  // ── Reportes ──
  {
    path: '/reports/stock',
    name: 'report-stock',
    layout: 'app',
    section: 'Reportes',
    icon: '📉',
    label: 'Stock actual',
    title: 'Reporte de stock',
    roles: EVERYONE,
    component: () => import('../pages/ReportStock.vue'),
  },
  {
    path: '/reports/movements',
    name: 'report-movements',
    layout: 'app',
    section: 'Reportes',
    icon: '📑',
    label: 'Movimientos',
    title: 'Reporte de movimientos',
    roles: EVERYONE,
    component: () => import('../pages/ReportMovements.vue'),
  },

  // ── Administración ──
  {
    path: '/users',
    name: 'users',
    layout: 'app',
    section: 'Administración',
    icon: '👥',
    label: 'Usuarios',
    title: 'Usuarios',
    roles: ADMINS,
    component: () => import('../pages/Users.vue'),
  },
]

/**
 * Menú lateral agrupado por secciones, en orden de aparición y
 * filtrado por los roles del usuario autenticado.
 */
export function buildMenu(screens, role, roleAllows) {
  const sections = []

  for (const screen of screens) {
    if (!screen.section || !roleAllows(role, screen.roles)) continue

    let section = sections.find((s) => s.name === screen.section)
    if (!section) sections.push((section = { name: screen.section, items: [] }))
    section.items.push(screen)
  }

  return sections
}
