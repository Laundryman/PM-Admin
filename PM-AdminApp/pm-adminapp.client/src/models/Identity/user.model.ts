import { Country } from '@/models/Countries/country.model'
import { Brand } from '../Brands/brand.model'
import type { Region } from '../Countries/region.model'
import { Role } from './role.model'
export class User {
  password!: string
  id!: string
  diamCountryId!: number
  country!: Country | null
  diamCountryName!: string
  diamUserId!: number
  brandIds!: number[]
  brands: Brand[] = []
  brandNameList: string[] = []
  brandNameFilterList!: string
  roleIds!: number[]
  roles: Role[] = []
  roleNameList: string[] = []
  roleNameFilterList!: string
  givenName!: string
  surname!: string
  mail!: string
  userName!: string
  newUserName!: string
  displayName!: string
  mailNickName!: string
  identities!: Identity[]
  userEmailAddress!: string
  regions!: Region[]
  countries: Country[] = []
  countryList!: string
  regionList!: string

  extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId!: number
  extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands!: string
  extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles!: string
  extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress!: string
}

export class Identity {
  signInType: string = ''
  issuer: string = ''
  issuerAssignedId: string = ''
}
