import http from './http'

export const getProducts = (params) => http.get('/products', { params }).then((r) => r.data)
export const getProduct = (id) => http.get(`/products/${id}`).then((r) => r.data)
export const createProduct = (data) => http.post('/products', data).then((r) => r.data)
export const updateProduct = (id, data) => http.put(`/products/${id}`, data).then((r) => r.data)
export const deleteProduct = (id) => http.delete(`/products/${id}`)
