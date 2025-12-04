import { Country } from '@/models/Countries/country.model'

export class SearchPartInfo {
  id!: number
  brandId!: number
  name!: string
  description!: string
  partNumber!: string
  altPartNumber!: string
  customerRefNo!: string
  facings!: number
  height!: number
  width!: number
  depth!: number
  stock!: number
  categoryId!: number
  categoryName!: string
  parentCategoryId!: number
  parentCategoryName!: string
  partTypeId!: number
  partTypeName!: string
  regionList!: string
  dateCreated!: Date
  dateUpdated!: Date
  published!: boolean
  countryList!: string
  discontinued!: boolean
  testingType!: string
  shoppable!: boolean
  countries!: Country[]
  //PartType!: PartType;

  contstructor() {
    this.id = 0
    this.facings = 0
    this.partNumber = ''
  }
}
