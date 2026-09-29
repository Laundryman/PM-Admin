export class Column {
  id!: number
  standId!: number
  position!: number
  width!: number
  columnUprightList!: Upright[]
}

export class Upright {
  columnId!: number
  standId!: number
  id!: number
  position!: number
  width!: number
}

export class Row {
  standId!: number
  id!: number
  position!: number
  height!: number
}
