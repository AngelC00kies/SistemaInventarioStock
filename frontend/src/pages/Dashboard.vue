<template>
  <div>
    <div class="kpi-grid">
      <div class="kpi">
        <div>
          <div class="label">Productos activos</div>
          <div class="value">{{ data.activeProducts }}</div>
        </div>
        <div class="icon">🏷️</div>
      </div>

      <div class="kpi success">
        <div>
          <div class="label">Unidades en stock</div>
          <div class="value">{{ formatNumber(data.totalUnits) }}</div>
        </div>
        <div class="icon">📦</div>
      </div>

      <div class="kpi">
        <div>
          <div class="label">Valor de inventario</div>
          <div class="value">{{ formatMoney(data.inventoryValue) }}</div>
        </div>
        <div class="icon">💰</div>
      </div>

      <div class="kpi danger">
        <div>
          <div class="label">Stock crítico</div>
          <div class="value">{{ data.lowStockCount }}</div>
        </div>
        <div class="icon">⚠️</div>
      </div>

      <div class="kpi warning">
        <div>
          <div class="label">Movimientos hoy</div>
          <div class="value">{{ data.movementsToday }}</div>
        </div>
        <div class="icon">🔄</div>
      </div>

      <div class="kpi">
        <div>
          <div class="label">Almacenes / Proveedores</div>
          <div class="value" style="font-size:20px">
            {{ data.warehouses }} / {{ data.suppliers }}
          </div>
        </div>
        <div class="icon">🏬</div>
      </div>
    </div>

    <div class="grid-2">
      <div class="card">
        <div class="card-header">
          <h2>Últimos movimientos</h2>
          <router-link to="/movements" class="btn secondary sm">Ver todos</router-link>
        </div>
        <div class="table-wrap">
          <table v-if="data.recentMovements.length">
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Tipo</th>
                <th>Producto</th>
                <th class="num">Cant.</th>
                <th>Usuario</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="m in data.recentMovements" :key="m.id">
                <td class="muted">{{ formatDate(m.date) }}</td>
                <td>
                  <span class="pill" :class="m.type === 'Entrada' ? 'green' : 'red'">
                    {{ m.type }}
                  </span>
                </td>
                <td>{{ m.productName }}</td>
                <td class="num">{{ m.quantity }}</td>
                <td class="muted">{{ m.userName }}</td>
              </tr>
            </tbody>
          </table>
          <div v-else class="empty">Sin movimientos registrados</div>
        </div>
      </div>

      <div class="card">
        <div class="card-header">
          <h2>Alertas de stock bajo</h2>
          <router-link to="/notifications" class="btn secondary sm">Ver todas</router-link>
        </div>
        <div class="table-wrap">
          <table v-if="data.lowStockAlerts.length">
            <thead>
              <tr>
                <th>Producto</th>
                <th>Almacén</th>
                <th class="num">Actual</th>
                <th class="num">Mínimo</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="n in data.lowStockAlerts" :key="n.id">
                <td>{{ n.productName }}</td>
                <td class="muted">{{ n.warehouseName }}</td>
                <td class="num">
                  <span class="pill red">{{ n.quantityAtDetection }}</span>
                </td>
                <td class="num">{{ n.minimumStock }}</td>
              </tr>
            </tbody>
          </table>
          <div v-else class="empty">🎉 No hay alertas pendientes</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, reactive } from 'vue'
import api from '../api/client'

const data = reactive({
  activeProducts: 0,
  totalProducts: 0,
  totalUnits: 0,
  inventoryValue: 0,
  lowStockCount: 0,
  movementsToday: 0,
  entriesThisMonth: 0,
  exitsThisMonth: 0,
  warehouses: 0,
  suppliers: 0,
  categories: 0,
  recentMovements: [],
  lowStockAlerts: [],
})

onMounted(async () => {
  const { data: d } = await api.get('/dashboard')
  Object.assign(data, d)
})

const formatNumber = (n) => new Intl.NumberFormat('es').format(n || 0)
const formatMoney = (n) =>
  new Intl.NumberFormat('es', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(n || 0)

const formatDate = (d) =>
  new Date(d).toLocaleString('es', {
    day: '2-digit',
    month: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
</script>
