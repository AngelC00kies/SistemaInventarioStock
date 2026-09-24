<template>
  <div>
    <PageHeader
      section="Análisis"
      title="Reportes"
      description="Genere documentos de inventario listos para imprimir o compartir, con los filtros que está visualizando."
    >
      <template #actions>
        <ExportMenu :report="selected" :params="exportParams" @start="exporting = true" @done="exporting = false" />
      </template>
    </PageHeader>

    <div class="mt-6 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      <button
        v-for="(def, key) in visibleReports"
        :key="key"
        class="card group p-4 text-left transition"
        :class="selected === key ? 'border-brand-500 ring-2 ring-brand-500/20' : 'hover:border-ink-300'"
        @click="selectReport(key)"
      >
        <span
          class="flex h-9 w-9 items-center justify-center rounded-lg transition"
          :class="selected === key ? 'bg-brand-600 text-white' : 'bg-ink-100 text-ink-500 group-hover:bg-brand-50 group-hover:text-brand-600'"
        >
          <component :is="iconOf(def.icon)" :size="17" :stroke-width="1.9" />
        </span>
        <p class="mt-3 text-sm font-semibold text-ink-800">{{ def.title }}</p>
        <p class="mt-0.5 text-[12.5px] leading-snug text-ink-500">{{ def.description }}</p>
      </button>
    </div>

    <div class="card mt-5">
      <div class="flex flex-wrap items-end gap-3 border-b border-ink-200 px-4 py-3.5">
        <div v-if="hasFilter('search')" class="relative min-w-[200px] flex-1">
          <Search :size="15" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
          <input
            v-model="filters.search"
            type="search"
            class="input py-1.5 pl-9 text-sm"
            placeholder="Buscar…"
            @input="debouncedPreview"
          />
        </div>

        <select v-if="hasFilter('warehouse')" v-model="filters.warehouseId" class="input w-auto py-1.5 text-sm" @change="loadPreview(1)">
          <option :value="null">Todos los almacenes</option>
          <option v-for="w in options.warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
        </select>

        <select v-if="hasFilter('category')" v-model="filters.categoryId" class="input w-auto py-1.5 text-sm" @change="loadPreview(1)">
          <option :value="null">Todas las categorías</option>
          <option v-for="c in options.categories" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>

        <select v-if="hasFilter('supplier')" v-model="filters.supplierId" class="input w-auto py-1.5 text-sm" @change="loadPreview(1)">
          <option :value="null">Todos los proveedores</option>
          <option v-for="s in options.suppliers" :key="s.id" :value="s.id">{{ s.name }}</option>
        </select>

        <select v-if="hasFilter('type')" v-model="filters.type" class="input w-auto py-1.5 text-sm" @change="loadPreview(1)">
          <option :value="null">Entradas y salidas</option>
          <option value="Entrada">Solo entradas</option>
          <option value="Salida">Solo salidas</option>
        </select>

        <div v-if="hasFilter('from')" class="flex items-center gap-1.5 text-sm text-ink-500">
          <input v-model="filters.from" type="date" class="input w-auto py-1.5 text-[13px]" @change="loadPreview(1)" />
          <span>—</span>
          <input v-model="filters.to" type="date" class="input w-auto py-1.5 text-[13px]" @change="loadPreview(1)" />
        </div>
      </div>

      <div v-if="preview.loading" class="space-y-2 p-4">
        <div v-for="i in 6" :key="i" class="h-10 animate-pulse rounded bg-ink-100"></div>
      </div>

      <div v-else-if="error" class="px-4">
        <EmptyState :title="error" description="Verifique sus permisos o intente nuevamente." />
      </div>

      <div v-else-if="!preview.rows.length" class="px-4">
        <EmptyState title="Sin datos para este reporte" description="Ajuste los filtros para obtener resultados." />
      </div>

      <template v-else>
        <div class="flex flex-wrap items-center justify-between gap-2 border-b border-ink-100 bg-ink-50/60 px-4 py-2.5">
          <p class="text-[13px] font-medium text-ink-600">
            {{ currentDef.title }}
          </p>
          <p class="text-[13px] text-ink-500">{{ preview.summary }}</p>
        </div>

        <div class="max-h-[520px] overflow-auto">
          <table class="table-base min-w-[760px]">
            <thead class="sticky top-0 z-10">
              <tr>
                <th v-for="col in preview.columns" :key="col" :class="alignClass(col)">{{ col }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, i) in preview.rows" :key="i">
                <td v-for="(cell, j) in row" :key="j" :class="cell.cls">
                  <StatusBadge v-if="cell.badge" :status="cell.text" />
                  <template v-else>{{ cell.text }}</template>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>

      <div
        class="flex flex-wrap items-center justify-between gap-3 border-t border-ink-200 px-4 py-3.5"
      >
        <p class="text-[13px] text-ink-500">
          Los archivos se generan en el servidor e incluyen encabezado, filtros aplicados y totales.
        </p>
        <div class="flex items-center gap-2">
          <button class="btn-secondary" :disabled="exporting || preview.loading" @click="exportNow('pdf')">
            <FileDown :size="16" />
            Descargar PDF
          </button>
          <button class="btn-primary" :disabled="exporting || preview.loading" @click="exportNow('xlsx')">
            <FileSpreadsheet :size="16" />
            Descargar Excel
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import {
  ArrowLeftRight,
  Boxes,
  Building2,
  FileDown,
  FileSpreadsheet,
  Package,
  Search,
  Tags,
  TriangleAlert,
  Users,
} from '@lucide/vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ExportMenu from '@/components/ui/ExportMenu.vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import StatusBadge from '@/components/ui/StatusBadge.vue'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import { downloadReport } from '@/api/reports'
import { loadCatalogOptions, reportDefs } from './reportDefs'

