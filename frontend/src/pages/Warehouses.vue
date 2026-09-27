<template>
  <div class="card">
    <div class="card-header">
      <h2>Almacenes</h2>
      <button v-if="auth.canEdit" class="btn" @click="open()">+ Nuevo almacén</button>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Código</th>
            <th>Nombre</th>
            <th>Dirección</th>
            <th class="num">Productos con stock</th>
            <th>Estado</th>
            <th style="text-align:right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="w in items" :key="w.id">
            <td><strong>{{ w.code }}</strong></td>
            <td>{{ w.name }}</td>
            <td class="muted">{{ w.address || '—' }}</td>
            <td class="num">{{ w.productCount }}</td>
            <td>
              <span class="pill" :class="w.isActive ? 'green' : 'gray'">
                {{ w.isActive ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td style="text-align:right;white-space:nowrap">
              <button v-if="auth.canEdit" class="btn ghost sm" @click="open(w)">✏️</button>
              <button v-if="auth.isAdmin" class="btn ghost sm" @click="remove(w)">🗑️</button>
            </td>
          </tr>
          <tr v-if="items.length === 0">
            <td colspan="6" class="empty">No hay almacenes registrados</td>
          </tr>
        </tbody>
      </table>
    </div>

    <Modal v-if="show" :title="form.id ? 'Editar almacén' : 'Nuevo almacén'" @close="show = false">
      <div class="field" style="margin-bottom:16px">
        <label>Código <span class="req">*</span></label>
        <input v-model.trim="form.code" type="text" maxlength="20" placeholder="ALM-01" />
      </div>
      <div class="field" style="margin-bottom:16px">
        <label>Nombre <span class="req">*</span></label>
        <input v-model.trim="form.name" type="text" maxlength="100" placeholder="Almacén Central" />
      </div>
      <div class="field">
        <label>Dirección</label>
        <input v-model.trim="form.address" type="text" maxlength="300" />
      </div>

      <template #footer>
        <button class="btn secondary" @click="show = false">Cancelar</button>
        <button class="btn" :disabled="saving || !form.code || !form.name" @click="save">
          {{ saving ? 'Guardando…' : 'Guardar' }}
        </button>
      </template>
    </Modal>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import api, { errorMessage } from '../api/client'
import { useAuthStore } from '../stores/auth'
import { useUiStore } from '../stores/ui'
import Modal from '../components/Modal.vue'

const auth = useAuthStore()
const ui = useUiStore()

const items = ref([])
const show = ref(false)
const saving = ref(false)

const form = reactive({ id: null, code: '', name: '', address: '' })

async function load() {
  const { data } = await api.get('/warehouses', { params: { includeInactive: true } })
  items.value = data
}

function open(w = null) {
  form.id = w?.id ?? null
  form.code = w?.code ?? ''
  form.name = w?.name ?? ''
  form.address = w?.address ?? ''
  show.value = true
}

async function save() {
  saving.value = true
  try {
    const payload = { code: form.code, name: form.name, address: form.address || null }
    if (form.id) {
      await api.put(`/warehouses/${form.id}`, payload)
      ui.notify('Almacén actualizado')
    } else {
      await api.post('/warehouses', payload)
      ui.notify('Almacén creado')
    }
    show.value = false
    load()
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  } finally {
    saving.value = false
  }
}

async function remove(w) {
  if (!confirm(`¿Eliminar el almacén "${w.name}"?`)) return
  try {
    await api.delete(`/warehouses/${w.id}`)
    ui.notify('Almacén eliminado')
    load()
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  }
}

onMounted(load)
</script>
