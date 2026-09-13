import { ShelfInfo } from './Shelf';
import { PartInfo } from './PartInfo';


export class ShelfInfoList {
  planogramId: number;
  clusterId!: number;
  shelfInfos: ShelfInfo[];
  partInfos!: PartInfo[];

  constructor(planogramId: number, shelfInfos: ShelfInfo[]) {
    this.planogramId = planogramId;
    this.shelfInfos = shelfInfos;
  }
}
