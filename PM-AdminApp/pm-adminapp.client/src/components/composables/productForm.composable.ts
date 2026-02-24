import type { Product } from '@/models/Products/product.model'
import { useProductStore } from '@/stores/productStore'
import { type Ref } from 'vue'

export function useProductForm() {
  const productStore = useProductStore()
  function createProductFormData(productModel: Ref<Product>): FormData {
    const productData = new FormData()
    productData.append('id', productModel.value.id?.toString() || '0')
    productData.append('brandId', productModel.value.brandId?.toString() || '0')
    productData.append('name', productModel.value.name || '')
    productData.append('fullDescription', productModel.value.fullDescription || '')
    productData.append('shortDescription', productModel.value.shortDescription || '')
    productData.append('categoryId', productModel.value.categoryId?.toString() || '0')
    productData.append('parentCategoryId', productModel.value.parentCategoryId?.toString() || '0')
    productData.append('categoryName', productModel.value.categoryName || '')
    productData.append('parentCategoryName', productModel.value.parentCategoryName || '')
    productData.append('productImage', productModel.value.productImage || '')
    productData.append(
      'dateCreated',
      productModel.value.dateCreated?.toISOString() || new Date().toISOString(),
    )
    productData.append('dateUpdated', new Date().toISOString())
    productData.append('published', productModel.value.published?.toString() || 'false')
    productData.append('discontinued', productModel.value.discontinued?.toString() || 'false')
    productData.append('countriesList', productModel.value.countriesList || '')
    productData.append('regionsList', productModel.value.regionsList || '')
    productData.append('regions', JSON.stringify(productModel.value.regions || []))
    productData.append('countries', JSON.stringify(productModel.value.countries || []))
    productData.append('shades', JSON.stringify(productModel.value.shades || []))

    return productData
  }
  return { createProductFormData }
}
