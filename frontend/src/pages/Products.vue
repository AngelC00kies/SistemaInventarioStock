<template>
  <div class="card">
    <div class="card-header">
      <div class="filters">
        <div class="field">
          <label>Buscar</label>
          <input
            v-model.trim="filters.search"
            type="text"
            placeholder="Código o nombre…"
            @keyup.enter="reload"
          />
        </div>

        <div class="field">
          <label>Categoría</label>
          <select v-model="filters.categoryId">
            <option :value="null">Todas</option>
            <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>

        <div class="field">
          <label>Proveedor</label>
          <select v-model="filters.supplierId">
            <option :value="null">Todos</option>
            <option v-for="s in suppliers" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>

        <div class="field">
          <label>Estado de stock</label>
          <select v-model="stockFilter">
            <option value="">Todos</option>
            <option value="low">Stock bajo</option>
            <option value="out">Sin stock</option>
          </select>
        </div>

        <button class="btn secondary" @click="reload">🔍 Buscar</button>
      </div>

      <router-link v-if="auth.canEdit" to="/products/new" class="btn">
        + Nuevo producto
      </router-link>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Código</th>
            <th>Nombre</th>
            <th>Categoría</th>
            <th>Proveedor</th>
            <th class="num">Precio venta</th>
            <th class="num">Stock</th>
            <th class="num">Mínimo</th>
            <th>Estado</th>
            <th style="text-align:right">Acciones</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="p in items" :key="p.id">
            <td><strong>{{ p.code }}</strong></td>
            <td class="wrap">{{ p.name }}</td>
            <td>{{ p.categoryName }}</td>
            <td class="muted">{{ p.supplierName || '—' }}</td>
            <td class="num">{{ money(p.salePrice) }}</td>
            <td class="num"><strong>{{ p.totalStock }}</strong></td>
            <td class="num muted">{{ p.minimumStock }}</td>
            <td>
              <StockBadge :quantity="p.totalStock" :minimum="p.minimumStock" />
              <span v-if="!p.isActive" class="pill gray" style="margin-left:4px">Inactivo</span>
            </td>
            <td style="text-align:right;white-space:nowrap">
              <button class="btn ghost sm" title="Ver stock" @click="showStock(p)">📦</button>
              <router-link
                v-if="auth.canEdit"
                :to="`/products/${p.id}`"
                class="btn ghost sm"
                title="Editar"
              >✏️</router-link>
              <button
                v-if="auth.isAdmin"
                class="btn ghost sm"
                title="Dar de baja"
                @click="remove(p)"
              >🗑️</button>
            </td>
          </tr>

          <tr v-if="!loading && items.length === 0">
            <td colspan="9" class="empty">No se encontraron productos</td>
          </tr>
          <tr v-if="loading">
            <td colspan="9" class="empty">Cargando…</td>
          </tr>
        </tbody>
      </table>
    </div>

    <Pagination v-model="page" :total="total" :page-size="pageSize" />

    <!-- Modal: stock por almacén -->
    <Modal
      v-if="stockProduct"
      :title="`Stock de ${stockProduct.name}`"
      @close="stockProduct = null"
    >
      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Almacén</th>
              <th class="num">Cantidad</th>
              <th>Actualizado</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in stockEntries" :key="s.warehouseId">
              <td>{{ s.warehouseName }}</td>
              <td class="num"><strong>{{ s.quantity }}</strong></td>
              <td class="muted">{{ new Date(s.updatedAt).toLocaleString('es') }}</td>
            </tr>
            <tr v-if="stockEntries.length === 0">
              <td colspan="3" class="empty">Sin stock registrado en ningún almacén</td>
            </tr>
          </tbody>
        </table>
      </div>

      <template #footer>
        <button class="btn secondary" @click="stockProduct = null">Cerrar</button>
      </template>
    </Modal>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import api, { errorMessage } from '../api/client'
import { useAuthStore } from '../stores/auth'
import { useUiStore } from '../stores/ui'
import Pagination from '../components/Pagination.vue'
import StockBadge from '../components/StockBadge.vue'
import Modal from '../components/Modal.vue'

const auth = useAuthStore()
const ui = useUiStore()

const items = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = 20
const loading = ref(false)

const categories = ref([])
const suppliers = ref([])

const filters = reactive({
  search: '',
  categoryId: null,
  supplierId: null,
})
const stockFilter = ref('')

const stockProduct = ref(null)
const stockEntries = ref([])

const query = computed(() => ({
  page: page.value,
  pageSize,
  search: filters.search || undefined,
  categoryId: filters.categoryId ?? undefined,
  supplierId: filters.supplierId ?? undefined,
  lowStockOnly: stockFilter.value === 'low' || undefined,
  outOfStockOnly: stockFilter.value === 'out' || undefined,
}))

async function load() {
  loading.value = true
  try {
    const { data } = await api.get('/products', { params: query.value })
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

async function remove(p) {
  if (!confirm(`¿Dar de baja el producto "${p.name}"?`)) return
  try {
    await api.delete(`/products/${p.id}`)
    ui.notify('Producto dado de baja')
    load()
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  }
}

async function showStock(p) {
  stockProduct.value = p
  stockEntries.value = []
  try {
    const { data } = await api.get(`/products/${p.id}/stock`)
    stockEntries.value = data
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  }
}

const money = (n) =>
  new Intl.NumberFormat('es', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(n || 0)

watch(page, load)
watch([() => filters.categoryId, () => filters.supplierId, stockFilter], reload)

onMounted(async () => {
  load()
  const [c, s] = await Promise.all([
    api.get('/categories').catch(() => ({ data: [] })),
    api.get('/suppliers', { params: { pageSize: 200 } }).catch(() => ({ data: { items: [] } })),
  ])
  categories.value = c.data
  suppliers.value = s.data.items || []
})
</script>
