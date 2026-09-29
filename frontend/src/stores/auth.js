import { defineStore } from 'pinia'
import api from '../api/client'
import { ROLES, WRITERS, roleAllows } from '../config/roles'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || null,
    user: JSON.parse(localStorage.getItem('user') || 'null'),
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    role: (state) => state.user?.role || null,

    // ── Permisos por rol (derivados de config/roles.js) ──
    canEdit: (state) => roleAllows(state.user?.role, WRITERS),
    isAdmin: (state) => state.user?.role === ROLES.ADMIN,
    isAuditor: (state) => state.user?.role === ROLES.AUDITOR,
  },

  actions: {
    /** ¿Tiene el usuario alguno de los roles indicados? (sin roles = todos) */
    hasRole(roles) {
      return roleAllows(this.role, roles)
    },

    async login(userName, password) {
      const { data } = await api.post('/auth/login', { userName, password })
      this.setSession(data)
      return data
    },

    setSession(data) {
      this.token = data.token
      this.user = data.user
      localStorage.setItem('token', data.token)
      localStorage.setItem('refreshToken', data.refreshToken)
      localStorage.setItem('user', JSON.stringify(data.user))
    },

    async logout() {
      const refreshToken = localStorage.getItem('refreshToken')
      try {
        if (refreshToken) await api.post('/auth/logout', { refreshToken })
      } catch {
        // ignoramos errores de red al cerrar sesión
      }
      this.token = null
      this.user = null
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('user')
    },
  },
})
