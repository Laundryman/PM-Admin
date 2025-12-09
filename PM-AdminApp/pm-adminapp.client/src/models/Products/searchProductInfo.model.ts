import { ProductList } from './product-list.model'

export class searchProductInfo {
  id!: number
  name!: string
  categoryName!: string
  categoryId!: number
  parentCategoryName!: string
  parentCategoryId!: number
  regionsList!: string
  countriesList!: string
  brandId!: number
  dateCreated!: Date
  dateUpdated!: Date
  published!: boolean
  discontinued!: boolean
  fullDescription!: string
  shortDescription!: string
  // productImage!: string
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
