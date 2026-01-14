import type { Brand } from '@/models/Brands/brand.model'
import brandService from '@/services/Brands/BrandService'
import { acceptHMRUpdate, defineStore } from 'pinia'

import { ref } from 'vue'

export const useBrandStore = defineStore('brandStore', () => {
  const brands = ref<Brand[]>([])
  const brandsLoaded = ref<boolean>(false)
  const loadingBrands = ref<boolean>(true)
  const activeBrand = ref<Brand | null>(null)
  const selectBrandPH = ref<string>('Loading Brands...')

  async function loadBrands() {
    if (!brandService.isInitialised) await brandService.initialise()
    console.log('Fetching brands...')
    await brandService
      .getBrands()
      .then((data) => {
        // create brandOptions array for select dropdown
        brands.value = data.data
        // let brandOptionsLocal: BrandOption[] = []
        // for (const brand of brands.value) {
        //   let brandOption = { name: brand.name, code: brand.id }
        //   brandOptionsLocal.push(brandOption)
        // }
        // console.log('Brand Options:', brandOptionsLocal)
        // brandOptions.value = brandOptionsLocal
        activeBrand.value = null //brands.value[0]
        loadingBrands.value = false
        brandsLoaded.value = true
        selectBrandPH.value = 'Select a Brand'
        console.log('Brands:', brands.value)
      })
      .catch((error) => {
        console.error('Error fetching brands:', error)
      })
    console.log('Completed fetching brands.')
  }

  return {
    brands,
    brandsLoaded,
    loadingBrands,
    activeBrand,
    selectBrandPH,
    loadBrands,
  }
})

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useBrandStore, import.meta.hot))
}
