export class Brand {
  id!: number;
  name!: string;
  brandLogo!: string;
  shelfLock!: boolean;
  disabled!: boolean;
  themeId!: number;
  brandShop!: boolean;
  appVersion!: number;
  contstructor() {
    this.id = 0;
    this.name = "";
  }
}
