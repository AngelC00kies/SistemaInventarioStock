<template>
  <div>
    <PageHeader
      section="Catálogos"
      title="Productos"
      description="Alta, modificación y consulta del catálogo de productos con su stock por almacén."
    >
      <template #actions>
        <ExportMenu report="products" :params="exportParams" />
        <button v-if="auth.canWrite" class="btn-primary" @click="openCreate">
          <Plus :size="16" />
          Nuevo producto
        </button>
      </template>
    </PageHeader>

    <div class="card mt-6">
      <div class="flex flex-wrap items-center gap-3 border-b border-ink-200 px-4 py-3.5">
        <div class="relative min-w-[220px] flex-1">
          <Search :size="15" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
          <input
            v-model="filters.search"
            type="search"
            class="input py-1.5 pl-9 text-sm"
            placeholder="Buscar por código, nombre o descripción…"
            @input="debouncedLoad"
          />
        </div>

        <select v-model="filters.categoryId" class="input w-auto py-1.5 text-sm" @change="load(1)">
          <option :value="null">Todas las categorías</option>
          <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>

        <select v-model="filters.supplierId" class="input w-auto py-1.5 text-sm" @change="load(1)">
          <option :value="null">Todos los proveedores</option>
          <option v-for="s in suppliers" :key="s.id" :value="s.id">{{ s.name }}</option>
        </select>

        <label class="flex cursor-pointer select-none items-center gap-2 text-[13px] text-ink-600">
          <input v-model="filters.onlyLowStock" type="checkbox" class="h-4 w-4 rounded border-ink-300 text-brand-600 focus:ring-brand-500" @change="load(1)" />
          Solo stock bajo
        </label>
      </div>

      <div v-if="loading" class="space-y-2 p-4">
        <div v-for="i in 6" :key="i" class="h-11 animate-pulse rounded bg-ink-100"></div>
      </div>

      <div v-else-if="!items.length" class="px-4">
        <EmptyState
          title="No hay productos registrados"
          description="Ajuste los filtros o registre un nuevo producto para comenzar a controlar el inventario."
        />
      </div>

      <div v-else class="overflow-x-auto">
        <table class="table-base min-w-[900px]">
          <thead>
            <tr>
              <th>Código</th>
              <th>Producto</th>
              <th>Proveedor</th>
              <th class="w-[190px]">Stock total</th>
              <th class="text-right">Mínimo</th>
              <th class="text-right">P. compra</th>
              <th class="text-right">P. venta</th>
              <th>Estado</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in items" :key="p.id" :class="!p.isActive ? 'opacity-60' : ''">
              <td class="font-mono text-[12.5px] text-ink-500">{{ p.code }}</td>
              <td>
                <p class="font-medium text-ink-800">{{ p.name }}</p>
                <p class="text-xs text-ink-400">{{ p.categoryName }}</p>
              </td>
              <td class="text-ink-500">{{ p.supplierName || '—' }}</td>
              <td>
                <div class="flex items-center gap-2">
                  <span class="w-9 text-right font-semibold tabular-nums" :class="stockText(p.status)">
                    {{ p.totalStock }}
                  </span>
                  <div class="h-1.5 flex-1 overflow-hidden rounded-full bg-ink-100">
                    <div
                      class="h-full rounded-full transition-all"
                      :class="stockBar(p.status)"
                      :style="{ width: stockWidth(p) + '%' }"
                    ></div>
                  </div>
                </div>
                <p class="mt-0.5 text-[11px] text-ink-400">{{ p.unit }}</p>
              </td>
              <td class="text-right tabular-nums text-ink-500">{{ p.minStock }}</td>
              <td class="text-right tabular-nums text-ink-500">{{ money(p.purchasePrice) }}</td>
              <td class="text-right tabular-nums font-medium text-ink-700">{{ money(p.salePrice) }}</td>
              <td><StatusBadge :status="p.isActive ? p.status : 'inactive'" /></td>
              <td>
                <div class="flex items-center justify-end gap-1">
                  <button
                    v-if="auth.canWrite"
                    class="rounded-md p-1.5 text-ink-400 transition hover:bg-brand-50 hover:text-brand-600"
                    title="Editar producto"
                    @click="openEdit(p)"
                  >
                    <Pencil :size="16" />
                  </button>
                  <button
                    v-if="auth.canWrite"
                    class="rounded-md p-1.5 text-ink-400 transition hover:bg-rose-50 hover:text-rose-600"
                    title="Eliminar producto"
                    @click="askDelete(p)"
                  >
                    <Trash2 :size="16" />
                  </button>
                  <button
                    class="rounded-md p-1.5 text-ink-400 transition hover:bg-ink-100 hover:text-ink-700"
                    title="Ver detalle"
                    @click="openDetail(p)"
                  >
                    <Eye :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

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
      :title="form.id ? 'Editar producto' : 'Nuevo producto'"
      subtitle="Los campos marcados con * son obligatorios"
      size="lg"
      @close="reset"
    >
      <form class="grid gap-4 sm:grid-cols-2" @submit.prevent="save">
        <div>
          <label class="label">Código *</label>
          <input v-model.trim="form.code" class="input font-mono uppercase" placeholder="PRD-0013" required />
        </div>
        <div>
          <label class="label">Nombre *</label>
          <input v-model.trim="form.name" class="input" placeholder="Nombre comercial del producto" required />
        </div>

        <div class="sm:col-span-2">
          <label class="label">Descripción</label>
          <textarea v-model.trim="form.description" rows="2" class="input resize-none" placeholder="Especificaciones, modelo u observaciones"></textarea>
        </div>

        <div>
          <label class="label">Categoría *</label>
          <select v-model="form.categoryId" class="input" required>
            <option disabled value="">Seleccione…</option>
            <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>

        <div>
          <label class="label">Proveedor</label>
          <select v-model="form.supplierId" class="input">
            <option :value="null">Sin proveedor asignado</option>
            <option v-for="s in suppliers" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>

        <div>
          <label class="label">Unidad de medida *</label>
          <select v-model="form.unit" class="input">
            <option v-for="u in units" :key="u">{{ u }}</option>
          </select>
        </div>

        <div>
          <label class="label">Stock mínimo *</label>
          <input v-model.number="form.minStock" type="number" min="0" class="input" required />
        </div>

        <div>
          <label class="label">Precio de compra *</label>
          <input v-model.number="form.purchasePrice" type="number" min="0" step="0.01" class="input" required />
        </div>

        <div>
          <label class="label">Precio de venta *</label>
          <input v-model.number="form.salePrice" type="number" min="0" step="0.01" class="input" required />
        </div>

        <label class="flex items-center gap-2 text-sm text-ink-600 sm:col-span-2">
          <input v-model="form.isActive" type="checkbox" class="h-4 w-4 rounded border-ink-300 text-brand-600 focus:ring-brand-500" />
          Producto activo (visible para registrar movimientos)
        </label>
      </form>

      <template #footer>
        <button class="btn-secondary" @click="modal = false">Cancelar</button>
        <button class="btn-primary" :disabled="saving" @click="save">
          <LoaderCircle v-if="saving" :size="16" class="animate-spin" />
          {{ saving ? 'Guardando…' : 'Guardar producto' }}
        </button>
      </template>
    </BaseModal>

    <BaseModal v-model="detail" :title="selected?.name || 'Detalle de producto'" size="md">
      <template v-if="selected">
        <dl class="grid grid-cols-2 gap-x-6 gap-y-3 text-sm">
          <div><dt class="text-xs text-ink-400">Código</dt><dd class="font-mono text-ink-800">{{ selected.code }}</dd></div>
          <div><dt class="text-xs text-ink-400">Categoría</dt><dd class="text-ink-800">{{ selected.categoryName }}</dd></div>
          <div><dt class="text-xs text-ink-400">Proveedor</dt><dd class="text-ink-800">{{ selected.supplierName || '—' }}</dd></div>
          <div><dt class="text-xs text-ink-400">Unidad</dt><dd class="text-ink-800">{{ selected.unit }}</dd></div>
          <div><dt class="text-xs text-ink-400">Precio compra</dt><dd class="text-ink-800">{{ money(selected.purchasePrice) }}</dd></div>
          <div><dt class="text-xs text-ink-400">Precio venta</dt><dd class="text-ink-800">{{ money(selected.salePrice) }}</dd></div>
        </dl>

        <p v-if="selected.description" class="mt-4 rounded-md bg-ink-50 p-3 text-[13px] leading-relaxed text-ink-600">
          {{ selected.description }}
        </p>

        <h4 class="mt-5 text-xs font-semibold uppercase tracking-wide text-ink-500">
          Distribución por almacén
        </h4>
        <ul class="mt-2 space-y-2">
          <li
            v-for="s in selected.stockByWarehouse"
            :key="s.warehouseId"
            class="flex items-center justify-between rounded-md border border-ink-200 px-3 py-2 text-sm"
          >
            <span class="text-ink-600">{{ s.warehouseName }}</span>
            <span class="font-semibold tabular-nums text-ink-900">{{ s.quantity }} {{ selected.unit.toLowerCase() }}</span>
          </li>
          <li v-if="!selected.stockByWarehouse.length" class="text-sm text-ink-400">
            Este producto aún no tiene stock registrado.
          </li>
        </ul>
      </template>
    </BaseModal>

    <ConfirmDialog
      v-model="confirm"
      title="Eliminar producto"
      danger
      :headline="`¿Eliminar '${selected?.name}'?`"
      message="Esta acción no se puede deshacer. Si el producto tiene movimientos registrados, se marcará como inactivo en lugar de eliminarse."
      confirm-text="Eliminar"
      :action="remove"
    />
  </div>
