<script setup lang="ts">
import { ref } from 'vue'

import AppMenuItem from './AppMenuItem.vue'

const model = ref([
  {
    label: 'Home',
    items: [{ label: 'Dashboard', icon: 'pi pi-fw pi-home', to: '/home' }],
  },
  {
    label: 'Parts & Products',
    items: [
      { label: 'Parts', icon: 'pi pi-fw pi-id-card', to: '/parts' },
      { label: 'Products', icon: 'pi pi-fw pi-box', to: '/products' },
      // { label: 'Shades', icon: 'pi pi-fw pi-mobile', to: '/shades', class: 'rotated-icon' },
    ],
  },
  {
    label: 'Carcasses',
    icon: 'pi pi-fw pi-table',
    to: '/stands',
    items: [
      {
        label: 'Stands',
        icon: 'pi pi-fw pi-check-square',
        to: '/stands',
      },
      {
        label: 'Clusters',
        icon: 'pi pi-fw pi-briefcase',
        to: '/clusters',
      },
    ],
  },
  {
    label: 'Jobs',
    icon: 'pi pi-fw pi-table',
    to: '/jobfolders',
    items: [
      {
        label: 'Job Folders',
        icon: 'pi pi-fw pi-check-square',
        to: '/jobfolders',
      },
    ],
  },
  {
    label: 'Settings',
    icon: 'pi pi-fw pi-table',
    to: '/brands',
    items: [
      {
        label: 'Brands',
        icon: 'pi pi-fw pi-check-square',
        to: '/brands',
      },
      {
        label: 'Categories',
        icon: 'pi pi-fw pi-briefcase',
        to: '/categories',
      },
      {
        label: 'Stand Types',
        icon: 'pi pi-fw pi-briefcase',
        to: '/standtypes',
      },
      {
        label: 'Regions',
        icon: 'pi pi-fw pi-briefcase',
        to: '/regions',
      },
      {
        label: 'Countries',
        icon: 'pi pi-fw pi-briefcase',
        to: '/countries',
      },
    ],
  },
  {
    label: 'Users',
    icon: 'pi pi-fw pi-table',
    to: '/users',
    items: [
      {
        label: 'Users',
        icon: 'pi pi-fw pi-check-square',
        to: '/users',
      },
    ],
  },

  // {
  //   label: 'Hierarchy',
  //   items: [
  //     {
  //       label: 'Submenu 1',
  //       icon: 'pi pi-fw pi-bookmark',
  //       items: [
  //         {
  //           label: 'Submenu 1.1',
  //           icon: 'pi pi-fw pi-bookmark',
  //           items: [
  //             { label: 'Submenu 1.1.1', icon: 'pi pi-fw pi-bookmark' },
  //             { label: 'Submenu 1.1.2', icon: 'pi pi-fw pi-bookmark' },
  //             { label: 'Submenu 1.1.3', icon: 'pi pi-fw pi-bookmark' },
  //           ],
  //         },
  //         {
  //           label: 'Submenu 1.2',
  //           icon: 'pi pi-fw pi-bookmark',
  //           items: [{ label: 'Submenu 1.2.1', icon: 'pi pi-fw pi-bookmark' }],
  //         },
  //       ],
  //     },
  //     {
  //       label: 'Submenu 2',
  //       icon: 'pi pi-fw pi-bookmark',
  //       items: [
  //         {
  //           label: 'Submenu 2.1',
  //           icon: 'pi pi-fw pi-bookmark',
  //           items: [
  //             { label: 'Submenu 2.1.1', icon: 'pi pi-fw pi-bookmark' },
  //             { label: 'Submenu 2.1.2', icon: 'pi pi-fw pi-bookmark' },
  //           ],
  //         },
  //         {
  //           label: 'Submenu 2.2',
  //           icon: 'pi pi-fw pi-bookmark',
  //           items: [{ label: 'Submenu 2.2.1', icon: 'pi pi-fw pi-bookmark' }],
  //         },
  //       ],
  //     },
  //   ],
  // },
  // {
  //   label: 'Get Started',
  //   items: [
  //     {
  //       label: 'Documentation',
  //       icon: 'pi pi-fw pi-book',
  //       to: '/documentation',
  //     },
  //     {
  //       label: 'View Source',
  //       icon: 'pi pi-fw pi-github',
  //       url: 'https://github.com/primefaces/sakai-vue',
  //       target: '_blank',
  //     },
  //   ],
  // },
])
</script>

<template>
  <ul class="layout-menu">
    <template v-for="(item, i) in model" :key="item">
      <!-- <app-menu-item v-if="!item.separator" :item="item" :index="i"></app-menu-item> -->
      <app-menu-item :item="item" :index="i"></app-menu-item>
      <!-- <li v-if="item.separator" class="menu-separator"></li> -->
    </template>
  </ul>
</template>

<style lang="scss" scoped></style>
