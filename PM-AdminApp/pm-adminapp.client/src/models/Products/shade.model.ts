import type { Country } from '../Countries/country.model'
import type { Region } from '../Countries/region.model'

export class Shade {
  id!: number
  shadeNumber!: string
  shadeDescription!: string
  productId!: number
  published!: boolean
  CMYK!: string
  RGB!: string
  pantone!: string
  dateCreated!: Date
  dateUpdated!: Date
  dateAvailable!: Date
  countriesList!: string
  countries?: Country[]
  regionsList!: string
  regions?: Region[]

  constructor() {
    this.id = 0
  }
}
