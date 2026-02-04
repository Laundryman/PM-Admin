import { Part } from '@/models/Parts/part.model'
import type { PartFilter } from '@/models/Parts/partFilter.model'
import type { Product } from '@/models/Products/product.model'
import { partService } from '@/services/Parts/partService'
import { acceptHMRUpdate, defineStore } from 'pinia'
import { ref } from 'vue'

export const usePartStore = defineStore('partStore', () => {
  const error = ref<string>()
  const part = ref<Part>(new Part())
  // const products = ref<Product[]>()
  const initialized = ref(false)
  const activeProduct = ref<Product>()

  async function initialize(partFilter?: PartFilter): Promise<void> {
    await partService.initialise()

    if (partFilter?.Id != 0) {
      await partService
        .getPart(partFilter?.Id ?? 0)
        .then((data) => {
          part.value = data
          initialized.value = true
        })
        .catch((err) => {
          error.value = err.message
        })
    } else {
      part.value = new Part()
      initialized.value = true
    }
  }

  async function savePart(updatedPart: FormData, id: Number): Promise<void> {
    await partService.initialise()
    if (id == 0) {
      await partService
        .createPart(updatedPart)
        .then((data) => {
          part.value = data
        })
        .catch((err) => {
          error.value = err.message
        })
      return
    } else {
      // update the existing part
      await partService
        .savePart(updatedPart)
        .then((data) => {
          part.value = data
        })
        .catch((err) => {
          error.value = err.message
        })
    }
  }
  return { part, error, initialize, activeProduct, savePart }
})

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(usePartStore, import.meta.hot))
}
