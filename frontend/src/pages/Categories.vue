<template>
  <div>
    <PageHeader
      section="Catálogos"
      title="Categorías"
      description="Agrupación lógica de productos para consultas, reportes y análisis de stock."
    >
      <template #actions>
        <ExportMenu report="categories" />
        <button v-if="auth.canWrite" class="btn-primary" @click="openCreate">
          <Plus :size="16" />
          Nueva categoría
        </button>
      </template>
    </PageHeader>

    <div class="card mt-6">
      <div class="flex flex-wrap items-center gap-3 border-b border-ink-200 px-4 py-3.5">
        <div class="relative min-w-[220px] flex-1">
          <Search :size="15" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
          <input v-model="search" type="search" class="input py-1.5 pl-9 text-sm" placeholder="Buscar categoría…" />
        </div>
        <label class="flex cursor-pointer select-none items-center gap-2 text-[13px] text-ink-600">
          <input v-model="includeInactive" type="checkbox" class="h-4 w-4 rounded border-ink-300 text-brand-600 focus:ring-brand-500" />
          Incluir inactivas
        </label>
      </div>

      <div v-if="loading" class="space-y-2 p-4">
        <div v-for="i in 5" :key="i" class="h-11 animate-pulse rounded bg-ink-100"></div>
      </div>

      <div v-else-if="!filtered.length" class="px-4">
        <EmptyState title="No hay categorías" description="Cree categorías para organizar el catálogo de productos." />
      </div>
      <div v-else class="overflow-x-auto">
        <table class="table-base">
          <thead>
            <tr>
              <th>Categoría</th>
              <th>Descripción</th>
              <th class="text-right">Productos</th>
              <th>Estado</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in filtered" :key="c.id" :class="!c.isActive ? 'opacity-60' : ''">
              <td class="font-medium text-ink-800">{{ c.name }}</td>
              <td class="max-w-md text-ink-500">{{ c.description || '—' }}</td>
              <td class="text-right tabular-nums text-ink-700">{{ c.productCount }}</td>
              <td><StatusBadge :status="c.isActive ? 'active' : 'inactive'" /></td>
              <td>
                <div class="flex items-center justify-end gap-1">
                  <button
                    v-if="auth.canWrite"
                    class="rounded-md p-1.5 text-ink-400 transition hover:bg-brand-50 hover:text-brand-600"
                    title="Editar"
                    @click="openEdit(c)"
                  >
                    <Pencil :size="16" />
                  </button>
                  <button
                    v-if="auth.canWrite"
                    class="rounded-md p-1.5 text-ink-400 transition hover:bg-rose-50 hover:text-rose-600"
                    title="Eliminar"
                    @click="askDelete(c)"
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

    <BaseModal v-model="modal" :title="form.id ? 'Editar categoría' : 'Nueva categoría'" size="sm" @close="reset">
      <div class="space-y-4">
        <div>
          <label class="label">Nombre *</label>
          <input v-model.trim="form.name" class="input" placeholder="Ej: Electrónica" required />
        </div>
        <div>
          <label class="label">Descripción</label>
          <textarea v-model.trim="form.description" rows="3" class="input resize-none" placeholder="Detalle opcional de la categoría"></textarea>
        </div>
        <label class="flex items-center gap-2 text-sm text-ink-600">
          <input v-model="form.isActive" type="checkbox" class="h-4 w-4 rounded border-ink-300 text-brand-600 focus:ring-brand-500" />
          Categoría activa
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
      title="Eliminar categoría"
      danger
      :headline="`¿Eliminar '${selected?.name}'?`"
      message="Si la categoría tiene productos asociados se marcará como inactiva para conservar la integridad del historial."
      confirm-text="Eliminar"
      :action="remove"
    />
  </div>
</template>

<script setup>
// CRUD de categorías: listado con búsqueda client-side y alta/edición en modal. El borrado se
// degrada a inactivación cuando la categoría tiene productos, para no romper el historial.
// Las acciones de escritura solo aparecen con auth.canWrite (roles Admin y Usuario).
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { LoaderCircle, Pencil, Plus, Search, Trash2 } from '@lucide/vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ExportMenu from '@/components/ui/ExportMenu.vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import StatusBadge from '@/components/ui/StatusBadge.vue'
import { createCategory, deleteCategory, getCategories, updateCategory } from '@/api/catalogs'
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

const form = reactive({ id: null, name: '', description: '', isActive: true })

// Filtro de texto client-side: la API solo recibe includeInactive, nunca el término de búsqueda.
const filtered = computed(() => {
  const term = search.value.trim().toLowerCase()
  if (!term) return items.value
  return items.value.filter((c) => c.name.toLowerCase().includes(term))
})

async function load() {
  loading.value = true
  try {
    items.value = await getCategories({ includeInactive: includeInactive.value })
  } catch (e) {
    toast.error(e.message)
  } finally {
    loading.value = false
  }
}

function openCreate() {
  Object.assign(form, { id: null, name: '', description: '', isActive: true })
  modal.value = true
}

function openEdit(c) {
  Object.assign(form, { id: c.id, name: c.name, description: c.description || '', isActive: c.isActive })
  modal.value = true
}

function reset() {
  Object.assign(form, { id: null, name: '', description: '', isActive: true })
}

async function save() {
  if (!form.name) return
  saving.value = true
  try {
    const payload = { name: form.name, description: form.description, isActive: form.isActive }
    if (form.id) {
      await updateCategory(form.id, payload)
      toast.success('Categoría actualizada.')
    } else {
      await createCategory(payload)
      toast.success('Categoría creada.')
    }
    modal.value = false
    await load()
  } catch (e) {
    toast.error(e.message)
  } finally {
    saving.value = false
  }
}

function askDelete(c) {
  selected.value = c
  confirm.value = true
}

async function remove() {
  await deleteCategory(selected.value.id)
  toast.success('Categoría eliminada.')
  confirm.value = false
  await load()
}

// Recarga al alternar "incluir inactivas": ese filtro lo aplica el servidor, no el cliente.
watch(includeInactive, load)

onMounted(load)
</script>
