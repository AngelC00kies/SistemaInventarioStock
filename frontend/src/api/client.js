import axios from 'axios'

/**
 * Cliente HTTP configurado contra la API .NET.
 * - Adjunta el JWT en cada petición.
 * - Renueva el token automáticamente ante un 401.
 */
const api = axios.create({
  baseURL: '/api',
  timeout: 30000,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

let refreshing = null

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config

    // Intenta refrescar una sola vez ante 401
    if (error.response?.status === 401 && !original._retry && !original.url.includes('auth/')) {
      original._retry = true
      const refreshToken = localStorage.getItem('refreshToken')

      if (refreshToken) {
        try {
          refreshing =
            refreshing ||
            axios.post('/api/auth/refresh', { refreshToken }, { timeout: 15000 })
          const { data } = await refreshing
          refreshing = null

          localStorage.setItem('token', data.token)
          localStorage.setItem('refreshToken', data.refreshToken)
          original.headers.Authorization = `Bearer ${data.token}`
          return api(original)
        } catch {
          refreshing = null
          localStorage.removeItem('token')
          localStorage.removeItem('refreshToken')
          window.location.href = '/login'
        }
      } else {
        localStorage.removeItem('token')
        window.location.href = '/login'
      }
    }

    return Promise.reject(error)
  }
)

/** Extrae un mensaje legible desde la respuesta de error. */
export function errorMessage(error) {
  const data = error?.response?.data

  if (data?.errors) {
    // Formato de validaciones de ASP.NET Core
    const first = Object.values(data.errors)[0]
    if (Array.isArray(first)) return first[0]
    if (typeof first === 'string') return first
  }

  return data?.message || data?.title || error?.message || 'Error inesperado'
}

export default api
