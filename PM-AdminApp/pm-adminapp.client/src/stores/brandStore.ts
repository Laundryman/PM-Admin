import type { Brand } from '@/models/Brands/brand.model'
import brandService from '@/services/Brands/BrandService'
import { useLocalStorage } from '@vueuse/core'
import { acceptHMRUpdate, defineStore, skipHydrate } from 'pinia'

import { ref } from 'vue'

export const useBrandStore = defineStore('brandStore', () => {
  const brands = ref<Brand[]>([])
  const brandsLoaded = ref<boolean>(false)
  const loadingBrands = ref<boolean>(true)
  const activeBrand = useLocalStorage('activeBrand', {} as Brand)
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
        console.log('active brand before check:', activeBrand.value)
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
    activeBrand: skipHydrate(activeBrand),
    selectBrandPH,
    loadBrands,
  }
})

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useBrandStore, import.meta.hot))
}
