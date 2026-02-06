import type { Product } from '@/models/Products/product.model'
import type { ProductFilter } from '@/models/Products/productFilter.model'
import type { searchProductInfo } from '@/models/Products/searchProductInfo.model'
import ProductService from '@/services/Products/ProductService'
import { acceptHMRUpdate, defineStore } from 'pinia'
import { ref } from 'vue'

export const useProductStore = defineStore('productStore', () => {
  const error = ref<string>()
  const product = ref<Product>()
  const productList = ref<searchProductInfo[]>()
  const products = ref<Product[]>([])
  const initialized = ref(false)

  async function initialize(id: number) {
    if (initialized.value === true) {
      return productList.value
    }
    await ProductService.initialise()
    return await ProductService.getProduct(id)
      .then((data: Product) => {
        product.value = data
        initialized.value = true
        return data
      })
      .catch((err) => {
        error.value = err.message
        initialized.value = false
      })
  }

  async function getProductsByCategory(productFilter: ProductFilter) {
    await ProductService.initialise()

    await ProductService.getProductsByCategory(productFilter).then((data: Product[]) => {
      products.value = data
      return data
    })
    return products.value
  }

  return { initialized, product, error, initialize, products, productList, getProductsByCategory }
})

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useProductStore, import.meta.hot))
}
