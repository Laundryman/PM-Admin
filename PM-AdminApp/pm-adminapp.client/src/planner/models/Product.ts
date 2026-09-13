import type * as planmatr from './PlanMatr.Shapes';
import { Shade } from './Shade';

// import partAttributes = planmatr.shapes.PlanMatrAttributes.planxAttributes;

export class Product implements planmatr.shapes.PlanMatrAttributes.PlanMatrProductAttributes {
    //data fields
    id!: number;
    productId!: number;
    name!: string;
    shortDescription!: string;
    fullDescription!: string;
    brandId!: number;
    categoryId!: number;
    parentCategoryId!: number;
    productImage!: string;
    discontinued!: boolean;
    shades!: Shade[];
    published!: boolean;
    countriesList!: string;
}
