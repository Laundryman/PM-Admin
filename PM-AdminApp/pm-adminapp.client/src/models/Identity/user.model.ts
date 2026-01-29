import { Country } from '@/models/Countries/country.model'
import { Brand } from '../Brands/brand.model'
import { Role } from './role.model'
export class User {
  password!: String
  id!: String
  diamCountryId!: Number
  country!: Country | null
  diamCountryName!: String
  diamUserId!: Number
  brandIds!: Number[]
  brands: Brand[] = []
  brandNameList: String[] = []
  brandNameFilterList!: String
  roleIds!: Number[]
  roles: Role[] = []
  roleNameList: String[] = []
  roleNameFilterList!: String
  givenName!: String
  surname!: String
  email!: String
  userName!: String
  newUserName!: String
  displayName!: String
  mailNickName!: String
  identities!: Identity[]
  userEmailAddress!: string
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
