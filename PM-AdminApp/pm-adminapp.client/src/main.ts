import './assets/main.css'

import { definePreset } from '@primeuix/themes'
import Lara from '@primeuix/themes/aura'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import { createApp } from 'vue'

import '@/assets/styles.scss'
import { auth as Auth } from '@/stores/auth'
import App from './App.vue'
import router from './router'

import Button from 'primevue/button'
import Divider from 'primevue/divider'

import '@/assets/styles.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)

const PMAdmin = definePreset(Lara, {
  //Your customizations, see the following sections for examples
  semantic: {
    primary: {
      50: '{sky.50}',
      100: '{sky.100}',
      200: '{sky.200}',
      300: '{sky.300}',
      400: '{sky.400}',
      500: '{sky.500}',
      600: '{sky.600}',
      700: '{sky.700}',
      800: '{sky.800}',
      900: '{sky.900}',
      950: '{sky.950}',
    },
  },
})

app.use(PrimeVue, {
  theme: {
    preset: PMAdmin,
  },
})

app.component('Button', Button)
app.component('Divider', Divider)

app.mount('#app')

// @ts-ignore
window.Auth = Auth
