import { defineStore } from 'pinia'

// Store de avisos flotantes (toasts): cola compartida por toda la app y renderizada por <ToastHost/>.
// Solo vive en memoria, no se persiste: al recargar la página la cola desaparece.
let counter = 0

export const useToastStore = defineStore('toast', {
  state: () => ({
    items: [],
  }),

  actions: {
    // El id único permite auto-retirar cada aviso: los errores se quedan 7 s (más tiempo para leerlos) y el resto 4 s.
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
