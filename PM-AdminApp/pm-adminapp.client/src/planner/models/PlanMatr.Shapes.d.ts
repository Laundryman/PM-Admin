import { type attributes } from '@joint/plus';
// import { type PlanMatrAttributes } from './PlanMatr.Shapes';

namespace shapes {
    interface StandSelectors extends dia.Cell.Selectors {
        '.label'?: attributes.SVGTextAttributes;
        '.body'?: attributes.SVGRectAttributes;
        '.header'?: attributes.SVGRectAttributes;
        '.header-graphic'?: attributes.SVGRectAttributes;
        '.footer'?: attributes.SVGRectAttributes;
        shapeType?: PlanMatrAttributes.PlanMatrStandAttributes;
        disableMove?: PlanMatrAttributes.PlanMatrStandAttributes;
    }

    interface StandAttributes extends dia.Element.GenericAttributes<StandSelectors> {
        Columns?: string[];
        Rows?: string[];
    }

    type ColRowAttributes = dia.Element.GenericAttributes<StandSelectors>;

    interface ColumnUprights {
        columnUprightId?: number;
        columnId?: number;
        standId?: number;
        position?: number;
        width?: number;
        height?: number;
    }
    class Carcass extends basic.Generic {
        constructor(attributes?: StandAttributes, opt?: { [key: string]: object });

        addColumn(column: string, opt?: object): this;
        addRow(row: string, opt?: object): this;

        // removeOutPort(port: string, opt?: object): this;

        // removeInPort(port: string, opt?: object): this;
    }

    class Column extends basic.Generic {
        constructor(attributes?: StandSelectors, opt?: { [key: string]: object });
        'uprights'?: ColumnUprights[];
    }

    class Row extends basic.Generic {
        constructor(attributes?: StandSelectors, opt?: { [key: string]: object });
    }

    interface UprightSelectors extends dia.Cell.Selectors {
        col?: PlanMatrAttributes.PlanMatrStandAttributes;
        '.upright'?: attributes.SVGTextAttributes;
        path?: attributes.SVGImageAttributes;
    }

    class Upright extends basic.Generic {
        constructor(attributes?: StandSelectors, opt?: { [key: string]: object });
        // constructor(
        //     attributes?: UprightSelectors,
        //     opt?: { [key: string]: any }
        // );
    }

    namespace Part {
        interface PartSelectors extends dia.Cell.Selectors {
            planogramInfo?: PlanMatrAttributes.PlanMatrPlanogramAttributes;
            partInfo?: PlanMatrAttributes.PlanMatrPartAttributes;
            shapeType?: PlanMatrAttributes.PlanMatrStandAttributes;
            '.name'?: attributes.SVGTextAttributes;
            image?: attributes.SVGImageAttributes;
            disableMove?: PlanMatrAttributes.PlanMatrStandAttributes;
        }

        interface ShelfSelectors extends dia.Cell.Selectors {
            planogramInfo?: PlanMatrAttributes.PlanMatrPlanogramAttributes;
            shelfInfo?: PlanMatrAttributes.PlanMatrShelfAttributes;
            shapeType?: PlanMatrAttributes.PlanMatrStandAttributes;
            '.name'?: attributes.SVGTextAttributes;
            image?: attributes.SVGImageAttributes;
            disableMove?: PlanMatrAttributes.PlanMatrStandAttributes;
        }

        class Cassette extends dia.Element {
            constructor(attributes?: dia.Element.GenericAttributes<PartSelectors>, disabled?: boolean, opt?: { [key: string]: object });
        }
        class MenuCassette extends dia.Element {
            constructor(attributes?: dia.Element.GenericAttributes<PartSelectors>, opt?: { [key: string]: object });
        }
        class Shelf extends dia.Element {
            constructor(attributes?: dia.Element.GenericAttributes<ShelfSelectors>, opt?: { [key: string]: object });
        }
        class MenuShelf extends dia.Element {
            constructor(attributes?: dia.Element.GenericAttributes<ShelfSelectors>, opt?: { [key: string]: object });
        }
    }

    interface partType {
        type: string;
        id: number;
    }

    namespace PlanMatrAttributes {
        interface PlanMatrStandAttributes extends attributes.SVGAttributes {
            shapeType?: string;
            disableMove?: boolean;
            col?: number;
            merchHeight?: number;
            merchWidth?: number;
            headerHeight?: number;
            headerWidth?: number;
            headerGraphic?: string;
            footerHeight?: number;
            footerWidth?: number;
        }

        interface PlanMatrPartAttributes extends attributes.SVGAttributes {
            id?: string;
            planogramId?: number;
            scratchPadId?: number;
            partId?: number;
            planogramPartId?: number;
            parentPlanogramPartId?: number;
            planogramShelfId?: number;
            partTypeId?: number;
            partNumber?: string;
            facings?: number;
            stock?: number;
            height?: number;
            width?: number;
            notes?: string;
            label?: string;
            statusId?: number;
            status?: string;
            column: number;
            svgLineGraphic?: string;
            packShotImageSrc?: string;
            render2dImage?: string;
            products: PlanMatrProductAttributes[];
            facingProducts: PlanMatrFacingAttributes[];
            position: dia.Point;
        }

        interface PlanMatrShelfAttributes extends attributes.SVGAttributes {
            id?: string;
            planogramId?: number;
            partId?: number;
            planogramShelfId?: number;
            clusterShelfId?: number;
            shelfTypeId?: number;
            partTypeId?: number;
            partNumber?: string;
            height?: number;
            width?: number;
            label?: string;
            status: number;
            column: number;
            svgLineGraphic?: string;
            packShotImageSrc?: string;
            render2dImage?: string;
            position: dia.Point;
        }

        interface PlanMatrPlanogramAttributes {
            x?: number;
            y?: number;
            planogramId?: number;
            scratchPadId?: number;
        }

        interface PlanMatrPositionAttributes {
            x?: number;
            y?: number;
        }

        interface PlanMatrProductAttributes {
            id: number;
            name: string;
            shortDescription: string;
            fullDescription: string;
            brandId: number;
            categoryId: number;
            parentCategoryId: number;
            productImage: string;
            discontinued: boolean;
            shades: PlanMatrShadeAttributes[];
        }

        interface PlanMatrFacingAttributes {
            facingNo: number;
            productId: number;
            shadeId: number;
            productImage: string;
            facingStatus: number;
        }

        interface PlanMatrShadeAttributes {
            id: number;
            shadeNumber: string;
            shadeDescription: string;
            fullDescription: string;
        }
    }
}
