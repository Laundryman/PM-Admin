import { Brand } from '@/models/Brands/brand.model'
import type { MenuItem } from 'primevue/menuitem'
import { computed, reactive } from 'vue'
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
  brandsLoaded?: boolean
}

const layoutConfig = reactive<LayoutConfig>({
  preset: 'Lara',
  primary: 'blue',
  surface: null,
  darkTheme: false,
  menuMode: 'static',
})

const layoutState = reactive<LayoutState>({
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

export function useLayout() {
  const setActiveMenuItem = (item: MenuItem) => {
    const anyItem = item as { value?: MenuItem }
    layoutState.activeMenuItem = anyItem?.value !== undefined ? anyItem.value : item
  }

  const toggleDarkMode = () => {
    if (!document.startViewTransition) {
      executeDarkModeToggle()
      return
    }
    document.startViewTransition(() => executeDarkModeToggle())
  }

  const executeDarkModeToggle = () => {
    layoutConfig.darkTheme = !layoutConfig.darkTheme
    document.documentElement.classList.toggle('app-dark')
  }
  const setActiveBrand = (brand: Brand | null) => {
    if (brand) {
      layoutState.activeBrand = brand
      console.log('Active brand set to:', layoutState.activeBrand)
    }
  }

  const toggleMenu = () => {
    if (layoutConfig.menuMode === 'overlay') {
      layoutState.overlayMenuActive = !layoutState.overlayMenuActive
    }

    if (window.innerWidth > 991) {
      layoutState.staticMenuDesktopInactive = !layoutState.staticMenuDesktopInactive
    } else {
      layoutState.staticMenuMobileActive = !layoutState.staticMenuMobileActive
    }
  }

  const isSidebarActive = computed(
    () => layoutState.overlayMenuActive || layoutState.staticMenuMobileActive,
  )
  const isDarkTheme = computed(() => layoutConfig.darkTheme)
  const getPrimary = computed(() => layoutConfig.primary)
  const getSurface = computed(() => layoutConfig.surface)
  const getActiveBrand = computed(() => layoutState.activeBrand)
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
}
