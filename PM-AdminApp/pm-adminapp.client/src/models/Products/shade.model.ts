export class Shade {
    id!: number;
    shadeNumber!: string;
    shadeDescription!: string;
    productId!: number;
    published!: boolean;
    CMYK!: string;
    RGB!: string;
    pantone!: string;
    dateCreated!: Date;
    dateUpdated!: Date;
    dateAvailable!: Date;
    countryList!: string;

    constructor() {
        this.id = 0;
    }
}