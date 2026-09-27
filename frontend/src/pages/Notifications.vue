<template>
  <div class="card">
    <div class="card-header">
      <div class="row">
        <button class="btn" :class="pendingOnly ? '' : 'secondary'" @click="toggle(true)">
          Pendientes ({{ pendingCount }})
        </button>
        <button class="btn" :class="!pendingOnly ? '' : 'secondary'" @click="toggle(false)">
          Todas
        </button>
      </div>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Detectada</th>
            <th>Producto</th>
            <th>Almacén</th>
            <th class="num">Stock actual</th>
            <th class="num">Stock mínimo</th>
            <th>Estado</th>
            <th style="text-align:right">Acciones</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="n in items" :key="n.id">
            <td class="muted">{{ formatDate(n.detectedAt) }}</td>
            <td class="wrap">
              <strong>{{ n.productCode }}</strong> — {{ n.productName }}
            </td>
            <td>{{ n.warehouseName }}</td>
            <td class="num">
              <span class="pill" :class="n.quantityAtDetection <= 0 ? 'red' : 'yellow'">
                {{ n.quantityAtDetection }}
              </span>
            </td>
            <td class="num">{{ n.minimumStock }}</td>
            <td>
              <span class="pill" :class="n.isResolved ? 'green' : 'red'">
                {{ n.isResolved ? 'Atendida' : 'Pendiente' }}
              </span>
            </td>
            <td style="text-align:right;white-space:nowrap">
              <button
                v-if="!n.isResolved && auth.canEdit"
                class="btn sm secondary"
                @click="ack(n)"
              >
                ✓ Marcar atendida
              </button>
            </td>
          </tr>

          <tr v-if="items.length === 0">
            <td colspan="7" class="empty">
              {{ pendingOnly ? '🎉 No hay alertas de stock bajo pendientes' : 'No hay notificaciones' }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import api, { errorMessage } from '../api/client'
import { useAuthStore } from '../stores/auth'
import { useUiStore } from '../stores/ui'

const auth = useAuthStore()
const ui = useUiStore()

const items = ref([])
const pendingOnly = ref(true)
const pendingCount = ref(0)

async function load() {
  const { data } = await api.get('/notifications', {
    params: { pendingOnly: pendingOnly.value },
  })
  items.value = data
  pendingCount.value = data.filter((n) => !n.isResolved).length
  ui.fetchPendingCount()
}

function toggle(value) {
  pendingOnly.value = value
  load()
}

async function ack(n) {
  try {
    await api.post(`/notifications/${n.id}/ack`)
    ui.notify('Notificación marcada como atendida')
    load()
  } catch (e) {
    ui.notify(errorMessage(e), 'error')
  }
}

const formatDate = (d) =>
  new Date(d).toLocaleString('es', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })

onMounted(load)
</script>
