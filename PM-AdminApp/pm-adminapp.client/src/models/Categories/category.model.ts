export class Category {
  id!: number
  parentCategoryId!: number
  name!: string
  parentCategoryName!: string
  subCategories!: Category[]
  contstructor() {
    this.id = 0
    this.name = ''
    this.parentCategoryId = 0
  }
}
