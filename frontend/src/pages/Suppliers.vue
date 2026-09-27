<template>
  <div class="card">
    <div class="card-header">
      <div class="filters">
        <div class="field">
          <label>Buscar</label>
          <input
            v-model.trim="search"
            type="text"
            placeholder="Nombre, contacto o email…"
            @keyup.enter="reload"
          />
        </div>
        <button class="btn secondary" @click="reload">🔍 Buscar</button>
      </div>

      <button v-if="auth.canEdit" class="btn" @click="open()">+ Nuevo proveedor</button>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Contacto</th>
            <th>Teléfono</th>
            <th>Email</th>
            <th class="num">Productos</th>
            <th>Estado</th>
            <th style="text-align:right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="s in items" :key="s.id">
            <td><strong>{{ s.name }}</strong></td>
            <td>{{ s.contactName || '—' }}</td>
            <td class="muted">{{ s.phone || '—' }}</td>
            <td class="muted">{{ s.email || '—' }}</td>
            <td class="num">{{ s.productCount }}</td>
            <td>
              <span class="pill" :class="s.isActive ? 'green' : 'gray'">
                {{ s.isActive ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td style="text-align:right;white-space:nowrap">
              <button v-if="auth.canEdit" class="btn ghost sm" @click="open(s)">✏️</button>
              <button v-if="auth.isAdmin" class="btn ghost sm" @click="remove(s)">🗑️</button>
            </td>
          </tr>
          <tr v-if="items.length === 0">
            <td colspan="7" class="empty">No hay proveedores registrados</td>
          </tr>
        </tbody>
      </table>
    </div>

    <Pagination v-model="page" :total="total" :page-size="pageSize" />

    <Modal
      v-if="show"
      :title="form.id ? 'Editar proveedor' : 'Nuevo proveedor'"
      wide
      @close="show = false"
    >
      <div class="form-grid">
        <div class="field full">
          <label>Nombre <span class="req">*</span></label>
          <input v-model.trim="form.name" type="text" maxlength="150" />
        </div>
        <div class="field">
          <label>Contacto</label>
          <input v-model.trim="form.contactName" type="text" maxlength="150" />
        </div>
        <div class="field">
          <label>Teléfono</label>
          <input v-model.trim="form.phone" type="text" maxlength="50" />
        </div>
        <div class="field">
          <label>Email</label>
          <input v-model.trim="form.email" type="email" maxlength="150" />
        </div>
        <div class="field full">
          <label>Dirección</label>
          <input v-model.trim="form.address" type="text" maxlength="300" />
        </div>
      </div>

      <template #footer>
        <button class="btn secondary" @click="show = false">Cancelar</button>
        <button class="btn" :disabled="saving || !form.name" @click="save">
          {{ saving ? 'Guardando…' : 'Guardar' }}
        </button>
      </template>
    </Modal>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref, watch } from 'vue'
import api, { errorMessage } from '../api/client'
import { useAuthStore } from '../stores/auth'
import { useUiStore } from '../stores/ui'
import Modal from '../components/Modal.vue'
import Pagination from '../components/Pagination.vue'

const auth = useAuthStore()
const ui = useUiStore()

const items = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = 20
const search = ref('')
const show = ref(false)
const saving = ref(false)

const form = reactive({
  id: null,
  name: '',
  contactName: '',
  phone: '',
  email: '',
  address: '',
})

async function load() {
  const { data } = await api.get('/suppliers', {
    params: { page: page.value, pageSize, search: search.value || undefined, includeInactive: true },
  })
  items.value = data.items
  total.value = data.totalCount
}

function reload() {
  page.value = 1
  load()
}

function open(s = null) {
  Object.assign(form, {
    id: s?.id ?? null,
    name: s?.name ?? '',
    contactName: s?.contactName ?? '',
    phone: s?.phone ?? '',
    email: s?.email ?? '',
    address: s?.address ?? '',
  })
  show.value = true
}

async function save() {
  saving.value = true
  try {
    const payload = {
      name: form.name,
      contactName: form.contactName || null,
      phone: form.phone || null,
      email: form.email || null,
      address: form.address || null,
    }

    if (form.id) {
      await api.put(`/suppliers/${form.id}`, payload)
      ui.notify('Proveedor actualizado')
    } else {
      await api.post('/suppliers', payload)
      ui.notify('Proveedor creado')
    }
    show.value = false
    load()
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  } finally {
    saving.value = false
  }
}

async function remove(s) {
  if (!confirm(`¿Eliminar el proveedor "${s.name}"?`)) return
  try {
    await api.delete(`/suppliers/${s.id}`)
    ui.notify('Proveedor eliminado')
    load()
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  }
}

watch(page, load)
onMounted(load)
</script>
