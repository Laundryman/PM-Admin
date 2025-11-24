import './assets/main.css'

import { definePreset } from '@primeuix/themes'
import Lara from '@primeuix/themes/aura'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import { createApp } from 'vue'

import '@/assets/styles.scss'
import App from './App.vue'
import router from './router'

import '@/assets/styles.scss'
import {
  AnimateOnScroll,
  BadgeDirective,
  FocusTrap,
  Ripple,
  SelectButton,
  StyleClass,
  Tooltip,
} from 'primevue'
import Button from 'primevue/button'
import Chart from 'primevue/chart'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Divider from 'primevue/divider'
import Menu from 'primevue/menu'
import Paginator from 'primevue/paginator'
import Toast from 'primevue/toast'
import ToastService from 'primevue/toastservice'
const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
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
app.component('SelectButton', SelectButton)
app.component('Chart', Chart)
app.component('DataTable', DataTable)
app.component('Column', Column)
app.component('Paginator', Paginator)
app.component('Toast', Toast)
app.component('Menu', Menu)

app.directive('tooltip', Tooltip)
app.directive('badge', BadgeDirective)
app.directive('ripple', Ripple)
app.directive('styleclass', StyleClass)
app.directive('focustrap', FocusTrap)
app.directive('animateonscroll', AnimateOnScroll)

app.use(ToastService)

app.mount('#app')

// @ts-ignore
// window.Auth = Auth
