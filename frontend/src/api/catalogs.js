import http from './http'

export const getCategories = (params) => http.get('/categories', { params }).then((r) => r.data)
export const createCategory = (data) => http.post('/categories', data).then((r) => r.data)
export const updateCategory = (id, data) => http.put(`/categories/${id}`, data).then((r) => r.data)
export const deleteCategory = (id) => http.delete(`/categories/${id}`)

export const getSuppliers = (params) => http.get('/suppliers', { params }).then((r) => r.data)
export const createSupplier = (data) => http.post('/suppliers', data).then((r) => r.data)
export const updateSupplier = (id, data) => http.put(`/suppliers/${id}`, data).then((r) => r.data)
export const deleteSupplier = (id) => http.delete(`/suppliers/${id}`)

export const getWarehouses = (params) => http.get('/warehouses', { params }).then((r) => r.data)
export const createWarehouse = (data) => http.post('/warehouses', data).then((r) => r.data)
export const updateWarehouse = (id, data) => http.put(`/warehouses/${id}`, data).then((r) => r.data)
export const deleteWarehouse = (id) => http.delete(`/warehouses/${id}`)
