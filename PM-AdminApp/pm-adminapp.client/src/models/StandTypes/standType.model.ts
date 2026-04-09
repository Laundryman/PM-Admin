export class StandType {
  id!: number
  name!: string
  description!: string
  parentStandTypeId!: number
  parentStandTypeName!: string
  parentStandType!: StandType
  brandId!: number
  brandName!: string
  brandLogo!: string
  lock!: boolean
  standCount!: number
  standImage!: string
  hidePrices!: boolean
  childStandTypes!: StandType[]
}
