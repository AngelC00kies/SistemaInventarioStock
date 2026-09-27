<template>
  <div class="layout">
    <aside class="sidebar">
      <div class="sidebar-brand">
        <span class="logo">📦</span>
        <span class="txt">Inventario</span>
      </div>

      <nav class="nav">
        <div class="nav-section">General</div>
        <router-link to="/dashboard">
          <span class="icon">📊</span><span class="txt">Dashboard</span>
        </router-link>
        <router-link to="/movements">
          <span class="icon">🔄</span><span class="txt">Movimientos</span>
        </router-link>
        <router-link to="/notifications">
          <span class="icon">🔔</span><span class="txt">Alertas</span>
          <span v-if="ui.pendingCount > 0" class="badge" style="margin-left:auto">
            {{ ui.pendingCount }}
          </span>
        </router-link>

        <div class="nav-section">Inventario</div>
        <router-link to="/products">
          <span class="icon">🏷️</span><span class="txt">Productos</span>
        </router-link>
        <router-link to="/categories">
          <span class="icon">📁</span><span class="txt">Categorías</span>
        </router-link>
        <router-link to="/suppliers">
          <span class="icon">🚚</span><span class="txt">Proveedores</span>
        </router-link>
        <router-link to="/warehouses">
          <span class="icon">🏬</span><span class="txt">Almacenes</span>
        </router-link>

        <div class="nav-section">Reportes</div>
        <router-link to="/reports/stock">
          <span class="icon">📉</span><span class="txt">Stock actual</span>
        </router-link>
        <router-link to="/reports/movements">
          <span class="icon">📑</span><span class="txt">Movimientos</span>
        </router-link>

        <template v-if="auth.isAdmin">
          <div class="nav-section">Administración</div>
          <router-link to="/users">
            <span class="icon">👥</span><span class="txt">Usuarios</span>
          </router-link>
        </template>
      </nav>

      <div class="sidebar-footer">
        <div class="user">
          <span class="avatar">{{ initials }}</span>
          <div>
            <div style="color:#fff;font-weight:600">{{ auth.user?.fullName || auth.user?.userName }}</div>
            <div style="font-size:12px;color:#64748b">{{ auth.user?.role }}</div>
          </div>
        </div>
        <button class="btn secondary sm" style="width:100%" @click="onLogout">
          ⏻ <span class="logout-txt">Cerrar sesión</span>
        </button>
      </div>
    </aside>

    <div class="main">
      <header class="topbar">
        <h1>{{ pageTitle }}</h1>
        <div class="row">
          <router-link to="/notifications" class="btn ghost sm">
            🔔
            <span v-if="ui.pendingCount > 0" class="badge">{{ ui.pendingCount }}</span>
          </router-link>
        </div>
      </header>

      <main class="content">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useUiStore } from '../stores/ui'

const auth = useAuthStore()
const ui = useUiStore()
const route = useRoute()
const router = useRouter()

const pageTitle = computed(() => route.meta.title || titleFromPath())
const initials = computed(() => {
  const name = auth.user?.fullName || auth.user?.userName || '?'
  return name
    .split(' ')
    .map((w) => w[0])
    .slice(0, 2)
    .join('')
    .toUpperCase()
})

function titleFromPath() {
  const map = {
    '/dashboard': 'Dashboard',
    '/products': 'Productos',
    '/movements': 'Movimientos',
    '/categories': 'Categorías',
    '/suppliers': 'Proveedores',
    '/warehouses': 'Almacenes',
    '/notifications': 'Alertas de stock',
    '/reports/stock': 'Reporte de stock',
    '/reports/movements': 'Reporte de movimientos',
    '/users': 'Usuarios',
  }

  if (route.path.startsWith('/products/new')) return 'Nuevo producto'
  if (route.path.match(/^\/products\/\d+$/)) return 'Editar producto'
  if (route.path.startsWith('/movements/new')) return 'Registrar movimiento'

  return map[route.path] || 'Inventario'
}

async function onLogout() {
  await auth.logout()
  router.push('/login')
}

onMounted(() => {
  ui.fetchPendingCount()
  // Refresca el contador de alertas periódicamente
  setInterval(() => {
    if (auth.isAuthenticated) ui.fetchPendingCount()
  }, 60000)
})

watch(
  () => route.fullPath,
  () => ui.fetchPendingCount()
)
</script>
