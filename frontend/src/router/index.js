import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { roleAllows } from '../config/roles'
import { screens } from '../config/navigation'

/**
 * Las rutas se derivan del registro único de pantallas (config/navigation.js).
 * No se declaran rutas sueltas: para añadir una pantalla se edita navigation.js.
 */
const toRoute = (screen) => ({
  path: screen.layout === 'bare' ? screen.path : screen.path.replace(/^\//, ''),
  name: screen.name,
  component: screen.component,
  meta: {
    public: !!screen.public,
    roles: screen.roles,
    title: screen.title,
    fallback: screen.fallback || '/dashboard',
  },
})

const routes = [
  ...screens.filter((s) => s.layout === 'bare').map(toRoute),
  {
    path: '/',
    component: () => import('../layouts/MainLayout.vue'),
    children: screens.filter((s) => s.layout !== 'bare').map(toRoute),
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.public) {
    if (auth.isAuthenticated && to.name === 'login') return '/dashboard'
    return true
  }

  if (!auth.isAuthenticated) return '/login'

  // Permiso declarado en navigation.js (mismo criterio que el menú lateral)
  if (!roleAllows(auth.role, to.meta.roles)) return to.meta.fallback

  return true
})

export default router
