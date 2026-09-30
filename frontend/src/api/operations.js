// Operación diaria y administración: movimientos de stock, datos del panel, notificaciones y gestión de
// usuarios/roles. Las notificaciones se leen aquí porque las consume el store homónimo del topbar.
import http from './http'

export const getMovements = (params) => http.get('/movements', { params }).then((r) => r.data)
export const getMovement = (id) => http.get(`/movements/${id}`).then((r) => r.data)
export const createMovement = (data) => http.post('/movements', data).then((r) => r.data)

export const getDashboard = () => http.get('/dashboard').then((r) => r.data)

export const getNotifications = (params) => http.get('/notifications', { params }).then((r) => r.data)
export const getUnreadCount = () => http.get('/notifications/unread-count').then((r) => r.data)
export const markNotificationRead = (id) => http.post(`/notifications/${id}/read`)
export const markAllNotificationsRead = () => http.post('/notifications/read-all')

export const getUsers = (params) => http.get('/users', { params }).then((r) => r.data)
export const getRoles = () => http.get('/users/roles').then((r) => r.data)
export const createUser = (data) => http.post('/users', data).then((r) => r.data)
export const updateUser = (id, data) => http.put(`/users/${id}`, data).then((r) => r.data)
export const deleteUser = (id) => http.delete(`/users/${id}`)
