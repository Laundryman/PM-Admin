import { PlanogramFilter } from '@/models/Planograms/planogramFilter.model'
import { Planogram } from '@/planner/models/Planogram'
import { default as planogramService } from '@/services/Planograms/PlanogramService'

import { acceptHMRUpdate, defineStore } from 'pinia'
import { ref } from 'vue'

export const usePlanogramStore = defineStore('planogramStore', () => {
  const error = ref<string>()
  const planogram = ref<Planogram>(new Planogram())
  const initialized = ref(false)
  const activePlanogram = ref<Planogram>()
  const dirty = ref<boolean>(false)

  async function initialize(planogramFilter: PlanogramFilter): Promise<void> {
    await planogramService.initialise()

    if (planogramFilter?.id != 0) {
      await planogramService
        .getPlanogram(planogramFilter.id)
        .then((data) => {
          planogram.value = data as Planogram
          initialized.value = true
        })
        .catch((err) => {
          error.value = err.message
        })
    } else {
      planogram.value = new Planogram()
      planogram.value.id = 0
      initialized.value = true
    }
  }

  //   async function savePlanogram(updatedPlanogram: FormData, id: Number): Promise<void> {
  //     await planogramService.initialise()
  //     if (id == 0) {
  //       await planogramService
  //         .createPlanogram(updatedPlanogram)
  //         .then((data) => {
  //           planogram.value = data
  //         })
  //         .catch((err) => {
  //           error.value = err.message
  //         })
  //       return
  //     } else {
  //       // update the existing planogram
  //       await planogramService
  //         .savePlanogram(updatedPlanogram, id)
  //         .then((data) => {
  //           planogram.value = data
  //         })
  //         .catch((err) => {
  //           error.value = err.message
  //         })
  //     }
  //   }
  return { planogram, error, initialize, activePlanogram, dirty }
})

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(usePlanogramStore, import.meta.hot))
}
