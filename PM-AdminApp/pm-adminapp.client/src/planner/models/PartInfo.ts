import * as joint from '@joint/plus';

import type * as planmatr from './PlanMatr.Shapes';

import { PartFacing } from './PartFacing';
import { Product } from './Product';

export class PartInfo implements planmatr.shapes.PlanMatrAttributes.PlanMatrPartAttributes {
    //constructor
    constructor(
        id: string,
        planogramId: number,
        clusterId: number,
        scratchPadId: number,
        partId: number,
        planogramPartId: number,
        parentPlanogramPartId: number,
        planogramShelfId: number,
        planxPartId: string,
        parentPlanxPartId: string,
        planxShelfId: string,
        partTypeId: number,
        partNumber: string,
        facings: number,
        stock: number,
        height: number,
        width: number,
        column: number,
        notes: string,
        position: joint.dia.Point,
        statusId?: number,
        status?: string,
        svgLineGraphic?: string,
        packShotImageSrc?: string,
        render2dImage?: string,
        products?: Product[],
        facingProducts?: PartFacing[],
        countryIds?: number[]
    ) {
        this.id = id;
        this.planogramId = planogramId;
        this.clusterId = clusterId;
        this.scratchPadId = scratchPadId;
        this.partId = partId;
        this.planogramPartId = planogramPartId;
        this.parentPlanogramPartId = parentPlanogramPartId;
        this.planogramShelfId = planogramShelfId;
        this.planxPartId = planxPartId;
        this.parentPlanxPartId = parentPlanxPartId;
        this.planxShelfId = planxShelfId;
        this.partTypeId = partTypeId;
        this.partNumber = partNumber;
        this.facings = facings;
        this.stock = stock;
        this.height = height;
        this.width = width;
        this.column = column;
        this.notes = notes;
        this.status = status;
        this.statusId = statusId;
        this.svgLineGraphic = svgLineGraphic;
        this.packShotImageSrc = packShotImageSrc;
        this.render2dImage = render2dImage;
        this.products = products as Product[];
        this.facingProducts = facingProducts as PartFacing[];
        this.position = position;
        this.countryIds = countryIds as number[];
    }

    id: string;
    planogramId?: number;
    clusterId?: number;
    scratchPadId?: number;
    partId: number;
    planogramPartId: number;
    parentPlanogramPartId: number;
    planogramShelfId?: number;
    clusterPartId!: number;
    clusterShelfId?: number;
    planxPartId: string;
    parentPlanxPartId: string;
    planxShelfTypeId?: number;
    planxShelfId: string;
    partTypeId: number;
    partNumber: string;
    facings: number;
    stock: number;
    height: number;
    width: number;
    column: number;
    notes?: string;
    statusId?: number;
    status?: string;
    svgLineGraphic?: string;
    products!: Product[];
    facingProducts!: PartFacing[];
    countryIds!: number[];
    position!: joint.dia.Point;

    name!: string;
    partType!: string;
    planogramPartPlanogramPartsId?: number;
    shelfLabel?: string;
    label?: string;
    manufacturer?: string;
    presentation!: string;
    testerType!: string;

    unitPrice!: number;
    description!: string;
    cassetteBio?: string;
    packShotImageSrc?: string;
    render2dImage?: string;
    parentCategoryId!: number;
    categoryId!: number;
    category!: string;

    launchDate?: Date;
    launchPrice?: number;
    currentPrice!: number;
    standTypeId!: number;
    dmiReco!: boolean;
    hidePrices!: boolean;
    nonMarket!: boolean;
}
