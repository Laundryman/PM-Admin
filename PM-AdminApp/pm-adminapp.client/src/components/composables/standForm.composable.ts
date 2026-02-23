import { Stand } from '@/models/Stands/stand.model'
import { useStandStore } from '@/stores/standStore'
import { type Ref } from 'vue'

export function useStandForm() {
  const standStore = useStandStore()
  function createStandFormData(standModel: Ref<Stand>): FormData {
    const standData = new FormData()
    standData.append('id', standModel.value.id?.toString() || '0')
    standData.append('brandId', standModel.value.brandId?.toString() || '0')
    standData.append('name', standModel.value.name || '')
    standData.append('description', standModel.value.description || '')
    standData.append(
      'dateCreated',
      standModel.value.dateCreated?.toISOString() || new Date().toISOString(),
    )
    standData.append('dateUpdated', new Date().toISOString())
    standData.append('dateAvailable', new Date().toISOString())
    standData.append('published', standModel.value.published?.toString() || 'false')
    standData.append('height', standModel.value.height?.toString() || '0')
    standData.append('width', standModel.value.width?.toString() || '0')
    standData.append('standTypeId', standModel.value.standTypeId?.toString() || '0')
    standData.append('standTypeName', standModel.value.standTypeName || '')
    standData.append('parentStandTypeId', standModel.value.parentStandTypeId?.toString() || '0')
    standData.append('parentStandTypeName', standModel.value.parentStandTypeName || '')
    standData.append('merchHeight', standModel.value.merchHeight?.toString() || '0')
    standData.append('merchWidth', standModel.value.merchWidth?.toString() || '0')
    standData.append('headerHeight', standModel.value.headerHeight?.toString() || '0')
    standData.append('headerWidth', standModel.value.headerWidth?.toString() || '0')
    standData.append('headerGraphic', standModel.value.headerGraphic?.toString() || '0')
    standData.append('footerHeight', standModel.value.footerHeight?.toString() || '0')
    standData.append('footerWidth', standModel.value.footerWidth?.toString() || '0')
    standData.append(
      'horizontalPitchCount',
      standModel.value.horizontalPitchCount?.toString() || '0',
    )
    standData.append('horizontalPitchSize', standModel.value.horizontalPitchSize?.toString() || '')
    standData.append('cols', standModel.value.cols?.toString() || '0')
    standData.append('columnList', JSON.stringify(standModel.value.columnList || []))
    standData.append('rowList', JSON.stringify(standModel.value.rowList || []))
    standData.append('equalCols', standModel.value.equalCols?.toString() || 'false')
    standData.append('defaultColWidth', standModel.value.defaultColWidth?.toString() || '')
    standData.append('standAssemblyNumber', standModel.value.standAssemblyNumber?.toString() || '')
    standData.append('layoutStyle', standModel.value.layoutStyle?.toString() || '0')
    standData.append('spanShelves', standModel.value.spanShelves?.toString() || 'false')
    standData.append('rows', standModel.value.rows?.toString() || '')
    standData.append('equalRows', standModel.value.equalRows?.toString() || 'false')
    standData.append('defaultRowHeight', standModel.value.defaultRowHeight?.toString() || '')
    standData.append('shelfIncrement', standModel.value.shelfIncrement?.toString() || '')
    standData.append('discontinued', standModel.value.discontinued?.toString() || 'false')
    standData.append('allowOverHang', standModel.value.allowOverHang?.toString() || 'false')
    standData.append('countriesList', standModel.value.countriesList || '')
    standData.append('regionsList', standModel.value.regionsList || '')
    standData.append('standType', JSON.stringify(standModel.value.standType || {}))
    standData.append('regions', JSON.stringify(standModel.value.regions || []))
    standData.append('countries', JSON.stringify(standModel.value.countries || []))
    standData.append('totalColWidth', standModel.value.totalColWidth?.toString() || '0')
    standData.append('totalRowHeight', standModel.value.totalRowHeight?.toString() || '0')
    return standData
  }
  return { createStandFormData }
}
