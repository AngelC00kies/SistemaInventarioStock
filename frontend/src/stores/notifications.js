// Store de notificaciones de stock: estado compartido entre la campana y el panel desplegable del Topbar.
// El listado se carga de forma perezosa al abrir el panel; el contador de no leídas se mantiene aparte
// para poder pintar el badge sin descargar nada.
import { defineStore } from 'pinia'
import {
  getNotifications,
  getUnreadCount,
  markAllNotificationsRead,
  markNotificationRead,
} from '@/api/operations'
import { useToastStore } from './toast'

export const useNotificationsStore = defineStore('notifications', {
  state: () => ({
    items: [],
    unread: 0,
    loading: false,
  }),

  actions: {
    // Listado y contador se piden a la vez (Promise.all) para que el badge no tarde más que el panel.
    // Si algo falla se avisa con un toast en vez de romper la cabecera.
    async fetch() {
      this.loading = true
      try {
        const [items, unread] = await Promise.all([getNotifications({ limit: 20 }), getUnreadCount()])
        this.items = items
        this.unread = unread
      } catch (e) {
        useToastStore().error(e.message)
      } finally {
        this.loading = false
      }
    },

    // Tras marcar (individual o todas) se recarga el listado para que el badge de no leídas quede al día.
    async markRead(id) {
      await markNotificationRead(id)
      await this.fetch()
    },

    async markAllRead() {
      await markAllNotificationsRead()
      await this.fetch()
    },
  },
})
