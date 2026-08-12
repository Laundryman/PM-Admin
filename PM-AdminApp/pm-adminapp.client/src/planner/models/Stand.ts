import { type Column } from './Column';
import { Row } from './Row';

export interface Stand {

  //data fields

       standId: number;
       standTypeId: number;
       parentStandTypeId: number;
       standTypeName: string;
       parentStandTypeName: string;
       brandId: number;
       isUsed: number;
       shelfLock: boolean;
       countryIds?: number[];
        //--------------------- Section 1 GENERAL ----------------------------------
       name: string;
       standAssemblyNumber: string;
       layoutStyle: number;
       height: number;
       width: number;


        //--------------------- Section 2 MERCHANDISING ---------------------------
       merchHeight: number;
       merchWidth: number;
       headerHeight: number;
       headerWidth: number;
       footerHeight: number;
       footerWidth: number;


        //--------------------- Section 3 COLS ------------------------------------
       cols: number;
       equalCols: boolean;
       defaultColWidth: number;
       horizontalPitchCount: number;
       horizontalPitchSize: number;

        //--------------------- Section 4 ROWS ------------------------------------
       rows : number;
       equalRows: number;
       defaultRowHeight : number;
       shelfIncrement : number;

        //--------------------- Section 5 OTHER -----------------------------------
       headerGraphic: string;
       headerGraphicLocation: string;

       standCost: number;
       dateCreated: Date;
       dateUpdated: Date;
       dateAvailable: Date;

       published: boolean;
       discontinued: boolean;
       countryId: number;

       spanShelves : boolean;
       allowOverHang: boolean;

       columnList: Column[];
       rowList: Row[];

}
