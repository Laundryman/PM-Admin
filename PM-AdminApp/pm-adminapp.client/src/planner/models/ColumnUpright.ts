export interface ColumnUpright {
  columnUprightId: number;
  columnId: number;
  standId: number;
  position: number;
  width: number;
  height: number;
}


export class StandColUpright implements ColumnUpright {
  columnUprightId: number;
  columnId: number;
  standId: number;
  position: number;
  width: number;
  height: number;

  constructor(colUpId: number, colId: number, standId: number, position: number, width: number, height: number) {
    this.columnUprightId = colUpId;
    this.columnId = colId;
    this.standId = standId;
    this.position = position;
    this.width = width;
    this.height = height;
    
  }
}