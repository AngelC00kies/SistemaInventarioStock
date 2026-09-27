<template>
  <div class="pagination">
    <span>
      {{ total }} registro{{ total === 1 ? '' : 's' }}
      <template v-if="pageCount > 1">
        — página {{ modelValue }} de {{ pageCount }}
      </template>
    </span>

    <div class="pages" v-if="pageCount > 1">
      <button class="btn secondary sm" :disabled="modelValue <= 1" @click="go(modelValue - 1)">
        ‹ Anterior
      </button>

      <button
        v-for="p in visiblePages"
        :key="p"
        class="btn sm"
        :class="p === modelValue ? '' : 'secondary'"
        @click="go(p)"
      >
        {{ p }}
      </button>

      <button class="btn secondary sm" :disabled="modelValue >= pageCount" @click="go(modelValue + 1)">
        Siguiente ›
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: { type: Number, default: 1 },
  total: { type: Number, default: 0 },
  pageSize: { type: Number, default: 20 },
})

const emit = defineEmits(['update:modelValue'])

const pageCount = computed(() =>
  props.pageSize > 0 ? Math.max(1, Math.ceil(props.total / props.pageSize)) : 1
)

const visiblePages = computed(() => {
  const count = pageCount.value
  const current = props.modelValue
  const window = 2
  let start = Math.max(1, current - window)
  let end = Math.min(count, current + window)

  if (end - start < window * 2) {
    if (start === 1) end = Math.min(count, start + window * 2)
    else start = Math.max(1, end - window * 2)
  }

  const pages = []
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

function go(p) {
  if (p < 1 || p > pageCount.value || p === props.modelValue) return
  emit('update:modelValue', p)
}
</script>
