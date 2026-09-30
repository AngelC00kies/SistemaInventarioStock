// Router con historial HTML5 y cargas perezosas: cada página se importa solo al navegar hacia ella.
// El control de acceso vive en `beforeEach` y se apoya en los getters del store de auth.
// Toda la sección autenticada cuelga de '/' bajo AppLayout; el `meta` de cada ruta alimenta a la vez
// a los guards (requiresAuth / guest / adminOnly) y a la cabecera (section / title) del Topbar.
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/pages/Login.vue'),
    meta: { guest: true, layout: 'blank' },
  },
  {
    path: '/',
    component: () => import('@/layout/AppLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: '/dashboard' },
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('@/pages/Dashboard.vue'),
        meta: { title: 'Panel general', section: 'Resumen' },
      },
      {
        path: 'movements',
        name: 'movements',
        component: () => import('@/pages/Movements.vue'),
        meta: { title: 'Movimientos', section: 'Operación' },
      },
      {
        path: 'products',
        name: 'products',
        component: () => import('@/pages/Products.vue'),
        meta: { title: 'Productos', section: 'Catálogos' },
      },
      {
        path: 'categories',
        name: 'categories',
        component: () => import('@/pages/Categories.vue'),
        meta: { title: 'Categorías', section: 'Catálogos' },
      },
      {
        path: 'suppliers',
        name: 'suppliers',
        component: () => import('@/pages/Suppliers.vue'),
        meta: { title: 'Proveedores', section: 'Catálogos' },
      },
      {
        path: 'warehouses',
        name: 'warehouses',
        component: () => import('@/pages/Warehouses.vue'),
        meta: { title: 'Almacenes', section: 'Catálogos' },
      },
      {
        path: 'reports',
        name: 'reports',
        component: () => import('@/pages/Reports.vue'),
        meta: { title: 'Reportes', section: 'Análisis' },
      },
      {
        path: 'users',
        name: 'users',
        component: () => import('@/pages/Users.vue'),
        // Única ruta reservada a Admin: el sidebar ya la oculta y este meta bloquea el acceso directo por URL.
        meta: { title: 'Usuarios', section: 'Sistema', adminOnly: true },
      },
    ],
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/pages/NotFound.vue'),
    meta: { layout: 'blank' },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 }),
})

// Guard único de navegación: sesión y rol se comprueban en cada salto, no solo al entrar en la app.
router.beforeEach((to) => {
  const auth = useAuthStore()

  // Ruta protegida sin sesión -> login, recordando a dónde iba el usuario para volver tras autenticarse.
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  // Sesión ya iniciada en /login -> panel: el formulario no debe estorbar a un usuario autenticado.
  if (to.meta.guest && auth.isAuthenticated) {
    return { name: 'dashboard' }
  }

  // Autorización por rol: se redirige al panel (en vez de un 403) y la ruta restringida queda fuera de la URL.
  if (to.meta.adminOnly && !auth.isAdmin) {
    return { name: 'dashboard' }
  }
})

export default router
