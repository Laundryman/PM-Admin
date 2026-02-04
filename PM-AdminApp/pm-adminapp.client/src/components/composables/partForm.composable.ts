import { Part } from '@/models/Parts/part.model'
import { usePartStore } from '@/stores/partStore'
import { ref, type Ref } from 'vue'

export function usePartForm() {
  const partStore = usePartStore()
  const cassettePhotoSrc = ref<string | null>(null)
  const cassetteRenderSrc = ref<string | null>(null)
  const renderFile = ref<File | null>(null)
  const iconFile = ref<File | null>(null)

  function createPartFormData(partModel: Ref<Part>): FormData {
    const partData = new FormData()
    partData.append('id', partModel.value.id?.toString() || '0')
    partData.append('brandId', partModel.value.brandId?.toString() || '0')
    partData.append('name', partModel.value.name || '')
    partData.append('description', partModel.value.description || '')
    partData.append('partNumber', partModel.value.partNumber || '')
    partData.append('altPartNumber', partModel.value.altPartNumber || '')
    partData.append('customerRefNo', partModel.value.customerRefNo || '')
    partData.append('published', partModel.value.published?.toString() || 'false')
    partData.append('discontinued', partModel.value.discontinued?.toString() || 'false')
    partData.append('facings', partModel.value.facings?.toString() || '0')
    partData.append('height', partModel.value.height?.toString() || '0')
    partData.append('width', partModel.value.width?.toString() || '0')
    partData.append('depth', partModel.value.depth?.toString() || '0')
    partData.append('stock', partModel.value.stock?.toString() || '0')
    partData.append('regionsList', partModel.value.regionsList || '')
    partData.append('countriesList', partModel.value.countriesList || '')
    partData.append('categoryId', partModel.value.categoryId?.toString() || '0')
    partData.append('parentCategoryId', partModel.value.parentCategoryId?.toString() || '0')
    partData.append('categoryName', partModel.value.categoryName || '')
    partData.append('parentCategoryName', partModel.value.parentCategoryName || '')
    partData.append('partTypeId', partModel.value.partTypeId?.toString() || '0')
    partData.append('partTypeName', partModel.value.partTypeName || '')
    partData.append('shoppingHeight', partModel.value.shoppingHeight?.toString() || '0')
    partData.append(
      'dateCreated',
      partModel.value.dateCreated?.toString() || new Date().toISOString(),
    )
    partData.append('dateUpdated', new Date().toISOString())
    partData.append('shoppable', partModel.value.shoppable?.toString() || 'false')
    partData.append('packShotImageSrc', partModel.value.packShotImageSrc || '')
    partData.append('render2dImage', partModel.value.render2dImage || '')
    partData.append('svgLineGraphic', partModel.value.svgLineGraphic || '')
    partData.append('unitCost', partModel.value.unitCost?.toString() || '0')
    partData.append('launchPrice', partModel.value.launchPrice?.toString() || '0')
    partData.append('launchDate', partModel.value.launchDate?.toISOString() || '')
    partData.append('presentation', partModel.value.presentation || '')
    partData.append('cassetteBio', partModel.value.cassetteBio || '')
    partData.append('manufacturingProcess', partModel.value.manufacturingProcess || '')
    partData.append('testingType', partModel.value.testingType || '')
    partData.append('internationalPart', partModel.value.internationalPart?.toString() || 'false')
    partData.append('dmiReco', partModel.value.dmiReco?.toString() || 'false')
    partData.append('hidePrices', partModel.value.hidePrices?.toString() || 'false')
    partData.append('regions', JSON.stringify(partModel.value.regions || []))
    partData.append('countries', JSON.stringify(partModel.value.countries || []))
    partData.append('products', JSON.stringify(partModel.value.products || []))
    // partData.append('partType', JSON.stringify(partModel.value.PartType || {}))
    //   // Category!: Category
    partData.append('standTypes', JSON.stringify(partModel.value.standTypes || []))
    return partData
  }
  return { createPartFormData }
}
