<template>
  <div class="card p-5">
    <div class="flex items-start justify-between gap-3">
      <div class="min-w-0">
        <p class="text-[11px] font-semibold uppercase tracking-wider text-ink-400">{{ label }}</p>
        <p class="mt-2 text-2xl font-semibold tracking-tight text-ink-900 tabular-nums">
          {{ display }}
        </p>
        <p v-if="hint" class="mt-1.5 text-xs text-ink-500">{{ hint }}</p>
      </div>

      <span
        class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg"
        :class="toneClasses"
      >
        <component :is="icon" :size="19" :stroke-width="1.9" />
      </span>
    </div>

    <div v-if="trend" class="mt-4 flex items-center gap-1.5 text-xs" :class="trendClasses">
      <component :is="trendIcon" :size="14" />
      <span class="font-medium">{{ trend }}</span>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { TrendingDown, TrendingUp } from '@lucide/vue'

const props = defineProps({
  label: { type: String, required: true },
  value: { type: [Number, String], default: 0 },
  icon: { type: Object, required: true },
  hint: { type: String, default: '' },
  format: { type: String, default: 'number' },
  tone: { type: String, default: 'brand' },
  trend: { type: String, default: '' },
  trendDirection: { type: String, default: 'up' },
})

const tones = {
  brand: 'bg-brand-50 text-brand-600',
  emerald: 'bg-emerald-50 text-emerald-600',
  amber: 'bg-amber-50 text-amber-600',
  rose: 'bg-rose-50 text-rose-600',
  violet: 'bg-violet-50 text-violet-600',
  ink: 'bg-ink-100 text-ink-600',
}

const toneClasses = computed(() => tones[props.tone] || tones.brand)
const trendClasses = computed(() =>
  props.trendDirection === 'down' ? 'text-rose-600' : 'text-emerald-600'
)
const trendIcon = computed(() => (props.trendDirection === 'down' ? TrendingDown : TrendingUp))

const display = computed(() => {
  if (props.format === 'money') {
    return new Intl.NumberFormat('es-CL', {
      style: 'currency',
      currency: 'CLP',
      maximumFractionDigits: 0,
    }).format(Number(props.value) || 0)
  }
  if (props.format === 'number') {
    return new Intl.NumberFormat('es-CL').format(Number(props.value) || 0)
  }
  return props.value
})
</script>
