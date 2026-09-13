import * as planx from '@joint/plus'
import type * as planmatr from './PlanMatr.Shapes'

import Dia = planx.dia

import { PartInfo } from './PartInfo'

export class ShelfInfo implements planmatr.shapes.PlanMatrAttributes.PlanMatrShelfAttributes {
  //constructor
  constructor(
    id: string,
    planogramId: number,
    partId: number,
    planogramShelfId: number,
    planmatrShelfId: string,
    scratchPadId: number,
    partTypeId: number,
    partNumber: string,
    height: number,
    width: number,
    label: string,
    column: number,
    notes: string,
    status: number,
    svgLineGraphic: string,
    position: Dia.Point,
  ) {
    this.id = id
    this.planogramId = planogramId
    this.partId = partId
    this.planogramShelfId = planogramShelfId
    this.scratchPadId = scratchPadId
    this.planmatrShelfId = planmatrShelfId
    this.partTypeId = partTypeId
    this.partNumber = partNumber
    this.height = height
    this.width = width
    this.label = label
    this.column = column
    this.notes = notes
    this.status = status
    this.svgLineGraphic = svgLineGraphic
    this.position = position
  }

  id!: string
  planogramId!: number
  partId!: number
  planogramShelfId!: number
  scratchPadId?: number
  clusterShelfId!: number
  planmatrShelfId!: string
  partTypeId!: number
  partNumber!: string
  height!: number
  width!: number
  label!: string
  column!: number
  notes!: string
  status!: number
  svgLineGraphic!: string
  parts!: PartInfo[]
  position!: Dia.Point
}
