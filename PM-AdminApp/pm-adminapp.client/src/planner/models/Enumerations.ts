export enum PlanogramStatusEnum {
    Editing = 1,
    Submitted = 2,
    Ordered = 3,
    Deleted = 4,
    Approved = 5,
    Validated = 6,
    Archived = 7
}
export enum StandLayoutEnum {
    'Column' = 1,
    'Pitch' = 2,
    'ColumnPitch' = 3
}

export enum StatusEnum {
    'New Module' = 1,
    'Moved Module' = 2,
    'New Graphic' = 3,
    'New Sticker' = 4,
    'Not Changed' = 0
}

export enum StatusColourEnum {
    '#FF6666' = 1,
    '#6699FF' = 2,
    '#FFFF66' = 3,
    '#66FF66' = 4,
    '#FFFFFF' = 0
}

export enum ShadeStatusColourEnum {
    '#e6282a' = 1,
    '#145da1' = 2,
    '#FFFF66 ' = 3,
    '#66FF66 ' = 4,
    '#000000' = 0
}

export enum CurrentView {
    cassette = 0,
    render = 2,
    shade = 1
}

export enum AppMode {
    'Planogram' = 1,
    'Cluster' = 2
}

export enum PartTypes {
    Cassette = 1,
    Glorifier = 2,
    Factice = 3,
    Shelf = 4,
    Accessory = 5,
    Blanking = 8,
    RedFrame = 9,
    BaseShelf = 10,
    SparePart = 11,
    FasciaPlate = 12
}

export enum ElementPosition {
    'Inside' = 1,
    'Partial' = 2,
    'Outside' = 3
}

export enum Permissions {
    Create = 1,
    Edit = 2,
    Approve = 3,
    Validate = 4,
    Archive = 5,
    Shop = 6,
    Order = 7
}
