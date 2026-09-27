<template>
  <div class="card">
    <div class="card-header">
      <div class="filters">
        <div class="field">
          <label>Buscar</label>
          <input
            v-model.trim="search"
            type="text"
            placeholder="Usuario, nombre o email…"
            @keyup.enter="reload"
          />
        </div>
        <button class="btn secondary" @click="reload">🔍 Buscar</button>
      </div>

      <button class="btn" @click="open()">+ Nuevo usuario</button>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Usuario</th>
            <th>Nombre completo</th>
            <th>Email</th>
            <th>Rol</th>
            <th>Estado</th>
            <th style="text-align:right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="u in items" :key="u.id">
            <td><strong>{{ u.userName }}</strong></td>
            <td>{{ u.fullName }}</td>
            <td class="muted">{{ u.email }}</td>
            <td><span class="pill blue">{{ u.role }}</span></td>
            <td>
              <span class="pill" :class="u.isActive ? 'green' : 'gray'">
                {{ u.isActive ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td style="text-align:right">
              <button class="btn ghost sm" @click="open(u)">✏️</button>
            </td>
          </tr>
          <tr v-if="items.length === 0">
            <td colspan="6" class="empty">No hay usuarios</td>
          </tr>
        </tbody>
      </table>
    </div>

    <Pagination v-model="page" :total="total" :page-size="pageSize" />

    <Modal
      v-if="show"
      :title="form.id ? 'Editar usuario' : 'Nuevo usuario'"
      @close="show = false"
    >
      <div class="field" style="margin-bottom:16px">
        <label>Nombre completo <span class="req">*</span></label>
        <input v-model.trim="form.fullName" type="text" maxlength="150" />
      </div>

      <template v-if="!form.id">
        <div class="field" style="margin-bottom:16px">
          <label>Usuario <span class="req">*</span></label>
          <input v-model.trim="form.userName" type="text" maxlength="50" autocomplete="off" />
        </div>
        <div class="field" style="margin-bottom:16px">
          <label>Email <span class="req">*</span></label>
          <input v-model.trim="form.email" type="email" maxlength="150" />
        </div>
      </template>

      <div class="field" style="margin-bottom:16px">
        <label>Rol <span class="req">*</span></label>
        <select v-model="form.role">
          <option v-for="r in roles" :key="r" :value="r">{{ r }}</option>
        </select>
        <span class="hint">
          Admin: acceso total · Usuario: registra productos y movimientos · Auditor: solo lectura
        </span>
      </div>

      <div class="field" style="margin-bottom:16px">
        <label>{{ form.id ? 'Nueva contraseña (opcional)' : 'Contraseña *' }}</label>
        <input
          v-model="form.password"
          type="password"
          autocomplete="new-password"
          :placeholder="form.id ? 'Dejar vacío para mantener la actual' : 'Mín. 8 caracteres, mayúscula y número'"
        />
      </div>

      <div v-if="form.id" class="field">
        <label>Estado</label>
        <select v-model="form.isActive">
          <option :value="true">Activo</option>
          <option :value="false">Inactivo</option>
        </select>
      </div>

      <template #footer>
        <button class="btn secondary" @click="show = false">Cancelar</button>
        <button class="btn" :disabled="saving" @click="save">
          {{ saving ? 'Guardando…' : 'Guardar' }}
        </button>
      </template>
    </Modal>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref, watch } from 'vue'
import api, { errorMessage } from '../api/client'
import { useUiStore } from '../stores/ui'
import Modal from '../components/Modal.vue'
import Pagination from '../components/Pagination.vue'

const ui = useUiStore()

const items = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = 20
const search = ref('')
const show = ref(false)
const saving = ref(false)
const roles = ref(['Admin', 'Usuario', 'Auditor'])

const form = reactive({
  id: null,
  userName: '',
  fullName: '',
  email: '',
  password: '',
  role: 'Usuario',
  isActive: true,
})

async function load() {
  const { data } = await api.get('/users', {
    params: { page: page.value, pageSize, search: search.value || undefined },
  })
  items.value = data.items
  total.value = data.totalCount
}

function reload() {
  page.value = 1
  load()
}

function open(u = null) {
  Object.assign(form, {
    id: u?.id ?? null,
    userName: u?.userName ?? '',
    fullName: u?.fullName ?? '',
    email: u?.email ?? '',
    password: '',
    role: u?.role ?? 'Usuario',
    isActive: u?.isActive ?? true,
  })
  show.value = true
}

async function save() {
  saving.value = true
  try {
    if (form.id) {
      await api.put(`/users/${form.id}`, {
        fullName: form.fullName,
        email: form.email,
        role: form.role,
        isActive: form.isActive,
        newPassword: form.password || null,
      })
      ui.notify('Usuario actualizado')
    } else {
      await api.post('/users', {
        userName: form.userName,
        fullName: form.fullName,
        email: form.email,
        password: form.password,
        role: form.role,
      })
      ui.notify('Usuario creado')
    }
    show.value = false
    load()
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  } finally {
    saving.value = false
  }
}

watch(page, load)

onMounted(async () => {
  load()
  const r = await api.get('/users/roles').catch(() => ({ data: roles.value }))
  if (Array.isArray(r.data) && r.data.length) roles.value = r.data
})
</script>
