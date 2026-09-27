<template>
  <div class="card" style="max-width:760px">
    <div class="card-header">
      <h2>Registrar movimiento</h2>
      <router-link to="/movements" class="btn secondary sm">← Volver</router-link>
    </div>

    <form class="card-body" @submit.prevent="onSubmit">
      <div v-if="error" class="alert" style="margin-bottom:18px">{{ error }}</div>

      <!-- Selector de tipo -->
      <div class="row" style="margin-bottom:22px;gap:12px">
        <button
          type="button"
          class="btn"
          :class="form.type === 'Entrada' ? 'success' : 'secondary'"
          style="flex:1;justify-content:center"
          @click="form.type = 'Entrada'"
        >
          ⬆ Entrada
        </button>
        <button
          type="button"
          class="btn"
          :class="form.type === 'Salida' ? 'danger' : 'secondary'"
          style="flex:1;justify-content:center"
          @click="form.type = 'Salida'"
        >
          ⬇ Salida
        </button>
      </div>

      <div class="form-grid">
        <div class="field full">
          <label>Producto <span class="req">*</span></label>
          <select v-model.number="form.productId" required @change="loadStock">
            <option disabled :value="0">Seleccione…</option>
            <option v-for="p in products" :key="p.id" :value="p.id">
              {{ p.code }} — {{ p.name }} (stock: {{ p.totalStock }})
            </option>
          </select>
        </div>

        <div class="field">
          <label>Almacén <span class="req">*</span></label>
          <select v-model.number="form.warehouseId" required @change="loadStock">
            <option disabled :value="0">Seleccione…</option>
            <option v-for="w in warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
          </select>
          <span class="hint" v-if="available >= 0">
            Disponible en el almacén: <strong>{{ available }}</strong>
          </span>
        </div>

        <div class="field">
          <label>Cantidad <span class="req">*</span></label>
          <input
            v-model.number="form.quantity"
            type="number"
            min="1"
            :max="form.type === 'Salida' && available >= 0 ? available : undefined"
            required
          />
          <span v-if="form.type === 'Salida' && form.quantity > available && available >= 0" class="error-text">
            Supera el stock disponible ({{ available }})
          </span>
        </div>

        <div class="field full">
          <label>Motivo <span class="req">*</span></label>
          <input
            v-model.trim="form.reason"
            type="text"
            placeholder="Ej: Venta a cliente, compra a proveedor, ajuste de inventario…"
            required
            maxlength="300"
          />
        </div>

        <div class="field full">
          <label>Documento de referencia</label>
          <input
            v-model.trim="form.documentReference"
            type="text"
            placeholder="Nº de factura, orden de compra u otro documento (opcional)"
            maxlength="50"
          />
        </div>
      </div>

      <div class="row" style="margin-top:24px;justify-content:flex-end">
        <router-link to="/movements" class="btn secondary">Cancelar</router-link>
        <button
          class="btn"
          :class="form.type === 'Salida' ? 'danger' : 'success'"
          type="submit"
          :disabled="saving || !isValid"
        >
          {{ saving ? 'Registrando…' : form.type === 'Entrada' ? 'Registrar entrada' : 'Registrar salida' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import api, { errorMessage } from '../api/client'
import { useUiStore } from '../stores/ui'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const ui = useUiStore()
const auth = useAuthStore()

const products = ref([])
const warehouses = ref([])
const saving = ref(false)
const error = ref('')
const available = ref(-1)

const form = reactive({
  type: 'Entrada',
  productId: 0,
  warehouseId: 0,
  quantity: 1,
  reason: '',
  documentReference: '',
})

const isValid = computed(() => {
  if (!form.productId || !form.warehouseId) return false
  if (!form.quantity || form.quantity < 1) return false
  if (!form.reason) return false
  if (form.type === 'Salida' && available.value >= 0 && form.quantity > available.value) return false
  return true
})

async function loadStock() {
  available.value = -1
  if (!form.productId || !form.warehouseId) return

  try {
    const { data } = await api.get(`/products/${form.productId}/stock`)
    const entry = data.find((s) => s.warehouseId === form.warehouseId)
    available.value = entry ? entry.quantity : 0
  } catch {
    available.value = -1
  }
}

async function onSubmit() {
  if (!isValid.value) return

  saving.value = true
  error.value = ''

  try {
    const { data } = await api.post('/movements', {
      type: form.type,
      productId: form.productId,
      warehouseId: form.warehouseId,
      quantity: Number(form.quantity),
      reason: form.reason,
      documentReference: form.documentReference || null,
    })

    ui.notify(
      `${form.type === 'Entrada' ? 'Entrada' : 'Salida'} registrada. Stock resultante: ${data.stockAfter}`,
      'success'
    )

    // Recarga el contador de alertas del sidebar
    const ui2 = useUiStore()
    ui2.fetchPendingCount()

    router.push('/movements')
  } catch (e) {
    error.value = errorMessage(e)
    if (form.productId && form.warehouseId) loadStock()
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  if (!auth.canEdit) {
    router.push('/movements')
    return
  }

  const [p, w] = await Promise.all([
    api.get('/products', { params: { pageSize: 200 } }),
    api.get('/warehouses'),
  ])
  products.value = p.data.items
  warehouses.value = w.data
})
</script>
