// Descarga de reportes: pide el archivo al backend como `blob` y lo fuerza a guardarlo en disco desde el
// navegador (no existe un enlace de descarga directa, así que se simula el clic sobre un <a> temporal).
import http from './http'

function filenameFrom(response, fallback) {
  // Lee el nombre que sugiere el backend en Content-Disposition (admite filename* UTF-8); si falta, el de `fallback`.
  const header = response.headers?.['content-disposition'] || ''
  const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(header)
  return match ? decodeURIComponent(match[1]) : fallback
}

/**
 * Descarga un reporte generado por el backend (PDF o Excel).
 * @param {string} report stock | critical-stock | movements | products | categories | suppliers | users
 * @param {'pdf'|'xlsx'} format
 * @param {object} params filtros
 */
export async function downloadReport(report, format, params = {}) {
  // Se descartan los filtros vacíos o ausentes para que el backend no reciba parámetros en blanco
  // (por eso también se excluye `false`: solo se envían valores con significado).
  const cleanParams = Object.fromEntries(
    Object.entries(params).filter(([, v]) => v !== undefined && v !== null && v !== '' && v !== false)
  )

  const response = await http.get(`/reports/${report}/${format}`, {
    params: cleanParams,
    responseType: 'blob',
    timeout: 120000,
  })

  const fallback = `${report}_${new Date().toISOString().slice(0, 10)}.${format}`
  const name = filenameFrom(response, fallback)
  const url = URL.createObjectURL(response.data)

  const link = document.createElement('a')
  link.href = url
  link.download = name
  document.body.appendChild(link)
  link.click()
  link.remove()

  // La URL temporal se libera pasados 4 s: suficiente para que el navegador arranque la descarga,
  // y así no se acumulan objetos blob en memoria.
  setTimeout(() => URL.revokeObjectURL(url), 4000)
  return name
}
