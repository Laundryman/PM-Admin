<script setup lang="ts">
import { ref } from 'vue'

const menu = ref()

const items = ref([
  { label: 'Add New', icon: 'pi pi-fw pi-plus' },
  { label: 'Remove', icon: 'pi pi-fw pi-trash' },
])

const toggleMenu = (event: Event) => {
  menu.value?.toggle(event)
}
</script>

<template>
  <div class="card">
    <div class="flex justify-between items-center mb-6">
      <div class="font-semibold text-xl">Most Used Products</div>
      <div>
        <Button
          icon="pi pi-ellipsis-v"
          class="p-button-text p-button-plain p-button-rounded"
          @click="toggleMenu"
        ></Button>

        <Menu ref="menu" popup :model="items" class="!min-w-40"></Menu>
      </div>
    </div>
    <ul class="list-none p-0 m-0">
      <li class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
        <div>
          <span class="text-surface-900 dark:text-surface-0 font-medium mr-2 mb-1 md:mb-0"
            >Striking Colour Gloss</span
          >
          <div class="mt-1 text-muted-color">Lips</div>
        </div>
        <div class="mt-2 md:mt-0 flex items-center">
          <div
            class="bg-surface-300 dark:bg-surface-500 rounded-border overflow-hidden w-40 lg:w-24"
            style="height: 8px"
          >
            <div class="bg-orange-500 h-full" style="width: 50%"></div>
          </div>
          <span class="text-orange-500 ml-4 font-medium">%50</span>
        </div>
      </li>
      <li class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
        <div>
          <span class="text-surface-900 dark:text-surface-0 font-medium mr-2 mb-1 md:mb-0"
            >Super Large Lashes</span
          >
          <div class="mt-1 text-muted-color">Eyes</div>
        </div>
        <div class="mt-2 md:mt-0 ml-0 md:ml-20 flex items-center">
          <div
            class="bg-surface-300 dark:bg-surface-500 rounded-border overflow-hidden w-40 lg:w-24"
            style="height: 8px"
          >
            <div class="bg-cyan-500 h-full" style="width: 16%"></div>
          </div>
          <span class="text-cyan-500 ml-4 font-medium">%16</span>
        </div>
      </li>
      <li class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
        <div>
          <span class="text-surface-900 dark:text-surface-0 font-medium mr-2 mb-1 md:mb-0"
            >Supernova Extravagant Blush</span
          >
          <div class="mt-1 text-muted-color">Face</div>
        </div>
        <div class="mt-2 md:mt-0 ml-0 md:ml-20 flex items-center">
          <div
            class="bg-surface-300 dark:bg-surface-500 rounded-border overflow-hidden w-40 lg:w-24"
            style="height: 8px"
          >
            <div class="bg-pink-500 h-full" style="width: 67%"></div>
          </div>
          <span class="text-pink-500 ml-4 font-medium">%67</span>
        </div>
      </li>
      <li class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
        <div>
          <span class="text-surface-900 dark:text-surface-0 font-medium mr-2 mb-1 md:mb-0"
            >Clearly Superior Tint</span
          >
          <div class="mt-1 text-muted-color">Face</div>
        </div>
        <div class="mt-2 md:mt-0 ml-0 md:ml-20 flex items-center">
          <div
            class="bg-surface-300 dark:bg-surface-500 rounded-border overflow-hidden w-40 lg:w-24"
            style="height: 8px"
          >
            <div class="bg-green-500 h-full" style="width: 35%"></div>
          </div>
          <span class="text-primary ml-4 font-medium">%35</span>
        </div>
      </li>
      <li class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
        <div>
          <span class="text-surface-900 dark:text-surface-0 font-medium mr-2 mb-1 md:mb-0"
            >Mat Lashes</span
          >
          <div class="mt-1 text-muted-color">Eyes</div>
        </div>
        <div class="mt-2 md:mt-0 ml-0 md:ml-20 flex items-center">
          <div
            class="bg-surface-300 dark:bg-surface-500 rounded-border overflow-hidden w-40 lg:w-24"
            style="height: 8px"
          >
            <div class="bg-purple-500 h-full" style="width: 75%"></div>
          </div>
          <span class="text-purple-500 ml-4 font-medium">%75</span>
        </div>
      </li>
      <li class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
        <div>
          <span class="text-surface-900 dark:text-surface-0 font-medium mr-2 mb-1 md:mb-0"
            >Superga Volumizing Mascara</span
          >
          <div class="mt-1 text-muted-color">Eyes</div>
        </div>
        <div class="mt-2 md:mt-0 ml-0 md:ml-20 flex items-center">
          <div
            class="bg-surface-300 dark:bg-surface-500 rounded-border overflow-hidden w-40 lg:w-24"
            style="height: 8px"
          >
            <div class="bg-teal-500 h-full" style="width: 40%"></div>
          </div>
          <span class="text-teal-500 ml-4 font-medium">%40</span>
        </div>
      </li>
    </ul>
  </div>
</template>
