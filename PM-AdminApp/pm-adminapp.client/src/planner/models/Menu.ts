import { Category } from '../models/Category'
import type { MenuPart } from './MenuPart'

export class Menu {
  categories!: Category[]
  parts!: MenuPart[]
}
