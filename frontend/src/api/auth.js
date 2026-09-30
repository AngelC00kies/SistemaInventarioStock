// Endpoints de autenticación: `login` obtiene el token + usuario y `fetchMe` devuelve el perfil vigente
// de la sesión (se usa para refrescar datos y rol después de recargar la página).
import http from './http'

export const login = (username, password) =>
  http.post('/auth/login', { username, password }).then((r) => r.data)

export const fetchMe = () => http.get('/auth/me').then((r) => r.data)
