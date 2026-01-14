import { Brand } from '@/models/Brands/brand.model'
import type { searchClusterInfo } from '@/models/Clusters/searchClusterInfo.model'
import { defineStore } from 'pinia'
import type { MenuItem } from 'primevue/menuitem'
import { computed, reactive, ref } from 'vue'
interface LayoutConfig {
  preset: string
  primary: string
  surface: string | null
  darkTheme: boolean
  menuMode: 'static' | 'overlay'
}

interface SystemState {
  staticMenuDesktopInactive: boolean
  overlayMenuActive: boolean
  profileSidebarVisible: boolean
  configSidebarVisible: boolean
  staticMenuMobileActive: boolean
  menuHoverActive: boolean
  activeMenuItem: MenuItem | null
  activeBrand: Brand | null
  activePart: Brand | null
  activeCluster: searchClusterInfo | null
  brandsLoaded?: boolean
  plannerLoaded?: boolean
}

const layoutConfig = reactive<LayoutConfig>({
  preset: 'Lara',
  primary: 'blue',
  surface: null,
  darkTheme: false,
  menuMode: 'static',
})

// const layoutState = reactive<LayoutState>({
//   staticMenuDesktopInactive: false,
//   overlayMenuActive: false,
//   profileSidebarVisible: false,
//   configSidebarVisible: false,
//   staticMenuMobileActive: false,
//   menuHoverActive: false,
//   activeMenuItem: null,
//   activeBrand: null,
//   brandsLoaded: false,
// })

export const useSystemStore = defineStore('layout', () => {
  const systemState = ref<SystemState>({
    staticMenuDesktopInactive: false,
    overlayMenuActive: false,
    profileSidebarVisible: false,
    configSidebarVisible: false,
    staticMenuMobileActive: false,
    menuHoverActive: false,
    activeMenuItem: null,
    activeBrand: null,
    activePart: null,
    activeCluster: null,
    brandsLoaded: false,
    plannerLoaded: false,
  })

  function setActiveMenuItem(item: MenuItem) {
    const anyItem = item as { value?: MenuItem }
    systemState.value.activeMenuItem = anyItem?.value !== undefined ? anyItem.value : item
  }

  function toggleDarkMode() {
    if (!document.startViewTransition) {
      executeDarkModeToggle()
      return
    }
    document.startViewTransition(() => executeDarkModeToggle())
  }

  function togglePlannerLayout() {
    systemState.value.plannerLoaded = !systemState.value.plannerLoaded
  }

  function executeDarkModeToggle() {
    layoutConfig.darkTheme = !layoutConfig.darkTheme
    document.documentElement.classList.toggle('app-dark')
  }
  function setActiveBrand(brand: Brand | null) {
    if (brand) {
      systemState.value.activeBrand = brand
      console.log('Active brand set to:', systemState.value.activeBrand)
    }
  }
  function setActiveCluster(cluster: searchClusterInfo | null) {
    if (cluster) {
      systemState.value.activeCluster = cluster
      console.log('Active cluster set to:', systemState.value.activeCluster)
    }
  }

  function toggleMenu() {
    if (layoutConfig.menuMode === 'overlay') {
      systemState.value.overlayMenuActive = !systemState.value.overlayMenuActive
    }

    if (window.innerWidth > 991) {
      systemState.value.staticMenuDesktopInactive = !systemState.value.staticMenuDesktopInactive
    } else {
      systemState.value.staticMenuMobileActive = !systemState.value.staticMenuMobileActive
    }
  }

  const isSidebarActive = computed(
    () => systemState.value.overlayMenuActive || systemState.value.staticMenuMobileActive,
  )
  const isDarkTheme = computed(() => layoutConfig.darkTheme)
  const getPrimary = computed(() => layoutConfig.primary)
  const getSurface = computed(() => layoutConfig.surface)
  const getActiveBrand = computed(() => systemState.value.activeBrand)

  return {
    layoutConfig,
    layoutState: systemState,
    toggleMenu,
    isSidebarActive,
    isDarkTheme,
    getPrimary,
    getSurface,
    setActiveMenuItem,
    toggleDarkMode,
    setActiveBrand,
    setActiveCluster,
    getActiveBrand,
    togglePlannerLayout,
  }
})

// export function useLayout() {
//   const setActiveMenuItem = (item: MenuItem) => {
//     const anyItem = item as { value?: MenuItem }
//     layoutState.activeMenuItem = anyItem?.value !== undefined ? anyItem.value : item
//   }

//   const toggleDarkMode = () => {
//     if (!document.startViewTransition) {
//       executeDarkModeToggle()
//       return
//     }
//     document.startViewTransition(() => executeDarkModeToggle())
//   }

//   const executeDarkModeToggle = () => {
//     layoutConfig.darkTheme = !layoutConfig.darkTheme
//     document.documentElement.classList.toggle('app-dark')
//   }
//   const setActiveBrand = (brand: Brand | null) => {
//     if (brand) {
//       layoutState.activeBrand = brand
//       console.log('Active brand set to:', layoutState.activeBrand)
//     }
//   }

//   const toggleMenu = () => {
//     if (layoutConfig.menuMode === 'overlay') {
//       layoutState.overlayMenuActive = !layoutState.overlayMenuActive
//     }

//     if (window.innerWidth > 991) {
//       layoutState.staticMenuDesktopInactive = !layoutState.staticMenuDesktopInactive
//     } else {
//       layoutState.staticMenuMobileActive = !layoutState.staticMenuMobileActive
//     }
//   }

//   const isSidebarActive = computed(
//     () => layoutState.overlayMenuActive || layoutState.staticMenuMobileActive,
//   )
//   const isDarkTheme = computed(() => layoutConfig.darkTheme)
//   const getPrimary = computed(() => layoutConfig.primary)
//   const getSurface = computed(() => layoutConfig.surface)
//   const getActiveBrand = computed(() => layoutState.activeBrand)
//   return {
//     layoutConfig,
//     layoutState,
//     toggleMenu,
//     isSidebarActive,
//     isDarkTheme,
//     getPrimary,
//     getSurface,
//     setActiveMenuItem,
//     toggleDarkMode,
//     setActiveBrand,
//     getActiveBrand,
//   }
// }
