<template>
  <div>
    <PageHeader
      section="Operación"
      title="Movimientos de inventario"
      description="Registro detallado de entradas y salidas con responsable, motivo y documento asociado."
    >
      <template #actions>
        <ExportMenu report="movements" :params="exportParams" />
        <button v-if="auth.canWrite" class="btn-primary" @click="openCreate">
          <Plus :size="16" />
          Registrar movimiento
        </button>
      </template>
    </PageHeader>

    <!-- Resumen del mes en curso: ignora los filtros de la tabla y se recalcula
         con cada recarga, así que no cambia al buscar o filtrar más abajo. -->
    <div class="mt-6 grid gap-4 sm:grid-cols-3">
      <div class="card flex items-center gap-3 p-4">
        <span class="flex h-9 w-9 items-center justify-center rounded-lg bg-emerald-50 text-emerald-600">
          <ArrowDownToLine :size="17" />
        </span>
        <div>
          <p class="text-[11px] uppercase tracking-wide text-ink-400">Entradas del mes</p>
          <p class="text-lg font-semibold tabular-nums text-ink-900">{{ summary.entries }}</p>
        </div>
      </div>
      <div class="card flex items-center gap-3 p-4">
        <span class="flex h-9 w-9 items-center justify-center rounded-lg bg-rose-50 text-rose-600">
          <ArrowUpFromLine :size="17" />
        </span>
        <div>
          <p class="text-[11px] uppercase tracking-wide text-ink-400">Salidas del mes</p>
          <p class="text-lg font-semibold tabular-nums text-ink-900">{{ summary.exits }}</p>
        </div>
      </div>
      <div class="card flex items-center gap-3 p-4">
        <span class="flex h-9 w-9 items-center justify-center rounded-lg bg-brand-50 text-brand-600">
          <ListChecks :size="17" />
        </span>
        <div>
          <p class="text-[11px] uppercase tracking-wide text-ink-400">Movimientos totales</p>
          <p class="text-lg font-semibold tabular-nums text-ink-900">{{ total }}</p>
        </div>
      </div>
    </div>

    <div class="card mt-5">
      <div class="flex flex-wrap items-center gap-3 border-b border-ink-200 px-4 py-3.5">
        <div class="relative min-w-[200px] flex-1">
          <Search :size="15" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
          <input
            v-model="filters.search"
            type="search"
            class="input py-1.5 pl-9 text-sm"
            placeholder="Buscar producto, motivo o documento…"
            @input="debouncedLoad"
          />
        </div>

        <select v-model="filters.type" class="input w-auto py-1.5 text-sm" @change="load(1)">
          <option :value="null">Entradas y salidas</option>
          <option value="Entrada">Solo entradas</option>
          <option value="Salida">Solo salidas</option>
        </select>

        <select v-model="filters.warehouseId" class="input w-auto py-1.5 text-sm" @change="load(1)">
          <option :value="null">Todos los almacenes</option>
          <option v-for="w in warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
        </select>

        <div class="flex items-center gap-1.5 text-sm text-ink-500">
          <input v-model="filters.from" type="date" class="input w-auto py-1.5 text-[13px]" @change="load(1)" />
          <span>—</span>
          <input v-model="filters.to" type="date" class="input w-auto py-1.5 text-[13px]" @change="load(1)" />
        </div>
      </div>

      <div v-if="loading" class="space-y-2 p-4">
        <div v-for="i in 6" :key="i" class="h-11 animate-pulse rounded bg-ink-100"></div>
      </div>

      <div v-else-if="!items.length" class="px-4">
        <EmptyState
          title="No hay movimientos registrados"
          description="Registre una entrada o salida de mercadería para comenzar el historial de inventario."
        />
      </div>

      <div v-else class="overflow-x-auto">
        <table class="table-base min-w-[1050px]">
          <thead>
            <tr>
              <th>Fecha</th>
              <th>Tipo</th>
              <th>Producto</th>
              <th>Almacén</th>
              <th class="text-right">Cantidad</th>
              <th class="text-right">Total</th>
              <th class="text-right">Stock resultante</th>
              <th>Motivo</th>
              <th>Usuario</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="m in items" :key="m.id">
              <td class="whitespace-nowrap text-[13px] text-ink-500">{{ formatDate(m.date) }}</td>
              <td><StatusBadge :status="m.type" /></td>
              <td>
                <p class="font-medium text-ink-800">{{ m.productName }}</p>
                <p class="font-mono text-[11px] text-ink-400">{{ m.productCode }}</p>
              </td>
              <td class="text-ink-500">{{ m.warehouseName }}</td>
              <td
                class="text-right font-semibold tabular-nums"
                :class="m.type === 'Entrada' ? 'text-emerald-600' : 'text-rose-600'"
              >
                {{ m.type === 'Entrada' ? '+' : '−' }}{{ m.quantity }}
              </td>
              <td class="text-right tabular-nums text-ink-600">{{ money(m.total) }}</td>
              <!-- Existencia tras aplicar esta fila: la entrada suma y la salida resta. -->
              <td class="text-right tabular-nums text-ink-700">{{ m.stockAfter }}</td>
              <td>
                <p class="max-w-[220px] truncate text-[13px] text-ink-600" :title="m.reason">{{ m.reason }}</p>
                <p v-if="m.documentReference" class="text-[11px] text-brand-600">{{ m.documentReference }}</p>
              </td>
              <td class="whitespace-nowrap text-[13px] text-ink-500">{{ m.userName }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- El número de página llega como argumento de load() vía update:model-value. -->
      <Pagination
        v-if="!loading && items.length"
        v-model="filters.page"
        :total="total"
        :page-size="filters.pageSize"
        @update:model-value="load"
      />
    </div>

    <BaseModal
      v-model="modal"
      title="Registrar movimiento"
      subtitle="El stock se actualiza automáticamente al confirmar"
      size="md"
      @close="reset"
    >
      <div class="space-y-4">
        <div>
          <label class="label">Tipo de movimiento *</label>
          <div class="grid grid-cols-2 gap-2 rounded-lg bg-ink-100 p-1">
            <button
              type="button"
              class="flex items-center justify-center gap-2 rounded-md py-2 text-sm font-medium transition"
              :class="form.type === 'Entrada' ? 'bg-white text-emerald-700 shadow-sm' : 'text-ink-500 hover:text-ink-700'"
              @click="form.type = 'Entrada'"
            >
              <ArrowDownToLine :size="16" />
              Entrada
            </button>
            <button
              type="button"
              class="flex items-center justify-center gap-2 rounded-md py-2 text-sm font-medium transition"
              :class="form.type === 'Salida' ? 'bg-white text-rose-700 shadow-sm' : 'text-ink-500 hover:text-ink-700'"
              @click="form.type = 'Salida'"
            >
              <ArrowUpFromLine :size="16" />
              Salida
            </button>
          </div>
        </div>

        <div @focusout="onProductFocusOut">
          <label class="label">Producto *</label>
          <input
            v-model="productSearch"
            class="input"
            placeholder="Escriba para filtrar por código o nombre…"
            @focus="productOpen = true"
          />
          <div
            v-if="productOpen"
            class="mt-1 max-h-52 overflow-y-auto rounded-md border border-ink-200 bg-white shadow-card"
          >
            <button
              v-for="p in filteredProducts"
              :key="p.id"
              type="button"
              class="flex w-full items-center justify-between gap-3 px-3 py-2 text-left text-sm transition hover:bg-ink-50"
              :class="form.productId === p.id ? 'bg-brand-50' : ''"
              @click="selectProduct(p)"
            >
              <span class="min-w-0">
                <span class="block truncate font-medium text-ink-800">{{ p.name }}</span>
                <span class="font-mono text-[11px] text-ink-400">{{ p.code }}</span>
              </span>
              <span class="shrink-0 text-[11px] text-ink-400">
                {{ p.totalStock }} {{ p.unit.toLowerCase() }}
              </span>
            </button>
            <p v-if="!filteredProducts.length" class="px-3 py-4 text-center text-[13px] text-ink-400">
              Sin coincidencias
            </p>
          </div>
        </div>

        <div>
          <label class="label">Almacén destino/origen *</label>
          <select v-model="form.warehouseId" class="input" required>
            <option disabled :value="null">Seleccione…</option>
            <option v-for="w in warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
          </select>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="label">Cantidad *</label>
            <input v-model.number="form.quantity" type="number" min="1" class="input" required />
            <p v-if="availableHint" class="mt-1 text-[11px] text-ink-400">{{ availableHint }}</p>
          </div>
          <div>
            <label class="label">Precio unitario</label>
            <input
              v-model.number="form.unitPrice"
              type="number"
              min="0"
              step="0.01"
              class="input"
              placeholder="Según catálogo"
            />
          </div>
        </div>

        <div>
          <label class="label">Motivo *</label>
          <input v-model.trim="form.reason" class="input" placeholder="Ej: Ingreso por compra a proveedor" required />
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="label">Documento de referencia</label>
            <input v-model.trim="form.documentReference" class="input" placeholder="OC-1234 / GD-5678" />
          </div>
          <div>
            <label class="label">Fecha</label>
            <input v-model="form.date" type="date" class="input" />
          </div>
        </div>

        <p v-if="formError" class="rounded-md bg-rose-50 px-3 py-2.5 text-[13px] text-rose-700 ring-1 ring-inset ring-rose-600/20">
          {{ formError }}
        </p>
      </div>

      <template #footer>
        <button class="btn-secondary" @click="modal = false">Cancelar</button>
        <button class="btn-primary" :disabled="saving" @click="save">
          <LoaderCircle v-if="saving" :size="16" class="animate-spin" />
          {{ saving ? 'Registrando…' : 'Registrar movimiento' }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
// Vista de movimientos: lista paginada de entradas y salidas con filtros combinados,
// resumen del mes en curso y alta de movimientos que actualizan el stock del almacén.
import { computed, onMounted, reactive, ref, watch } from 'vue'
import {
  ArrowDownToLine,
  ArrowUpFromLine,
  ListChecks,
  LoaderCircle,
  Plus,
  Search,
} from '@lucide/vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ExportMenu from '@/components/ui/ExportMenu.vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import Pagination from '@/components/ui/Pagination.vue'
import StatusBadge from '@/components/ui/StatusBadge.vue'
import { createMovement, getMovements } from '@/api/operations'
import { getProducts } from '@/api/products'
import { getWarehouses } from '@/api/catalogs'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'

const auth = useAuthStore()
const toast = useToastStore()

const items = ref([])
const total = ref(0)
const loading = ref(true)
const saving = ref(false)
const modal = ref(false)
const formError = ref('')

const products = ref([])
const warehouses = ref([])

const productSearch = ref('')
const productOpen = ref(false)

// Filtros combinados de la tabla: los selects y las fechas recargan desde la
// página 1, mientras que la búsqueda se envía con debounce.
const filters = reactive({
  search: '',
  type: null,
  warehouseId: null,
  from: '',
  to: '',
  page: 1,
  pageSize: 15,
})

const summary = reactive({ entries: 0, exits: 0 })

const form = reactive({
  type: 'Entrada',
  productId: null,
  warehouseId: null,
  quantity: 1,
  reason: '',
  documentReference: '',
  unitPrice: null,
  date: '',
})

// Opciones del selector de producto: solo activos, coincidencia por nombre o
// código y tope de 50 para mantener el desplegable ágil.
const filteredProducts = computed(() => {
  const term = productSearch.value.trim().toLowerCase()
  const list = products.value.filter((p) => p.isActive !== false)
  if (!term) return list.slice(0, 50)
  return list
    .filter((p) => p.name.toLowerCase().includes(term) || p.code.toLowerCase().includes(term))
    .slice(0, 50)
})

const selectedProduct = computed(() => products.value.find((p) => p.id === form.productId) || null)

// Stock del producto elegido en el almacén seleccionado: orienta la cantidad
// máxima que puede salir en una Salida.
const availableHint = computed(() => {
  const p = selectedProduct.value
  if (!p || !form.warehouseId) return ''
  const stock = p.stockByWarehouse.find((s) => s.warehouseId === form.warehouseId) || { quantity: 0 }
  return `Disponible en el almacén seleccionado: ${stock.quantity} ${p.unit.toLowerCase()}`
})

// Filtros vigentes que se replican en la exportación del listado.
const exportParams = computed(() => ({
  search: filters.search,
  type: filters.type,
  warehouseId: filters.warehouseId,
  from: filters.from || undefined,
  to: filters.to || undefined,
}))

// Espera 350 ms tras la última tecla antes de recargar, para no saturar el backend.
let timer = null
function debouncedLoad() {
  clearTimeout(timer)
  timer = setTimeout(() => load(1), 350)
}

// Carga la página pedida y, en paralelo, los movimientos del mes en curso que
// alimentan las tarjetas del resumen (limitados a 200: el resumen es aproximado
// si el mes supera ese volumen) e independientes de los filtros de la tabla.
async function load(page) {
  if (page) filters.page = page
  loading.value = true
  try {
    const [result, monthStats] = await Promise.all([
      getMovements({
        search: filters.search || undefined,
        type: filters.type || undefined,
        warehouseId: filters.warehouseId ?? undefined,
        from: filters.from || undefined,
        to: filters.to || undefined,
        page: filters.page,
        pageSize: filters.pageSize,
      }),
      getMovements({
        from: monthStart(),
        pageSize: 200,
      }),
    ])

    items.value = result.items
    total.value = result.total

    summary.entries = monthStats.items.filter((m) => m.type === 'Entrada').reduce((a, m) => a + m.quantity, 0)
    summary.exits = monthStats.items.filter((m) => m.type === 'Salida').reduce((a, m) => a + m.quantity, 0)
  } catch (e) {
    toast.error(e.message)
  } finally {
    loading.value = false
  }
}

// Primer día del mes en curso en formato YYYY-MM-DD (zona local).
function monthStart() {
  const now = new Date()
  return new Date(now.getFullYear(), now.getMonth(), 1).toISOString().slice(0, 10)
}

function selectProduct(p) {
  form.productId = p.id
  productSearch.value = `${p.code} — ${p.name}`
  productOpen.value = false
}

// El desplegable se cierra al salir del bloque, con 120 ms de margen para que el
// clic en una opción llegue al botón antes de que el DOM del listado desaparezca.
function onProductFocusOut(e) {
  const container = e.currentTarget
  setTimeout(() => {
    if (!container.contains(document.activeElement)) productOpen.value = false
  }, 120)
}

// Preselecciona el primer almacén activo y deja el formulario en modo Entrada.
function openCreate() {
  formError.value = ''
  Object.assign(form, {
    type: 'Entrada',
    productId: null,
    warehouseId: warehouses.value.find((w) => w.isActive)?.id ?? null,
    quantity: 1,
    reason: '',
    documentReference: '',
    unitPrice: null,
    date: '',
  })
  productSearch.value = ''
  modal.value = true
}

function reset() {
  formError.value = ''
}

async function save() {
  // Validación en cliente antes de enviar; el backend vuelve a comprobar stock y permisos.
  formError.value = ''
  if (!form.productId) {
    formError.value = 'Seleccione el producto involucrado.'
    return
  }
  if (!form.warehouseId) {
    formError.value = 'Seleccione el almacén.'
    return
  }
  if (!form.quantity || form.quantity < 1) {
    formError.value = 'La cantidad debe ser mayor a cero.'
    return
  }
  if (!form.reason) {
    formError.value = 'Indique el motivo del movimiento.'
    return
  }

  saving.value = true
  try {
    // La fecha se envía a mediodía local para que la conversión a ISO no corrija
    // el día por la zona horaria; el backend aplica el movimiento y recalcula el stock.
    await createMovement({
      type: form.type,
      productId: form.productId,
      warehouseId: form.warehouseId,
      quantity: form.quantity,
      reason: form.reason,
      documentReference: form.documentReference || null,
      unitPrice: form.unitPrice || null,
      date: form.date ? new Date(`${form.date}T12:00:00`).toISOString() : null,
    })
    toast.success(`Movimiento de ${form.type.toLowerCase()} registrado y stock actualizado.`)
    modal.value = false
    await Promise.all([load(), refreshProducts()])
  } catch (e) {
    formError.value = e.message
  } finally {
    saving.value = false
  }
}

// Se recarga tras cada movimiento porque cambia el stock que enseñan el
// desplegable de productos y la pista de disponibilidad.
async function refreshProducts() {
  products.value = await getProducts({ pageSize: 200, includeInactive: false }).then((r) => r.items)
}

const formatDate = (value) =>
  new Date(value).toLocaleString('es-CL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })

const money = (v) =>
  new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP', maximumFractionDigits: 0 }).format(v || 0)

onMounted(async () => {
  await load()
  const [wh] = await Promise.all([getWarehouses()])
  warehouses.value = wh
  await refreshProducts()
})
</script>
