<template>
  <div class="card">
    <div class="card-header">
      <h2>Categorías de productos</h2>
      <button v-if="auth.canEdit" class="btn" @click="open()">+ Nueva categoría</button>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Descripción</th>
            <th class="num">Productos</th>
            <th>Estado</th>
            <th style="text-align:right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in items" :key="c.id">
            <td><strong>{{ c.name }}</strong></td>
            <td class="wrap muted">{{ c.description || '—' }}</td>
            <td class="num">{{ c.productCount }}</td>
            <td>
              <span class="pill" :class="c.isActive ? 'green' : 'gray'">
                {{ c.isActive ? 'Activa' : 'Inactiva' }}
              </span>
            </td>
            <td style="text-align:right;white-space:nowrap">
              <button v-if="auth.canEdit" class="btn ghost sm" @click="open(c)">✏️</button>
              <button v-if="auth.isAdmin" class="btn ghost sm" @click="remove(c)">🗑️</button>
            </td>
          </tr>
          <tr v-if="items.length === 0">
            <td colspan="5" class="empty">No hay categorías registradas</td>
          </tr>
        </tbody>
      </table>
    </div>

    <Modal v-if="show" :title="form.id ? 'Editar categoría' : 'Nueva categoría'" @close="show = false">
      <div class="field" style="margin-bottom:16px">
        <label>Nombre <span class="req">*</span></label>
        <input v-model.trim="form.name" type="text" maxlength="100" placeholder="Ej: Electrónica" />
      </div>
      <div class="field">
        <label>Descripción</label>
        <textarea v-model.trim="form.description" maxlength="500" placeholder="Descripción opcional"></textarea>
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

const form = reactive({ id: null, name: '', description: '' })

async function load() {
  const { data } = await api.get('/categories', { params: { includeInactive: true } })
  items.value = data
}

function open(c = null) {
  form.id = c?.id ?? null
  form.name = c?.name ?? ''
  form.description = c?.description ?? ''
  show.value = true
}

async function save() {
  saving.value = true
  try {
    const payload = { name: form.name, description: form.description || null }
    if (form.id) {
      await api.put(`/categories/${form.id}`, payload)
      ui.notify('Categoría actualizada')
    } else {
      await api.post('/categories', payload)
      ui.notify('Categoría creada')
    }
    show.value = false
    load()
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  } finally {
    saving.value = false
  }
}

async function remove(c) {
  if (!confirm(`¿Eliminar la categoría "${c.name}"?`)) return
  try {
    await api.delete(`/categories/${c.id}`)
    ui.notify('Categoría eliminada')
    load()
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  }
}

onMounted(load)
</script>
