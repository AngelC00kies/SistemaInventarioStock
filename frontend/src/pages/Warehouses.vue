<template>
  <div>
    <PageHeader
      section="Catálogos"
      title="Almacenes"
      description="Defina los almacenes o bodegas donde se controla el stock de productos."
    >
      <template #actions>
        <button v-if="auth.canWrite" class="btn-primary" @click="openCreate">
          <Plus :size="16" />
          Nuevo almacén
        </button>
      </template>
    </PageHeader>

    <div class="card mt-6">
      <div class="flex flex-wrap items-center gap-3 border-b border-ink-200 px-4 py-3.5">
        <div class="relative min-w-[220px] flex-1">
          <Search :size="15" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
          <input v-model="search" type="search" class="input py-1.5 pl-9 text-sm" placeholder="Buscar almacén o código…" />
        </div>
        <label class="flex cursor-pointer select-none items-center gap-2 text-[13px] text-ink-600">
          <input v-model="includeInactive" type="checkbox" class="h-4 w-4 rounded border-ink-300 text-brand-600 focus:ring-brand-500" />
          Incluir inactivos
        </label>
      </div>

      <div v-if="loading" class="space-y-2 p-4">
        <div v-for="i in 4" :key="i" class="h-11 animate-pulse rounded bg-ink-100"></div>
      </div>

      <div v-else-if="!filtered.length" class="px-4">
        <EmptyState title="No hay almacenes" description="Registre al menos un almacén para comenzar a controlar el stock." />
      </div>

      <div v-else class="grid gap-4 p-4 sm:grid-cols-2 xl:grid-cols-3">
        <article
          v-for="w in filtered"
          :key="w.id"
          class="rounded-xl border p-4 transition"
          :class="w.isActive ? 'border-ink-200 bg-white hover:border-brand-300' : 'border-ink-200 bg-ink-50 opacity-70'"
        >
          <div class="flex items-start justify-between gap-3">
            <span class="flex h-10 w-10 items-center justify-center rounded-lg bg-brand-50 text-brand-600">
              <Warehouse :size="19" />
            </span>
            <StatusBadge :status="w.isActive ? 'active' : 'inactive'" />
          </div>

          <h3 class="mt-3 text-sm font-semibold text-ink-900">{{ w.name }}</h3>
          <p class="font-mono text-[11px] uppercase tracking-wide text-ink-400">{{ w.code }}</p>
          <p class="mt-1 text-[13px] text-ink-500">{{ w.location || 'Sin ubicación registrada' }}</p>

          <div class="mt-4 flex items-center justify-between border-t border-ink-100 pt-3">
            <div>
              <p class="text-lg font-semibold tabular-nums text-ink-900">{{ w.totalUnits.toLocaleString('es-CL') }}</p>
              <p class="text-[11px] uppercase tracking-wide text-ink-400">unidades · {{ w.productCount }} SKU</p>
            </div>

            <div v-if="auth.canWrite" class="flex gap-1">
              <button
                class="rounded-md p-1.5 text-ink-400 transition hover:bg-brand-50 hover:text-brand-600"
                title="Editar"
                @click="openEdit(w)"
              >
                <Pencil :size="16" />
              </button>
              <button
                class="rounded-md p-1.5 text-ink-400 transition hover:bg-rose-50 hover:text-rose-600"
                title="Eliminar"
                @click="askDelete(w)"
              >
                <Trash2 :size="16" />
              </button>
            </div>
          </div>
        </article>
      </div>
    </div>

    <BaseModal v-model="modal" :title="form.id ? 'Editar almacén' : 'Nuevo almacén'" size="sm" @close="reset">
      <div class="space-y-4">
        <div>
          <label class="label">Nombre *</label>
          <input v-model.trim="form.name" class="input" placeholder="Ej: Almacén Central" required />
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="label">Código *</label>
            <input v-model.trim="form.code" class="input font-mono uppercase" placeholder="ALM-04" required />
          </div>
          <div>
            <label class="label">Ubicación</label>
            <input v-model.trim="form.location" class="input" placeholder="Ciudad / sector" />
          </div>
        </div>
        <label class="flex items-center gap-2 text-sm text-ink-600">
          <input v-model="form.isActive" type="checkbox" class="h-4 w-4 rounded border-ink-300 text-brand-600 focus:ring-brand-500" />
          Almacén activo
        </label>
      </div>

      <template #footer>
        <button class="btn-secondary" @click="modal = false">Cancelar</button>
        <button class="btn-primary" :disabled="saving" @click="save">
          <LoaderCircle v-if="saving" :size="16" class="animate-spin" />
          Guardar
        </button>
      </template>
    </BaseModal>

    <ConfirmDialog
      v-model="confirm"
      title="Eliminar almacén"
      danger
      :headline="`¿Eliminar '${selected?.name}'?`"
      message="Si el almacén presenta movimientos o stock positivo quedará marcado como inactivo."
      confirm-text="Eliminar"
      :action="remove"
    />
  </div>
