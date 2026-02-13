import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import { createApp } from 'vue'
import './assets/main.css'
//import '@/assets/styles.scss'
import '@/assets/styles.scss'
import 'primeicons/primeicons.css'
import { AnimateOnScroll, BadgeDirective, FocusTrap, Ripple, StyleClass } from 'primevue'
import Button from 'primevue/button'
import Chart from 'primevue/chart'
import Chip from 'primevue/chip'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import DatePicker from 'primevue/datepicker'
import Dialog from 'primevue/dialog'
import Divider from 'primevue/divider'
import FileUpload from 'primevue/fileupload'
import IconField from 'primevue/iconfield'
import Image from 'primevue/image'
import InputIcon from 'primevue/inputicon'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Listbox from 'primevue/listbox'

import Menu from 'primevue/menu'
import Message from 'primevue/message'
import MultiSelect from 'primevue/multiselect'
import OverlayBadge from 'primevue/overlaybadge'
import Paginator from 'primevue/paginator'
import Password from 'primevue/password'
import Select from 'primevue/select'
import SelectButton from 'primevue/selectbutton'
import Skeleton from 'primevue/skeleton'
import Slider from 'primevue/slider'
import Tab from 'primevue/tab'
import TabList from 'primevue/tablist'
import TabPanel from 'primevue/tabpanel'
import TabPanels from 'primevue/tabpanels'
import Tabs from 'primevue/tabs'
import Tag from 'primevue/tag'
import Textarea from 'primevue/textarea'
import Toast from 'primevue/toast'
import ToastService from 'primevue/toastservice'
import ToggleButton from 'primevue/togglebutton'
import ToggleSwitch from 'primevue/toggleswitch'

import ConfirmationService from 'primevue/confirmationservice'
import Toolbar from 'primevue/toolbar'
import Tooltip from 'primevue/tooltip'
import App from './App.vue'
import PMAdmin from './planmatrThemePreset'
import router from './router'
const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(ToastService)
app.use(ConfirmationService)
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
app.component('Listbox', Listbox)
app.component('Menu', Menu)
app.component('MultiSelect', MultiSelect)
app.component('OverlayBadge', OverlayBadge)
app.component('Paginator', Paginator)
app.component('Password', Password)
app.component('Select', Select)
app.component('SelectButton', SelectButton)
app.component('Skeleton', Skeleton)
app.component('Slider', Slider)
app.component('TabPanel', TabPanel)
app.component('Tabs', Tabs)
app.component('TabList', TabList)
app.component('Tab', Tab)
app.component('TabPanels', TabPanels)
app.component('Tag', Tag)
app.component('Textarea', Textarea)
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
app.component('Message', Message)
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
