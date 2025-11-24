import { Brand } from '@/models/Brands/brand.model'
<<<<<<< HEAD
import { defineStore } from 'pinia'
import type { MenuItem } from 'primevue/menuitem'
import { computed, reactive, ref } from 'vue'
=======
import type { MenuItem } from 'primevue/menuitem'
import { computed, reactive } from 'vue'
>>>>>>> 02efb5c (working signin and api authorize)
interface LayoutConfig {
  preset: string
  primary: string
  surface: string | null
  darkTheme: boolean
  menuMode: 'static' | 'overlay'
}

interface LayoutState {
  staticMenuDesktopInactive: boolean
  overlayMenuActive: boolean
  profileSidebarVisible: boolean
  configSidebarVisible: boolean
  staticMenuMobileActive: boolean
  menuHoverActive: boolean
  activeMenuItem: MenuItem | null
  activeBrand: Brand | null
<<<<<<< HEAD
  brandsLoaded?: boolean
=======
>>>>>>> 02efb5c (working signin and api authorize)
}

const layoutConfig = reactive<LayoutConfig>({
  preset: 'Lara',
  primary: 'blue',
  surface: null,
  darkTheme: false,
  menuMode: 'static',
})

<<<<<<< HEAD
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
=======
const layoutState = reactive<LayoutState>({
  staticMenuDesktopInactive: false,
  overlayMenuActive: false,
  profileSidebarVisible: false,
  configSidebarVisible: false,
  staticMenuMobileActive: false,
  menuHoverActive: false,
  activeMenuItem: null,
  activeBrand: null,
})
>>>>>>> 02efb5c (working signin and api authorize)

export const useLayoutStore = defineStore('layout', () => {
  const layoutState = ref<LayoutState>({
    staticMenuDesktopInactive: false,
    overlayMenuActive: false,
    profileSidebarVisible: false,
    configSidebarVisible: false,
    staticMenuMobileActive: false,
    menuHoverActive: false,
    activeMenuItem: null,
    activeBrand: null,
    brandsLoaded: false,
  })

  function setActiveMenuItem(item: MenuItem) {
    const anyItem = item as { value?: MenuItem }
    layoutState.value.activeMenuItem = anyItem?.value !== undefined ? anyItem.value : item
  }

  function toggleDarkMode() {
    if (!document.startViewTransition) {
      executeDarkModeToggle()
      return
    }
    document.startViewTransition(() => executeDarkModeToggle())
  }

  function executeDarkModeToggle() {
    layoutConfig.darkTheme = !layoutConfig.darkTheme
    document.documentElement.classList.toggle('app-dark')
  }
<<<<<<< HEAD
  function setActiveBrand(brand: Brand | null) {
    if (brand) {
      layoutState.value.activeBrand = brand
      console.log('Active brand set to:', layoutState.value.activeBrand)
    }
  }

  function toggleMenu() {
=======
  const setActiveBrand = (brand: Brand) => {
    layoutState.activeBrand = brand
  }
  const toggleMenu = () => {
>>>>>>> 02efb5c (working signin and api authorize)
    if (layoutConfig.menuMode === 'overlay') {
      layoutState.value.overlayMenuActive = !layoutState.value.overlayMenuActive
    }

    if (window.innerWidth > 991) {
      layoutState.value.staticMenuDesktopInactive = !layoutState.value.staticMenuDesktopInactive
    } else {
      layoutState.value.staticMenuMobileActive = !layoutState.value.staticMenuMobileActive
    }
  }

  const isSidebarActive = computed(
    () => layoutState.value.overlayMenuActive || layoutState.value.staticMenuMobileActive,
  )
  const isDarkTheme = computed(() => layoutConfig.darkTheme)
  const getPrimary = computed(() => layoutConfig.primary)
  const getSurface = computed(() => layoutConfig.surface)
<<<<<<< HEAD
  const getActiveBrand = computed(() => layoutState.value.activeBrand)

=======
  const getActiveBrand = computed(() => layoutState.activeBrand)
>>>>>>> 02efb5c (working signin and api authorize)
  return {
    layoutConfig,
    layoutState,
    toggleMenu,
    isSidebarActive,
    isDarkTheme,
    getPrimary,
    getSurface,
    setActiveMenuItem,
    toggleDarkMode,
    setActiveBrand,
    getActiveBrand,
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
