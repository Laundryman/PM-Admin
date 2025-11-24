import { Category } from '@/models/Categories/category.model'
import { Country } from '@/models/Countries/country.model'
import { Product } from '@/models/Products/product.model'

export class Part {
  id!: number
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
  parentCategoryId!: number
  categoryName!: string
  parentCategoryName!: string
  partTypeId!: number
  partTypeName!: string
  regiondId!: number
  regionName!: string
  shoppingHeight!: number
  brandId!: number
  dateCreated!: Date
  dateUpdated!: Date
  published!: boolean
  countryList!: string

  planoImageSrc!: string
  packShotImageSrc!: string
  render2dImage!: string
  svgLineGraphic!: string

  shadeSelectByFacing: any
  discontinued!: boolean
  unitCost!: number
  manufacturingProcess!: string
  testingType!: string
  internationalPart!: boolean
  shoppable!: boolean
  launchPrice!: number
  launchDate!: Date
  cassetteBio!: string
  presentation!: string
  dmiReco!: boolean
  countries!: Country[]
  products!: Product[]
  //PartType!: PartType;
  Category!: Category

  contstructor() {
    this.id = 0
    this.facings = 0
    this.partNumber = ''
  }
}
