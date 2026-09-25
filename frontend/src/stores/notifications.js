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
