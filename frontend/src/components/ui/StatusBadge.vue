<template>
  <span class="badge" :class="classes">
    <span class="h-1.5 w-1.5 rounded-full" :class="dot"></span>
    {{ label }}
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  status: { type: String, default: 'ok' },
  label: { type: String, default: '' },
})

const map = {
  ok: { cls: 'badge-ok', label: 'Óptimo', dot: 'bg-emerald-500' },
  low: { cls: 'badge-low', label: 'Stock bajo', dot: 'bg-amber-500' },
  critical: { cls: 'badge-critical', label: 'Crítico', dot: 'bg-rose-500' },
  empty: { cls: 'badge-empty', label: 'Sin stock', dot: 'bg-ink-400' },
  Entrada: { cls: 'badge-ok', label: 'Entrada', dot: 'bg-emerald-500' },
  Salida: { cls: 'badge-critical', label: 'Salida', dot: 'bg-rose-500' },
  active: { cls: 'badge-ok', label: 'Activo', dot: 'bg-emerald-500' },
  inactive: { cls: 'badge-empty', label: 'Inactivo', dot: 'bg-ink-400' },
}

const entry = computed(() => map[props.status] || map.ok)
const classes = computed(() => entry.value.cls)
const label = computed(() => props.label || entry.value.label)
const dot = computed(() => entry.value.dot)
</script>
