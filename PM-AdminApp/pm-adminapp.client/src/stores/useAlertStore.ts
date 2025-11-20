import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAlertStore = defineStore('alert', () => {
  const alert = ref()
  function success(message: string) {
    alert.value = 'alert-success'
  }
  function error(message: string) {
    alert.value = 'alert-danger'
  }
  function clear() {
    alert.value = null
  }

  return { alert, success, error, clear }
})
