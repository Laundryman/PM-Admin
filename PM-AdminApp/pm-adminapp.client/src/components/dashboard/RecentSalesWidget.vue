<script setup lang="ts">
import { widgetService } from '@/services/Widgets/widgetService'
import { onMounted, ref } from 'vue'

const products = ref<any[]>()

function formatCurrency(value: number) {
  return value.toLocaleString('en-US', { style: 'currency', currency: 'USD' })
}

onMounted(() => {
  products.value = widgetService.getProductsData()
})
</script>

<template>
  <div class="card">
    <div class="font-semibold text-xl mb-4">Recent Approved Planograms</div>
    <DataTable :value="products" :rows="5" :paginator="true" responsiveLayout="scroll">
      <Column style="width: 15%" header="Image">
        <template #body="slotProps">
          <Skeleton height="2rem" width="3rem" class="mr-2 bg-amber-400"></Skeleton>
          <Skeleton size="3rem" class="mr-2 bg-amber-400"></Skeleton>
        </template>
      </Column>
      <Column field="name" header="Name" :sortable="true" style="width: 35%"></Column>
      <Column field="skus" header="Skus" :sortable="true" style="width: 35%">
        <template #body="slotProps">
          {{ slotProps.data.skus }}
        </template>
      </Column>
      <Column style="width: 15%" header="View">
        <template #body>
          <Button icon="pi pi-search" type="button" class="p-button-text"></Button>
        </template>
      </Column>
    </DataTable>
  </div>
</template>
