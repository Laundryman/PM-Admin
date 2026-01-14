import type { PartType } from '@/models/Parts/partType.model'
import { PartTypeService } from '@/services/PartTypes/partTypeService'
import { ref } from 'vue'

export function usePartTypes() {
  const partTypes = ref<PartType[] | null>([])
  async function getPartTypes() {
    await PartTypeService.initialise().catch((error) =>
      console.error('Error initializing PartType Service:', error),
    )
    await PartTypeService.getPartTypes().then((response: PartType[]) => {
      partTypes.value = response
      console.log('Part Types loaded', partTypes.value)
    })
    return partTypes.value
  }

  return { partTypes, getPartTypes }
}
