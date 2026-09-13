// import * as planx from '@joint/plus';
import type * as planmatr from './PlanMatr.Shapes';

export class Shade implements planmatr.shapes.PlanMatrAttributes.PlanMatrShadeAttributes {
    id!: number;
    shadeNumber!: string;
    shadeDescription!: string;
    fullDescription!: string;
    productId!: number;
    published!: boolean;
}
