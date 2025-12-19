export class Category {
  id!: number
  parentId!: number
  name!: string
  parentCategoryName!: string
  subCategories!: Category[]
  contstructor() {
    this.id = 0
    this.name = ''
    this.parentId = 0
  }
}
