<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal" :class="{ wide }">
      <div class="modal-header">
        <h3>{{ title }}</h3>
        <button class="close-btn" type="button" @click="$emit('close')">&times;</button>
      </div>
      <div class="modal-body">
        <slot />
      </div>
      <div class="modal-footer">
        <slot name="footer">
          <button class="btn secondary" type="button" @click="$emit('close')">Cancelar</button>
        </slot>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, required: true },
  wide: { type: Boolean, default: false },
})

defineEmits(['close'])
</script>