</template>

<script setup>
// Catálogo de productos: listado paginado con filtros combinados, alta y edición,
// detalle con stock por almacén y eliminación que el backend puede degradar a baja lógica.
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { Eye, LoaderCircle, Pencil, Plus, Search, Trash2 } from '@lucide/vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ExportMenu from '@/components/ui/ExportMenu.vue'
import Pagination from '@/components/ui/Pagination.vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import StatusBadge from '@/components/ui/StatusBadge.vue'
import { createProduct, deleteProduct, getProducts, updateProduct } from '@/api/products'
import { getCategories, getSuppliers } from '@/api/catalogs'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'

const auth = useAuthStore()
const toast = useToastStore()
const route = useRoute()

const items = ref([])
const total = ref(0)
const loading = ref(true)
const saving = ref(false)
const modal = ref(false)
const detail = ref(false)
const confirm = ref(false)
const selected = ref(null)

const categories = ref([])
const suppliers = ref([])
const units = ['Unidad', 'Caja', 'Resma', 'Set', 'Bidón', 'Bobina', 'Paquete', 'Kilogramo', 'Metro', 'Par']

// Filtros combinados: cada select o checkbox recarga desde la página 1 y la
// búsqueda de texto espera 350 ms antes de disparar la petición.
const filters = reactive({
  search: '',
  categoryId: null,
  supplierId: null,
  onlyLowStock: false,
  page: 1,
  pageSize: 15,
})

