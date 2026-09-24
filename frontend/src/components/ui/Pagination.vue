<template>
  <div class="flex flex-wrap items-center justify-between gap-3 px-4 py-3">
    <p class="text-[13px] text-ink-500">
      <template v-if="total > 0">
        Mostrando
        <span class="font-medium text-ink-700">{{ from }}–{{ to }}</span>
        de
        <span class="font-medium text-ink-700">{{ total }}</span>
        registros
      </template>
      <template v-else>Sin registros</template>
    </p>

    <div v-if="pages > 1" class="flex items-center gap-1">
      <button class="btn-ghost btn-sm" :disabled="modelValue <= 1" @click="go(modelValue - 1)">
        <ChevronLeft :size="15" />
      </button>

      <button
        v-for="p in visiblePages"
        :key="p"
        class="min-w-[30px] rounded-md px-2 py-1.5 text-xs font-medium transition"
        :class="
          p === modelValue
            ? 'bg-brand-600 text-white shadow-sm'
            : 'text-ink-600 hover:bg-ink-100'
        "
        @click="go(p)"
      >
        {{ p }}
      </button>

      <button class="btn-ghost btn-sm" :disabled="modelValue >= pages" @click="go(modelValue + 1)">
        <ChevronRight :size="15" />
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { ChevronLeft, ChevronRight } from '@lucide/vue'

const props = defineProps({
  modelValue: { type: Number, default: 1 },
  total: { type: Number, default: 0 },
  pageSize: { type: Number, default: 15 },
})

const emit = defineEmits(['update:modelValue'])

const pages = computed(() => Math.max(1, Math.ceil(props.total / props.pageSize)))
const from = computed(() => (props.modelValue - 1) * props.pageSize + 1)
const to = computed(() => Math.min(props.total, props.modelValue * props.pageSize))

const visiblePages = computed(() => {
  const current = props.modelValue
  const last = pages.value
  const start = Math.max(1, Math.min(current - 2, last - 4))
  const end = Math.min(last, start + 4)
  const list = []
  for (let i = start; i <= end; i++) list.push(i)
  return list
})

function go(page) {
  if (page < 1 || page > pages.value || page === props.modelValue) return
  emit('update:modelValue', page)
}
</script>
