import type { Country } from '../Countries/country.model'
import type { Region } from '../Countries/region.model'
import type { StandType } from '../StandTypes/standType.model'
import { Row, type Column } from './columns.model'

export class Stand {
  id!: number
  brandId!: number
  name!: string
  description!: string
  dateCreated!: Date
  dateUpdated!: Date
  dateAvailable!: Date
  published!: boolean
  height!: number
  width!: number
  standTypeId!: number
  standTypeName!: string
  parentStandTypeId!: number
  parentStandTypeName!: string
  merchHeight!: number
  merchWidth!: number
  headerHeight!: number
  headerWidth!: number
  headerGraphic!: string
  footerHeight!: number
  footerWidth!: number
  horizontalPitchCount!: number
  horizontalPitchSize!: number
  cols!: number
  columnList!: Column[] | null
  rowList!: Row[] | null
  equalCols!: boolean
  defaultColWidth!: number
  standAssemblyNumber!: string
  layoutStyle!: number
  spanShelves!: boolean
  rows!: number
  equalRows!: boolean
  defaultRowHeight!: number
  shelfIncrement!: number
  standCost!: number
  discontinued!: boolean
  allowOverHang!: boolean
  countriesList!: string
  regionsList!: string
  standType!: StandType
  regions!: Region[]
  countries: Country[] = []
  totalColWidth!: number
  totalRowHeight!: number
}
