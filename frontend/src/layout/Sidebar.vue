<template>
  <aside
    class="fixed inset-y-0 left-0 z-50 flex w-[248px] flex-col bg-ink-950 text-ink-300 transition-transform duration-200 lg:translate-x-0"
    :class="open ? 'translate-x-0' : '-translate-x-full'"
  >
    <div class="flex h-16 shrink-0 items-center gap-2.5 border-b border-white/5 px-5">
      <span class="flex h-8 w-8 items-center justify-center rounded-md bg-brand-600 text-white shadow-sm">
        <Boxes :size="18" :stroke-width="2" />
      </span>
      <div class="min-w-0">
        <p class="truncate text-sm font-semibold leading-tight text-white">Sistema de Inventario</p>
        <p class="text-[10px] uppercase tracking-widest text-ink-500">Gestión de almacenes</p>
      </div>

      <button
        class="ml-auto rounded p-1 text-ink-400 hover:bg-white/10 hover:text-white lg:hidden"
        @click="$emit('close')"
        aria-label="Cerrar menú"
      >
        <X :size="18" />
      </button>
    </div>

    <nav class="flex-1 overflow-y-auto px-3 py-4">
      <div v-for="group in groups" :key="group.label" class="mb-5">
        <p class="px-3 pb-2 text-[10px] font-semibold uppercase tracking-widest text-ink-600">
          {{ group.label }}
        </p>

        <router-link
          v-for="item in group.items"
          :key="item.to"
          :to="item.to"
          class="nav-item"
          :class="isActive(item) ? 'nav-item-active' : 'nav-item-idle'"
          @click="$emit('close')"
        >
          <component :is="item.icon" :size="17" :stroke-width="1.9" />
          <span>{{ item.label }}</span>
          <span
            v-if="item.badge"
            class="ml-auto rounded-full bg-rose-500/15 px-1.5 py-0.5 text-[10px] font-semibold text-rose-400"
          >
            {{ item.badge }}
          </span>
        </router-link>
      </div>
    </nav>

    <div class="border-t border-white/5 p-3">
      <div class="flex items-center gap-3 rounded-lg bg-white/[0.04] px-3 py-2.5">
        <span class="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-brand-600/20 text-xs font-bold text-brand-300 ring-1 ring-brand-500/30">
          {{ auth.initials }}
        </span>
        <div class="min-w-0 flex-1">
          <p class="truncate text-[13px] font-medium text-white">{{ auth.fullName }}</p>
          <p class="truncate text-[11px] text-ink-500">{{ auth.role }}</p>
        </div>
        <button
          class="rounded-md p-1.5 text-ink-500 transition hover:bg-white/10 hover:text-white"
          @click="logout"
          title="Cerrar sesión"
          aria-label="Cerrar sesión"
        >
          <LogOut :size="16" />
        </button>
      </div>
    </div>
  </aside>

  <transition name="backdrop">
    <div
      v-if="open"
      class="fixed inset-0 z-40 bg-ink-950/50 backdrop-blur-sm lg:hidden"
      @click="$emit('close')"
    ></div>
  </transition>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeftRight, Boxes, FileBarChart, LayoutDashboard, LogOut, Package, Tags, Users, Warehouse, X, Building2 } from '@lucide/vue'
import { useAuthStore } from '@/stores/auth'

defineProps({ open: { type: Boolean, default: false } })
defineEmits(['close'])

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const groups = computed(() => {
  const list = [
    {
      label: 'Resumen',
      items: [{ to: '/dashboard', label: 'Panel general', icon: LayoutDashboard }],
    },
    {
      label: 'Operación',
      items: [{ to: '/movements', label: 'Movimientos', icon: ArrowLeftRight }],
    },
    {
      label: 'Catálogos',
      items: [
        { to: '/products', label: 'Productos', icon: Package },
        { to: '/categories', label: 'Categorías', icon: Tags },
        { to: '/suppliers', label: 'Proveedores', icon: Building2 },
        { to: '/warehouses', label: 'Almacenes', icon: Warehouse },
      ],
    },
    {
      label: 'Análisis',
      items: [{ to: '/reports', label: 'Reportes', icon: FileBarChart }],
    },
  ]

  if (auth.isAdmin) {
    list.push({
      label: 'Sistema',
      items: [{ to: '/users', label: 'Usuarios', icon: Users }],
    })
  }

  return list
})

const isActive = (item) => route.path.startsWith(item.to)

function logout() {
  auth.logout()
  router.push({ name: 'login' })
}
</script>

<style scoped>
.nav-item {
  @apply mb-1 flex items-center gap-3 rounded-md px-3 py-2 text-[13px] font-medium
    transition-colors duration-150;
}
.nav-item-idle {
  @apply text-ink-400 hover:bg-white/[0.06] hover:text-white;
}
.nav-item-active {
  @apply bg-brand-600 text-white shadow-sm shadow-brand-900/40;
}
.backdrop-enter-active,
.backdrop-leave-active {
  transition: opacity 0.2s ease;
}
.backdrop-enter-from,
.backdrop-leave-to {
  opacity: 0;
}
</style>
