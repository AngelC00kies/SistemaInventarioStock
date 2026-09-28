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

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.guest && auth.isAuthenticated) {
    return { name: 'dashboard' }
  }

  if (to.meta.adminOnly && !auth.isAdmin) {
    return { name: 'dashboard' }
  }
})

export default router
