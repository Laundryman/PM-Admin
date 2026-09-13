import { Product } from './Product';

export interface PartProducts
{
  PlanogramPartsId : number;
  PartId: number;
  PlanogramShelfId: number;
  Products: Product[];
}