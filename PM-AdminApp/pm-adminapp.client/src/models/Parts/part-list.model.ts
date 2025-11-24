export class ListPart {
  id!: number;
  name!: string;
  facings!: number;
  height!: number;
  width!: number;
  depth!: number;
  stock!: number;
  partNumber!: string;
  altPartNumber!: string;
  customerRefNo!: string;
  categoryId!: number;
  parentCategoryId!: number;
  categoryName!: string;
  parentCategoryName!: string;
  partTypeId!: number;
  partTypeName!: string;
  regiondId!: number;
  regionName!: string;
  shoppingHeight!: number;
  brandId!: number;
  dateCreated!: Date;
  dateUpdated!: Date;
  published!: boolean;
  contstructor() {
    this.id = 0;
    this.facings = 0;
    this.partNumber = "";
  }
}


export class PagedPartList {
  totalItems!: number;
  totalPages!: number;
  currentPage!: number;
  parts!: ListPart[];
}
