<template>
  <div class="flex flex-col items-center justify-center px-6 py-14 text-center">
    <span class="flex h-14 w-14 items-center justify-center rounded-full bg-ink-100 text-ink-400">
      <component :is="icon" :size="24" :stroke-width="1.6" />
    </span>
    <p class="mt-4 text-sm font-semibold text-ink-700">{{ title }}</p>
    <p class="mt-1 max-w-sm text-[13px] leading-relaxed text-ink-500">{{ description }}</p>
    <div v-if="$slots.action" class="mt-5">
      <slot name="action" />
    </div>
  </div>
</template>

<script setup>
import { PackageSearch } from '@lucide/vue'

defineProps({
  title: { type: String, default: 'Sin resultados' },
  description: { type: String, default: 'No se encontraron registros que coincidan con los filtros aplicados.' },
  icon: { type: Function, default: PackageSearch },
})
</script>
