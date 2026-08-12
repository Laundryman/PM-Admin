import { type ColumnUpright } from '@/planner/models/ColumnUpright';

export interface Column {
    columnId: number;
    standId: number;
    position: number;
    width: number;
    columnUprightList: ColumnUpright[];
}

export class StandColumn implements Column {
    columnId: number;
    standId: number;
    position: number;
    width: number;
    columnUprightList: ColumnUpright[];

    constructor(columnId: number, standId: number, position: number, width: number) {
        this.columnId = columnId;
        this.standId = standId;
        this.position = position;
        this.width = width;
        this.columnUprightList = [];
    }
}
