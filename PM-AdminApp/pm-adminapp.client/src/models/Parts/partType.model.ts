export class PartType {
    id!: number;
    name!: string;
    description!: string; 
  contstructor() {
    this.id = 0;
    this.name = "";
    this.description = "";
  }
}

export class PartTypeSelectList {
  value!: number;
  viewValue!: string;
}