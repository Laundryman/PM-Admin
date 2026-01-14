import type { StandType } from '@/models/StandTypes/standType.model'
import { standTypeFilter } from '@/models/StandTypes/standTypeFilter.model'
import StandTypeService from '@/services/StandTypes/StandTypeService'
import { useBrandStore } from '@/stores/brandStore'
import { ref } from 'vue'

export function useStandTypes() {
  const brandStore = useBrandStore()
  const standTypes = ref<StandType[] | null>([])

  async function getPartStandTypes() {
    await StandTypeService.initialise().catch((error) =>
      console.error('Error initializing StandType Service:', error),
    )
    var standTypeFilter = {} as standTypeFilter
    standTypeFilter.brandId = brandStore.activeBrand?.id || 0
    await StandTypeService.getAllStandTypes(standTypeFilter).then((response) => {
      standTypes.value = response.data
      console.log('Stand Types loaded', standTypes.value)
    })
    return standTypes.value
  }

  return { standTypes, getPartStandTypes }
}
