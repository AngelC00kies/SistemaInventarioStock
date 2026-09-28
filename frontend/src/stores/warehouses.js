import { defineStore } from 'pinia'
import { getWarehouses } from '@/api/catalogs'

export const useWarehouseStore = defineStore('warehouses', {
  state: () => ({
    list: [],
    selectedId: Number(localStorage.getItem('si_warehouse')) || null,
    loaded: false,
  }),

  getters: {
    options: (state) => state.list,
    selected: (state) => state.list.find((w) => w.id === state.selectedId) || null,
  },

  actions: {
    async load(force = false) {
      if (this.loaded && !force) return
      this.list = await getWarehouses()
      this.loaded = true
      if (!this.selectedId && this.list.length) this.select(this.list[0].id)
    },

    select(id) {
      this.selectedId = id
      localStorage.setItem('si_warehouse', String(id))
    },
  },
})
