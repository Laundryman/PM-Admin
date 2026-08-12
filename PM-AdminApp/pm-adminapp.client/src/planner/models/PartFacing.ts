// import * as planx from '@joint/plus';
import type * as planmatr from './PlanMatr.Shapes';


export class PartFacing implements planmatr.shapes.PlanMatrAttributes.PlanMatrFacingAttributes {
  partFacingId!: number;
  facingNo!: number;
  productId!: number;
  partId!: number;
  facingType!: number;
  shadeId!: number;
  productImage!: string;
  facingStatus!: number;
  stockCount!: number;
  shadeNumber!: number;
  shadeColour!: string;
  published!: boolean;

  constructor(facingNo: number) {
    this.facingNo = facingNo;
  }
}
