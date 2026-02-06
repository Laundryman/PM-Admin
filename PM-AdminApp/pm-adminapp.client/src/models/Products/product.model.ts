import type { Country } from '../Countries/country.model'
import type { Region } from '../Countries/region.model'
import { ProductList } from './product-list.model'
import type { Shade } from './shade.model'

export class Product {
  id!: number
  name!: string
  fullDescription!: string
  shortDescription!: string
  brandId!: number
  categoryId!: number
  parentCategoryId!: number
  categoryName!: string
  parentCategoryName!: string
  productImage!: string
  dateCreated!: Date
  dateUpdated!: Date
  published!: boolean
  discontinued!: boolean
  countriesList!: string
  regionsList!: string
  regions!: Region[]
  countries: Country[] = []
  shades: Shade[] = []
  contstructor() {
    this.id = 0
  }
}

export class CountryList {
  id!: number
  name!: string

  contstructor() {
    this.id = 0
    this.name = ''
  }
}

export class ProductBasicListItem {
  _id!: number
  _name!: string
}

export class PagedProductList {
  totalItems!: number
  totalPages!: number
  currentPage!: number
  products!: ProductList[]
}
