<template>
  <div>
    <PageHeader
      section="Sistema"
      title="Usuarios y roles"
      description="Administre las cuentas de acceso y los permisos asociados a cada rol del sistema."
    >
      <template #actions>
        <ExportMenu report="users" :params="{ search }" />
        <button class="btn-primary" @click="openCreate">
          <UserPlus :size="16" />
          Nuevo usuario
        </button>
      </template>
    </PageHeader>

    <div class="mt-6 grid gap-4 sm:grid-cols-3">
      <div v-for="r in roles" :key="r.id" class="card p-4">
        <div class="flex items-center gap-2.5">
          <span class="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-50 text-brand-600">
            <ShieldCheck :size="16" />
          </span>
          <div>
            <p class="text-sm font-semibold text-ink-800">{{ r.name }}</p>
            <p class="text-[12px] text-ink-500">{{ r.description }}</p>
          </div>
        </div>
      </div>
    </div>

    <div class="card mt-5">
      <div class="border-b border-ink-200 px-4 py-3.5">
        <div class="relative max-w-sm">
          <Search :size="15" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
          <input
            v-model="search"
            type="search"
            class="input py-1.5 pl-9 text-sm"
            placeholder="Buscar usuario, nombre o correo…"
            @input="debouncedLoad"
          />
        </div>
      </div>

      <div v-if="loading" class="space-y-2 p-4">
        <div v-for="i in 5" :key="i" class="h-11 animate-pulse rounded bg-ink-100"></div>
      </div>

      <div v-else-if="!items.length" class="px-4">
        <EmptyState title="Sin usuarios" description="No se encontraron cuentas que coincidan con la búsqueda." />
      </div>

      <div v-else class="overflow-x-auto">
        <table class="table-base min-w-[820px]">
          <thead>
            <tr>
              <th>Usuario</th>
              <th>Nombre completo</th>
              <th>Correo</th>
              <th>Rol</th>
              <th>Estado</th>
              <th>Creado</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in items" :key="u.id" :class="!u.isActive ? 'opacity-60' : ''">
              <td class="font-mono text-[13px] text-ink-700">{{ u.username }}</td>
              <td class="font-medium text-ink-800">{{ u.fullName }}</td>
              <td class="text-ink-500">{{ u.email || '—' }}</td>
              <td><span class="badge badge-info">{{ u.role }}</span></td>
              <td><StatusBadge :status="u.isActive ? 'active' : 'inactive'" /></td>
              <td class="whitespace-nowrap text-[13px] text-ink-500">
                {{ new Date(u.createdAt).toLocaleDateString('es-CL') }}
              </td>
              <td>
                <div class="flex items-center justify-end gap-1">
                  <button
                    class="rounded-md p-1.5 text-ink-400 transition hover:bg-brand-50 hover:text-brand-600"
                    title="Editar usuario"
                    @click="openEdit(u)"
                  >
                    <Pencil :size="16" />
                  </button>
                  <button
                    class="rounded-md p-1.5 text-ink-400 transition hover:bg-rose-50 hover:text-rose-600"
                    title="Eliminar usuario"
                    :disabled="u.id === auth.user?.id"
                    @click="askDelete(u)"
                  >
                    <Trash2 :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <Pagination
        v-if="!loading && items.length"
        v-model="page"
        :total="total"
        :page-size="pageSize"
        @update:model-value="load"
      />
    </div>

    <BaseModal v-model="modal" :title="form.id ? 'Editar usuario' : 'Nuevo usuario'" size="md" @close="reset">
      <div class="grid gap-4 sm:grid-cols-2">
        <div v-if="!form.id">
          <label class="label">Usuario *</label>
          <input v-model.trim="form.username" class="input font-mono lowercase" placeholder="jperez" required />
        </div>
        <div :class="form.id ? 'sm:col-span-2' : ''">
          <label class="label">Nombre completo *</label>
          <input v-model.trim="form.fullName" class="input" placeholder="Nombre y apellido" required />
        </div>
        <div>
          <label class="label">Correo</label>
          <input v-model.trim="form.email" type="email" class="input" placeholder="usuario@sistema.com" />
        </div>
        <div>
          <label class="label">Rol *</label>
          <select v-model="form.roleId" class="input" required>
            <option disabled :value="null">Seleccione…</option>
            <option v-for="r in roles" :key="r.id" :value="r.id">{{ r.name }}</option>
          </select>
        </div>
        <div>
          <label class="label">{{ form.id ? 'Nueva contraseña (opcional)' : 'Contraseña *' }}</label>
          <input
            v-model="form.password"
            type="password"
            class="input"
            :placeholder="form.id ? 'Dejar en blanco para mantener' : 'Mínimo 6 caracteres'"
            :required="!form.id"
            autocomplete="new-password"
          />
        </div>
        <label class="flex items-center gap-2 self-end text-sm text-ink-600">
          <input v-model="form.isActive" type="checkbox" class="h-4 w-4 rounded border-ink-300 text-brand-600 focus:ring-brand-500" />
          Cuenta activa
        </label>
      </div>

      <template #footer>
        <button class="btn-secondary" @click="modal = false">Cancelar</button>
        <button class="btn-primary" :disabled="saving" @click="save">
          <LoaderCircle v-if="saving" :size="16" class="animate-spin" />
          Guardar usuario
        </button>
      </template>
    </BaseModal>

    <ConfirmDialog
      v-model="confirm"
      title="Eliminar usuario"
      danger
      :headline="`¿Eliminar la cuenta '${selected?.username}'?`"
      message="La cuenta dejará de tener acceso inmediatamente. Los movimientos históricos se conservan."
      confirm-text="Eliminar"
      :action="remove"
    />
  </div>
