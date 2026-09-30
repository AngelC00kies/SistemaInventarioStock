<template>
  <div>
    <PageHeader
      section="Catálogos"
      title="Proveedores"
      description="Registro de proveedores y sus datos de contacto para la gestión de compras."
    >
      <template #actions>
        <ExportMenu report="suppliers" :params="{ search }" />
        <button v-if="auth.canWrite" class="btn-primary" @click="openCreate">
          <Plus :size="16" />
          Nuevo proveedor
        </button>
      </template>
    </PageHeader>

    <div class="card mt-6">
      <div class="flex flex-wrap items-center gap-3 border-b border-ink-200 px-4 py-3.5">
        <div class="relative min-w-[220px] flex-1">
          <Search :size="15" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
          <input v-model="search" type="search" class="input py-1.5 pl-9 text-sm" placeholder="Buscar por nombre o contacto…" />
        </div>
        <label class="flex cursor-pointer select-none items-center gap-2 text-[13px] text-ink-600">
          <input v-model="includeInactive" type="checkbox" class="h-4 w-4 rounded border-ink-300 text-brand-600 focus:ring-brand-500" />
          Incluir inactivos
        </label>
      </div>

      <div v-if="loading" class="space-y-2 p-4">
        <div v-for="i in 5" :key="i" class="h-11 animate-pulse rounded bg-ink-100"></div>
      </div>

      <div v-else-if="!filtered.length" class="px-4">
        <EmptyState title="No hay proveedores" description="Registre proveedores para asociarlos a los productos del catálogo." />
      </div>

      <div v-else class="overflow-x-auto">
        <table class="table-base min-w-[900px]">
          <thead>
            <tr>
              <th>Proveedor</th>
              <th>Contacto</th>
              <th>Teléfono</th>
              <th>Correo</th>
              <th class="text-right">Productos</th>
              <th>Estado</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in filtered" :key="s.id" :class="!s.isActive ? 'opacity-60' : ''">
              <td>
                <p class="font-medium text-ink-800">{{ s.name }}</p>
                <p class="text-xs text-ink-400">{{ s.address || 'Sin dirección registrada' }}</p>
              </td>
              <td class="text-ink-600">{{ s.contactName || '—' }}</td>
              <td class="whitespace-nowrap text-ink-600">{{ s.phone || '—' }}</td>
              <td>
                <a v-if="s.email" :href="`mailto:${s.email}`" class="text-brand-600 hover:underline">{{ s.email }}</a>
                <span v-else class="text-ink-400">—</span>
              </td>
              <td class="text-right tabular-nums text-ink-700">{{ s.productCount }}</td>
              <td><StatusBadge :status="s.isActive ? 'active' : 'inactive'" /></td>
              <td>
                <div class="flex items-center justify-end gap-1">
                  <button
                    v-if="auth.canWrite"
                    class="rounded-md p-1.5 text-ink-400 transition hover:bg-brand-50 hover:text-brand-600"
                    title="Editar"
                    @click="openEdit(s)"
                  >
                    <Pencil :size="16" />
                  </button>
                  <button
                    v-if="auth.canWrite"
                    class="rounded-md p-1.5 text-ink-400 transition hover:bg-rose-50 hover:text-rose-600"
                    title="Eliminar"
                    @click="askDelete(s)"
                  >
                    <Trash2 :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <BaseModal v-model="modal" :title="form.id ? 'Editar proveedor' : 'Nuevo proveedor'" size="md" @close="reset">
      <div class="grid gap-4 sm:grid-cols-2">
        <div class="sm:col-span-2">
          <label class="label">Razón social o nombre *</label>
          <input v-model.trim="form.name" class="input" placeholder="Ej: Distribuidora Nacional S.A." required />
        </div>
        <div>
          <label class="label">Persona de contacto</label>
          <input v-model.trim="form.contactName" class="input" placeholder="Nombre del contacto" />
        </div>
        <div>
          <label class="label">Teléfono</label>
          <input v-model.trim="form.phone" class="input" placeholder="+56 2 2345 6789" />
        </div>
        <div>
          <label class="label">Correo electrónico</label>
          <input v-model.trim="form.email" type="email" class="input" placeholder="ventas@proveedor.cl" />
        </div>
        <div>
          <label class="label">Dirección</label>
          <input v-model.trim="form.address" class="input" placeholder="Calle, número, ciudad" />
        </div>
        <label class="flex items-center gap-2 text-sm text-ink-600 sm:col-span-2">
          <input v-model="form.isActive" type="checkbox" class="h-4 w-4 rounded border-ink-300 text-brand-600 focus:ring-brand-500" />
          Proveedor activo
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
      title="Eliminar proveedor"
      danger
      :headline="`¿Eliminar '${selected?.name}'?`"
      message="Si el proveedor está asociado a productos, quedará marcado como inactivo."
      confirm-text="Eliminar"
      :action="remove"
    />
  </div>
</template>

<script setup>
// CRUD de proveedores: listado con búsqueda client-side, alta/edición en modal y borrado con
// confirmación. Si el proveedor tiene productos asociados, el backend lo deja inactivo en vez
// de eliminarlo (el diálogo ya anticipa esa consecuencia).
// Las acciones de escritura solo aparecen con auth.canWrite (roles Admin y Usuario).
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { LoaderCircle, Pencil, Plus, Search, Trash2 } from '@lucide/vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ExportMenu from '@/components/ui/ExportMenu.vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import StatusBadge from '@/components/ui/StatusBadge.vue'
import { createSupplier, deleteSupplier, getSuppliers, updateSupplier } from '@/api/catalogs'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'

const auth = useAuthStore()
const toast = useToastStore()

const items = ref([])
const loading = ref(true)
const saving = ref(false)
const modal = ref(false)
const confirm = ref(false)
const selected = ref(null)
const search = ref('')
const includeInactive = ref(true)

const empty = () => ({ id: null, name: '', contactName: '', phone: '', email: '', address: '', isActive: true })
const form = reactive(empty())

// Filtro de texto client-side: la API solo recibe includeInactive, nunca el término de búsqueda.
const filtered = computed(() => {
  const term = search.value.trim().toLowerCase()
  if (!term) return items.value
  return items.value.filter(
    (s) => s.name.toLowerCase().includes(term) || (s.contactName || '').toLowerCase().includes(term)
  )
})

async function load() {
  loading.value = true
  try {
    items.value = await getSuppliers({ includeInactive: includeInactive.value })
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

function openEdit(s) {
  // Parte de empty() para que un campo ausente no herede el valor del registro anterior.
  Object.assign(form, { ...empty(), ...s })
  modal.value = true
}

function reset() {
  Object.assign(form, empty())
}

async function save() {
  if (!form.name) return
  saving.value = true
  try {
    const payload = {
      name: form.name,
      contactName: form.contactName,
      phone: form.phone,
      email: form.email,
      address: form.address,
      isActive: form.isActive,
    }
    if (form.id) {
      await updateSupplier(form.id, payload)
      toast.success('Proveedor actualizado.')
    } else {
      await createSupplier(payload)
      toast.success('Proveedor creado.')
    }
    modal.value = false
    await load()
  } catch (e) {
    toast.error(e.message)
  } finally {
    saving.value = false
  }
}

function askDelete(s) {
  selected.value = s
  confirm.value = true
}

async function remove() {
  await deleteSupplier(selected.value.id)
  toast.success('Proveedor eliminado.')
  confirm.value = false
  await load()
}

// Recarga al alternar "incluir inactivos": ese filtro lo aplica el servidor, no el cliente.
watch(includeInactive, load)

onMounted(load)
</script>
