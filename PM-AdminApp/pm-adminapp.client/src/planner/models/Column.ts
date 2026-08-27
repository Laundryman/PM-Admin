import { type ColumnUpright } from '@/planner/models/ColumnUpright'

export interface Column {
  id: number
  standId: number
  position: number
  width: number
  columnUprightList: ColumnUpright[]
}

export class StandColumn implements Column {
  id: number
  standId: number
  position: number
  width: number
  columnUprightList: ColumnUpright[]

  constructor(id: number, standId: number, position: number, width: number) {
    this.id = id
    this.standId = standId
    this.position = position
    this.width = width
    this.columnUprightList = []
  }
}
