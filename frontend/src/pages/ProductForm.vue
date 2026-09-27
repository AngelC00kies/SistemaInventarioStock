<template>
  <div class="card" style="max-width:860px">
    <div class="card-header">
      <h2>{{ isEdit ? 'Editar producto' : 'Nuevo producto' }}</h2>
      <router-link to="/products" class="btn secondary sm">← Volver</router-link>
    </div>

    <form class="card-body" @submit.prevent="onSubmit">
      <div v-if="error" class="alert" style="margin-bottom:18px">{{ error }}</div>

      <div class="form-grid">
        <div class="field">
          <label>Código <span class="req">*</span></label>
          <input v-model.trim="form.code" type="text" placeholder="P-0001" required maxlength="30" />
        </div>

        <div class="field">
          <label>Nombre <span class="req">*</span></label>
          <input v-model.trim="form.name" type="text" placeholder="Nombre del producto" required maxlength="150" />
        </div>

        <div class="field">
          <label>Categoría <span class="req">*</span></label>
          <select v-model="form.categoryId" required>
            <option disabled value="">Seleccione…</option>
            <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>

        <div class="field">
          <label>Proveedor</label>
          <select v-model="form.supplierId">
            <option :value="null">Sin proveedor</option>
            <option v-for="s in suppliers" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>

        <div class="field">
          <label>Unidad de medida</label>
          <select v-model="form.unitOfMeasure">
            <option v-for="u in units" :key="u" :value="u">{{ u }}</option>
          </select>
        </div>

        <div class="field">
          <label>Stock mínimo</label>
          <input v-model.number="form.minimumStock" type="number" min="0" />
        </div>

        <div class="field">
          <label>Precio de compra</label>
          <input v-model.number="form.purchasePrice" type="number" min="0" step="0.01" placeholder="0.00" />
        </div>

        <div class="field">
          <label>Precio de venta</label>
          <input v-model.number="form.salePrice" type="number" min="0" step="0.01" placeholder="0.00" />
        </div>

        <div class="field full">
          <label>Descripción</label>
          <textarea v-model.trim="form.description" placeholder="Descripción opcional del producto" maxlength="1000"></textarea>
        </div>
      </div>

      <div class="row" style="margin-top:24px;justify-content:flex-end">
        <router-link to="/products" class="btn secondary">Cancelar</router-link>
        <button class="btn" type="submit" :disabled="saving">
          {{ saving ? 'Guardando…' : isEdit ? 'Guardar cambios' : 'Crear producto' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api, { errorMessage } from '../api/client'
import { useUiStore } from '../stores/ui'

const route = useRoute()
const router = useRouter()
const ui = useUiStore()

const isEdit = computed(() => !!route.params.id)
const saving = ref(false)
const error = ref('')

const categories = ref([])
const suppliers = ref([])
const units = ['Unidad', 'Kilogramo', 'Gramo', 'Litro', 'Caja', 'Bolsa', 'Paquete', 'Metro', 'Par']

const form = reactive({
  code: '',
  name: '',
  description: '',
  categoryId: '',
  supplierId: null,
  unitOfMeasure: 'Unidad',
  purchasePrice: 0,
  salePrice: 0,
  minimumStock: 0,
})

onMounted(async () => {
  const [c, s] = await Promise.all([
    api.get('/categories'),
    api.get('/suppliers', { params: { pageSize: 200 } }),
  ])
  categories.value = c.data
  suppliers.value = s.data.items || []

  if (isEdit.value) {
    try {
      const { data } = await api.get(`/products/${route.params.id}`)
      Object.assign(form, {
        code: data.code,
        name: data.name,
        description: data.description || '',
        categoryId: data.categoryId,
        supplierId: data.supplierId,
        unitOfMeasure: data.unitOfMeasure,
        purchasePrice: data.purchasePrice,
        salePrice: data.salePrice,
        minimumStock: data.minimumStock,
      })
    } catch (e) {
      ui.notify(errorMessage(e), 'error')
      router.push('/products')
    }
  }
})

async function onSubmit() {
  saving.value = true
  error.value = ''

  const payload = {
    ...form,
    categoryId: Number(form.categoryId),
    supplierId: form.supplierId == null ? null : Number(form.supplierId),
    purchasePrice: Number(form.purchasePrice) || 0,
    salePrice: Number(form.salePrice) || 0,
    minimumStock: Number(form.minimumStock) || 0,
  }

  try {
    if (isEdit.value) {
      await api.put(`/products/${route.params.id}`, payload)
      ui.notify('Producto actualizado correctamente')
    } else {
      await api.post('/products', payload)
      ui.notify('Producto creado correctamente')
    }
    router.push('/products')
  } catch (e) {
    error.value = errorMessage(e)
  } finally {
    saving.value = false
  }
}
</script>
