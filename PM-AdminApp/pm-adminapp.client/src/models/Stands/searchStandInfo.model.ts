export class searchStandInfo {
  id!: number
  name!: string
  standTypeName!: string
  parentStandTypeName!: string
  categoryId!: number
  parentCategoryId!: number
  regionsList!: string
  countriesList!: string
  brandId!: number
  dateCreated!: Date
  dateUpdated!: Date
  published!: boolean
  discontinued!: boolean
  cols!: number
  rows!: number
  allowOverhang!: string
  layoutStyle!: number
  StandAssemblyNumber!: string
  height!: number
  width!: number
  spanShelves!: boolean
  // productImage!: string
  contstructor() {
    this.id = 0
  }
}
