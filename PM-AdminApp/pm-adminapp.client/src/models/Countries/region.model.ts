import type { Country } from './country.model'

export class Region {
  id!: number
  name!: string
  brandId!: number
  countryList!: string
  countries!: Country[]
  contstructor() {
    this.id = 0
    this.name = ''
    this.brandId = 0
  }
}

export class RegionSelectList {
  value!: number
  viewValue!: string
}

export class PagedRegionList {
  totalItems!: number
  totalPages!: number
  currentPage!: number
  regions!: Region[]
}
