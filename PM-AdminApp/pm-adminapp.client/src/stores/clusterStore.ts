import { Cluster } from '@/models/Clusters/cluster.model'
import { ClusterFilter } from '@/models/Clusters/clusterFilter.model'
import { default as clusterService } from '@/services/Clusters/ClusterService'

import { acceptHMRUpdate, defineStore } from 'pinia'
import { ref } from 'vue'

export const useClusterStore = defineStore('clusterStore', () => {
  const error = ref<string>()
  const cluster = ref<Cluster>(new Cluster())
  const initialized = ref(false)
  const activeCluster = ref<Cluster>()
  const dirty = ref<boolean>(false)

  async function initialize(clusterFilter: ClusterFilter): Promise<void> {
    await clusterService.initialise()

    if (clusterFilter?.id != 0) {
      await clusterService
        .getCluster(clusterFilter.id)
        .then((data) => {
          cluster.value = data as Cluster
          initialized.value = true
        })
        .catch((err) => {
          error.value = err.message
        })
    } else {
      cluster.value = new Cluster()
      initialized.value = true
    }
  }

  //   async function saveCluster(updatedCluster: FormData, id: Number): Promise<void> {
  //     await planogramService.initialise()
  //     if (id == 0) {
  //       await planogramService
  //         .createCluster(updatedCluster)
  //         .then((data) => {
  //           cluster.value = data
  //         })
  //         .catch((err) => {
  //           error.value = err.message
  //         })
  //       return
  //     } else {
  //       // update the existing cluster
  //       await planogramService
  //         .saveCluster(updatedCluster, id)
  //         .then((data) => {
  //           cluster.value = data
  //         })
  //         .catch((err) => {
  //           error.value = err.message
  //         })
  //     }
  //   }
  return { cluster, error, initialize, activeCluster, dirty }
})

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useClusterStore, import.meta.hot))
}
