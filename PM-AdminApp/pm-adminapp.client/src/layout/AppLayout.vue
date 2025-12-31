<script setup lang="ts">
import { useLayoutStore } from '@/layout/composables/layout'
import { storeToRefs } from 'pinia'
import { computed, ref, watch } from 'vue'
import AppFooter from './AppFooter.vue'
import AppSidebar from './AppSidebar.vue'
import AppTopbar from './AppTopbar.vue'

const layoutStore = useLayoutStore()
const { layoutConfig, layoutState } = useLayoutStore()
const { isSidebarActive } = storeToRefs(layoutStore)

const outsideClickListener = ref()

watch(isSidebarActive, (newVal) => {
  if (newVal) {
    bindOutsideClickListener()
  } else {
    unbindOutsideClickListener()
  }
})

const containerClass = computed(() => {
  return {
    'layout-overlay': layoutConfig.menuMode === 'overlay',
    'layout-static': layoutConfig.menuMode === 'static',
    'layout-static-inactive':
      layoutState.staticMenuDesktopInactive && layoutConfig.menuMode === 'static',
    'layout-overlay-active': layoutState.overlayMenuActive,
    'layout-mobile-active': layoutState.staticMenuMobileActive,
    'layout-planner-loaded': layoutState.plannerLoaded,
  }
})

const plannerLoaded = computed(() => {
  return layoutState.plannerLoaded ? 'planner-loaded' : ''
})

function bindOutsideClickListener() {
  if (!outsideClickListener.value) {
    outsideClickListener.value = (event: MouseEvent) => {
      if (isOutsideClicked(event)) {
        layoutState.overlayMenuActive = false
        layoutState.staticMenuMobileActive = false
        layoutState.menuHoverActive = false
      }
    }
    document.addEventListener('click', outsideClickListener.value)
  }
}

function unbindOutsideClickListener() {
  if (outsideClickListener.value) {
    document.removeEventListener('click', outsideClickListener.value)
    outsideClickListener.value = null
  }
}

function isOutsideClicked(event: MouseEvent) {
  const sidebarEl = document.querySelector('.layout-sidebar')
  const topbarEl = document.querySelector('.layout-menu-button')

  return !(
    sidebarEl?.isSameNode(event.target as Node) ||
    sidebarEl?.contains(event.target as Node) ||
    topbarEl?.isSameNode(event.target as Node) ||
    topbarEl?.contains(event.target as Node)
  )
}
</script>

<template>
  <div class="layout-wrapper" :class="containerClass">
    <app-topbar></app-topbar>
    <app-sidebar></app-sidebar>
    <div class="layout-main-container">
      <div class="layout-main">
        <router-view></router-view>
      </div>
      <app-footer></app-footer>
    </div>
    <div class="layout-mask animate-fadein"></div>
  </div>
  <Toast />
</template>