// Fábrica del formulario vacío: se usa para crear, para reiniciar al cerrar el
// modal y como estado inicial de `form`.
const emptyForm = () => ({
  id: null,
  code: '',
  name: '',
  description: '',
  categoryId: '',
  supplierId: null,
  purchasePrice: 0,
  salePrice: 0,
  unit: 'Unidad',
  minStock: 0,
  isActive: true,
})

const form = reactive(emptyForm())

// Filtros visibles que acompañan a la exportación; la paginación no se exporta.
const exportParams = computed(() => ({
  search: filters.search,
  categoryId: filters.categoryId,
  supplierId: filters.supplierId,
}))

// Agrupa las teclas durante 350 ms para no lanzar una petición por carácter.
let timer = null
function debouncedLoad() {
  clearTimeout(timer)
  timer = setTimeout(() => load(1), 350)
}

// Todos los filtros viajan en la misma petición; los nulos se mandan como
// `undefined` para que el backend los omita en lugar de filtrar por ellos.
async function load(page) {
  if (page) filters.page = page
  loading.value = true
  try {
    const result = await getProducts({
      search: filters.search || undefined,
      categoryId: filters.categoryId ?? undefined,
      supplierId: filters.supplierId ?? undefined,
      onlyLowStock: filters.onlyLowStock || undefined,
      page: filters.page,
      pageSize: filters.pageSize,
    })
    items.value = result.items
    total.value = result.total
  } catch (e) {
    toast.error(e.message)
  } finally {
    loading.value = false
  }
}