</template>

<script setup>
// CRUD de almacenes en formato de tarjetas: nombre, código, ubicación y ocupación actual
// (unidades y SKU). El borrado se degrada a inactivación si hay movimientos o stock positivo.
// Las acciones de escritura solo aparecen con auth.canWrite (roles Admin y Usuario).
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { LoaderCircle, Pencil, Plus, Search, Trash2, Warehouse } from '@lucide/vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import StatusBadge from '@/components/ui/StatusBadge.vue'
import { createWarehouse, deleteWarehouse, getWarehouses, updateWarehouse } from '@/api/catalogs'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import { useWarehouseStore } from '@/stores/warehouses'

const auth = useAuthStore()
const toast = useToastStore()
const warehouseStore = useWarehouseStore()

const items = ref([])
const loading = ref(true)
const saving = ref(false)
const modal = ref(false)
const confirm = ref(false)
const selected = ref(null)
const search = ref('')
const includeInactive = ref(true)

const empty = () => ({ id: null, name: '', code: '', location: '', isActive: true })
const form = reactive(empty())

// Filtro de texto client-side por nombre o código; el servidor solo recibe includeInactive.
const filtered = computed(() => {
  const term = search.value.trim().toLowerCase()
  if (!term) return items.value
  return items.value.filter(
    (w) => w.name.toLowerCase().includes(term) || w.code.toLowerCase().includes(term)
  )
})

async function load() {
  loading.value = true
  try {
    items.value = await getWarehouses({ includeInactive: includeInactive.value })
    // Fuerza la recarga del store compartido para que el selector del Topbar y el resto de la
    // app vean de inmediato los cambios hechos aquí (el store cachea en memoria).
    await warehouseStore.load(true)
  } catch (e) {
    toast.error(e.message)
  } finally {
    loading.value = false
  }
}

function openCreate() {
  Object.assign(form, empty())
  modal.value = true
}

function openEdit(w) {
  Object.assign(form, { ...empty(), ...w })
  modal.value = true
}

function reset() {
  Object.assign(form, empty())
}

async function save() {
  if (!form.name || !form.code) return
  saving.value = true
  try {
    const payload = { name: form.name, code: form.code, location: form.location, isActive: form.isActive }
    if (form.id) {
      await updateWarehouse(form.id, payload)
      toast.success('Almacén actualizado.')
    } else {
      await createWarehouse(payload)
      toast.success('Almacén creado.')
    }
    modal.value = false
    await load()
  } catch (e) {
    toast.error(e.message)
  } finally {
    saving.value = false
  }
}

function askDelete(w) {
  selected.value = w
  confirm.value = true
}

async function remove() {
  await deleteWarehouse(selected.value.id)
  toast.success('Almacén eliminado.')
  confirm.value = false
  await load()
}

// Recarga al alternar "incluir inactivos": ese filtro lo aplica el servidor, no el cliente.
watch(includeInactive, load)

onMounted(load)
</script>
