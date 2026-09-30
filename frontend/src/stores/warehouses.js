// Store de almacenes: catálogo compartido y almacén activo de la sesión. Lo consumen el selector del Topbar
// (lo carga al montar) y la página de Almacenes, que fuerza su recarga al actualizar su propio listado.
import { defineStore } from 'pinia'
import { getWarehouses } from '@/api/catalogs'

export const useWarehouseStore = defineStore('warehouses', {
  state: () => ({
    list: [],
    // El almacén elegido se recuerda en localStorage para conservar el mismo contexto entre recargas.
    selectedId: Number(localStorage.getItem('si_warehouse')) || null,
    loaded: false,
  }),

  getters: {
    options: (state) => state.list,
    selected: (state) => state.list.find((w) => w.id === state.selectedId) || null,
  },

  actions: {
    // Carga perezosa con caché en memoria: solo se vuelve a llamar a la API si aún no se cargó o si se
    // fuerza (`force`), que es lo que hace la página de Almacenes tras modificar el catálogo.
    // Si no hay almacén activo se selecciona el primero, para no arrancar sin contexto.
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
