export class searchClusterInfo {
  id!: number
  name!: string
  clusterPartNumber!: string
  standId!: number
  standName!: number
  standTypeName!: string
  StandAssemblyNumber!: string
  discription!: boolean
  rows!: number
  cols!: number
  height!: number
  width!: number
  shelfIncrement!: string
  dateCreated!: Date
  dateUpdated!: Date
  published!: boolean
  statusId!: number
  brandId!: number
  LUBName!: string
  UserName!: boolean
  regionsList!: string
  countriesList!: string
  contstructor() {
    this.id = 0
  }
}
