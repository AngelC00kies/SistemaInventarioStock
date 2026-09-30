// Punto de entrada de la SPA: crea la app y la monta en #app con el estado global (Pinia) y el router ya
// conectados; las páginas se importan de forma perezosa desde router/index.js.
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import './style.css'

const app = createApp(App)

// El orden importa: Pinia se instala antes que el router porque los guards (p. ej. el de auth) usan stores.
app.use(createPinia())
app.use(router)

app.mount('#app')
