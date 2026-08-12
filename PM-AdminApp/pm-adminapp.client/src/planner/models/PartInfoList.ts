import { PartInfo } from './PartInfo';


export class PartInfoList {
  planogramId: number;
  partInfos: PartInfo[];

  constructor(planogramId: number, partInfos: PartInfo[]) {
    this.planogramId = planogramId;
    this.partInfos = partInfos;
  }
}