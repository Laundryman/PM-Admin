import { PartInfo } from './PartInfo';
import { ShelfInfoList } from './ShelfInfoList';

export class PlanogramInfo {
    planogramId!: number;
    planogramName!: string;
    clusterId!: number;
    countryId!: number;
    brandId!: number;
    planogramInfo!: ShelfInfoList;
    cassetteInfo!: PartInfo[];
    scratchPadInfo!: ShelfInfoList;
    deletedInfo!: ShelfInfoList;

    constructor(planogramId: number, name: string, shelfInfoList: ShelfInfoList, cassetteInfoList: PartInfo[]) {
        this.planogramId = planogramId;
        this.planogramName = name;
        this.planogramInfo = shelfInfoList;
        this.cassetteInfo = cassetteInfoList;
    }
}