function openCreate() {
  Object.assign(form, emptyForm())
  modal.value = true
}

function openEdit(p) {
  Object.assign(form, {
    id: p.id,
    code: p.code,
    name: p.name,
    description: p.description || '',
    categoryId: p.categoryId,
    supplierId: p.supplierId,
    purchasePrice: p.purchasePrice,
    salePrice: p.salePrice,
    unit: p.unit,
    minStock: p.minStock,
    isActive: p.isActive,
  })
  modal.value = true
}

function openDetail(p) {
  selected.value = p
  detail.value = true
}

function reset() {
  Object.assign(form, emptyForm())
}

// La presencia de `form.id` distingue actualización de alta; el select de
// categoría entrega un string, por eso se convierte a número antes de enviar.
async function save() {
  saving.value = true
  try {
    const payload = { ...form, categoryId: Number(form.categoryId), supplierId: form.supplierId || null }
    if (form.id) {
      await updateProduct(form.id, payload)
      toast.success('Los datos del producto fueron actualizados.')
    } else {
      await createProduct(payload)
      toast.success('El producto fue registrado correctamente.')
    }
    modal.value = false
    reset()
    await load()
  } catch (e) {
    toast.error(e.message)
  } finally {
    saving.value = false
  }
}

function askDelete(p) {
  selected.value = p
  confirm.value = true
}

// El backend puede convertir la baja en una marcación como inactivo cuando el
// producto ya tiene movimientos asociados, en vez de eliminarlo.
async function remove() {
  await deleteProduct(selected.value.id)
  toast.success('El producto fue eliminado.')
  confirm.value = false
  await load()
}

const money = (v) =>
  new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP', maximumFractionDigits: 0 }).format(v || 0)

// La barra se mide contra el doble del stock mínimo: al 50 % el stock está
// justo en el mínimo; al 100 % lo duplica (o es máximo con mínimo en cero).
const stockWidth = (p) => {
  const target = Math.max(p.minStock * 2, 1)
  return Math.min(100, Math.round((p.totalStock / target) * 100))
}
// Colores derivados del estado que calcula el backend (ok/low/critical/empty).
const stockBar = (s) =>
  ({ ok: 'bg-emerald-500', low: 'bg-amber-500', critical: 'bg-rose-500', empty: 'bg-ink-300' })[s] || 'bg-ink-300'
const stockText = (s) =>
  ({ ok: 'text-ink-800', low: 'text-amber-600', critical: 'text-rose-600', empty: 'text-ink-400' })[s] || 'text-ink-800'

// Los query (?search=, ?low=) permiten enlazar desde el dashboard ya filtrado.
onMounted(async () => {
  if (route.query.search) filters.search = route.query.search
  if (route.query.low) filters.onlyLowStock = true

  const [cats, sups] = await Promise.all([getCategories(), getSuppliers()])
  categories.value = cats
  suppliers.value = sups
  await load()
})

// Refleja en los filtros los query de navegación externa; el chequeo de
// `route.name` evita recargar si la query cambia estando en una ruta hija.
watch(
  () => route.query,
  () => {
    if (route.name === 'products') {
      filters.search = route.query.search || ''
      filters.onlyLowStock = !!route.query.low
      load(1)
    }
  }
)
</script>
