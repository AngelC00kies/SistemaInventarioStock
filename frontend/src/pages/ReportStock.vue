<template>
  <div class="card">
    <div class="card-header">
      <div class="filters">
        <div class="field">
          <label>Categoría</label>
          <select v-model="filters.categoryId">
            <option :value="null">Todas</option>
            <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
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
          <label>Alcance</label>
          <select v-model="filters.criticalOnly">
            <option :value="false">Stock actual (todo)</option>
            <option :value="true">Solo stock crítico</option>
          </select>
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

    <!-- Resumen de valorización -->
    <div class="card-body" style="padding-bottom:0">
      <div class="kpi-grid" style="margin-bottom:0">
        <div class="kpi">
          <div>
            <div class="label">Productos</div>
            <div class="value">{{ summary.totalProducts }}</div>
          </div>
          <div class="icon">🏷️</div>
        </div>
        <div class="kpi success">
          <div>
            <div class="label">Unidades totales</div>
            <div class="value">{{ formatNumber(summary.totalUnits) }}</div>
          </div>
          <div class="icon">📦</div>
        </div>
        <div class="kpi">
          <div>
            <div class="label">Valor a precio de compra</div>
            <div class="value" style="font-size:20px">{{ money(summary.totalPurchaseValue) }}</div>
          </div>
          <div class="icon">💰</div>
        </div>
        <div class="kpi warning">
          <div>
            <div class="label">Productos críticos</div>
            <div class="value">{{ summary.criticalProducts }}</div>
          </div>
          <div class="icon">⚠️</div>
        </div>
      </div>
    </div>

    <div class="table-wrap" style="margin-top:20px">
      <table>
        <thead>
          <tr>
            <th>Código</th>
            <th>Producto</th>
            <th>Categoría</th>
            <th>Proveedor</th>
            <th>Almacén</th>
            <th class="num">Cantidad</th>
            <th class="num">Mínimo</th>
            <th class="num">Valor stock</th>
            <th>Estado</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(r, i) in rows" :key="i">
            <td><strong>{{ r.productCode }}</strong></td>
            <td class="wrap">{{ r.productName }}</td>
            <td>{{ r.category }}</td>
            <td class="muted">{{ r.supplier || '—' }}</td>
            <td>{{ r.warehouse }}</td>
            <td class="num"><strong>{{ r.quantity }}</strong></td>
            <td class="num muted">{{ r.minimumStock }}</td>
            <td class="num">{{ money(r.stockValue) }}</td>
            <td>
              <span class="pill" :class="statusColor(r.status)">{{ r.status }}</span>
            </td>
          </tr>
          <tr v-if="!loading && rows.length === 0">
            <td colspan="9" class="empty">Sin resultados para los filtros seleccionados</td>
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
const categories = ref([])
const warehouses = ref([])

const filters = reactive({
  categoryId: null,
  warehouseId: null,
  criticalOnly: false,
})

const summary = reactive({
  totalProducts: 0,
  totalUnits: 0,
  totalPurchaseValue: 0,
  totalSaleValue: 0,
  criticalProducts: 0,
})

async function load() {
  loading.value = true
  try {
    const params = {
      categoryId: filters.categoryId ?? undefined,
      warehouseId: filters.warehouseId ?? undefined,
      criticalOnly: filters.criticalOnly || undefined,
    }

    const { data } = await api.get('/reports/stock', { params })
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
    const { data } = await api.get('/reports/stock', {
      params: {
        categoryId: filters.categoryId ?? undefined,
        warehouseId: filters.warehouseId ?? undefined,
        criticalOnly: filters.criticalOnly || undefined,
        format,
      },
      responseType: 'blob',
    })

    downloadBlob(data, `reporte-stock-${Date.now()}.${format === 'excel' ? 'xlsx' : 'pdf'}`)
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

const statusColor = (s) =>
  ({ Crítico: 'red', 'Sin stock': 'red', 'Stock bajo': 'yellow', Ajustado: 'blue', Disponible: 'green' }[s] || 'gray')

const formatNumber = (n) => new Intl.NumberFormat('es').format(n || 0)
const money = (n) =>
  new Intl.NumberFormat('es', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(n || 0)

watch([() => filters.categoryId, () => filters.warehouseId, () => filters.criticalOnly], load)

onMounted(async () => {
  load()
  const [c, w, v] = await Promise.all([
    api.get('/categories').catch(() => ({ data: [] })),
    api.get('/warehouses').catch(() => ({ data: [] })),
    api.get('/reports/inventory-value').catch(() => ({ data: summary })),
  ])
  categories.value = c.data
  warehouses.value = w.data
  Object.assign(summary, v.data)
})
</script>
