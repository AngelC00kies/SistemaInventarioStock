// Store de autenticación: mantiene el JWT y el usuario de la sesión, y expone los getters de permisos
// (isAdmin, canWrite) que consultan el router, el sidebar y las páginas.
import { defineStore } from 'pinia'
import { login as apiLogin, fetchMe } from '@/api/auth'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    // La sesión se hidrata desde localStorage al crear el store: así sobrevive a recargas de la página.
    token: localStorage.getItem('si_token') || null,
    user: JSON.parse(localStorage.getItem('si_user') || 'null'),
    loading: false,
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    role: (state) => state.user?.role || null,
    isAdmin: (state) => state.user?.role === 'Admin',
    // Escritura reservada a Admin y Usuario; el resto de roles solo consultan datos.
    canWrite: (state) => ['Admin', 'Usuario'].includes(state.user?.role),
    fullName: (state) => state.user?.fullName || state.user?.username || '',
    initials() {
      return this.fullName
        .split(' ')
        .filter(Boolean)
        .slice(0, 2)
        .map((p) => p[0].toUpperCase())
        .join('')
    },
  },

  actions: {
    // Vuelca token y usuario a localStorage: es lo que mantiene la sesión viva entre recargas.
    persist() {
      localStorage.setItem('si_token', this.token)
      localStorage.setItem('si_user', JSON.stringify(this.user))
    },

    // Inicio de sesión: guarda el token/usuario que devuelve la API y los persiste enseguida, para que el
    // interceptor de http.js los adjunte ya a las siguientes peticiones.
    async login(username, password) {
      this.loading = true
      try {
        const data = await apiLogin(username, password)
        this.token = data.token
        this.user = data.user
        this.persist()
        return data
      } finally {
        this.loading = false
      }
    },

    async refreshUser() {
      if (!this.token) return
      try {
        this.user = await fetchMe()
        this.persist()
      } catch {
        /* se ignora: el interceptor se encarga de sesiones vencidas */
      }
    },

    // Cierre de sesión: limpia memoria y localStorage. También lo invoca el interceptor de http.js ante un 401.
    logout() {
      this.token = null
      this.user = null
      localStorage.removeItem('si_token')
      localStorage.removeItem('si_user')
    },
  },
})
