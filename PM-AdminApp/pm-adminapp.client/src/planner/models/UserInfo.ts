export class MenuPart {

  //data fields
  id!: number;
  partNumber!: string;
  altPartNumber!: string;
  customerRefNo!: string;
  partTypeId!: number;
  partType!: string;
  parentCategoryId!: number;
  categoryName!: string;
  name!: string;
  facings!: number;
  stock!: number;
  published!: boolean;
  discontinued!: boolean;
  svgLineGraphic?: string;
  packShotImageSrc?: string;
  render2dImage?: string;
  width!: number;
  height!: number;
  shoppingHeight!: number;
}

