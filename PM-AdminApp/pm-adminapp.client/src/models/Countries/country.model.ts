export class Country {
    id!: number;
    name!: string;
    regionId !: number;
    threeLetterIsoCode !: string;
    twoLetterIsoCode !: string;
    active !: boolean;
    flagFileName !: string;    
  contstructor() {
    this.id = 0;
    this.name = "";
    this.regionId = 0;
    this.threeLetterIsoCode = "";
    this.twoLetterIsoCode = "";
    this.active = false;
    this.flagFileName = "";
  }
}

export class CountryList {
  id!: number;
  name!: string;
  regionId !: number;
  active !: boolean;
  flagFileName !: string;    
contstructor() {
  this.id = 0;
  this.name = "";
  this.regionId = 0;
  this.active = false;
  this.flagFileName = "";
}

}

export class CountrySelectListItem {
  value!: number;
  viewValue!: string;
}

export class CountryBasicListItem {
  _id!: number;
  _name!: string;
}

export class PagedCountryList {
  totalItems!: number;
  totalPages!: number;
  currentPage!: number;
  countries!: CountryList[];
}