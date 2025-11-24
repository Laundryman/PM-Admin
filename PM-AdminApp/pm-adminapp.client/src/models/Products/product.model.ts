import { ProductList } from './product-list.model'

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
  countryList!: string
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
