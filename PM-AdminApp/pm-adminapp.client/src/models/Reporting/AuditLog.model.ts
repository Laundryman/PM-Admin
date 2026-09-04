export class AuditLog {
  id: number
  date: Date
  message: string

  legacyUserId?: number

  brandId?: number
  brandName?: string

  userName?: string

  action?: number
  actionName?: string

  actionType?: number

  planoId?: number

  planoName?: string

  orderId?: number
  orderName?: string

  userId?: string
  roles?: string

  roleId?: number

  roleName?: string
  regionId?: number
  regionName?: string

  countryId?: number
  countryName?: string

  permissions?: string
}
