export class AddEditPart {
  id!: number;
  name!: string;
  partNumber!: string;
  altPartNumber!: string;
  customerRef!: string;
  contstructor() {
    this.id = 0;
    // this.facings = 0;
    this.partNumber = "";
  }
}


