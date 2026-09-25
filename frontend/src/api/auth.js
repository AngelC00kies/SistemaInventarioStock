import http from './http'

export const login = (username, password) =>
  http.post('/auth/login', { username, password }).then((r) => r.data)

export const fetchMe = () => http.get('/auth/me').then((r) => r.data)
