import { defineStore } from 'pinia'
import { login as apiLogin, fetchMe } from '@/api/auth'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('si_token') || null,
    user: JSON.parse(localStorage.getItem('si_user') || 'null'),
    loading: false,
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    role: (state) => state.user?.role || null,
    isAdmin: (state) => state.user?.role === 'Admin',
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
    persist() {
      localStorage.setItem('si_token', this.token)
      localStorage.setItem('si_user', JSON.stringify(this.user))
    },

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

    logout() {
      this.token = null
      this.user = null
      localStorage.removeItem('si_token')
      localStorage.removeItem('si_user')
    },
  },
})
