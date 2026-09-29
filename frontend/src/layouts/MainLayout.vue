<template>
  <div class="layout">
    <aside class="sidebar">
      <div class="sidebar-brand">
        <span class="logo">📦</span>
        <span class="txt">Inventario</span>
      </div>

      <nav class="nav">
        <template v-for="section in menuSections" :key="section.name">
          <div class="nav-section">{{ section.name }}</div>
          <router-link v-for="item in section.items" :key="item.path" :to="item.path">
            <span class="icon">{{ item.icon }}</span><span class="txt">{{ item.label }}</span>
            <span
              v-if="item.badge === 'alerts' && ui.pendingCount > 0"
              class="badge"
              style="margin-left:auto"
            >
              {{ ui.pendingCount }}
            </span>
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
import { screens, buildMenu } from '../config/navigation'
import { roleAllows } from '../config/roles'

const auth = useAuthStore()
const ui = useUiStore()
const route = useRoute()
const router = useRouter()

// Título del topbar y menú lateral: derivados del registro de pantallas,
// de modo que una pantalla nueva aparece sola, sin tocar este componente.
const pageTitle = computed(() => route.meta.title || 'Inventario')
const menuSections = computed(() => buildMenu(screens, auth.role, roleAllows))

const initials = computed(() => {
  const name = auth.user?.fullName || auth.user?.userName || '?'
  return name
    .split(' ')
    .map((w) => w[0])
    .slice(0, 2)
    .join('')
    .toUpperCase()
})

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