</template>

<script setup>
// Gestión de usuarios: listado paginado con búsqueda y alta/edición/baja de cuentas.
// La ruta es adminOnly, así que solo Admin llega aquí; el rol elegido en el formulario
// (Admin, Usuario, Auditor) es el que el backend usará para autorizar cada petición.
// Estados de la vista: cargando, guardando, modal de formulario y diálogo de borrado.
import { onMounted, reactive, ref } from 'vue'
import { LoaderCircle, Pencil, Search, ShieldCheck, Trash2, UserPlus } from '@lucide/vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ExportMenu from '@/components/ui/ExportMenu.vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import Pagination from '@/components/ui/Pagination.vue'
import StatusBadge from '@/components/ui/StatusBadge.vue'
import { createUser, deleteUser, getRoles, getUsers, updateUser } from '@/api/operations'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'

const auth = useAuthStore()
const toast = useToastStore()

const items = ref([])
const roles = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = 15
const loading = ref(true)
const saving = ref(false)
const modal = ref(false)
const confirm = ref(false)
const selected = ref(null)
const search = ref('')

const empty = () => ({ id: null, username: '', password: '', fullName: '', email: '', roleId: null, isActive: true })
const form = reactive(empty())

// 350 ms de espera tras la última tecla para no disparar una petición por cada carácter.
let timer = null
function debouncedLoad() {
  clearTimeout(timer)
  timer = setTimeout(() => load(1), 350)
}

async function load(p = page.value) {
  page.value = p
  loading.value = true
  try {
    const data = await getUsers({ search: search.value || undefined, page: p, pageSize })
    items.value = data.items
    total.value = data.total
  } catch (e) {
    toast.error(e.message)
  } finally {
    loading.value = false
  }
}

function openCreate() {
  // Preselecciona roles[1] ("Usuario") para que un alta rápida no quede con rol Admin.
  Object.assign(form, empty(), { roleId: roles.value[1]?.id ?? null })
  modal.value = true
}

function openEdit(u) {
  Object.assign(form, {
    id: u.id,
    username: u.username,
    password: '',
    fullName: u.fullName,
    email: u.email || '',
    roleId: u.roleId,
    isActive: u.isActive,
  })
  modal.value = true
}

function reset() {
  Object.assign(form, empty())
}

async function save() {
  saving.value = true
  try {
    if (form.id) {
      await updateUser(form.id, {
        // Contraseña vacía en edición → null, señal para el backend de que se mantiene la actual.
        password: form.password || null,
        fullName: form.fullName,
        email: form.email || null,
        roleId: form.roleId,
        isActive: form.isActive,
      })
      toast.success('Usuario actualizado correctamente.')
    } else {
      await createUser({ ...form })
      toast.success('Usuario creado correctamente.')
    }
    modal.value = false
    await load()
  } catch (e) {
    toast.error(e.message)
  } finally {
    saving.value = false
  }
}

function askDelete(u) {
  // El botón ya viene deshabilitado para la cuenta activa; aquí se avisa en lugar de fallar después.
  if (u.id === auth.user?.id) {
    toast.info('No puede eliminar la cuenta desde la que está sesión.')
    return
  }
  selected.value = u
  confirm.value = true
}

async function remove() {
  await deleteUser(selected.value.id)
  toast.success('Usuario eliminado.')
  confirm.value = false
  await load()
}

onMounted(async () => {
  roles.value = await getRoles()
  await load()
})
</script>
