export class searchPlanogramInfo {
  id!: number
  planogramId!: number
  clusterId!: number
  brandId!: number
  scratchPadId!: number

  name!: string
  orderRef!: string
  description!: string
  statusId!: number
  planogramPreviewSrc!: string
  standId!: number
  standTypeName!: string
  standName!: string
  standHeight!: number
  standWidth!: number
  userId!: number
  legacyUserId!: number
  dateCreated!: Date
  dateUpdated!: Date
  formattedDateUpdated!: string
  dateSubmitted!: Date
  currentVersion!: number
  template!: string
  lastUpdatedBy!: number
  legacyLastUpdatedBy!: number
  countryId!: number
  countryName!: string
  regionList!: string
  shelfCount!: number
  accessoryCount!: number
  userName!: string
  lubName!: string
  jobNumber!: string
  jobId!: number
  hasComments!: boolean
  commentCount!: number
  locked!: boolean
  lockingUserName!: string
  lockingUserId!: number
  dateLocked!: Date

  // archived!: boolean
  // legacyArchivedBy!: number
  // archivedDate!: Date
  // legacyArchivedId!: number
  // archivedByName!: string
  // archivedBy!: number
}
