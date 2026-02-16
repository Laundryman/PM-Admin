import type { Brand } from '../Brands/brand.model'
import type { Country } from '../Countries/country.model'
import type { Region } from '../Countries/region.model'
import type { Job } from './Job.model'

export class JobFolder {
  id!: number
  name!: string
  description!: string
  regionId!: number
  countryId!: number
  brandId!: number
  brand!: Brand
  countries!: Country[]
  region!: Region
  jobs!: Job[]
}
