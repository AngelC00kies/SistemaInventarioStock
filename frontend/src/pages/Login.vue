<template>
  <div class="flex min-h-screen bg-white">
    <div class="relative hidden w-[46%] flex-col justify-between overflow-hidden bg-ink-950 p-12 lg:flex">
      <div class="absolute inset-0 opacity-[0.35]">
        <div class="absolute -left-24 -top-24 h-96 w-96 rounded-full bg-brand-600/30 blur-3xl"></div>
        <div class="absolute bottom-0 right-0 h-96 w-96 rounded-full bg-sky-500/20 blur-3xl"></div>
      </div>

      <div class="relative flex items-center gap-3">
        <span class="flex h-9 w-9 items-center justify-center rounded-md bg-brand-600 text-white">
          <Boxes :size="20" />
        </span>
        <span class="text-sm font-semibold text-white">Sistema de Inventario</span>
      </div>

      <div class="relative">
        <h1 class="max-w-md text-3xl font-semibold leading-tight tracking-tight text-white">
          Control total sobre el stock de sus almacenes.
        </h1>
        <p class="mt-4 max-w-md text-sm leading-relaxed text-ink-400">
          Registre entradas y salidas, supervise niveles críticos y genere reportes confiables
          para la toma de decisiones.
        </p>

        <ul class="mt-8 space-y-3">
          <li v-for="item in features" :key="item" class="flex items-center gap-3 text-sm text-ink-300">
            <span class="flex h-6 w-6 items-center justify-center rounded-full bg-brand-600/20 text-brand-400 ring-1 ring-brand-500/30">
              <Check :size="13" :stroke-width="2.5" />
            </span>
            {{ item }}
          </li>
        </ul>
      </div>

      <p class="relative text-xs text-ink-600">
        © {{ new Date().getFullYear() }} — Sistema de Inventario · Uso interno
      </p>
    </div>

    <div class="flex flex-1 items-center justify-center px-5 py-10 sm:px-10">
      <div class="w-full max-w-sm">
        <div class="mb-8 flex items-center gap-3 lg:hidden">
          <span class="flex h-9 w-9 items-center justify-center rounded-md bg-brand-600 text-white">
            <Boxes :size="20" />
          </span>
          <span class="text-sm font-semibold text-ink-900">Sistema de Inventario</span>
        </div>

        <p class="text-[11px] font-semibold uppercase tracking-widest text-brand-600">Acceso seguro</p>
        <h2 class="mt-2 text-2xl font-semibold tracking-tight text-ink-900">Inicie sesión</h2>
        <p class="mt-1.5 text-sm text-ink-500">
          Ingrese sus credenciales para acceder al panel.
        </p>

        <form class="mt-8 space-y-4" @submit.prevent="submit">
          <div>
            <label class="label" for="username">Usuario</label>
            <div class="relative">
              <User :size="16" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
              <input
                id="username"
                v-model.trim="username"
                class="input pl-9"
                type="text"
                autocomplete="username"
                placeholder="admin"
                required
                autofocus
              />
            </div>
          </div>

          <div>
            <label class="label" for="password">Contraseña</label>
            <div class="relative">
              <Lock :size="16" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
              <input
                id="password"
                v-model="password"
                class="input pl-9 pr-10"
                :type="showPassword ? 'text' : 'password'"
                autocomplete="current-password"
                placeholder="••••••••"
                required
              />
              <button
                type="button"
                class="absolute right-2.5 top-1/2 -translate-y-1/2 rounded p-1 text-ink-400 hover:text-ink-600"
                @click="showPassword = !showPassword"
                aria-label="Mostrar contraseña"
              >
                <Eye v-if="!showPassword" :size="16" />
                <EyeOff v-else :size="16" />
              </button>
            </div>
          </div>

          <p v-if="error" class="flex items-start gap-2 rounded-md bg-rose-50 px-3 py-2.5 text-[13px] text-rose-700 ring-1 ring-inset ring-rose-600/20">
            <CircleAlert :size="15" class="mt-0.5 shrink-0" />
            {{ error }}
          </p>

          <button type="submit" class="btn-primary w-full py-2.5" :disabled="auth.loading">
            <LoaderCircle v-if="auth.loading" :size="16" class="animate-spin" />
            <LogIn v-else :size="16" />
            {{ auth.loading ? 'Verificando…' : 'Ingresar al sistema' }}
          </button>
        </form>

        <div class="mt-6 rounded-lg border border-ink-200 bg-ink-50 p-3.5">
          <p class="text-[11px] font-semibold uppercase tracking-wide text-ink-400">
            Credenciales de demostración
          </p>
          <div class="mt-2 space-y-1 text-[12.5px] text-ink-600">
            <p><span class="font-medium text-ink-800">admin</span> / Admin123! · acceso total</p>
            <p><span class="font-medium text-ink-800">operador</span> / Usuario123! · operación</p>
            <p><span class="font-medium text-ink-800">auditor</span> / Usuario123! · solo lectura</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
// Acceso al sistema: delega el ciclo de sesión en el store de auth, que tras el login guarda
// token y usuario en localStorage (si_token / si_user) para que la sesión sobreviva a una
// recarga; el guard del router y el interceptor HTTP leen esos mismos valores.
// Los errores de credenciales se pintan en el formulario, no como toast.
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Boxes,
  Check,
  CircleAlert,
  Eye,
  EyeOff,
  LoaderCircle,
  Lock,
  LogIn,
  User,
} from '@lucide/vue'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'

const auth = useAuthStore()
const toast = useToastStore()
const router = useRouter()
const route = useRoute()

const username = ref('admin')
const password = ref('')
const showPassword = ref(false)
const error = ref('')

const features = [
  'Entradas y salidas con trazabilidad completa',
  'Alertas automáticas ante stock crítico',
  'Reportes de inventario en PDF y Excel',
  'Control de acceso por roles y usuarios',
]

// Al autenticar se vuelve a la ruta que originó el guard (?redirect=) o al panel principal.
async function submit() {
  error.value = ''
  try {
    const data = await auth.login(username.value, password.value)
    toast.success(`Bienvenido(a), ${data.user.fullName}.`, 'Sesión iniciada')
    router.push(route.query.redirect || { name: 'dashboard' })
  } catch (e) {
    error.value = e.message || 'No se pudo iniciar sesión.'
  }
}
</script>
