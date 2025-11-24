export class ProductList {
  id!: number;
  name!: string;
  fullDescription!: string;
  shortDescription!: string;
  brandId!: number;
  categoryId!: number;
  parentCategoryId!: number;
  categoryName!: string;
  parentCategoryName!: string;
  productImage!: string;
  dateCreated!: Date;
  dateUpdated!: Date;
  published!: boolean;
  discontinued!: boolean;
  contstructor() {
    this.id = 0;
    this.name = '';
  }
}


export class PagedProductList {
  totalItems!: number;
  totalPages!: number;
  currentPage!: number;
  products!: ProductList[];
}
