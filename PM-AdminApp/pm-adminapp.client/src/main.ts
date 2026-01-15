import './assets/main.css'

import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import { createApp } from 'vue'
import PMAdmin from './planmatrThemePreset'
//import '@/assets/styles.scss'
import '@/assets/styles.scss'
import 'primeicons/primeicons.css'
import {
  AnimateOnScroll,
  BadgeDirective,
  Dialog,
  FocusTrap,
  Ripple,
  SelectButton,
  StyleClass,
  Tag,
  Tooltip,
} from 'primevue'
import Button from 'primevue/button'
import Chart from 'primevue/chart'
import Chip from 'primevue/chip'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import DatePicker from 'primevue/datepicker'
import Divider from 'primevue/divider'
import FileUpload from 'primevue/fileupload'
import IconField from 'primevue/iconfield'
import Image from 'primevue/image'
import InputIcon from 'primevue/inputicon'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Menu from 'primevue/menu'
import MultiSelect from 'primevue/multiselect'
import OverlayBadge from 'primevue/overlaybadge'
import Paginator from 'primevue/paginator'
import Select from 'primevue/select'
import Toast from 'primevue/toast'
import ToastService from 'primevue/toastservice'
import ToggleButton from 'primevue/togglebutton'
import ToggleSwitch from 'primevue/toggleswitch'
import Toolbar from 'primevue/toolbar'
import App from './App.vue'
import router from './router'
const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(ToastService)
// const PMAdmin = definePreset(Lara, {
//   //Your customizations, see the following sections for examples
//   semantic: {
//     primary: {
//       50: '{sky.50}',
//       100: '{sky.100}',
//       200: '{sky.200}',
//       300: '{sky.300}',
//       400: '{sky.400}',
//       500: '{sky.500}',
//       600: '{sky.600}',
//       700: '{sky.700}',
//       800: '{sky.800}',
//       900: '{sky.900}',
//       950: '{sky.950}',
//     },
//   },
// })

app.use(PrimeVue, {
  theme: {
    preset: PMAdmin,
  },
})

app.component('Button', Button)
app.component('Chart', Chart)
app.component('Chip', Chip)
app.component('Column', Column)
app.component('Divider', Divider)
app.component('DataTable', DataTable)
app.component('DatePicker', DatePicker)
app.component('Dialog', Dialog)
app.component('FileUpload', FileUpload)
app.component('Image', Image)
app.component('InputIcon', InputIcon)
app.component('IconField', IconField)
app.component('InputText', InputText)
app.component('InputNumber', InputNumber)
app.component('Menu', Menu)
app.component('MultiSelect', MultiSelect)
app.component('OverlayBadge', OverlayBadge)
app.component('Paginator', Paginator)
app.component('Select', Select)
app.component('SelectButton', SelectButton)
app.component('Tag', Tag)
app.component('Toast', Toast)
app.component('ToggleSwitch', ToggleSwitch)
app.component('ToggleButton', ToggleButton)
app.component('Toolbar', Toolbar)
app.directive('tooltip', Tooltip)
app.directive('badge', BadgeDirective)
app.directive('ripple', Ripple)
app.directive('styleclass', StyleClass)
app.directive('focustrap', FocusTrap)
app.directive('animateonscroll', AnimateOnScroll)
app.use(ToastService)

declare global {
  interface Window {
    rootInstance: any
  }
}
// Mount the app and store the root component instance in window
window.rootInstance = app.mount('#app')

// @ts-ignore
// window.Auth = Auth
