import axios from 'axios'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

// Cliente HTTP único de la aplicación: centraliza la baseURL, las cabeceras y el manejo de errores de toda la API.
// El prefijo '/api' es relativo a propósito: en desarrollo el proxy de Vite reenvía esas peticiones al backend
// (ver vite.config.js) y en producción las resuelve el mismo servidor que sirve el frontend.
const http = axios.create({
  baseURL: '/api',
  timeout: 60000,
  headers: { 'Content-Type': 'application/json' },
})

// Inyecta el token de la sesión (hidratado desde localStorage por el store de auth) como Bearer en cada petición.
http.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.token) config.headers.Authorization = `Bearer ${auth.token}`
  return config
})

// Normaliza los fallos: cualquier error de axios se convierte en un Error con mensaje legible, `status` y `data`,
// de modo que las vistas solo tienen que leer `e.message`.
http.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status
    const message = error.response?.data?.message || 'No se pudo completar la operación.'

    // Token caducado o inválido: se cierra la sesión y se redirige al login guardando la ruta original
    // en `redirect` para poder volver a ella tras autenticarse (evita el bucle si ya estamos en login).
    if (status === 401) {
      const auth = useAuthStore()
      auth.logout()
      if (router.currentRoute.value.name !== 'login') {
        router.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } })
      }
    }

    return Promise.reject(Object.assign(new Error(message), { status, data: error.response?.data }))
  }
)

export default http
