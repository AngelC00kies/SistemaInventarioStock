<template>
  <!-- Mientras procesa el modal es persistente: no se cierra por ESC, backdrop ni botón X -->
  <BaseModal v-model="open" :title="title" size="sm" :persistent="busy" @close="onCancel">
    <div class="flex gap-4">
      <span
        class="flex h-10 w-10 shrink-0 items-center justify-center rounded-full"
        :class="danger ? 'bg-rose-50 text-rose-600' : 'bg-amber-50 text-amber-600'"
      >
        <TriangleAlert :size="20" />
      </span>
      <div class="text-sm leading-relaxed text-ink-600">
        <p class="font-medium text-ink-800">{{ headline }}</p>
        <p v-if="message" class="mt-1">{{ message }}</p>
      </div>
    </div>

    <template #footer>
      <button type="button" class="btn-secondary" :disabled="busy" @click="onCancel">Cancelar</button>
      <button
        type="button"
        :class="danger ? 'btn-danger' : 'btn-primary'"
        :disabled="busy"
        @click="onConfirm"
      >
        <LoaderCircle v-if="busy" :size="16" class="animate-spin" />
        {{ busy ? 'Procesando…' : confirmText }}
      </button>
    </template>
  </BaseModal>
</template>

<script setup>
import { computed, ref, watch } from 'vue'
import { LoaderCircle, TriangleAlert } from '@lucide/vue'
import BaseModal from './BaseModal.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, default: 'Confirmar acción' },
  headline: { type: String, default: '¿Desea continuar?' },
  message: { type: String, default: '' },
  confirmText: { type: String, default: 'Confirmar' },
  danger: { type: Boolean, default: false },
  /** Función opcional (puede ser async) a ejecutar al confirmar. */
  action: { type: Function, default: null },
})

const emit = defineEmits(['update:modelValue', 'confirm'])
const open = ref(false)
const busy = ref(false)

watch(
  () => props.modelValue,
  (v) => {
    open.value = v
    if (v) busy.value = false
  },
  { immediate: true }
)
watch(open, (v) => emit('update:modelValue', v))

// `persistent` solo cubre el cierre desde fuera del modal: Cancelar está dentro, así que
// necesita su propia guardia para no descartar una confirmación mientras `busy`
function onCancel() {
  if (busy.value) return
  open.value = false
}

async function onConfirm() {
  busy.value = true
  try {
    if (props.action) await props.action()
    emit('confirm')
    open.value = false
  } catch {
    /* el error ya fue notificado por el llamador; se mantiene el diálogo abierto */
  } finally {
    busy.value = false
  }
}
</script>
