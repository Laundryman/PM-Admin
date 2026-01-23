import { Country } from '@/models/Countries/country.model'
import { Product } from '@/models/Products/product.model'
import type { Region } from '../Countries/region.model'
import type { StandType } from '../StandTypes/standType.model'
import type { PartType } from './partType.model'

export class Part {
  id!: number
  brandId!: number
  name!: string
  description!: string
  partNumber!: string
  altPartNumber!: string
  customerRefNo!: string
  published!: boolean
  discontinued!: boolean
  facings!: number
  height!: number
  width!: number
  depth!: number
  stock!: number
  regionsList!: string
  countriesList!: string
  categoryId!: number
  parentCategoryId!: number
  categoryName!: string
  parentCategoryName!: string
  partTypeId!: number
  partTypeName!: string
  shoppingHeight!: number
  dateCreated!: Date
  dateUpdated!: Date
  shoppable!: boolean
  packShotImageSrc!: string
  render2dImage!: string
  svgLineGraphic!: string

  unitCost!: number
  launchPrice!: number
  launchDate!: Date
  presentation!: string
  cassetteBio!: string
  manufacturingProcess!: string
  testingType!: string
  internationalPart!: boolean
  dmiReco!: boolean
  hidePrices!: boolean
  regions!: Region[]
  countries!: Country[]
  products!: Product[]
  PartType!: PartType
  // Category!: Category
  standTypes!: StandType[]
  // regionId!: number
  // regionName!: string
  // planoImageSrc!: string
  // countryList!: string
  // shadeSelectByFacing: any

  contstructor() {
    this.id = 0
    this.facings = 0
    this.partNumber = ''
    this.products = []
  }
}
