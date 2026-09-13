import { Planogram } from '@/planner/models/Planogram';
import { type Stand } from '@/planner/models/Stand';
import { PlanogramService } from '@/planner/services/planogram-service';
import * as joint from '@joint/plus';
import { ref } from '@vue/runtime-dom';
import axios from 'axios';
import { ElementPosition, PartTypes, StandLayoutEnum } from '../models/Enumerations';

import g = joint.g;
// import { partTypes } from '../config/partTypes';

const token = ref();
const idToken = ref();
const initialized = ref(false);
const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_ROOT + '/api/planograms',
    withCredentials: false,
    headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token.value}`,
        ClaimsAuth: idToken.value || ''
    }
});

export class ValidationService {
    planogramService: PlanogramService;
    graph: joint.dia.Graph;
    paper: joint.dia.Paper;
    cassette: joint.dia.Cell;
    isCluster: boolean;

    constructor(planogramService: PlanogramService, graph: joint.dia.Graph, paper: joint.dia.Paper, cassette: joint.dia.Cell, isCluster: boolean) {
        this.planogramService = planogramService;
        this.graph = graph;
        this.paper = paper;
        this.cassette = cassette;
        this.isCluster = isCluster;
    }
    validateShelfPosition(cell: joint.dia.Element, stand: Stand, scratchPadId: number): boolean {
        //var cellView = this.paper.findViewByModel(cell);
        const bbox = cell.getBBox();
        //var bbox = cellView.getBBox(); --- cellview gives us the size at the zoom level - wrong for comparison
        //some strange decimal position value is breaking the api
        if (cell.attributes.position && !Number.isInteger(cell.attributes.position.x)) {
            cell.attributes.position.x = Math.round(cell.attributes.position.x);
        }
        if (cell.attributes.position && !Number.isInteger(cell.attributes.position.y)) {
            cell.attributes.position.y = Math.round(cell.attributes.position.y);
        }

        if (cell.attributes.position) {
            bbox.x = cell.attributes.position.x;
            bbox.y = cell.attributes.position.y;
        }

        const isOnCarcass = this.planogramService.isOnCarcass(cell, bbox);
        if (isOnCarcass == ElementPosition.Inside || isOnCarcass == ElementPosition.Partial) {
            if (isOnCarcass == ElementPosition.Partial && cell.position().y < 0) {
                //shelf is across the edge of the carcass - this is not allowed
                return false;
            }

            //Now check that shelf is not across columns / wider than column
            //determine if should snap to column if it fits

            const columnsUnderneath = this.planogramService.getColumnsUnderneath(bbox, this.graph);
            if (columnsUnderneath.length > 0) {
                //check each column for width
                //var permittedColumns = _.filter(columnsUnderneath, ['attributes.size.width', cell.attributes.size.width]);
                let permittedColumns = columnsUnderneath;
                if (stand.layoutStyle != StandLayoutEnum.Pitch && cell.attributes.shelfInfo.shelfTypeId != PartTypes.BaseShelf) {
                    permittedColumns = columnsUnderneath.filter(function (c: joint.dia.Element) {
                        return c.attributes.size && c.attributes.size.width >= cell.attributes.width;
                    });
                }

                if (permittedColumns.length > 0 || cell.attributes.shelfInfo.shelfTypeId == PartTypes.BaseShelf) {
                    //check that the new permitted column position doesn't already have something there.
                    let shelfPosX = bbox.x;
                    if (cell.attributes.shelfInfo.shelfTypeId != PartTypes.BaseShelf) {
                        shelfPosX = Math.floor(this.planogramService.snapShelfToColumn(cell, permittedColumns[0] as joint.dia.Element, stand));
                    }

                    bbox.y = bbox.y; // + cell.attributes.height; //(y is the top left of the object, so we need to add the height of the object first) - we don't for this now.
                    bbox.x = shelfPosX; // permittedColumns[0].attributes.position.x;

                    let elementsUnderneath = this.graph.findElementsInArea(bbox).filter(function (el) {
                        return el.attributes.type != 'planmatr.Carcass' && el.attributes.type != 'planmatr.Column' && el.attributes.type != 'planmatr.Upright' && el.attributes.type != 'planmatr.Row' && el.attributes.id !== cell.attributes.id;
                    });

                    elementsUnderneath = this.planogramService.remmoveAssociatedElements(elementsUnderneath, cell);
                    if (elementsUnderneath.length == 0 || (elementsUnderneath.length != 0 && cell.attributes.shelfInfo.shelfTypeId == PartTypes.BaseShelf)) {
                        const canPlace = this.elementsUnderneath(bbox, cell, stand, isOnCarcass, permittedColumns);
                        return canPlace;
                    } else if (elementsUnderneath.length != 0) {
                        //check that the elements underneath belong to the shelf
                        //if they don't then fail the placement, if they do, then its fine.
                        const result = elementsUnderneath.find(function (el) {
                            if (el.attributes.type == 'planmatr.Part.Shelf') {
                                if (cell.attributes.type == 'planmatr.Part.Shelf') {
                                    //if both are shelves then return a shelf as you can't place a shelf on a shelf
                                    return el;
                                }
                                return el.attributes.shelfInfo.planogramShelfId != cell.attributes.shelfInfo.planogramShelfId;
                            } else {
                                return el.attributes.partInfo.planogramShelfId != cell.attributes.shelfInfo.planogramShelfId;
                            }
                        });
                        if (result) {
                            return false;
                        } else {
                            return true;
                        }
                    } else {
                        return false;
                    }
                } else {
                    return false;
                }
            } else {
                //no column to place it on can go anywhere.
                //as long as it's in the merchandising area and not in the header or footer
                this.planogramService.setModelsUnderneath(cell);
                const canPlace = this.elementsUnderneath(bbox, cell, stand, isOnCarcass, []);
                const isMerch = this.planogramService.isInMerchSpace(cell, bbox);
                if (isMerch && canPlace) {
                    return true;
                } else {
                    return false;
                }
            }
        } //end of isOnCarcass
        else {
            if (!this.isCluster) {
                //shelf
                //added to scratchpad
                cell.attributes.planogramInfo.scratchPadId = scratchPadId;
                cell.attributes.shelfInfo.scratchPadId = scratchPadId;
                //cell.attributes.shelfInfo.planogramShelfId = null;
                return true;
            } else {
                return false; //no scratchpad on clusters
            }
        }
    }

    elementsUnderneath(bbox: g.Rect, cell: joint.dia.Element, stand: Stand, isOnCarcass: ElementPosition, permittedColumns: joint.dia.Element<joint.dia.Element.Attributes, joint.dia.ModelSetOptions>[]): boolean {
        let nearestPitch = bbox.y;
        //snap to shelf increment or row
        if (typeof stand.shelfIncrement !== 'undefined') {
            let ypos = cell.attributes.position?.y;
            if (cell.attributes.shelfInfo.shelfTypeId == PartTypes.BaseShelf) {
                //needs to snap to the merch base
                ypos = stand.height - stand.footerHeight - cell.attributes.height; //getnearestpitch expects the top of the object (the y position)
                bbox.y = ypos;
                nearestPitch = ypos;
            } else {
                nearestPitch = this.planogramService.getNearestPitch(ypos ?? 0, stand.shelfIncrement, stand, cell.attributes.height, cell.attributes.shelfInfo.shelfTypeId);

                bbox.y = nearestPitch;
            }
        }

        if (typeof stand.rows !== 'undefined') {
            if (stand.rows > 0 && cell.attributes.position != null) {
                nearestPitch = this.planogramService.getNearestRow(stand.headerHeight, cell.attributes.position.y, stand.rowList) - cell.attributes.height;
                bbox.y = nearestPitch;
            }
        }

        //here we need to test again that the final position of the shelf is not going to overlap any thing
        let elementsUnderneath = this.graph.findElementsInArea(bbox).filter(function (el) {
            return (
                el.attributes.type != 'planmatr.Carcass' &&
                el.attributes.type != 'planmatr.Column' &&
                el.attributes.type != 'planmatr.Upright' &&
                el.attributes.type != 'planmatr.Row' &&
                el.attributes.id !== cell.attributes.id &&
                el.attributes.shapeType != 'Fascia Plate'
            );
        });

        elementsUnderneath = this.planogramService.remmoveAssociatedElements(elementsUnderneath, cell);
        if (elementsUnderneath.length == 0 && isOnCarcass == ElementPosition.Inside && permittedColumns.length > 0) {
            if (cell.attributes.position) {
                cell.attributes.position.y = nearestPitch;
            }
            //here we need to test the snap positions see createXShelfLocations in standdetailsobject.as
            if (cell.attributes.shelfInfo.shelfTypeId != PartTypes.BaseShelf) {
                const shelfPosX: number = Math.floor(this.planogramService.snapShelfToColumn(cell, permittedColumns[0] as joint.dia.Element, stand));
                if (cell.attributes.position) {
                    cell.position(shelfPosX, cell.attributes.position.y, { deep: true }); // maybe we don't need this as we aren't actually moving the shelf just validating it.
                }
            }
            cell.attributes.planogramInfo.scratchPadId = 0;
            cell.attributes.shelfInfo.scratchPadId = 0;
            cell.findView(this.paper).render();
            return true;
        } else {
            return false;
        }
    }

    validateCassettePosition(cell: joint.dia.Element, command: joint.dia.CommandManager.Command, stand: Stand, planogram: Planogram): boolean {
        //Object.assign(currElement, cell);
        const cellView = this.paper.findViewByModel(cell);

        //some strange decimal position value is breaking the api
        if (cell.attributes.position && !Number.isInteger(cell.attributes.position.x)) {
            cell.attributes.position.x = Math.round(cell.attributes.position.x);
        }
        if (cell.attributes.position && !Number.isInteger(cell.attributes.position.y)) {
            cell.attributes.position.y = Math.round(cell.attributes.position.y);
        }
        let bbox = cell.getBBox();
        if (cell.changed.position != null && cell.changed.position != undefined) {
            bbox.x = cell.changed.position.x;
            bbox.y = cell.changed.position.y;
        } else {
            if (cell.attributes.position) {
                bbox.x = cell.attributes.position.x;
                bbox.y = cell.attributes.position.y;
            }
        }

        //implement overlap of cassettes here if needed
        if (this.planogramService.partOverlap) {
            bbox.x = bbox.x + this.planogramService.partOverlapAmount;
            bbox.width = bbox.width - this.planogramService.partOverlapAmount * 2;
        }

        this.planogramService.setModelsUnderneath(cell);
        const isOnCarcass = this.planogramService.isOnCarcass(cell, bbox);
        if (isOnCarcass == ElementPosition.Outside) {
            if (!this.isCluster) {
                ////unembed the cell from the shelf or stand before adding to the scratch pad.
                if (this.planogramService.partOverlap) {
                    //adjust overlap to be bigger if not on carcass
                    bbox.x = bbox.x + 15;
                    bbox.width = bbox.width - 20; //aplying a straight 15mm overlap on the scratchpad to accomodate adjacent items in a multiselect
                }
                //now check whether anything already exists in space
                //check there is nothing already there on the shelf
                const itemsInPlace = this.graph.findElementsInArea(bbox).filter(function (el) {
                    if (el.attributes.type == 'planmatr.Part.Cassette') {
                        return el.attributes.type != 'planmatr.Carcass' && el.id != cell.id && el.attributes.partInfo.partTypeId != PartTypes.Blanking && el.attributes.partInfo.partTypeId != PartTypes.FasciaPlate;
                    } else {
                        return null;
                    }
                });

                if (itemsInPlace.length == 0) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false; //no scratchpad on clusters
            }
        } else if (isOnCarcass == ElementPosition.Inside) {
            //check that the cassette is in the merchspace
            if (!this.planogramService.isInMerchSpace(cell, bbox)) {
                return false;
            }
            const nearestShelf = this.planogramService.getNearestShelf(this.graph, stand, cellView, cell);
            //check there is nothing already there on the shelf
            bbox = cell.getBBox();
            // bbox.x = cell.attributes.position.x; // don't need this if we are using cells not elements (cells are transformed by zoom)
            let partAllowed = false;
            if (nearestShelf != null) {
                bbox.y = nearestShelf.attributes.position?.y + cell.attributes.height;
                bbox.height = cell.attributes.height;
                partAllowed = this.planogramService.partAllowedToPlace(this.graph, bbox, nearestShelf, cell, stand.allowOverHang);
            } else {
                if (cell.attributes.partInfo.partTypeId == PartTypes.Blanking) {
                    partAllowed = true;
                } else {
                    return false;
                }
            }

            const t = cell; //this.graph.getCell(command.data.id).toJSON();

            if (nearestShelf?.attributes.position != null && t.attributes.size != null) {
                const area = new g.Rect(t.attributes.position?.x, nearestShelf.attributes.position.y - t.attributes.size.height, t.attributes.size?.width, t.attributes.size?.height);

                //handle overlap
                if (this.planogramService.partOverlap) {
                    area.x = area.x + this.planogramService.partOverlapAmount;
                    area.width = area.width - this.planogramService.partOverlapAmount * 2;
                }

                const result = this.graph.getElements().find(function (e: joint.dia.Element) {
                    const position = e.get('position') as g.Point;
                    const size = e.get('size') as joint.dia.Size;

                    const doesIntersect = area.intersect(new g.Rect(position.x, position.y, size.width, size.height));
                    if (
                        (e.id !== t.id && !partAllowed && e.attributes.type === 'planmatr.Part.Cassette') ||
                        (e.id !== t.id && e.attributes.type === 'planmatr.Part.Cassette' && doesIntersect && e.attributes.partInfo.partTypeId != PartTypes.Blanking && e.attributes.partInfo.partTypeId != PartTypes.FasciaPlate)
                    ) {
                        return true;
                    } else return false;
                });
                if (result || !partAllowed) {
                    return false;
                } else
                    //remove from scratchpad if necessary
                    //if (parent != null) { parent.unembed(cell); };
                    this.planogramService.snapToShelf(this.paper, this.graph, stand, cell, planogram.scratchPadId);
                return true;
            }
        } else {
            return false;
        }
        return false;
    }

    validateAccessoryPosition(cell: joint.dia.Element, command: joint.dia.CommandManager.Command, stand: Stand, planogram: Planogram): boolean {
        const cellView = this.paper.findViewByModel(cell);

        //check there is nothing already there on the shelf
        const bbox = cell.getBBox();
        bbox.height = cell.attributes.height;
        bbox.width = cell.attributes.width;

        if (command.data.next.position != null) {
            bbox.x = command.data.next.position.x;
            bbox.y = command.data.next.position.y;
        } else {
            if (cell.attributes.position) {
                bbox.x = cell.attributes.position.x;
                bbox.y = cell.attributes.position.y;
            }
        }

        const partAllowed = this.planogramService.accessoryAllowedToPlace(this.graph, bbox, cell);

        if (partAllowed) {
            const isOnCarcass = this.planogramService.isOnCarcass(cell, bbox);

            if (isOnCarcass == ElementPosition.Outside) {
                if (!this.isCluster) {
                    //added to scratchpad
                    cell.attributes.planogramInfo.scratchPadId = planogram.scratchPadId;
                    cell.attributes.partInfo.scratchPadId = planogram.scratchPadId;
                    cell.attributes.partInfo.planogramShelfId = null;
                    cell.attributes.partInfo.planmatrShelfId = 0;
                    //unembed the cell from the shelf or stand before adding to the scratch pad.
                    const parent = cell.getParentCell();
                    if (parent != null) {
                        parent.unembed(cell);
                    }

                    return true;
                } else {
                    return false; //no scratchpad on clusters
                }
            } else if (isOnCarcass == ElementPosition.Inside) {
                //remove from scratchpad if necessary
                cell.attributes.planogramInfo.scratchPadId = 0;
                cell.attributes.partInfo.scratchPadId = 0;
                const parent = cell.getParentCell();
                if (parent != null) {
                    if (parent.attributes.type == 'planmatr.Part.Shelf') {
                        parent.unembed(cell);
                    }
                }

                if (cell.attributes.partInfo.partTypeId == PartTypes.Blanking || cell.attributes.partInfo.partTypeId == PartTypes.FasciaPlate) {
                    //embed this item into the shelf - but don't change it's position
                    const nearestShelf = this.planogramService.getNearestShelf(this.graph, stand, cellView, cell);
                    if (nearestShelf != null) {
                        nearestShelf.embed(cell);
                        cell.attributes.partInfo.planogramShelfId = nearestShelf.attributes.shelfInfo.planogramShelfId;
                        cell.attributes.partInfo.planmatrShelfId = nearestShelf.attributes.id;
                    }
                }
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

    //These functions will be available via the authStore when we integrate into the
    async isAdminUser() {
        // const userrole = await Promise.resolve(this.getUserRole());
        // const userRole = ((userrole) as any);
        // if (userRole.role == "administrator") {
        //   return true;
        // }
        // else {
        //   return false;
        // }
        return true;
    }

    getUserName() {
        //ajax call to get json goes here
        // return $.ajax({
        //   type: "GET",
        //   url: '/Umbraco/Api/AuthApi/GetUserName',
        //   contentType: "application/json"
        // }).done(data => data)
        //   .fail(data => data);
        return 'Test User';
    }

    getUserRole() {
        // return $.ajax({
        //   type: "GET",
        //   url: '/Umbraco/Api/AuthApi/GetUserRole',
        //   contentType: "application/json"
        // }).done(data => data)
        //   .fail(data => data);
        return 'Test User';
    }

    setProperty(path: string, value: g.Point, opt: { rewrite?: boolean }) {
        opt = opt || {};

        // The model doesn't have to be a JointJS cell necessarily. It could be
        // an ordinary Backbone.Model and such would have no method 'prop'.
        const prop = joint.dia.Cell.prototype.prop;
        const model = this.cassette;
        const overwrite = opt.rewrite || false;

        if (value === undefined) {
            // Method prop can't handle undefined values in right way.
            // The model attributes would stay untouched if try to
            // set a nested property to undefined.
            joint.dia.Cell.prototype.removeProp.call(model, path, opt);
        } else {
            let updated;

            if (joint.util.isObject(value) && !overwrite) {
                const current = prop.call(model, path, opt);
                const targetType = Array.isArray(value) ? [] : {};
                updated = joint.util.merge(targetType, current, value);
            } else {
                updated = joint.util.clone(value);
            }

            //if (overwrite) opt.rewrite = true;
            prop.call(model, path, updated, opt);
        }
    }
}
