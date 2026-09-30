<template>
  <teleport to="body">
    <div class="fixed bottom-5 right-5 z-[80] flex w-[360px] max-w-[calc(100vw-2.5rem)] flex-col gap-2">
      <transition-group name="toast">
        <div
          v-for="toast in store.items"
          :key="toast.id"
          class="pointer-events-auto flex items-start gap-3 rounded-lg border bg-white p-3.5 shadow-pop"
          :class="borderClass(toast.type)"
        >
          <span class="mt-0.5 shrink-0" :class="iconClass(toast.type)">
            <component :is="icon(toast.type)" :size="18" :stroke-width="2" />
          </span>

          <div class="min-w-0 flex-1">
            <p class="text-sm font-semibold text-ink-900">{{ toast.title }}</p>
            <p class="mt-0.5 break-words text-[13px] leading-snug text-ink-600">{{ toast.message }}</p>
          </div>

          <button
            class="rounded p-1 text-ink-400 transition hover:bg-ink-100 hover:text-ink-600"
            @click="store.dismiss(toast.id)"
            aria-label="Cerrar"
          >
            <X :size="15" />
          </button>
        </div>
      </transition-group>
    </div>
  </teleport>
</template>

<script setup>
import { CheckCircle2, AlertTriangle, XCircle, Info, X } from '@lucide/vue'
import { useToastStore } from '@/stores/toast'

const store = useToastStore()

// Tipo de toast -> icono y color (borde e icono comparten la misma cadena de respaldo `info`
// cuando el tipo es desconocido)
const icons = { success: CheckCircle2, error: XCircle, warning: AlertTriangle, info: Info }
const icon = (type) => icons[type] || Info

const iconClass = (type) =>
  ({
    success: 'text-emerald-600',
    error: 'text-rose-600',
    warning: 'text-amber-600',
    info: 'text-brand-600',
  })[type] || 'text-brand-600'

const borderClass = (type) =>
  ({
    success: 'border-emerald-200',
    error: 'border-rose-200',
    warning: 'border-amber-200',
    info: 'border-brand-200',
  })[type] || 'border-ink-200'
</script>

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.25s cubic-bezier(0.21, 1.02, 0.73, 1);
}
.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateX(24px);
}
</style>
