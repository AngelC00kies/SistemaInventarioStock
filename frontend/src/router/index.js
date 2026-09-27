import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes = [
  { path: '/login', name: 'login', component: () => import('../pages/Login.vue'), meta: { public: true } },
  {
    path: '/',
    component: () => import('../layouts/MainLayout.vue'),
    children: [
      { path: '', redirect: '/dashboard' },
      { path: 'dashboard', component: () => import('../pages/Dashboard.vue') },
      { path: 'products', component: () => import('../pages/Products.vue') },
      { path: 'products/new', component: () => import('../pages/ProductForm.vue'), meta: { edit: true } },
      { path: 'products/:id', component: () => import('../pages/ProductForm.vue'), meta: { edit: true } },
      { path: 'movements', component: () => import('../pages/Movements.vue') },
      { path: 'movements/new', component: () => import('../pages/MovementForm.vue'), meta: { edit: true } },
      { path: 'notifications', component: () => import('../pages/Notifications.vue') },
      { path: 'categories', component: () => import('../pages/Categories.vue') },
      { path: 'suppliers', component: () => import('../pages/Suppliers.vue') },
      { path: 'warehouses', component: () => import('../pages/Warehouses.vue') },
      { path: 'reports/stock', component: () => import('../pages/ReportStock.vue') },
      { path: 'reports/movements', component: () => import('../pages/ReportMovements.vue') },
      { path: 'users', component: () => import('../pages/Users.vue'), meta: { admin: true } },
    ],
  },
  { path: '/:pathMatch(.*)*', redirect: '/dashboard' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  // Sesión
  if (to.meta.public) {
    if (auth.isAuthenticated) return '/dashboard'
    return true
  }
  if (!auth.isAuthenticated) return '/login'

  // Permisos por rol
  if (to.meta.admin && !auth.isAdmin) return '/dashboard'
  if (to.meta.edit && !auth.canEdit)
    return to.path.startsWith('/products') ? '/products' : '/movements'

  return true
})

export default router
