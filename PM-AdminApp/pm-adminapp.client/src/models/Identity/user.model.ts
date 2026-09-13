import { Country } from '@/models/Countries/country.model'
import { Brand } from '../Brands/brand.model'
import type { Region } from '../Countries/region.model'
import { Permission } from './permission.model'
import { Role } from './role.model'
export class User {
  password!: string
  id!: string
  countryId!: number
  country!: Country | null
  diamCountryName!: string
  diamUserId!: number
  brandIds!: number[]
  brands: Brand[] = []
  brandNameList: string[] = []
  brandNameFilterList!: string
  roleId!: number
  role: Role | null = null
  roleIds!: number[] //deprecated
  roles: Role[] = [] //deprecated
  permissions: Permission[] = []
  permissionIds!: number[] //deprecated
  shopper!: boolean
  orderManager!: boolean
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
  extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId!: string
  extension_ff5105e3fc0248fbad7979cfe9b62e1a_Permissions!: string
  extension_ff5105e3fc0248fbad7979cfe9b62e1a_Shopper!: boolean
  extension_ff5105e3fc0248fbad7979cfe9b62e1a_OrderManager!: boolean

  extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress!: string
  extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList!: string
  extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList!: string
}

export class Identity {
  signInType: string = ''
  issuer: string = ''
  issuerAssignedId: string = ''
}
