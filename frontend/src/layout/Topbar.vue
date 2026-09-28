<template>
  <header
    class="sticky top-0 z-30 border-b border-ink-200 bg-white/90 backdrop-blur supports-[backdrop-filter]:bg-white/75"
  >
    <div class="flex h-16 items-center gap-3 px-4 sm:px-6 lg:px-8">
      <button
        class="rounded-md p-2 text-ink-500 transition hover:bg-ink-100 hover:text-ink-800 lg:hidden"
        @click="$emit('toggle-menu')"
        aria-label="Abrir menú"
      >
        <Menu :size="20" />
      </button>

      <div class="hidden min-w-0 md:block">
        <p class="text-[11px] font-medium uppercase tracking-wider text-ink-400">
          {{ route.meta.section || 'Sistema' }}
        </p>
        <p class="truncate text-sm font-semibold text-ink-800">{{ route.meta.title || 'Panel' }}</p>
      </div>

      <div class="relative ml-auto hidden w-full max-w-xs sm:block">
        <Search :size="15" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
        <input
          v-model="search"
          type="search"
          placeholder="Buscar producto por nombre o código…"
          class="input pl-9 py-1.5 text-[13px]"
          @keyup.enter="submitSearch"
        />
      </div>

      <div class="ml-auto flex items-center gap-1.5 sm:ml-0">
        <div class="hidden items-center gap-2 rounded-md border border-ink-200 bg-white px-2.5 py-1.5 md:flex">
          <Warehouse :size="15" class="text-ink-400" />
          <select
            v-model="warehouseId"
            class="max-w-[150px] cursor-pointer appearance-none bg-transparent pr-4 text-[13px] font-medium text-ink-700 focus:outline-none"
          >
            <option v-for="w in warehouseStore.options" :key="w.id" :value="w.id">
              {{ w.name }}
            </option>
          </select>
        </div>

        <div class="relative">
          <button
            class="relative rounded-md p-2 text-ink-500 transition hover:bg-ink-100 hover:text-ink-800"
            @click="toggleNotifications"
            aria-label="Notificaciones"
          >
            <Bell :size="19" />
            <span
              v-if="notifications.unread > 0"
              class="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-rose-500 px-1 text-[9px] font-bold text-white ring-2 ring-white"
            >
              {{ notifications.unread > 9 ? '9+' : notifications.unread }}
            </span>
          </button>

          <transition name="menu">
            <div
              v-if="showNotifications"
              class="absolute right-0 z-50 mt-2 w-[360px] max-w-[calc(100vw-2rem)] overflow-hidden rounded-xl border border-ink-200 bg-white shadow-pop"
            >
              <div class="flex items-center justify-between border-b border-ink-100 px-4 py-3">
                <p class="text-sm font-semibold text-ink-800">Alertas de stock</p>
                <button
                  v-if="notifications.unread > 0"
                  class="text-[12px] font-medium text-brand-600 hover:text-brand-700"
                  @click="notifications.markAllRead()"
                >
                  Marcar todas como leídas
                </button>
              </div>

              <div class="max-h-[340px] overflow-y-auto">
                <p v-if="!notifications.items.length" class="px-4 py-8 text-center text-[13px] text-ink-500">
                  No hay notificaciones pendientes.
                </p>

                <button
                  v-for="n in notifications.items"
                  :key="n.id"
                  class="flex w-full gap-3 border-b border-ink-100 px-4 py-3 text-left transition last:border-0 hover:bg-ink-50"
                  :class="n.isRead ? 'opacity-60' : ''"
                  @click="notifications.markRead(n.id)"
                >
                  <span
                    class="mt-1 h-2 w-2 shrink-0 rounded-full"
                    :class="n.level === 'Critical' ? 'bg-rose-500' : 'bg-amber-500'"
                  ></span>
                  <span class="min-w-0">
                    <span class="block text-[13px] leading-snug text-ink-700">{{ n.message }}</span>
                    <span class="mt-1 block text-[11px] text-ink-400">
                      {{ formatDate(n.createdAt) }}
                      <template v-if="n.warehouseName"> · {{ n.warehouseName }}</template>
                    </span>
                  </span>
                </button>
              </div>
            </div>
          </transition>
        </div>

        <div class="relative">
          <button
            class="ml-1 flex items-center gap-2 rounded-md py-1 pl-1 pr-2 transition hover:bg-ink-100"
            @click="showUserMenu = !showUserMenu"
          >
            <span class="flex h-8 w-8 items-center justify-center rounded-full bg-brand-600 text-[11px] font-bold text-white">
              {{ auth.initials }}
            </span>
            <span class="hidden text-left lg:block">
              <span class="block max-w-[140px] truncate text-[13px] font-medium leading-tight text-ink-800">
                {{ auth.fullName }}
              </span>
              <span class="block text-[11px] leading-tight text-ink-400">{{ auth.role }}</span>
            </span>
            <ChevronDown :size="14" class="hidden text-ink-400 lg:block" />
          </button>

          <transition name="menu">
            <div
              v-if="showUserMenu"
              class="absolute right-0 z-50 mt-2 w-56 overflow-hidden rounded-lg border border-ink-200 bg-white py-1 shadow-pop"
            >
              <div class="border-b border-ink-100 px-4 py-3">
                <p class="truncate text-sm font-semibold text-ink-800">{{ auth.fullName }}</p>
                <p class="truncate text-xs text-ink-500">{{ auth.user?.email || auth.user?.username }}</p>
                <span class="badge badge-info mt-2">{{ auth.role }}</span>
              </div>
              <button class="menu-item" @click="logout">
                <LogOut :size="16" class="text-ink-500" />
                Cerrar sesión
              </button>
            </div>
          </transition>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Bell, ChevronDown, LogOut, Menu, Search, Warehouse } from '@lucide/vue'
import { useAuthStore } from '@/stores/auth'
import { useNotificationsStore } from '@/stores/notifications'
import { useWarehouseStore } from '@/stores/warehouses'

defineEmits(['toggle-menu'])

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const notifications = useNotificationsStore()
const warehouseStore = useWarehouseStore()

const search = ref('')
const showNotifications = ref(false)
const showUserMenu = ref(false)

const warehouseId = computed({
  get: () => warehouseStore.selectedId,
  set: (value) => warehouseStore.select(Number(value)),
})

function toggleNotifications() {
  showNotifications.value = !showNotifications.value
  showUserMenu.value = false
  if (showNotifications.value) notifications.fetch()
}

function submitSearch() {
  const term = search.value.trim()
  if (!term) return
  router.push({ name: 'products', query: { search: term } })
}

function logout() {
  auth.logout()
  router.push({ name: 'login' })
}

function formatDate(value) {
  return new Date(value).toLocaleString('es-CL', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function onClickOutside(e) {
  const target = e.target
  if (!target.closest('.relative')) {
    showNotifications.value = false
    showUserMenu.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', onClickOutside)
  notifications.fetch()
  warehouseStore.load()
})

onUnmounted(() => document.removeEventListener('click', onClickOutside))
</script>

<style scoped>
.menu-item {
  @apply flex w-full items-center gap-2.5 px-4 py-2 text-sm text-ink-700 transition hover:bg-ink-50;
}
.menu-enter-active,
.menu-leave-active {
  transition: all 0.14s ease;
}
.menu-enter-from,
.menu-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}
</style>
