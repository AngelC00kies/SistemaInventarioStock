<template>
  <div class="flex min-h-screen flex-col items-center justify-center bg-ink-50 px-6 text-center">
    <span class="flex h-14 w-14 items-center justify-center rounded-full bg-brand-50 text-brand-600">
      <FileQuestion :size="26" :stroke-width="1.6" />
    </span>
    <p class="mt-5 text-sm font-semibold uppercase tracking-widest text-brand-600">Error 404</p>
    <h1 class="mt-2 text-2xl font-semibold tracking-tight text-ink-900">
      La página solicitada no existe
    </h1>
    <p class="mt-2 max-w-md text-sm text-ink-500">
      La dirección puede haber cambiado o el módulo no está disponible en esta sesión.
    </p>
    <router-link to="/dashboard" class="btn-primary mt-6">
      <ArrowLeft :size="16" />
      Volver al panel
    </router-link>
  </div>
</template>

<script setup>
// Vista 404 del catch-all del router (/:pathMatch*): sin lógica, solo el enlace de vuelta al panel.
import { ArrowLeft, FileQuestion } from '@lucide/vue'
</script>