const auth = useAuthStore()
const toast = useToastStore()

const icons = { ArrowLeftRight, Boxes, Building2, Package, Tags, TriangleAlert, Users }
const iconOf = (name) => icons[name] || Boxes

const selected = ref('stock')
const exporting = ref(false)
const error = ref('')
const options = reactive({ warehouses: [], categories: [], suppliers: [] })

const filters = reactive({
  search: '',
  warehouseId: null,
  categoryId: null,
  supplierId: null,
  type: null,
  from: '',
  to: '',
})

const preview = reactive({ loading: true, columns: [], rows: [], summary: '' })

const visibleReports = computed(() =>
  Object.fromEntries(Object.entries(reportDefs).filter(([, def]) => !def.adminOnly || auth.isAdmin))
)

const currentDef = computed(() => reportDefs[selected.value])

const exportParams = computed(() => ({
  warehouseId: filters.warehouseId,
  categoryId: filters.categoryId,
  supplierId: filters.supplierId,
  type: filters.type,
  search: filters.search || undefined,
  from: filters.from || undefined,
  to: filters.to || undefined,
  criticalOnly: selected.value === 'critical-stock' || undefined,
}))

const hasFilter = (name) => currentDef.value.filters.includes(name)

function alignClass(col) {
  const right = ['Stock', 'Mínimo', 'Faltante', 'Valor stock', 'Cantidad', 'Total', 'P. compra', 'P. venta', 'Productos activos', 'Productos']
  return right.includes(col) ? 'text-right' : ''
}

function selectReport(key) {
  selected.value = key
  loadPreview(1)
}

let timer = null
function debouncedPreview() {
  clearTimeout(timer)
  timer = setTimeout(() => loadPreview(1), 350)
}

async function loadPreview() {
  preview.loading = true
  error.value = ''
  try {
    const result = await currentDef.value.load(filters)
    preview.columns = result.columns
    preview.rows = result.rows
    preview.summary = result.summary
  } catch (e) {
    error.value = e.message || 'No se pudo cargar la vista previa.'
    preview.columns = []
    preview.rows = []
  } finally {
    preview.loading = false
  }
}

async function exportNow(format) {
  exporting.value = true
  try {
    const name = await downloadReport(selected.value, format, exportParams.value)
    toast.success(`El archivo "${name}" fue generado correctamente.`, 'Exportación lista')
  } catch (e) {
    toast.error(e.message)
  } finally {
    exporting.value = false
  }
}

watch(
  () => [filters.warehouseId, filters.categoryId, filters.supplierId, filters.type],
  () => loadPreview()
)

onMounted(async () => {
  const loaded = await loadCatalogOptions()
  Object.assign(options, loaded)
  await loadPreview()
})
</script>
