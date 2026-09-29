/**
 * Registro único de roles y permisos.
 *
 * Es la única fuente de verdad del sistema de permisos del frontend:
 * - stores/auth.js deriva `isAdmin` / `canEdit` de aquí
 * - config/navigation.js declara qué roles pueden ver cada pantalla
 *
 * Para añadir un rol basta con declararlo aquí y usarlo en navigation.js
 * (Principio Abierto/Cerrado: no hay que tocar router ni el menú lateral).
 */
export const ROLES = {
  ADMIN: 'Admin',
  USER: 'Usuario',
  AUDITOR: 'Auditor',
}

/** Todos los roles autenticados pueden verlo. */
export const EVERYONE = [ROLES.ADMIN, ROLES.USER, ROLES.AUDITOR]

/** Roles con permiso de escritura (CRUD y movimientos). */
export const WRITERS = [ROLES.ADMIN, ROLES.USER]

/** Solo administración del sistema. */
export const ADMINS = [ROLES.ADMIN]

/**
 * Devuelve true si `role` está permitido en `roles`.
 * Sin `roles` (undefined/null) significa "sin restricción".
 */
export function roleAllows(role, roles) {
  if (!roles || roles.length === 0) return true
  return roles.includes(role)
}
