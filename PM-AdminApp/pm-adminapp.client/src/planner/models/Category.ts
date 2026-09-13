import { Part } from '../models/Part';

export class Category {
  categoryId!: number;
  name!: string;
  parentCategoryId!: number;
  parts!: Part[];
}
