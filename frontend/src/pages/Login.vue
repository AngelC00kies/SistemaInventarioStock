<template>
  <div class="login-page">
    <form class="login-card" @submit.prevent="onSubmit">
      <div class="brand">
        <div class="logo">📦</div>
        <h1>Sistema de Inventario</h1>
        <p>Ingrese con sus credenciales para continuar</p>
      </div>

      <div v-if="error" class="alert">{{ error }}</div>

      <div class="field">
        <label>Usuario</label>
        <input
          v-model.trim="userName"
          type="text"
          autocomplete="username"
          placeholder="admin"
          required
          autofocus
        />
      </div>

      <div class="field">
        <label>Contraseña</label>
        <input
          v-model="password"
          type="password"
          autocomplete="current-password"
          placeholder="••••••••"
          required
        />
      </div>

      <button class="btn" type="submit" :disabled="loading">
        {{ loading ? 'Ingresando…' : 'Ingresar' }}
      </button>

      <p class="hint" style="margin-top:18px;text-align:center">
        Demo: admin / Admin123!
      </p>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { errorMessage } from '../api/client'

const auth = useAuthStore()
const router = useRouter()

const userName = ref('admin')
const password = ref('')
const loading = ref(false)
const error = ref('')

async function onSubmit() {
  loading.value = true
  error.value = ''

  try {
    await auth.login(userName.value, password.value)
    router.push('/dashboard')
  } catch (e) {
    error.value = errorMessage(e)
  } finally {
    loading.value = false
  }
}
</script>
