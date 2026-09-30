<template>
  <!-- Va al body para que el overflow o el transform de algún ancestro no recorte ni aísle el modal -->
  <teleport to="body">
    <transition name="modal">
      <!-- Dos capas con `mousedown.self`: cierra al pulsar el fondo translúcido o el área que rodea al diálogo -->
      <div
        v-if="modelValue"
        class="fixed inset-0 z-[70] flex items-start justify-center overflow-y-auto p-4 sm:p-6"
        @mousedown.self="close"
      >
        <div class="fixed inset-0 bg-ink-950/50 backdrop-blur-[2px]" @mousedown.self="close"></div>

        <div
          class="relative z-10 my-8 w-full rounded-xl bg-white shadow-pop"
          :class="sizeClass"
          role="dialog"
          aria-modal="true"
        >
          <header class="flex items-start justify-between gap-4 border-b border-ink-200 px-6 py-4">
            <div>
              <h3 class="text-base font-semibold text-ink-900">{{ title }}</h3>
              <p v-if="subtitle" class="mt-0.5 text-[13px] text-ink-500">{{ subtitle }}</p>
            </div>
            <button
              type="button"
              class="-mr-1.5 rounded-md p-1.5 text-ink-400 transition hover:bg-ink-100 hover:text-ink-700"
              @click="close"
              aria-label="Cerrar"
            >
              <X :size="18" />
            </button>
          </header>

          <div class="px-6 py-5">
            <slot />
          </div>

          <footer
            v-if="$slots.footer"
            class="flex flex-wrap items-center justify-end gap-2 border-t border-ink-200 bg-ink-50/60 px-6 py-3.5"
          >
            <slot name="footer" />
          </footer>
        </div>
      </div>
    </transition>
  </teleport>
</template>

<script setup>
import { computed, onMounted, onUnmounted } from 'vue'
import { X } from '@lucide/vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
  size: { type: String, default: 'md' },
  persistent: { type: Boolean, default: false },
})

const emit = defineEmits(['update:modelValue', 'close'])

const sizes = {
  sm: 'max-w-md',
  md: 'max-w-2xl',
  lg: 'max-w-4xl',
  xl: 'max-w-6xl',
}
const sizeClass = computed(() => sizes[props.size] || sizes.md)

// `persistent` desactiva todas las vías de cierre (ESC, backdrop y botón X); ConfirmDialog
// lo activa mientras `busy` para que una confirmación en curso no se pueda descartar
function close() {
  if (props.persistent) return
  emit('update:modelValue', false)
  emit('close')
}

function onKey(e) {
  if (e.key === 'Escape' && props.modelValue) close()
}

onMounted(() => window.addEventListener('keydown', onKey))
onUnmounted(() => window.removeEventListener('keydown', onKey))
</script>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.18s ease;
}
.modal-enter-active > div:last-child,
.modal-leave-active > div:last-child {
  transition: transform 0.18s ease, opacity 0.18s ease;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
.modal-enter-from > div:last-child,
.modal-leave-to > div:last-child {
  transform: translateY(-8px) scale(0.985);
  opacity: 0;
}
</style>
