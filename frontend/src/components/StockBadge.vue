<template>
  <span class="pill" :class="color">{{ label }}</span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  quantity: { type: Number, required: true },
  minimum: { type: Number, default: 0 },
})

const color = computed(() => {
  if (props.quantity <= 0) return 'red'
  if (props.minimum > 0 && props.quantity <= props.minimum) return 'yellow'
  if (props.minimum > 0 && props.quantity <= props.minimum * 1.5) return 'blue'
  return 'green'
})

const label = computed(() => {
  if (props.quantity <= 0) return 'Sin stock'
  if (props.minimum > 0 && props.quantity <= props.minimum) return 'Stock bajo'
  return 'Disponible'
})
</script>
