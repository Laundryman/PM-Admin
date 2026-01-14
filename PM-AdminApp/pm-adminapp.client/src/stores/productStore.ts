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

  async function initialize(productFilter: ProductFilter) {
    if (initialized.value === true) {
      return productList.value
    }
    await ProductService.initialise()
    return await ProductService.searchProducts(productFilter ?? 0).then(
      (data: searchProductInfo[]) => {
        productList.value = data
        initialized.value = true
        return data
      },
    )
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
