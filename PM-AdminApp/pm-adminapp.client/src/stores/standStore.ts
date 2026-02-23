import type { searchStandInfo } from '@/models/Stands/searchStandInfo.model'
import type { Stand } from '@/models/Stands/stand.model'
import StandService from '@/services/Stands/StandService'
import { acceptHMRUpdate, defineStore } from 'pinia'
import { ref } from 'vue'

export const useStandStore = defineStore('standStore', () => {
  const error = ref<string>()
  const stand = ref<Stand>()
  const standList = ref<searchStandInfo[]>()
  const stands = ref<Stand[]>([])
  const initialized = ref(false)

  async function initialize(id: number) {
    if (initialized.value != true) await StandService.initialise()
    return await StandService.getStand(id)
      .then((data: Stand) => {
        stand.value = data
        initialized.value = true
        return data
      })
      .catch((err) => {
        error.value = err.message
        initialized.value = false
      })
  }

  async function saveStand(standData: Stand) {
    return await StandService.saveStand(standData)
      .then((response) => {
        stand.value = response
        return response
      })
      .catch((err) => {
        error.value = err.message
        return null
      })
  }

  return {
    initialized,
    stand,
    error,
    initialize,
    saveStand,
    stands,
    standList,
  }
})

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useStandStore, import.meta.hot))
}
