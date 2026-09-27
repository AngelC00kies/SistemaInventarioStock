<template>
  <div class="card">
    <div class="card-header">
      <div class="filters">
        <div class="field">
          <label>Buscar</label>
          <input
            v-model.trim="filters.search"
            type="text"
            placeholder="Producto, motivo, documento…"
            @keyup.enter="reload"
          />
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
          <label>Almacén</label>
          <select v-model="filters.warehouseId">
            <option :value="null">Todos</option>
            <option v-for="w in warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
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

        <button class="btn secondary" @click="reload">🔍 Filtrar</button>
      </div>

      <router-link v-if="auth.canEdit" to="/movements/new" class="btn">
        + Registrar movimiento
      </router-link>
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
            <th class="num">Stock resultante</th>
            <th>Motivo</th>
            <th>Documento</th>
            <th>Usuario</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="m in items" :key="m.id">
            <td class="muted">{{ formatDate(m.date) }}</td>
            <td>
              <span class="pill" :class="m.type === 'Entrada' ? 'green' : 'red'">
                {{ m.type === 'Entrada' ? '⬆ Entrada' : '⬇ Salida' }}
              </span>
            </td>
            <td class="wrap">
              <strong>{{ m.productCode }}</strong> — {{ m.productName }}
            </td>
            <td>{{ m.warehouseName }}</td>
            <td class="num"><strong>{{ m.quantity }}</strong></td>
            <td class="num">{{ m.stockAfter }}</td>
            <td class="wrap muted">{{ m.reason }}</td>
            <td>{{ m.documentReference || '—' }}</td>
            <td class="muted">{{ m.userName }}</td>
          </tr>

          <tr v-if="!loading && items.length === 0">
            <td colspan="9" class="empty">No se encontraron movimientos</td>
          </tr>
          <tr v-if="loading">
            <td colspan="9" class="empty">Cargando…</td>
          </tr>
        </tbody>
      </table>
    </div>

    <Pagination v-model="page" :total="total" :page-size="pageSize" />
  </div>
</template>

<script setup>
import { onMounted, reactive, ref, watch } from 'vue'
import api, { errorMessage } from '../api/client'
import { useAuthStore } from '../stores/auth'
import { useUiStore } from '../stores/ui'
import Pagination from '../components/Pagination.vue'

const auth = useAuthStore()
const ui = useUiStore()

const items = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = 20
const loading = ref(false)
const warehouses = ref([])

const filters = reactive({
  search: '',
  type: '',
  warehouseId: null,
  from: '',
  to: '',
})

async function load() {
  loading.value = true
  try {
    const { data } = await api.get('/movements', {
      params: {
        page: page.value,
        pageSize,
        search: filters.search || undefined,
        type: filters.type || undefined,
        warehouseId: filters.warehouseId ?? undefined,
        from: filters.from ? new Date(filters.from).toISOString() : undefined,
        to: filters.to ? new Date(filters.to).toISOString() : undefined,
      },
    })
    items.value = data.items
    total.value = data.totalCount
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  } finally {
    loading.value = false
  }
}

function reload() {
  page.value = 1
  load()
}

const formatDate = (d) =>
  new Date(d).toLocaleString('es', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })

watch(page, load)
watch([() => filters.type, () => filters.warehouseId], reload)

onMounted(async () => {
  load()
  const w = await api.get('/warehouses').catch(() => ({ data: [] }))
  warehouses.value = w.data
})
</script>
