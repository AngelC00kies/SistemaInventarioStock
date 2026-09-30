<template>
  <div>
    <PageHeader
      section="Resumen"
      title="Panel general"
      description="Indicadores clave del inventario y actividad reciente de los almacenes."
    >
      <template #actions>
        <button class="btn-secondary" :disabled="loading" @click="load">
          <RefreshCw :size="16" :class="loading ? 'animate-spin' : ''" />
          Actualizar
        </button>
        <router-link to="/movements" class="btn-primary">
          <Plus :size="16" />
          Registrar movimiento
        </router-link>
      </template>
    </PageHeader>

    <div v-if="loading" class="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <div v-for="i in 4" :key="i" class="card h-32 animate-pulse bg-ink-100"></div>
    </div>

    <template v-else-if="data">
      <!-- KPIs: cobertura del catálogo, unidades disponibles, alertas de reposición y flujo de valor del mes -->
      <div class="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard
          label="Productos activos"
          :value="data.totalProducts"
          :icon="Package"
          :hint="`${data.activeWarehouses} almacenes operando`"
          tone="brand"
        />
        <StatCard
          label="Unidades en stock"
          :value="data.totalUnits"
          :icon="Boxes"
          :hint="`${data.movementsToday} movimientos hoy`"
          tone="violet"
        />
        <StatCard
          label="Productos con stock bajo"
          :value="data.lowStockProducts"
          :icon="TriangleAlert"
          hint="Requieren reposición"
          tone="rose"
        />
        <StatCard
          label="Valor de salidas del mes"
          :value="data.monthlyOutValue"
          :icon="TrendingDown"
          format="money"
          :hint="`Entradas: ${money(data.monthlyInValue)}`"
          tone="amber"
        />
      </div>

      <div class="mt-5 grid gap-5 lg:grid-cols-3">
        <div class="card p-5 lg:col-span-2">
          <div class="mb-4 flex items-center justify-between">
            <div>
              <h2 class="text-sm font-semibold text-ink-800">Flujo de inventario</h2>
              <p class="text-xs text-ink-500">Unidades ingresadas y retiradas — últimos 30 días</p>
            </div>
            <div class="flex items-center gap-4 text-[11px] font-medium text-ink-500">
              <span class="flex items-center gap-1.5">
                <span class="h-2 w-2 rounded-full bg-emerald-500"></span> Entradas
              </span>
              <span class="flex items-center gap-1.5">
                <span class="h-2 w-2 rounded-full bg-rose-500"></span> Salidas
              </span>
            </div>
          </div>
          <div class="h-[260px]">
            <Line :data="flowData" :options="flowOptions" />
          </div>
        </div>

        <div class="card p-5">
          <h2 class="text-sm font-semibold text-ink-800">Stock por categoría</h2>
          <p class="text-xs text-ink-500">Unidades disponibles</p>
          <div class="mt-3 h-[260px]">
            <Doughnut :data="categoryData" :options="categoryOptions" />
          </div>
        </div>
      </div>

      <div class="mt-5 grid gap-5 lg:grid-cols-5">
        <div class="card overflow-hidden lg:col-span-3">
          <div class="flex items-center justify-between border-b border-ink-200 px-5 py-4">
            <div>
              <h2 class="text-sm font-semibold text-ink-800">Productos en nivel crítico</h2>
              <p class="text-xs text-ink-500">Stock igual o inferior al mínimo configurado</p>
            </div>
            <router-link to="/products?low=1" class="text-xs font-medium text-brand-600 hover:text-brand-700">
              Ver todos
            </router-link>
          </div>

          <div v-if="!data.criticalProducts.length" class="px-5 py-10 text-center text-sm text-ink-500">
            No hay productos con stock crítico. ¡Excelente!
          </div>

          <table v-else class="table-base">
            <thead>
              <tr>
                <th>Producto</th>
                <th>Almacén</th>
                <th class="text-right">Stock</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in data.criticalProducts" :key="`${p.productId}-${p.warehouseName}`">
                <td>
                  <p class="font-medium text-ink-800">{{ p.name }}</p>
                  <p class="text-xs text-ink-400">{{ p.code }}</p>
                </td>
                <td class="text-ink-500">{{ p.warehouseName }}</td>
                <td class="text-right font-semibold tabular-nums" :class="stockColor(p.status)">
                  {{ p.quantity }}
                  <span class="text-xs font-normal text-ink-400">/ {{ p.minStock }}</span>
                </td>
                <td><StatusBadge :status="p.status" /></td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="card overflow-hidden lg:col-span-2">
          <div class="flex items-center justify-between border-b border-ink-200 px-5 py-4">
            <div>
              <h2 class="text-sm font-semibold text-ink-800">Movimientos recientes</h2>
              <p class="text-xs text-ink-500">Últimos registros del sistema</p>
            </div>
            <router-link to="/movements" class="text-xs font-medium text-brand-600 hover:text-brand-700">
              Ver historial
            </router-link>
          </div>

          <ul class="divide-y divide-ink-100">
            <li v-for="m in data.recentMovements" :key="m.id" class="flex items-center gap-3 px-5 py-3">
              <span
                class="flex h-8 w-8 shrink-0 items-center justify-center rounded-full"
                :class="m.type === 'Entrada' ? 'bg-emerald-50 text-emerald-600' : 'bg-rose-50 text-rose-600'"
              >
                <ArrowDownToLine v-if="m.type === 'Entrada'" :size="15" />
                <ArrowUpFromLine v-else :size="15" />
              </span>
              <div class="min-w-0 flex-1">
                <p class="truncate text-[13px] font-medium text-ink-800">{{ m.productName }}</p>
                <p class="truncate text-[11px] text-ink-400">
                  {{ m.warehouseName }} · {{ relative(m.date) }}
                </p>
              </div>
              <span
                class="text-[13px] font-semibold tabular-nums"
                :class="m.type === 'Entrada' ? 'text-emerald-600' : 'text-rose-600'"
              >
                {{ m.type === 'Entrada' ? '+' : '−' }}{{ m.quantity }}
              </span>
            </li>
          </ul>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
