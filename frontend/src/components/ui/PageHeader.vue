<template>
  <div class="flex flex-wrap items-end justify-between gap-4">
    <div>
      <p class="text-[11px] font-semibold uppercase tracking-widest text-brand-600">
        {{ section }}
      </p>
      <h1 class="mt-1 text-xl font-semibold tracking-tight text-ink-900 sm:text-2xl">
        {{ title }}
      </h1>
      <p v-if="description" class="mt-1.5 max-w-2xl text-sm text-ink-500">{{ description }}</p>
    </div>

    <!-- Solo se pinta si el slot `actions` se usa: evita un contenedor vacío que sumaría gap -->
    <div v-if="$slots.actions" class="flex flex-wrap items-center gap-2">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup>
defineProps({
  section: { type: String, default: '' },
  title: { type: String, required: true },
  description: { type: String, default: '' },
})
</script>
