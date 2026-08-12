export class Part {

  //data fields
  partId!: number;
  planogramPartsId!: number;
  nonMarketPart!: boolean;
  planogramShelfId?: number;
  name!: string;
  partNumber!: string;
  partTypeId!: number;
  partType!: string;
  facings!: number;
  stock!: number;
  height!: number;
  width!: number;

  position_x?: number;
  position_y?: number;
  planogramId?: number;
  planogramPartPlanogramPartsId?: number;
  scratchPadId?: number;
  notes?: string;
  label?: string;
  svgLineGraphic!: string;

  manufacturer!: string;
  presentation!: string;
  testerType!: string;

  unitPrice!: number;
  description!: string;
  packShotImageSrc!: string;
  parentCategoryId!: number;
  categoryId!: number;
  category!: string;
  statusId?: number;
  status?: string;

  launchDate!: Date;
  launchPrice!: number;
  currentPrice!: number;
  standTypeId!: number;
  dmiReco!: boolean;
  hidePrices!: boolean;
  // additional fields for view model
  quantity: number = 1;
  errorMessage!: string;
  inProgress!: boolean;
  isAdded!: boolean;

}


