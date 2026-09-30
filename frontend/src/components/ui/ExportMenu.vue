<template>
  <div class="relative" ref="root">
    <button type="button" class="btn-secondary" :disabled="loading" @click="open = !open">
      <LoaderCircle v-if="loading" :size="16" class="animate-spin" />
      <Download v-else :size="16" />
      Exportar
      <ChevronDown :size="14" class="text-ink-400" />
    </button>

    <transition name="menu">
      <div
        v-if="open"
        class="absolute right-0 z-40 mt-2 w-56 overflow-hidden rounded-lg border border-ink-200 bg-white py-1 shadow-pop"
      >
        <p class="px-3 py-2 text-[11px] font-semibold uppercase tracking-wide text-ink-400">
          Formato de archivo
        </p>

        <button class="menu-item" @click="download('pdf')">
          <FileText :size="16" class="text-rose-600" />
          <span>
            <span class="block text-sm font-medium text-ink-800">Documento PDF</span>
            <span class="block text-[11px] text-ink-400">Listo para imprimir o archivar</span>
          </span>
        </button>

        <button class="menu-item" @click="download('xlsx')">
          <Sheet :size="16" class="text-emerald-600" />
          <span>
            <span class="block text-sm font-medium text-ink-800">Libro de Excel</span>
            <span class="block text-[11px] text-ink-400">Hoja .xlsx con filtros</span>
          </span>
        </button>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { onMounted, onUnmounted, ref } from 'vue'
import { ChevronDown, Download, FileText, LoaderCircle, Sheet } from '@lucide/vue'
import { downloadReport } from '@/api/reports'
import { useToastStore } from '@/stores/toast'

const props = defineProps({
  report: { type: String, required: true },
  params: { type: Object, default: () => ({}) },
})

// `start`/`done` enmarcan la exportación para que el padre sepa cuándo está en curso
// (p. ej. para bloquear la tabla); `done` se emite también cuando la petición falla
const emit = defineEmits(['start', 'done'])
const toast = useToastStore()

const open = ref(false)
const loading = ref(false)
const root = ref(null)

async function download(format) {
  open.value = false
  loading.value = true
  emit('start')
  try {
    const name = await downloadReport(props.report, format, props.params)
    toast.success(`El archivo "${name}" fue generado correctamente.`, 'Exportación lista')
  } catch (e) {
    toast.error(e.message || 'No se pudo generar el archivo.')
  } finally {
    loading.value = false
    emit('done')
  }
}

function onClickOutside(e) {
  if (root.value && !root.value.contains(e.target)) open.value = false
}

onMounted(() => document.addEventListener('click', onClickOutside))
onUnmounted(() => document.removeEventListener('click', onClickOutside))
</script>

<style scoped>
.menu-item {
  @apply flex w-full items-center gap-3 px-3 py-2 text-left transition hover:bg-ink-50;
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
