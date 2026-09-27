import { defineStore } from 'pinia'
import api from '../api/client'

let toastId = 0

export const useUiStore = defineStore('ui', {
  state: () => ({
    toasts: [],
    pendingCount: 0,
  }),

  actions: {
    notify(message, type = 'success') {
      const id = ++toastId
      this.toasts.push({ id, message, type })
      setTimeout(() => this.dismiss(id), 4500)
    },

    dismiss(id) {
      this.toasts = this.toasts.filter((t) => t.id !== id)
    },

    async fetchPendingCount() {
      try {
        const { data } = await api.get('/notifications/pending-count')
        this.pendingCount = typeof data === 'number' ? data : 0
      } catch {
        this.pendingCount = 0
      }
    },
  },
})
