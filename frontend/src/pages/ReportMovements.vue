<template>
  <div class="card">
    <div class="card-header">
      <div class="filters">
        <div class="field">
          <label>Producto</label>
          <select v-model="filters.productId">
            <option :value="null">Todos</option>
            <option v-for="p in products" :key="p.id" :value="p.id">
              {{ p.code }} — {{ p.name }}
            </option>
          </select>
        </div>

        <div class="field">
          <label>Almacén</label>
          <select v-model="filters.warehouseId">
            <option :value="null">Todos</option>
            <option v-for="w in warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
          </select>
        </div>

        <div class="field">
          <label>Tipo</label>
          <select v-model="filters.type">
            <option value="">Todos</option>
            <option value="Entrada">Entrada</option>
            <option value="Salida">Salida</option>
          </select>
        </div>

        <div class="field">
          <label>Desde</label>
          <input v-model="filters.from" type="date" />
        </div>

        <div class="field">
          <label>Hasta</label>
          <input v-model="filters.to" type="date" />
        </div>

        <button class="btn secondary" @click="load">🔍 Actualizar</button>
      </div>

      <div class="row">
        <button class="btn secondary" :disabled="exporting" @click="exportFile('excel')">
          📊 Excel
        </button>
        <button class="btn secondary" :disabled="exporting" @click="exportFile('pdf')">
          📄 PDF
        </button>
      </div>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Tipo</th>
            <th>Producto</th>
            <th>Almacén</th>
            <th class="num">Cantidad</th>
            <th class="num">Stock result.</th>
            <th>Motivo</th>
            <th>Documento</th>
            <th>Usuario</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(r, i) in rows" :key="i">
            <td class="muted">{{ formatDate(r.date) }}</td>
            <td>
              <span class="pill" :class="r.type === 'Entrada' ? 'green' : 'red'">{{ r.type }}</span>
            </td>
            <td class="wrap">{{ r.productCode }} — {{ r.productName }}</td>
            <td>{{ r.warehouse }}</td>
            <td class="num"><strong>{{ r.quantity }}</strong></td>
            <td class="num">{{ r.stockAfter }}</td>
            <td class="wrap muted">{{ r.reason }}</td>
            <td>{{ r.documentReference || '—' }}</td>
            <td class="muted">{{ r.userName }}</td>
          </tr>
          <tr v-if="!loading && rows.length === 0">
            <td colspan="9" class="empty">Sin movimientos para los filtros seleccionados</td>
          </tr>
          <tr v-if="loading">
            <td colspan="9" class="empty">Cargando…</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref, watch } from 'vue'
import api, { errorMessage } from '../api/client'
import { useUiStore } from '../stores/ui'

const ui = useUiStore()

const rows = ref([])
const loading = ref(false)
const exporting = ref(false)
const products = ref([])
const warehouses = ref([])

const filters = reactive({
  productId: null,
  warehouseId: null,
  type: '',
  from: '',
  to: '',
})

function buildParams() {
  return {
    productId: filters.productId ?? undefined,
    warehouseId: filters.warehouseId ?? undefined,
    type: filters.type || undefined,
    from: filters.from ? new Date(filters.from).toISOString() : undefined,
    to: filters.to ? new Date(filters.to).toISOString() : undefined,
  }
}

async function load() {
  loading.value = true
  try {
    const { data } = await api.get('/reports/movements', { params: buildParams() })
    rows.value = data
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  } finally {
    loading.value = false
  }
}

async function exportFile(format) {
  exporting.value = true
  try {
    const { data } = await api.get('/reports/movements', {
      params: { ...buildParams(), format },
      responseType: 'blob',
    })

    const ext = format === 'excel' ? 'xlsx' : 'pdf'
    downloadBlob(data, `reporte-movimientos-${Date.now()}.${ext}`)
    ui.notify(`Reporte exportado en ${format.toUpperCase()}`)
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  } finally {
    exporting.value = false
  }
}

function downloadBlob(blob, filename) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}

const formatDate = (d) =>
  new Date(d).toLocaleString('es', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })

watch([() => filters.type, () => filters.productId, () => filters.warehouseId], load)

onMounted(async () => {
  load()
  const [p, w] = await Promise.all([
    api.get('/products', { params: { pageSize: 200 } }).catch(() => ({ data: { items: [] } })),
    api.get('/warehouses').catch(() => ({ data: [] })),
  ])
  products.value = p.data.items
  warehouses.value = w.data
})
</script>
