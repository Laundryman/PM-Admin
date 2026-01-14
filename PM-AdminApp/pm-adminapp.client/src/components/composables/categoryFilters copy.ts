import { Category } from '@/models/Categories/category.model'
import { default as categoryService } from '@/services/Categories/CategoryService'
import { ref } from 'vue'

export function useCategoryFilters() {
  const parentCategories = ref<Category[] | null>([])
  const childCategories = ref<Category[] | null>([])

  async function getParentCategories() {
    await categoryService
      .initialise()
      .catch((error) => console.error('Error initializing Category Service:', error))
    await categoryService.getParentCategories().then((response) => {
      parentCategories.value = response.data
      console.log('Parent Categories loaded', parentCategories.value)
    })
    return parentCategories.value
  }

  async function getChildCategories(parentId: number) {
    await categoryService
      .initialise()
      .catch((error) => console.error('Error initializing Category Service:', error))
    await categoryService.getChildCategories(parentId).then((response) => {
      childCategories.value = response.data
      console.log('Child Categories loaded for parent', parentId, childCategories.value)
    })
    return childCategories.value
  }

  return { parentCategories, childCategories, getParentCategories, getChildCategories }
}
