import { defineStore } from 'pinia'

let counter = 0

export const useToastStore = defineStore('toast', {
  state: () => ({
    items: [],
  }),

  actions: {
    push(type, message, title = null) {
      const id = ++counter
      this.items.push({ id, type, message, title })
      setTimeout(() => this.dismiss(id), type === 'error' ? 7000 : 4000)
    },

    success(message, title = 'Listo') {
      this.push('success', message, title)
    },

    error(message, title = 'Error') {
      this.push('error', message, title)
    },

    info(message, title = 'Información') {
      this.push('info', message, title)
    },

    dismiss(id) {
      this.items = this.items.filter((t) => t.id !== id)
    },
  },
})
