<template>
  <router-view />
  <ToastHost />
</template>

<script setup>
// Raíz de la app: monta la vista activa del router y, por encima de ella, el host global de avisos
// flotantes, de modo que los toasts se ven también en las rutas con layout 'blank' (login y 404).
import ToastHost from '@/components/ui/ToastHost.vue'
</script>