// Panel principal: 4 KPIs (productos activos, unidades en stock, stock bajo y valor de salidas
// del mes), gráfica de flujo de 30 días, stock por categoría, productos en nivel crítico y
// movimientos recientes. Todo llega de golpe en una sola llamada a /api/dashboard.
import { computed, onMounted, ref } from 'vue'
import { Line, Doughnut } from 'vue-chartjs'
import {
  ArrowDownToLine,
  ArrowUpFromLine,
  Boxes,
  Package,
  Plus,
  RefreshCw,
  TrendingDown,
  TriangleAlert,
} from '@lucide/vue'
import {
  CategoryScale,
  Chart as ChartJS,
  Filler,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  ArcElement,
  Tooltip,
} from 'chart.js'
import PageHeader from '@/components/ui/PageHeader.vue'
import StatCard from '@/components/ui/StatCard.vue'
import StatusBadge from '@/components/ui/StatusBadge.vue'
import { getDashboard } from '@/api/operations'
import { useToastStore } from '@/stores/toast'

// Registro global de Chart.js: sin estos ejes, puntos, relleno y leyenda, los componentes
// <Line> y <Doughnut> de vue-chartjs no tienen con qué dibujar.
ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Filler, Tooltip, Legend, ArcElement)

const toast = useToastStore()
const data = ref(null)
const loading = ref(true)

const palette = ['#10b981', '#f43f5e', '#3b82f6', '#f59e0b', '#8b5cf6', '#06b6d4', '#84cc16', '#ec4899']

// Convierte dailyFlow en el dataset de la gráfica de líneas: etiquetas con fecha local corta
// y dos series (entradas en verde, salidas en rosa) rellenadas hasta el eje.
const flowData = computed(() => ({
  labels: (data.value?.dailyFlow || []).map((d) =>
    new Date(d.date).toLocaleDateString('es-CL', { day: '2-digit', month: 'short' })
  ),
  datasets: [
    {
      label: 'Entradas',
      data: (data.value?.dailyFlow || []).map((d) => d.entries),
      borderColor: '#10b981',
      backgroundColor: 'rgba(16,185,129,0.12)',
      fill: true,
      tension: 0.35,
      borderWidth: 2,
      pointRadius: 0,
      pointHoverRadius: 4,
    },
    {
      label: 'Salidas',
      data: (data.value?.dailyFlow || []).map((d) => d.exits),
      borderColor: '#f43f5e',
      backgroundColor: 'rgba(244,63,94,0.10)',
      fill: true,
      tension: 0.35,
      borderWidth: 2,
      pointRadius: 0,
      pointHoverRadius: 4,
    },
  ],
}))

// maintainAspectRatio: false obliga a que el contenedor del template fije la altura
// (h-[260px]); si no, Chart.js calcula el alto por ancho y se desborda de la tarjeta.
const flowOptions = {
  responsive: true,
  maintainAspectRatio: false,
  interaction: { mode: 'index', intersect: false },
  plugins: { legend: { display: false } },
  scales: {
    x: { grid: { display: false }, ticks: { maxTicksLimit: 8, font: { size: 10 }, color: '#94a3b8' } },
    y: { beginAtZero: true, grid: { color: '#f1f5f9' }, ticks: { font: { size: 10 }, color: '#94a3b8' } },
  },
}

// Unidades por categoría para el anillo; la paleta fija de 8 colores se repite si hay más series.
const categoryData = computed(() => ({
  labels: (data.value?.categoryShare || []).map((c) => c.name),
  datasets: [
    {
      data: (data.value?.categoryShare || []).map((c) => c.units),
      backgroundColor: palette,
      borderWidth: 2,
      borderColor: '#ffffff',
    },
  ],
}))

const categoryOptions = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '62%',
  plugins: {
    legend: { position: 'bottom', labels: { boxWidth: 8, boxHeight: 8, font: { size: 11 }, color: '#475569', padding: 10 } },
  },
}

const money = (v) =>
  new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP', maximumFractionDigits: 0 }).format(v || 0)

// Estado (critical/low/empty) → color con el que se resalta la cifra de stock en la tabla.
const stockColor = (status) =>
  ({ critical: 'text-rose-600', low: 'text-amber-600', empty: 'text-ink-500' })[status] || 'text-ink-800'

function relative(date) {
  const diff = Date.now() - new Date(date).getTime()
  const mins = Math.floor(diff / 60000)
  if (mins < 60) return `hace ${mins} min`
  const hours = Math.floor(mins / 60)
  if (hours < 24) return `hace ${hours} h`
  return `hace ${Math.floor(hours / 24)} d`
}

async function load() {
  loading.value = true
  try {
    data.value = await getDashboard()
  } catch (e) {
    toast.error(e.message)
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>
