import * as joint from '@joint/plus'
//import * as _ from 'lodash';
// import { CassetteService } from '../services/cassette.service'
import {
  CurrentView,
  ElementPosition,
  PartTypes,
  ShadeStatusColourEnum,
  StandLayoutEnum,
  StatusColourEnum,
} from '../models/Enumerations'

import type { PlanogramNote } from '@/models/Planograms/note.model'
import { PartInfo } from '@/planner/models/PartInfo'
import { PlanogramInfo } from '@/planner/models/PlanogramInfo'
import { PlanogramSvg } from '@/planner/models/PlanogramSvg'
import * as appShapes from '@/planner/models/shapes/planmatr-shapes'
import { planmatr } from '@/planner/models/shapes/planmatr-shapes'
import { ShelfInfoList } from '@/planner/models/ShelfInfoList'
import { type Stand } from '@/planner/models/Stand'
import { PlanogramRenderService } from '@/planner/services/planogram-render-service'
import { Auth } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'
import { Row } from '../models/Row'
import g = joint.g

const token = ref()
const idToken = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_ROOT + '/api/planograms',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
    ClaimsAuth: idToken.value || '',
  },
})

export class PlanogramService {
  _posX!: number
  _posY!: number
  _maxX!: number
  graph: joint.dia.Graph
  paper: joint.dia.Paper
  isCluster: boolean
  partOverlap: boolean
  partOverlapAmount: number
  modelsUnderneath!: object[]
  partTypes!: PartTypes
  planogramRenderService: PlanogramRenderService

  constructor(
    isCluster: boolean,
    partOverlap: boolean,
    partOverlapAmount: number,
    graph: joint.dia.Graph,
    paper: joint.dia.Paper,
  ) {
    this.isCluster = isCluster
    this.partOverlap = partOverlap
    this.partOverlapAmount = partOverlapAmount
    this.graph = graph
    this.paper = paper
    this.planogramRenderService = new PlanogramRenderService()
  }

  getSkuCount(planoData: PartInfo[]): { skuCount: number; shelfCount: number } {
    let skuCount = 0
    let shelfCount = 0
    planoData.forEach(function (part: PartInfo) {
      try {
        if (part.partTypeId === 4 || part.partTypeId === 10) {
          shelfCount += 1
        } else {
          skuCount += part.facings * part.stock
        }
      } catch (e) {
        //alert(e.message);
      }
    })
    return { skuCount: skuCount, shelfCount: shelfCount }
  }

  async displayComCount(planogramNotes: PlanogramNote[]) {
    //do something with data
    //var result = data;
    const comCount = planogramNotes.length
    // if ($(".joint-toolbar #plan-x-show-comments").find('.comment-count').length == 0) {
    if (document.querySelector('.joint-toolbar #planm-show-comments .comment-count') === null) {
      if (document.querySelector('.joint-toolbar #planm-show-comments') != null)
        document
          .querySelector('.joint-toolbar #planm-show-comments')!
          .insertAdjacentHTML('beforeend', "<span class='comment-count'>" + comCount + '</span>')
      // $(".joint-toolbar #plan-x-show-comments").append("<span class='comment-count'>" + data.result + "</span>");
    } else {
      document.querySelector('.joint-toolbar #planm-show-comments .comment-count')!.textContent =
        comCount.toString()
      // $(".joint-toolbar #plan-x-show-comments .comment-count").text(data.result);
    }
  }

  displaySkuCount(graph: joint.dia.Graph) {
    const allElems = graph.getElements()
    //allElems[0].attributes.type
    // var carcass = _.filter(allElems, function (el) { return el.attributes.type === 'planmatr.Carcass'; });
    // var planoParts = carcass[0].getEmbeddedCells();
    const cassettes = allElems.filter(function (el: joint.dia.Element) {
      return (
        el.attributes.type === 'planmatr.Part.Cassette' &&
        el.attributes.planogramInfo.scratchPadId === 0
      )
    })

    const shelves = allElems.filter(function (el: joint.dia.Element) {
      return (
        el.attributes.type === 'planmatr.Part.Shelf' &&
        el.attributes.planogramInfo.scratchPadId === 0
      )
    })

    let skuCount = 0
    let shelfCount = 0
    cassettes.forEach(function (cass: joint.dia.Element) {
      skuCount += cass.attributes.partInfo.facings * cass.attributes.partInfo.stock
    })
    shelfCount = shelves.length

    document.querySelector('.app-title .plan-x-type .sku-count span.count')!.textContent =
      skuCount.toString()
    document.querySelector('.app-title .plan-x-type .shelf-count span.count')!.textContent =
      shelfCount.toString()
    // $('.app-title .plan-x-type .sku-count span.count').text(skuCount.toString());
    // $('.app-title .plan-x-type .shelf-count span.count').text(shelfCount.toString());
  }

  populatePlanogram(
    graph: joint.dia.Graph,
    planoData: PartInfo[],
    carcass: planmatr.Carcass,
    stand: Stand,
    countryId: number,
  ) {
    const self = this

    const startXPos = 0 // (self.stand.Width - self.stand.MerchWidth) / 2;
    const startYPos = 0 //(self.stand.Height - self.stand.MerchHeight) / 2;
    planoData.forEach(function (part: PartInfo) {
      try {
        // var shape: string;
        let jointShape: joint.dia.Element
        const statusColour = StatusColourEnum[part.statusId ?? 0]

        if (part.partTypeId === 4 || part.partTypeId === 10) {
          //Its A Shelf

          if (part.svgLineGraphic != null) {
            jointShape = new planmatr.Part.Shelf({
              size: { width: 5, height: 1 },
              height: part.height,
              width: part.width,
              attrs: {
                shelf: {
                  'xlink:href':
                    'data:image/svg+xml;utf8,' + encodeURIComponent(part.svgLineGraphic),
                },
                body: { fill: statusColour, 'fill-opacity': 0.4 },
                label: {
                  text: part.shelfLabel == null ? '' : part.shelfLabel,
                  'font-size': 7 >= part.height ? part.height - 2 : 7,
                  textWrap: {
                    width: -10, // reference width minus 10
                    height: '90%', // half of the reference height
                    ellipsis: true, // could also be a custom string, e.g. '...!?'
                  },
                },
              },
            })
          } else {
            jointShape = new planmatr.Part.Shelf({
              size: { width: 5, height: 1 },
              height: part.height,
              width: part.width,
              attrs: {
                body: { fill: statusColour, 'fill-opacity': 0.4 },
                label: {
                  'font-size': 7 >= part.height ? part.height - 2 : 7,
                  '#label': part.shelfLabel == null ? '' : part.shelfLabel,
                  textWrap: {
                    width: -10, // reference width minus 10
                    height: '90%', // half of the reference height
                    ellipsis: true, // could also be a custom string, e.g. '...!?'
                  },
                },
              },
            })
          }

          if (self.isCluster) {
            jointShape.attributes.disableMove = false
          } else {
            jointShape.attributes.disableMove = stand.shelfLock
          }
          jointShape.attributes.shelfInfo.planogramId = part.planogramId
          jointShape.attributes.shelfInfo.name = part.name
          jointShape.attributes.shelfInfo.planogramShelfId = part.planogramShelfId
          jointShape.attributes.shelfInfo.clusterShelfId = part.clusterShelfId
          jointShape.attributes.shelfInfo.partId = part.partId
          jointShape.attributes.shelfInfo.shelfTypeId = part.partTypeId
          jointShape.attributes.shelfInfo.shelfType = part.partType
          jointShape.attributes.shelfInfo.id = part.id
          jointShape.attributes.shelfInfo.shelfId = part.partId
          jointShape.attributes.shelfInfo.partNumber = part.partNumber
          jointShape.attributes.shelfInfo.height = part.height
          jointShape.attributes.shelfInfo.width = part.width
          if (part.clusterShelfId != null) {
            jointShape.attributes.shelfInfo.label = ''
            jointShape.attributes.clusterId = part.planogramId
            jointShape.attributes.shelfInfo.planogramId = part.clusterId
          } else {
            jointShape.attributes.shelfInfo.label = part.shelfLabel
          }
          jointShape.attributes.shelfInfo.status = part.statusId
          jointShape.attributes.shelfInfo.statusId = part.statusId
          jointShape.attributes.shelfInfo.scratchPadId = part.scratchPadId
          jointShape.attributes.shelfInfo.svgLineGraphic = part.svgLineGraphic
          jointShape.attributes.shelfInfo.render2dImage = part.render2dImage ?? ''

          jointShape.attributes.planogramInfo.planogramId = part.planogramId
          jointShape.attributes.planogramInfo.scratchPadId =
            part.scratchPadId == null ? 0 : part.scratchPadId
          jointShape.attributes.planogramInfo.x = part.position.x
          jointShape.attributes.planogramInfo.y = part.position.y
          if (jointShape.attributes.position) {
            jointShape.attributes.position.x = part.position.x
            jointShape.attributes.position.y = part.position.y
          }
          jointShape.resize(part.width, part.height)
          jointShape.addTo(graph, { ignoreCommandManager: true })
          //stand.embed(jointShape);
          jointShape.position(
            jointShape.attributes.planogramInfo.x + startXPos,
            jointShape.attributes.planogramInfo.y + startYPos,
            { parentRelative: true },
          )
          //jointShape.attributes.textWrap.set()

          //to find the column
          const columnBelow = graph.findModelsUnderElement(jointShape).find(function (
            el: joint.dia.Element,
          ) {
            return el.attributes.type == 'planmatr.Column'
          })

          if (columnBelow !== undefined) {
            columnBelow.embed(jointShape, { ignoreCommandManager: true })
          } else {
            carcass.embed(jointShape, { ignoreCommandManager: true })
          }
        } else {
          //they are not shelves they are casette types

          if (part.svgLineGraphic != null) {
            //var parser = new DOMParser();
            //var doc = parser.parseFromString(part.svgLineGraphic, "image/svg+xml");
            jointShape = new appShapes.planmatr.Part.Cassette({
              size: { width: 5, height: 1 },
              height: part.height,
              width: part.width,
              attrs: {
                cassette: {
                  'xlink:href':
                    'data:image/svg+xml;utf8,' + encodeURIComponent(part.svgLineGraphic),
                },
                body: { fill: statusColour, 'fill-opacity': 0.4 },
                label: {
                  text: part.label == null ? '' : part.label,
                  textWrap: {
                    width: -10, // reference width minus 10
                    height: '90%', // half of the reference height
                    ellipsis: true, // could also be a custom string, e.g. '...!?'
                  },
                },
              },
            })
          } else {
            jointShape = new appShapes.planmatr.Part.Cassette({
              size: { width: 5, height: 1 },
              height: part.height,
              width: part.width,
              attrs: {
                body: { fill: statusColour, 'fill-opacity': 0.4 },
                text: {
                  text: part.label == null ? '' : part.label,
                  textWrap: {
                    width: -10, // reference width minus 10
                    height: '90%', // half of the reference height
                    ellipsis: true, // could also be a custom string, e.g. '...!?'
                  },
                },
              },
            })
          }

          jointShape.attributes.shapeType = part.partType
          jointShape.attributes.partInfo.planogramShelfId = part.planogramShelfId
          jointShape.attributes.partInfo.facings = part.facings
          jointShape.attributes.partInfo.stock = part.stock
          jointShape.attributes.partInfo.partid = part.partId
          jointShape.attributes.partInfo.planogramPartId = part.planogramPartId
          if (self.isCluster) {
            jointShape.attributes.partInfo.planogramPartId = part.clusterPartId
          }
          jointShape.attributes.partInfo.clusterPartId = part.clusterPartId
          jointShape.attributes.partInfo.partTypeId = part.partTypeId

          jointShape.attributes.partInfo.partType = part.partType
          jointShape.attributes.partInfo.partNumber = part.partNumber
          jointShape.attributes.partInfo.name = part.name
          jointShape.attributes.partInfo.status = part.status
          jointShape.attributes.partInfo.statusId = part.statusId
          if (part.nonMarket) {
            part.svgLineGraphic =
              '<svg id=\"Layer_1\" data-name=\"Layer 1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" viewBox=\"0 0 536.6 515.6\"><defs><clipPath id=\"clip-path\" transform=\"translate(-44.41 -114.59)\"><rect x=\"87.81\" y=\"136.31\" width=\"447.45\" height=\"447.45\" style=\"fill: none\"/></clipPath></defs><rect x=\"43.4\" y=\"21.73\" width=\"447.45\" height=\"447.45\" style=\"fill: #dadada\"/><g style=\"clip-path: url(#clip-path)\"><polygon points=\"536.6 487.23 296.68 247.3 515.6 28.38 487.23 0 268.3 218.93 49.37 0 21 28.38 239.92 247.3 0 487.23 28.38 515.6 268.3 275.68 508.22 515.6 536.6 487.23\" style=\"fill: #e41915\"/></g></svg>'
          }

          jointShape.attributes.partInfo.svgLineGraphic = part.svgLineGraphic
          jointShape.attributes.partInfo.render2dImage = part.render2dImage
          jointShape.attributes.partInfo.notes = part.notes == null ? '' : part.notes
          jointShape.attributes.partInfo.label = part.label

          jointShape.attributes.planogramInfo.x = part.position.x
          jointShape.attributes.planogramInfo.y = part.position.y
          if (jointShape.attributes.position) {
            jointShape.attributes.position.x = part.position.x
            jointShape.attributes.position.y = part.position.y
          }

          //////////////////
          //we are going to load products and shades into the partInfo when we edit the shades
          //////////////////
          if (part.products != null) {
            if (part.products.length !== 0 && part.facings > 0)
              jointShape.attributes.partInfo.products = part.products.filter(function (p) {
                return (
                  p.published == true && p.countriesList.split(',').includes(countryId.toString())
                )
              })
          }

          //////////////////
          if (part.facingProducts) {
            if (part.facingProducts.length !== 0) {
              for (let f = 0; f < part.facingProducts.length; f++) {
                const facing = part.facingProducts[f]
                let facingProduct = null
                if (facing?.productId != null) {
                  facingProduct = part.products.find((p) => p.id == facing.productId)
                  if (facingProduct != null) {
                    if (facingProduct.published == true) {
                      jointShape.attributes.partInfo['selectedProduct-facing-' + facing.facingNo] =
                        facing.productId
                      if (facing.shadeId != null)
                        jointShape.attributes.partInfo['selectedShade-facing-' + facing.facingNo] =
                          facing.shadeId
                      if (facing.facingStatus != null)
                        jointShape.attributes.partInfo['selectedStatus-facing-' + facing.facingNo] =
                          facing.facingStatus
                    }
                  } else {
                    jointShape.attributes.partInfo['selectedProduct-facing-' + facing.facingNo] =
                      null
                    if (facing.shadeId != null)
                      jointShape.attributes.partInfo['selectedShade-facing-' + facing.facingNo] =
                        null
                    if (facing.facingStatus != null)
                      jointShape.attributes.partInfo['selectedStatus-facing-' + facing.facingNo] =
                        null
                  }
                }
              }
            }
          }
          if (part.planogramId != null)
            jointShape.attributes.planogramInfo.planogramId = part.planogramId
          jointShape.attributes.planogramInfo.scratchPadId =
            part.scratchPadId == null ? 0 : part.scratchPadId
          jointShape.attributes.partInfo.scratchPadId =
            part.scratchPadId == null ? 0 : part.scratchPadId
          jointShape.resize(part.width, part.height)
          //Notes
          if (part.notes != null && part.notes.trim() != '') {
            jointShape.attributes['hasNotes'] = true
            if (
              jointShape.attributes.attrs != null &&
              jointShape.attributes.attrs['.comment-indi'] != null
            )
              jointShape.attributes.attrs['.comment-indi']['visibility'] = 'visible'
          } else {
            jointShape.attributes['hasNotes'] = false
            //temporarily hide the comment indicator until we have a better way of hiding it when on very narrow items like dividers (accessories)
            if (
              jointShape.attributes.attrs != null &&
              jointShape.attributes.attrs['.comment-indi'] != null
            )
              jointShape.attributes.attrs['.comment-indi']['display'] = 'none'
          }

          //show facing information:

          if (jointShape.attributes.attrs != null && jointShape.attributes.attrs.image != null)
            jointShape.attributes.attrs.image['xlink:href'] = ''
          if (
            jointShape.attributes.partInfo.partTypeId != PartTypes.Blanking &&
            jointShape.attributes.partInfo.partTypeId != PartTypes.FasciaPlate
          ) {
            const facingHtml = self.planogramRenderService.generateFacingHtml(jointShape)
            if (
              jointShape.attributes.attrs != null &&
              jointShape.attributes.attrs['facings'] != null
            ) {
              jointShape.attributes.attrs['facings']['html'] = facingHtml
              jointShape.attributes.attrs['facings']['opacity'] = 1
            }
          }
          //if (jointShape.attributes.partInfo.svgLineGraphic != null) {
          //  jointShape.attributes.attrs.image["xlink:href"] =
          //    'data:image/svg+xml;utf8,' + encodeURIComponent(jointShape.attributes.partInfo.svgLineGraphic);
          //}

          //var cellView = cassettes[i].findView(paper);
          //cellView.render();

          jointShape.addTo(graph, { ignoreCommandManager: true })

          ////replace the .icon with the svg
          //https://stackoverflow.com/questions/24933430/img-src-svg-changing-the-fill-color

          //// Remove any invalid XML tags as per http://validator.w3.org
          //var iconSVg = part.svgLineGraphic;
          //var shapeHtml = graph.
          //// Check if the viewport is set, if the viewport is not set the SVG wont't scale.
          //if (!$svg.attr('viewBox') && $svg.attr('height') && $svg.attr('width')) {
          //  $svg.attr('viewBox', '0 0 ' + $svg.attr('height') + ' ' + $svg.attr('width'))
          //}

          // Replace image with new S
          //stand.embed(jointShape);
          jointShape.position(
            jointShape.attributes.planogramInfo.x + startXPos,
            jointShape.attributes.planogramInfo.y + startYPos,
            { parentRelative: true, ignoreMove: true },
          )

          //WE NEED TO TEST IF THIS IS AN ACCESSORY AS ACCESSORIES DON'T ATTACH TO SHELVES
          if (jointShape.attributes.partInfo.partTypeId != 5) {
            if (jointShape.attributes.partInfo.partTypeId == PartTypes.Blanking) {
              //we need to make this part transparent
              if (jointShape.attributes.attrs && jointShape.attributes.attrs['.body']) {
                jointShape.attributes.attrs['.body']['fill-opacity'] = 20
              }
            }

            const columnBelow = graph.findElementsUnderElement(jointShape).find(function (
              el: joint.dia.Element,
            ) {
              return el.attributes.type == 'planmatr.Column'
            })

            if (
              columnBelow !== undefined ||
              stand.layoutStyle == StandLayoutEnum.Pitch ||
              (columnBelow == undefined && stand.layoutStyle == StandLayoutEnum.Column)
            ) {
              //get the shelf that this part belongs to and embed it.
              const bbox = jointShape.getBBox()
              if (jointShape.attributes.position) {
                bbox.x = jointShape.attributes.position.x
                bbox.y = jointShape.attributes.position.y + jointShape.attributes.height
              }
              bbox.height = stand.height //50 is arbitrary - enough to catch a shelf immediately below.

              const shelvesUnderneath = graph.findElementsInArea(bbox).filter(function (
                el: joint.dia.Element,
              ) {
                return (
                  el.attributes.shapeType === 'Shelf' || el.attributes.shapeType === 'Base Shelf'
                )
              })

              if (shelvesUnderneath.length > 0) {
                const theShelf = shelvesUnderneath.find(function (el: joint.dia.Element) {
                  return el.attributes.shelfInfo.planogramShelfId === part.planogramShelfId
                })
                if (theShelf != null) {
                  theShelf.embed(jointShape, { ignoreCommandManager: true })
                  jointShape.attributes.partInfo.planogramShelfId =
                    theShelf.attributes.shelfInfo.planogramShelfId
                  jointShape.attributes.partInfo.planmatrShelfId = theShelf.attributes.id
                } else {
                  columnBelow?.embed(jointShape, { ignoreCommandManager: true })
                }
              }
            } else {
              carcass.embed(jointShape, { ignoreCommandManager: true })
            }
          }
        }
      } catch (e) {
        console.log(e)
      }
    })
    // now sort out the overlays - Blanking and Fascia Plates - fascia plates uppermost
    this.orderOverlays(graph, PartTypes.FasciaPlate)
    this.orderOverlays(graph, PartTypes.Blanking)
  }

  findElement(elemsToSearch: joint.dia.Element[], part: PartInfo) {
    //var self = this;
    let element = null
    if (part.partTypeId === 4 || part.partTypeId === 10) {
      // shelf
      element = elemsToSearch.find(function (el: joint.dia.Element) {
        return (
          el.attributes.position?.x === part.position.x &&
          el.attributes.position?.y === part.position.y
        )
      })
    } else {
      element = elemsToSearch.find(function (el: joint.dia.Element) {
        return (
          el.attributes.position?.x === part.position.x &&
          el.attributes.position?.y === part.position.y &&
          el.attributes.partInfo?.partTypeId === part.partTypeId
        )
      })
    }
    return element
  }

  rePopulatePlanogram(
    graph: joint.dia.Graph,
    planoData: PartInfo[],
    carcass: appShapes.planmatr.Carcass,
    stand: Stand,
    appMode: number,
    countryId: number,
  ) {
    const self = this
    const allElems = graph.getElements()
    let planmatrPart = null
    const shelves = allElems.filter(function (el: joint.dia.Element) {
      return (
        (el.attributes.type === 'planmatr.Part.Shelf' &&
          el.attributes.planogramInfo.scratchPadId === 0) ||
        (el.attributes.type === 'planmatr.Part.Shelf' &&
          el.attributes.planogramInfo.scratchPadId === null)
      )
    })
    const cassettes = allElems.filter(function (el: joint.dia.Element) {
      return (
        (el.attributes.type === 'planmatr.Part.Cassette' &&
          el.attributes.planogramInfo.scratchPadId === 0) ||
        (el.attributes.type === 'planmatr.Part.Cassette' &&
          el.attributes.planogramInfo.scratchPadId === null)
      )
    })
    //now we need to update any parts with new planogram-partsId
    planoData.forEach(function (part: PartInfo) {
      try {
        if (part.partTypeId === 4 || part.partTypeId === 10) {
          // shelves
          planmatrPart = self.findElement(shelves, part)
        } else {
          planmatrPart = self.findElement(cassettes, part)
        }

        if (planmatrPart != null) {
          if (part.partTypeId === 4 || part.partTypeId === 10) {
            // shelves

            if (self.isCluster) {
              if (planmatrPart.attributes.shelfInfo.clusterShelfId == 0) {
                planmatrPart.attributes.shelfInfo.clusterShelfId = part.clusterShelfId
              }
            } else {
              if (planmatrPart.attributes.shelfInfo.planogramShelfId == 0) {
                if (appMode != 2)
                  planmatrPart.attributes.shelfInfo.planogramShelfId = part.planogramShelfId
              }
            }
          } else {
            if (self.isCluster) {
              if (planmatrPart.attributes.partInfo.planogramPartId == 0) {
                planmatrPart.attributes.partInfo.planogramPartId = part.clusterPartId
              }
            } else {
              if (planmatrPart.attributes.partInfo.planogramPartId == 0) {
                if (appMode != 2)
                  planmatrPart.attributes.partInfo.planogramPartId = part.planogramPartId
              }
            }
          }
        }
      } catch (e) {
        //alert(e);
        console.log(e)
      }
    })
  }

  populateScratchPad(
    graph: joint.dia.Graph,
    scratchData: PartInfo[],
    stand: Stand,
    countryId: number,
  ) {
    const self = this
    //var stand = self.standShape;

    const startXPos = 0 - 100
    const startYPos = 0
    self._posX = startXPos
    self._posY = startYPos

    let scratchCount = scratchData.length
    if (scratchCount > 10) {
      scratchCount = 1000 / scratchCount
    }
    self._maxX = scratchCount * -5 * 10

    scratchData.forEach((part: PartInfo, i: number) => {
      try {
        // var shape: string;
        let jointShape: joint.dia.Element
        const statusColour = StatusColourEnum[part.statusId ?? 0]

        let nextPart: PartInfo | undefined = part
        if (i + 1 < scratchData.length) nextPart = scratchData[i + 1]
        if (part.partTypeId === 4 || part.partTypeId === 10) {
          //Its A Shelf

          if (part.svgLineGraphic != null) {
            jointShape = new appShapes.planmatr.Part.Shelf({
              size: { width: 5, height: 1 },
              height: part.height,
              width: part.width,
              attrs: {
                shelf: {
                  'xlink:href':
                    'data:image/svg+xml;utf8,' + encodeURIComponent(part.svgLineGraphic),
                },
                body: { fill: statusColour, 'fill-opacity': 0.4 },
                label: {
                  text: part.shelfLabel == null ? '' : part.shelfLabel,
                  'font-size': 7 >= part.height ? part.height - 2 : 7,
                  textWrap: {
                    width: -10, // reference width minus 10
                    height: '90%', // half of the reference height
                    ellipsis: true, // could also be a custom string, e.g. '...!?'
                  },
                },
              },
            })
          } else {
            jointShape = new appShapes.planmatr.Part.Shelf({
              size: { width: 5, height: 1 },
              height: part.height,
              width: part.width,
              attrs: {
                label: {
                  'font-size': 7 >= part.height ? part.height - 2 : 7,
                  text: part.shelfLabel == null ? '' : part.shelfLabel,
                  textWrap: {
                    width: -10, // reference width minus 10
                    height: '90%', // half of the reference height
                    ellipsis: true, // could also be a custom string, e.g. '...!?'
                  },
                },
              },
            })
          }
          jointShape.attributes.shelfInfo.planogramId = part.planogramId
          jointShape.attributes.shelfInfo.planogramShelfId = part.planogramShelfId
          jointShape.attributes.shelfInfo.partId = part.partId
          jointShape.attributes.shelfInfo.shelfTypeId = part.partTypeId
          jointShape.attributes.shelfInfo.shelfType = part.partType
          jointShape.attributes.shelfInfo.id = part.id
          jointShape.attributes.shelfInfo.shelfId = part.partId
          jointShape.attributes.shelfInfo.partNumber = part.partNumber
          jointShape.attributes.shelfInfo.height = part.height
          jointShape.attributes.shelfInfo.width = part.width
          jointShape.attributes.shelfInfo.label = part.shelfLabel
          jointShape.attributes.shelfInfo.status = part.statusId
          jointShape.attributes.shelfInfo.statusId = part.statusId
          jointShape.attributes.shelfInfo.scratchPadId = part.scratchPadId

          jointShape.attributes.shelfInfo.svgLineGraphic = part.svgLineGraphic
          jointShape.attributes.shelfInfo.render2dImage = part.render2dImage ?? ''

          jointShape.attributes.planogramInfo.planogramId = part.planogramId
          jointShape.attributes.planogramInfo.scratchPadId = part.scratchPadId
          jointShape.attributes.planogramInfo.x = part.position.x
          jointShape.attributes.planogramInfo.y = part.position.y
          if (jointShape.attributes.position) {
            jointShape.attributes.position.x = part.position.x
            jointShape.attributes.position.y = part.position.y
          }
          jointShape.resize(part.width, part.height)
          jointShape.addTo(graph)

          if (part.position.x === 0 && part.position.y === 0) {
            //if the position is 0,0 then we need to layout the part as it comes from legacy data
            const position = self.layoutPart(part, nextPart as PartInfo, startXPos)
            part.position.x = position.x
            part.position.y = position.y
          }

          jointShape.addTo(graph, { ignoreCommandManager: true })

          jointShape.position(
            jointShape.attributes.planogramInfo.x,
            jointShape.attributes.planogramInfo.y,
            { parentRelative: true, ignoreMove: true },
          )
        } else {
          //they are not shelves they are casette types
          if (part.svgLineGraphic != null) {
            jointShape = new appShapes.planmatr.Part.Cassette({
              size: { width: 5, height: 1 },
              height: part.height,
              width: part.width,
              attrs: {
                cassette: {
                  'xlink:href':
                    'data:image/svg+xml;utf8,' + encodeURIComponent(part.svgLineGraphic),
                },
              },
            })
          } else {
            jointShape = new appShapes.planmatr.Part.Cassette({
              size: { width: 5, height: 1 },
              height: part.height,
              width: part.width,
            })
          }

          jointShape.attributes.shapeType = part.partType
          jointShape.attributes.partInfo.planogramShelfId = part.planogramShelfId
          jointShape.attributes.partInfo.facings = part.facings
          jointShape.attributes.partInfo.stock = part.stock
          jointShape.attributes.partInfo.partid = part.partId
          jointShape.attributes.partInfo.id = part.id
          jointShape.attributes.partInfo.planogramPartId = part.planogramPartId
          jointShape.attributes.partInfo.partTypeId = part.partTypeId
          jointShape.attributes.partInfo.partType = part.partType
          jointShape.attributes.partInfo.partNumber = part.partNumber
          jointShape.attributes.partInfo.name = part.name
          jointShape.attributes.partInfo.status = part.status
          jointShape.attributes.partInfo.statusId = part.statusId
          jointShape.attributes.partInfo.notes = part.notes == null ? '' : part.notes

          if (part.products != null) {
            if (part.products.length !== 0 && part.facings > 0)
              jointShape.attributes.partInfo.products = part.products.filter(function (p) {
                return (
                  p.published == true && p.countriesList.split(',').includes(countryId.toString())
                )
              })
          }

          if (part.facingProducts.length !== 0) {
            for (let f = 0; f < part.facingProducts.length; f++) {
              const facing = part.facingProducts[f]
              if (facing != null) {
                if (facing.productId != null)
                  jointShape.attributes.partInfo['selectedProduct-facing-' + facing.facingNo] =
                    facing.productId
                if (facing.shadeId != null)
                  jointShape.attributes.partInfo['selectedShade-facing-' + facing.facingNo] =
                    facing.shadeId
                if (facing.facingStatus != null)
                  jointShape.attributes.partInfo['selectedStatus-facing-' + facing.facingNo] =
                    facing.facingStatus
              }
            }
          }
          if (part.planogramId != null)
            jointShape.attributes.planogramInfo.planogramId = part.planogramId
          jointShape.attributes.planogramInfo.scratchPadId = part.scratchPadId
          jointShape.attributes.partInfo.scratchPadId = part.scratchPadId
          if (part.nonMarket) {
            part.svgLineGraphic =
              '<svg id=\"Layer_1\" data-name=\"Layer 1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" viewBox=\"0 0 536.6 515.6\"><defs><clipPath id=\"clip-path\" transform=\"translate(-44.41 -114.59)\"><rect x=\"87.81\" y=\"136.31\" width=\"447.45\" height=\"447.45\" style=\"fill: none\"/></clipPath></defs><rect x=\"43.4\" y=\"21.73\" width=\"447.45\" height=\"447.45\" style=\"fill: #dadada\"/><g style=\"clip-path: url(#clip-path)\"><polygon points=\"536.6 487.23 296.68 247.3 515.6 28.38 487.23 0 268.3 218.93 49.37 0 21 28.38 239.92 247.3 0 487.23 28.38 515.6 268.3 275.68 508.22 515.6 536.6 487.23\" style=\"fill: #e41915\"/></g></svg>'
          }
          jointShape.attributes.partInfo.svgLineGraphic = part.svgLineGraphic
          jointShape.attributes.partInfo.render2dImage = part.render2dImage

          jointShape.resize(part.width, part.height)
          //Notes
          if (part.notes != null && part.notes.trim() != '') {
            jointShape.attributes['hasNotes'] = true
            if (
              jointShape.attributes.attrs != null &&
              jointShape.attributes.attrs['.comment-indi'] != null
            ) {
              jointShape.attributes.attrs['.comment-indi']['visibility'] = 'visible'
            }
          } else {
            jointShape.attributes['hasNotes'] = false
            //temporarily hide the comment indicator until we have a better way of hiding it when on very narrow items like dividers (accessories)
            if (
              jointShape.attributes.attrs != null &&
              jointShape.attributes.attrs['.comment-indi'] != null
            ) {
              jointShape.attributes.attrs['.comment-indi']['display'] = 'none'
            }
          }

          if (part.position.x === 0 && part.position.y === 0) {
            //if the position is 0,0 then we need to layout the part as it comes from legacy data
            const position = self.layoutPart(part, nextPart as PartInfo, startXPos)
            part.position.x = position.x
            part.position.y = position.y

            if (jointShape.attributes.position) {
              jointShape.attributes.position.x = part.position.x
              jointShape.attributes.position.y = part.position.y
            }
            jointShape.attributes.planogramInfo.x = part.position.x
            jointShape.attributes.planogramInfo.y = part.position.y
          } else {
            if (jointShape.attributes.position) {
              jointShape.attributes.position.x = part.position.x
              jointShape.attributes.position.y = part.position.y
            }
            jointShape.attributes.planogramInfo.x = part.position.x
            jointShape.attributes.planogramInfo.y = part.position.y

            if (jointShape.attributes.partInfo.planogramShelfId != null) {
              // var pShelfId = jointShape.attributes.partInfo.planogramShelfId;
              // var parentShelf = _.find(graph.getElements(),
              //   function (e: joint.dia.Element) {
              //     if (e.attributes.shapeType == 'Shelf') {
              //       return e.attributes.shelfInfo.planogramShelfId == pShelfId;
              //     }
              //   });
              //parentShelf.embed(jointShape);
            }
          }

          jointShape.addTo(graph, { ignoreCommandManager: true })

          jointShape.position(
            jointShape.attributes.planogramInfo.x,
            jointShape.attributes.planogramInfo.y,
            { parentRelative: true, ignoreMove: true },
          )
        }
      } catch (e) {
        //alert(e);
      }
    })
  }

  rePopulateScratchPad(
    graph: joint.dia.Graph,
    scratchData: PartInfo[],
    carcass: appShapes.planmatr.Carcass,
    stand: Stand,
  ) {
    const self = this
    const allElems = graph.getElements()

    //now we need to update any parts with new planogram-partsId
    scratchData.forEach((part: PartInfo) => {
      try {
        const planmatrPart = allElems.find((el: joint.dia.Element) => {
          return (
            el.attributes.position?.x === part.position.x &&
            el.attributes.position?.y === part.position.y
          )
        })
        if (part.partTypeId === 4 || part.partTypeId === 10) {
          // shelves
          if (self.isCluster) {
            if (planmatrPart?.attributes.shelfInfo.clusterShelfId == 0) {
              planmatrPart.attributes.shelfInfo.clusterShelfId = part.clusterShelfId
            }
          } else {
            if (planmatrPart?.attributes.shelfInfo.planogramShelfId == 0) {
              planmatrPart.attributes.shelfInfo.planogramShelfId = part.planogramShelfId
            }
          }
        } else {
          if (self.isCluster) {
            if (planmatrPart?.attributes.partInfo.planogramPartId == 0) {
              planmatrPart.attributes.partInfo.planogramPartId = part.clusterPartId
            }
          } else {
            if (planmatrPart?.attributes.partInfo.planogramPartId == 0) {
              planmatrPart.attributes.partInfo.planogramPartId = part.planogramPartId
            }
          }
        }
      } catch (e) {
        //alert(e);
      }
    })
  }

  layoutPart(part: PartInfo, nextPart: PartInfo, startXPos: number) {
    const self = this
    const position = new g.Point()
    position.x = self._posX
    position.y = self._posY
    if (self._posX == -100) {
      if (part.width > 100) {
        self._posX = part.width * -1 - 20
        position.x = self._posX
      }
    }
    if (self._posX - nextPart.width - 40 > self._maxX) {
      self._posX = self._posX - nextPart.width - 40
    } else {
      self._posX = startXPos
      self._posY = self._posY + 200
    }

    return position
  }

  getColumnsUnderneath(bbox: g.Rect, graph: joint.dia.Graph) {
    const columnsUnderneath = graph.findElementsInArea(bbox).filter(function (
      el: joint.dia.Element,
    ) {
      return el.attributes.type == 'planmatr.Column'
    })

    if (columnsUnderneath.length > 0) {
      columnsUnderneath.sort(function (a: joint.dia.Element, b: joint.dia.Element) {
        return (a.attributes.position?.y ?? 0) - (b.attributes.position?.y ?? 0)
      })
    }
    return columnsUnderneath
  }

  getNearestShelf(
    graph: joint.dia.Graph,
    stand: Stand,
    cellView: joint.dia.CellView,
    cell: joint.dia.Cell,
  ) {
    if (
      cell.isElement() &&
      cell.attributes.type == 'planmatr.Part.Cassette' &&
      (cell.attributes.partInfo.partTypeId == PartTypes.Cassette ||
        cell.attributes.partInfo.partTypeId == PartTypes.Blanking ||
        cell.attributes.partInfo.partTypeId == PartTypes.FasciaPlate)
    ) {
      //find if cassette is being placed on/near a shelf
      const bbox = cellView.getBBox()
      bbox.x = cell.attributes.position?.x ?? 0
      bbox.y = cell.attributes.position?.y + cell.attributes.height
      bbox.height = stand.height - bbox.y
      bbox.width = 1 //need a width to find the shelf underneath - we only need to find the nearest shelf below the cassette

      //NEED TO EXCLUDE ANY THAT ARE above the cassette bottom
      const shelvesUnderneath = graph.findModelsInArea(bbox).filter(function (
        el: joint.dia.Element,
      ) {
        return el.attributes.type == 'planmatr.Part.Shelf'
      })
      if (shelvesUnderneath.length > 0) {
        shelvesUnderneath.sort(function (a: joint.dia.Element, b: joint.dia.Element) {
          return (a.attributes.position?.y ?? 0) - (b.attributes.position?.y ?? 0)
        })

        const nearestShelfElement = shelvesUnderneath[0]
        return nearestShelfElement
        // return  this.paper.findViewByModel(nearestShelfElement);
      } else return null
    } else {
      return null
    }
  }

  partAllowedToPlace(
    graph: joint.dia.Graph,
    bbox: g.Rect,
    shelfElement: joint.dia.Element,
    cell: joint.dia.Element,
    allowOverhang: boolean,
  ) {
    const cassetteId = cell.id
    const shelfBbox = shelfElement.getBBox()

    bbox.y = (shelfElement.attributes.position?.y ?? 0) - bbox.height
    if (!allowOverhang) {
      //overhang right
      if (bbox.x + bbox.width > shelfBbox.x + shelfBbox.width) {
        return false
      }
      //overhang left
      if (bbox.x < shelfBbox.x) {
        return false
      }
    }

    //implement overlap of cassettes here if needed
    if (this.partOverlap) {
      bbox.x = bbox.x + this.partOverlapAmount
      bbox.width = bbox.width - this.partOverlapAmount * 2
    }

    const cassettesUnderneath = graph.findElementsInArea(bbox).filter(function (
      el: joint.dia.Element,
    ) {
      if (el.attributes.type == 'planmatr.Part.Cassette') {
        if (
          el.attributes.partInfo.partTypeId != PartTypes.Blanking &&
          el.attributes.partInfo.partTypeId != PartTypes.FasciaPlate &&
          el.id != cassetteId
        ) {
          return el
        }
      }
    })

    this.orderCellOverlays(cell)
    const maxHeight = this.getMaxHeightAvailable(graph, shelfElement, bbox)
    if (cassettesUnderneath.length > 0) {
      //this cassette cannot be placed here
      return false
    } else {
      if (maxHeight > bbox.height) {
        return true
      } else return false
    }
  }

  accessoryAllowedToPlace(graph: joint.dia.Graph, bbox: g.Rect, cell: joint.dia.Element) {
    const itemsUnderneath = graph.findElementsInArea(bbox).filter(function (el: joint.dia.Element) {
      return (
        (el.attributes.type == 'planmatr.Part.Cassette' ||
          el.attributes.type == 'planmatr.Part.Shelf') &&
        el.id != cell.id
      )
    })

    const accessoriesUnderneath = itemsUnderneath.filter(function (el: joint.dia.Element) {
      return (
        el.attributes.type == 'planmatr.Part.Cassette' &&
        el.attributes.partInfo.partTypeId == PartTypes.Accessory &&
        el.id != cell.id
      )
    })

    if (itemsUnderneath.length > 0) {
      if (cell.attributes.partInfo.partTypeId == PartTypes.Blanking) {
        if (accessoriesUnderneath.length > 0) {
          return false
        } else {
          return true
        }
      } else if (cell.attributes.partInfo.partTypeId == PartTypes.FasciaPlate) {
        return true
      } else {
        //this cassette cannot be placed here
        return false
      }
    } else {
      return true
    }
  }
  getMaxHeightAvailable(graph: joint.dia.Graph, shelfElement: joint.dia.Element, bbox: g.Rect) {
    //only test within columns
    //var column = this.getCurrentColumn(graph, bbox)[0];
    //var rectWidth = shelfElement.attributes.size.width
    //if (column.attributes.size.width < shelfElement.attributes.size.width) {
    //  rectWidth = column.attributes.size.width;

    //}
    //used to determine the height between shelves
    const area = new g.Rect(
      bbox.x,
      (shelfElement.attributes.position?.y ?? 0) - bbox.height,
      bbox.width,
      bbox.height,
    )
    const shelvesAbove = graph.findModelsInArea(area).filter(function (el: joint.dia.Element) {
      return el.attributes.type == 'planmatr.Part.Shelf'
    })
    shelvesAbove.sort(function (a: joint.dia.Element, b: joint.dia.Element) {
      return (a.attributes.position?.y ?? 0) - (b.attributes.position?.y ?? 0)
    })
    //var lastItem = shelvesAbove.length;
    const shelfAbove = shelvesAbove[shelvesAbove.length - 1]
    if (shelfAbove) {
      const maxHeight =
        (shelfElement.attributes.position?.y ?? 0) -
        ((shelfAbove.attributes.position?.y ?? 0) + (shelfAbove.attributes.size?.height ?? 0)) -
        1
      return maxHeight
    } else return bbox.height + 10
  }

  getCurrentColumn(graph: joint.dia.Graph, bbox: g.Rect) {
    const colUnderneath = graph.findElementsInArea(bbox).filter(function (el: joint.dia.Element) {
      return el.attributes.type == 'planmatr.Column'
    })
    return colUnderneath
  }

  display(err: Error, next: (err: Error | null) => void) {
    if (err) alert(err)
    return next(err)
  }

  snapToShelf(
    paper: joint.dia.Paper,
    graph: joint.dia.Graph,
    stand: Stand,
    cell: joint.dia.Cell,
    scratchPadId: number,
  ) {
    //var cellView = paper.findViewByModel(cell);
    const canOverHang = stand.allowOverHang
    if (
      cell.isElement() &&
      cell.attributes.type == 'planmatr.Part.Cassette' &&
      cell.attributes.partInfo.partType == 'Cassette'
    ) {
      //some strange decimal position value is breaking the api
      if (cell.attributes.position && !Number.isInteger(cell.attributes.position.x)) {
        cell.attributes.position.x = Math.round(cell.attributes.position.x)
      }
      if (cell.attributes.position && !Number.isInteger(cell.attributes.position.y)) {
        cell.attributes.position.y = Math.round(cell.attributes.position.y)
      }

      //alert(cell.attributes.partInfo.partTypeId + config.partTypes);
      //find if cassette is being placed on/near a shelf
      //var elementsUnderneath = graph.findModelsUnderElement(cellView.el);

      const bbox = cell.getBBox()

      //NEED TO EXCLUDE ANY THAT ARE above the cassette bottom
      const carcassUnderneath = graph.findElementsInArea(bbox).filter(function (
        el: joint.dia.Element,
      ) {
        return el.attributes.type == 'planmatr.Carcass'
      })
      bbox.x = cell.attributes.position?.x ?? 0
      bbox.y = (cell.attributes.position?.y ?? 0) + cell.attributes.height
      bbox.height = stand.height - bbox.y

      //NEED TO EXCLUDE ANY THAT ARE above the cassette bottom
      const shelvesUnderneath = graph.findElementsInArea(bbox).filter(function (
        el: joint.dia.Element,
      ) {
        return el.attributes.type == 'planmatr.Part.Shelf'
      })
      if (carcassUnderneath.length > 0) {
        if (shelvesUnderneath.length > 0) {
          shelvesUnderneath.sort(function (a: joint.dia.Element, b: joint.dia.Element) {
            return (a.attributes.position?.y ?? 0) - (b.attributes.position?.y ?? 0)
          })

          const nearestShelfElement = shelvesUnderneath[0]

          if (nearestShelfElement) {
            //check there is nothing already there on the shelf
            const bbox = cell.getBBox()
            //bbox.x = cell.attributes.position.x;
            bbox.y = (nearestShelfElement?.attributes.position?.y ?? 0) + cell.attributes.height
            bbox.height = cell.attributes.height
            const partAllowed = this.partAllowedToPlace(
              graph,
              bbox,
              nearestShelfElement,
              cell,
              canOverHang,
            )

            if (partAllowed) {
              if (!canOverHang) {
                //then we have to check if the part is fully on the shelf: ie doesn't extend over the edge of it
                //find the right edge of the shelf and the right end of the cassette and compare
                const cassetteRightEdge = (cell.attributes.position?.x ?? 0) + cell.attributes.width
                const shelfRightEdge =
                  (nearestShelfElement.attributes.position?.x ?? 0) +
                  nearestShelfElement.attributes.width
                if (cassetteRightEdge > shelfRightEdge) {
                  //adjust the bbox to position the shelf on the shelf completely - no overhang
                  if (cell.attributes.position) {
                    cell.attributes.position.x =
                      (cell.attributes.position.x ?? 0) - (cassetteRightEdge - shelfRightEdge)
                    bbox.x = cell.attributes.position.x ?? 0
                  }
                }
              }
              const currElement = new joint.dia.Element()

              // var cellView = paper.findViewByModel(cell);
              Object.assign(currElement, cell)
              currElement.position(
                cell.attributes.position?.x ?? 0,
                (nearestShelfElement.attributes.position?.y ?? 0) - cell.attributes.height - 1,
              ) //for redo
              //}
              const parent = cell.getParentCell()
              if (parent != null) {
                parent.unembed(cell)
              }
              nearestShelfElement.embed(cell)
              cell.attributes.partInfo.planogramShelfId =
                nearestShelfElement.attributes.shelfInfo.planogramShelfId
              cell.attributes.partInfo.planmatrShelfId = nearestShelfElement.attributes.id
              cell.attributes.partInfo.position = cell.attributes.position
              cell.attributes.planogramInfo.scratchPadId = 0
              //cell.attributes.planogramInfo.x = cell.attributes.position.x;
              //cell.attributes.planogramInfo.y = cell.attributes.position.y;
              //update the skuCount
              this.displaySkuCount(graph)
            }
          }
        }
      } else {
        //adding straight to scratchpad
        cell.attributes.planogramInfo.scratchPadId = scratchPadId
        cell.attributes.partInfo.scratchPadId = scratchPadId
        cell.attributes.partInfo.planogramShelfId = 0
        cell.attributes.partInfo.planmatrShelfId = 0
        const parent = cell.getParentCell()
        if (parent != null) {
          parent.unembed(cell)
        }
      }
    } else if (
      cell.isElement() &&
      cell.attributes.type == 'planmatr.Part.Shelf' &&
      (cell.attributes.shelfInfo.shelfType == 'Shelf' ||
        cell.attributes.shelfInfo.shelfType == 'Base Shelf')
    ) {
      const currElement = new joint.dia.Element()

      Object.assign(currElement, cell)
    }
  }

  SnapShelfToStand(
    cell: joint.dia.Cell,
    graph: joint.dia.Graph,
    stand: Stand,
    scratchPadId: number,
  ): boolean {
    const bbox = cell.getBBox()
    const currElement = new joint.dia.Element()
    Object.assign(currElement, cell)

    //var bbox = cellView.getBBox(); --- cellview gives us the size at the zoom level - wrong for comparison
    //some strange decimal position value is breaking the api
    if (!Number.isInteger(cell.attributes.position.x)) {
      cell.attributes.position.x = Math.round(cell.attributes.position.x)
    }
    if (!Number.isInteger(cell.attributes.position.y)) {
      cell.attributes.position.y = Math.round(cell.attributes.position.y)
    }

    bbox.x = cell.attributes.position.x
    bbox.y = cell.attributes.position.y

    const isOnCarcass = this.isOnCarcass(currElement, bbox)
    if (isOnCarcass == ElementPosition.Inside || isOnCarcass == ElementPosition.Partial) {
      //Now check that shelf is not across columns / wider than column
      //determine if should snap to column if it fits
      if (isOnCarcass == ElementPosition.Partial && cell.position().y < 0) {
        //shelf is across the edge of the carcass - this is not allowed
        return false
      }
      const columnsUnderneath = this.getColumnsUnderneath(bbox, graph)
      if (columnsUnderneath.length > 0) {
        //check each column for width
        //var permittedColumns = _.filter(columnsUnderneath, ['attributes.size.width', cell.attributes.size.width]);
        let permittedColumns = columnsUnderneath
        if (
          stand.layoutStyle != StandLayoutEnum.Pitch &&
          cell.attributes.shelfInfo.shelfTypeId != PartTypes.BaseShelf
        ) {
          permittedColumns = columnsUnderneath.filter(function (c: joint.dia.Element) {
            return (c.attributes.size?.width ?? 0) >= (cell.attributes.width ?? 0)
          })
        }

        if (
          permittedColumns.length > 0 ||
          cell.attributes.shelfInfo.shelfTypeId == PartTypes.BaseShelf
        ) {
          //check that the new permitted column position doesn't already have something there.
          let shelfPosX = bbox.x
          const currElement = new joint.dia.Element()
          Object.assign(currElement, cell)
          if (cell.attributes.shelfInfo.shelfTypeId != PartTypes.BaseShelf) {
            shelfPosX = Math.floor(
              this.snapShelfToColumn(currElement, permittedColumns[0] as joint.dia.Element, stand),
            )
          }

          this.H_AlignEmbeddedParts(cell, shelfPosX - bbox.x)

          bbox.y = bbox.y // + cell.attributes.height; //(y is the top left of the object, so we need to add the height of the object first) - we don't for this now.
          bbox.x = shelfPosX // permittedColumns[0].attributes.position.x;
          let elementsUnderneath = this.graph.findModelsInArea(bbox).filter(function (el) {
            return (
              el.attributes.type != 'planmatr.Carcass' &&
              el.attributes.type != 'planmatr.Column' &&
              el.attributes.type != 'planmatr.Upright' &&
              el.attributes.type != 'planmatr.Row' &&
              el.attributes.id !== cell.attributes.id &&
              el.attributes.type != 'planmatr.Header'
            )
          })

          elementsUnderneath = this.remmoveAssociatedElements(elementsUnderneath, currElement)

          if (
            elementsUnderneath.length == 0 ||
            (elementsUnderneath.length != 0 &&
              cell.attributes.shelfInfo.shelfTypeId == PartTypes.BaseShelf)
          ) {
            let nearestPitch = bbox.y
            //snap to shelf increment or row
            if (typeof stand.shelfIncrement !== 'undefined') {
              let ypos = cell.attributes.position.y
              if (cell.attributes.shelfInfo.shelfTypeId == PartTypes.BaseShelf) {
                //needs to snap to the merch base
                ypos = stand.height - stand.footerHeight - cell.attributes.height //getnearestpitch expects the top of the object (the y position)
                bbox.y = ypos
                nearestPitch = ypos
              } else {
                nearestPitch = this.getNearestPitch(
                  ypos,
                  stand.shelfIncrement,
                  stand,
                  cell.attributes.height,
                  cell.attributes.shelfInfo.shelfTypeId,
                )

                bbox.y = nearestPitch
              }
            }

            if (typeof stand.rows !== 'undefined') {
              if (stand.rows != null && stand.rows > 0) {
                nearestPitch =
                  this.getNearestRow(
                    stand.headerHeight,
                    cell.attributes.position.y,
                    stand.rowList,
                  ) - cell.attributes.height
                bbox.y = nearestPitch
              }
            }

            //here we need to test again that the final position of the shelf is not going to overlap any thing
            let elementsUnderneath = this.graph.findModelsInArea(bbox).filter(function (el) {
              return (
                el.attributes.type != 'planmatr.Carcass' &&
                el.attributes.type != 'planmatr.Column' &&
                el.attributes.type != 'planmatr.Upright' &&
                el.attributes.type != 'planmatr.Row' &&
                el.attributes.id !== cell.attributes.id
              )
            })

            elementsUnderneath = this.remmoveAssociatedElements(elementsUnderneath, currElement)

            if (elementsUnderneath.length == 0 && isOnCarcass == ElementPosition.Inside) {
              //here we need to test the snap positions see createXShelfLocations in standdetailsobject.as
              // if (cell.attributes.shelfInfo.shelfTypeId != PartTypes.BaseShelf) {
              //   const shelfPosX: number = Math.floor(this.snapShelfToColumn(currElement, permittedColumns[0] as joint.dia.Element, stand));
              // }
              // const currElement = new joint.dia.Element;
              Object.assign(currElement, cell)

              this.H_AlignEmbeddedParts(cell, shelfPosX - bbox.x)
              this.V_AlignEmbeddedParts(cell, nearestPitch - currElement.position().y)
              currElement.position(shelfPosX, nearestPitch)
              // currElement.position(shelfPosX, cell.attributes.position.y);
              cell.attributes.planogramInfo.scratchPadId = 0
              cell.attributes.shelfInfo.scratchPadId = 0

              // //need to reembed any embedded elements after alignment
              // var embdededCells = cell.getEmbeddedCells();
              // for (var i = 0; i < embdededCells.length; i++) {
              //   cell.embed(embdededCells[i]);
              // }
              cell.findView(this.paper).render()
              return true
            }

            return false
          }
        }
      } else {
        //no column to place it on can go anywhere.
        //as long as it's in the merchandising area and not in the header or footer
        this.setModelsUnderneath(currElement)

        const isMerch = this.isInMerchSpace(currElement, bbox)
        if (isMerch) {
          return true
        } else {
          return false
        }
      }
    } //end of isOnCarcass
    return true //is in scratchpad
  }

  H_AlignEmbeddedParts(shelf: joint.dia.Cell, xChange: number) {
    if (xChange != 0) {
      //We need to adjust any embdeded elements to match the new position
      const embedded = shelf.getEmbeddedCells()
      if (embedded.length > 0) {
        for (let i = 0; i < embedded.length; i++) {
          const currElement = new joint.dia.Element()
          const emb = embedded[i]
          Object.assign(currElement, emb)
          const embPos = emb?.attributes.position as g.Point
          if (embPos) {
            embPos.x += xChange
            const opt = { rewrite: false }
            this.setProperty('position', embPos, opt, currElement)
          }
          // currElement.position(embPos.x, embPos.y, { deep: true });
          // currElement.set('position', embPos, { deep: true });
          shelf.embed(currElement, { deep: true })
          currElement.findView(this.paper).render()
        }
      }
    }
  }

  V_AlignEmbeddedParts(shelf: joint.dia.Cell, yChange: number) {
    if (yChange != 0) {
      //We need to adjust any embdeded elements to match the new position
      const embedded = shelf.getEmbeddedCells()
      if (embedded.length > 0) {
        for (let i = 0; i < embedded.length; i++) {
          if (embedded[i]?.attributes?.type == 'planmatr.Part.Cassette') {
            if (
              embedded[i]?.attributes?.partInfo?.partTypeId == PartTypes.Blanking ||
              embedded[i]?.attributes?.partInfo?.partTypeId == PartTypes.FasciaPlate
            ) {
              const currElement = new joint.dia.Element()
              const emb = embedded[i]
              Object.assign(currElement, emb)
              // var embPos = emb.attributes.position;
              // embPos.y += yChange;
              //var opt = { overwrite: false };
              if (currElement.attributes.position) {
                currElement.position(
                  emb?.attributes.position?.x,
                  emb?.attributes.position?.y + yChange,
                  { deep: true },
                )
              }
              //this.setProperty('position', embPos, opt, currElement);
              currElement.findView(this.paper).render()
            }
          }
        }
      }
    }
  }

  createShelfSnapLocations(
    shelf: joint.dia.Element,
    column: planmatr.Column,
    midSnapNeeded: Boolean = true,
    onlyLeftSnap: Boolean = false,
    ignoreShelfWidth: Boolean = false,
  ): number[] {
    const shelfSnapLocations: number[] = []
    if (shelf.attributes.width <= (column.attributes.size?.width ?? 0) || ignoreShelfWidth) {
      // if shelf fits into column width
      // left snap
      shelfSnapLocations.push(column.attributes.position?.x ?? 0)
      if (shelf.attributes.width < (column.attributes.size?.width ?? 0) && !onlyLeftSnap) {
        // if shelf is narrower than column it must snap to center and right also
        // middle snap
        if (midSnapNeeded) {
          shelfSnapLocations.push(
            (column.attributes.position?.x ?? 0) +
              ((column.attributes.size?.width ?? 0) / 2 - shelf.attributes.width / 2),
          )
        }
        // right snap
        shelfSnapLocations.push(
          (column.attributes.position?.x ?? 0) +
            ((column.attributes.size?.width ?? 0) - shelf.attributes.width),
        )
      }

      if (column.uprights != null) {
        let startX = column.attributes.position?.x ?? 0
        for (let i = 0; i < column.uprights?.length; i++) {
          startX = startX + (column.uprights[i]?.width ?? 0)
          if (
            (column.attributes.position?.x ?? 0) + (column.attributes.size?.width ?? 0) >
            (shelf.attributes.size?.width ?? 0) + startX
          ) {
            shelfSnapLocations.push(startX)
          }
        }
      }
    } else {
      shelfSnapLocations.push(column.attributes.position?.x ?? 0)
    }

    return shelfSnapLocations
  }

  snapShelfToColumn(shelf: joint.dia.Element, column: joint.dia.Element, stand: Stand): number {
    // swap back to commented getShelfXLocation function to remove snapping functionality
    let i: number
    let min: number
    let index: number = 0
    let n: number
    let offSet = 0
    if (stand.layoutStyle == 2)
      offSet = Math.floor(
        (stand.width - stand.horizontalPitchSize * stand.horizontalPitchCount) / 2,
      )

    // function updated 16/08/2015. Remove x snapping for open sell. Previously Open Sell was included (2) in first 'if' block
    // REMOVED: || myStandTypeId == 2

    //if (shelf.attributes.shelfInfo.shelfTypeId == "Base Shelf")
    //{

    //}
    //else
    if (stand.parentStandTypeId == 7) {
      // MIH
      // ..............................................
      if (stand.layoutStyle == 1 || stand.layoutStyle == 3) {
        // COLUMN LAYOUT
        const positionList = this.createShelfSnapLocations(
          shelf,
          column as planmatr.Column,
          true,
          false,
          false,
        ) // shelfWidth:Number, midSnapNeeded:Boolean = true, onlyLeftSnap:Boolean = false, ignoreShelfWidth:Boolean = false
        min = 1000 // just a high number at which to begin

        for (i = 0; i < positionList.length; i++) {
          const currPos = positionList[i]
          if (currPos != null) {
            n = Math.abs(currPos - (shelf.attributes.position?.x ?? 0))
            if (n < min) {
              min = n
              index = i
            }
          }
        }
        return positionList[index] ?? 0
      }
      // ..............................................
      else if (stand.layoutStyle == 2) {
        // PITCH LAYOUT
        let shelfPosX = shelf.attributes.position?.x ?? 0
        //ensure that the shelf does not exceed the merch space bounds
        if (shelfPosX + shelf.attributes.width > offSet + stand.merchWidth) {
          //move shelf so it's inside the merch area
          shelfPosX = offSet + stand.merchWidth - shelf.attributes.width
        }
        if (shelfPosX < offSet) {
          //move shelf so it's inside the merch area
          shelfPosX = offSet
        }

        return (
          offSet + Math.floor(shelfPosX / stand.horizontalPitchSize) * stand.horizontalPitchSize
        )
      }
    } else if (stand.parentStandTypeId == 1) {
      // WALL UNIT  (GTS [4] moved to be with Open Sell [2] below 02/09/2015
      // ..............................................
      if (stand.layoutStyle == 1 || stand.layoutStyle == 3) {
        // COLUMN LAYOUT & COLUMN/PITCH LAYOUT
        //createXSnapLocations(shelfWidth);
        const positionList = this.createShelfSnapLocations(
          shelf,
          column as planmatr.Column,
          true,
          false,
          false,
        ) // shelfWidth:Number, midSnapNeeded:Boolean = true, onlyLeftSnap:Boolean = false, ignoreShelfWidth:Boolean = false
        min = 10000 // just a high number at which to begin
        //var index:int;
        for (i = 0; i < positionList.length; i++) {
          const currPos = positionList[i]
          if (currPos != null) {
            n = Math.abs(currPos - (shelf.attributes.position?.x ?? 0))
            if (n < min) {
              min = n
              index = i
            }
          }
        }
        return positionList[index] ?? 0
      }
      // ..............................................
      else if (stand.layoutStyle == 2) {
        // PITCH LAYOUT
        let shelfPosX = shelf.attributes.position?.x ?? 0
        //ensure that the shelf does not exceed the merch space bounds
        if (shelfPosX + shelf.attributes.width > offSet + stand.merchWidth) {
          //move shelf so it's inside the merch area
          shelfPosX = offSet + stand.merchWidth - shelf.attributes.width
        }
        if (shelfPosX < offSet) {
          //move shelf so it's inside the merch area
          shelfPosX = offSet
        }

        return Math.floor(shelfPosX / stand.horizontalPitchSize) * stand.horizontalPitchSize
      }
    } else if (stand.parentStandTypeId == 2 || stand.parentStandTypeId == 4) {
      // OPEN SELL & GTS
      // ..............................................
      if (stand.layoutStyle == 3) {
        // COLUMN/PITCH LAYOUT
        //createXSnapLocations(shelfWidth, false);
        const positionList = this.createShelfSnapLocations(
          shelf,
          column as planmatr.Column,
          true,
          false,
          false,
        ) // shelfWidth:Number, midSnapNeeded:Boolean = true, onlyLeftSnap:Boolean = false, ignoreShelfWidth:Boolean = false
        min = 10000 // just a high number at which to begin
        //var index:int;
        for (i = 0; i < positionList.length; i++) {
          const currPos = positionList[i]
          if (currPos != null) {
            n = Math.abs(currPos - (shelf.attributes.position?.x ?? 0))
            if (n < min) {
              min = n
              index = i
            }
          }
        }

        return positionList[index] ?? 0
        //if (isWithinColumn(xPos, shelfWidth)) {
        //  if (min < 5) {
        //    return (positionList[index]);
        //  }
        //  else {
        //    return Math.round(xPos);
        //  }
        //}
        //else {
        //  return (positionList[index]);
        //}
      } else if (stand.layoutStyle == 1) {
        // COLUMN LAYOUT
        // SHELVES CAN SIT ANYWHERE ON THE X PLANE BUT INSIDE COLUMNS
        //check shelf does not overlap column
        let shelfPosX = shelf.attributes.position?.x ?? 0
        const columnPosX = column.attributes.position?.x ?? 0
        const columnWidth = column.attributes.size?.width ?? 0
        const shelfWidth = shelf.attributes.size?.width ?? 0
        if (shelfPosX >= columnPosX) {
          if (shelfPosX + shelfWidth > columnPosX + columnWidth) {
            shelfPosX = columnPosX + columnWidth - shelfWidth
          }
        } else {
          shelfPosX = columnPosX
        }
        return Math.round(shelfPosX)
      }
      // ..............................................
      else if (stand.layoutStyle == 2) {
        // PITCH LAYOUT
        let shelfPosX = shelf.attributes.position?.x ?? 0
        const shelfWidth = shelf.attributes.size?.width ?? 0
        //ensure that the shelf does not exceed the merch space bounds
        if (shelfPosX + shelfWidth > offSet + stand.merchWidth) {
          //move shelf so it's inside the merch area
          shelfPosX = offSet + stand.merchWidth - shelfWidth
        }
        if (shelfPosX < offSet) {
          //move shelf so it's inside the merch area
          shelfPosX = offSet
        }
        return Math.floor(shelfPosX / stand.horizontalPitchSize) * stand.horizontalPitchSize
      }
    } else if (stand.parentStandTypeId == 3 || stand.parentStandTypeId == 5) {
      // BERGERIE || NEO
      // ROW LAYOUT ONLY SO SHELVES CAN SIT ANYWHERE ON THE X PLANE
      return Math.round(shelf.attributes.position?.x ?? 0)
    }
    return 0
  }

  getStartYPos(stand: Stand) {
    //here we need to check if the merch overlaps the header
    //check stand height values match - seems to be some discrepancy between the height and the header/footer/merch heights.
    //here we need to overlap the header with the merch space - so minus the header by the overlap amount.
    let headerSubtract = 0
    if (stand.headerHeight + stand.merchHeight + stand.footerHeight > stand.height) {
      //self.stand.height = self.stand.headerHeight + self.stand.merchHeight + self.stand.footerHeight;
      headerSubtract = stand.headerHeight + stand.merchHeight + stand.footerHeight - stand.height
    }
    let startYPos = (stand.height - stand.merchHeight) / 2
    let uprStartYPos = startYPos
    if (stand.headerHeight + stand.footerHeight > 0) {
      startYPos = Math.round(stand.headerHeight - headerSubtract)
      uprStartYPos = Math.round(stand.headerHeight)
      //var startYPos = Math.round((self.stand.height - self.stand.headerHeight) / 2);
    }
    return uprStartYPos
  }

  getPitchArray(merchSpace: number, pitch: number, startYPos: number) {
    //returns an array of pitches that can be used in the merch space

    const pitchArray = Array.from(
      { length: merchSpace + startYPos },
      (_, i) => i * pitch + startYPos,
    )

    return pitchArray
  }

  findClosestIndex(arr: number[], element: number): number {
    let from = 0,
      until = arr.length - 1
    while (true) {
      const cursor = Math.floor((from + until) / 2)
      if (cursor === from) {
        const fromIndex = arr[from] ?? 0
        const diff1 = element - fromIndex
        const diff2 = (arr[until] ?? 0) - element
        return diff1 <= diff2 ? from : until
      }

      const found = arr[cursor] ?? 0
      if (found === element) return cursor

      if (found > element) {
        until = cursor
      } else if (found < element) {
        from = cursor
      }
    }
  }

  getNearestPitch(
    position: number,
    pitch: number,
    stand: Stand,
    shelfHeight: number,
    partType: number,
  ) {
    //footerposition
    const realPosition = position + shelfHeight //to account for shelves corner position being top left corner, not bottom left
    // let fy = stand.height - stand.footerHeight; // + stand.headerHeight;
    //discover the unusable space
    const merchspace = stand.height - stand.footerHeight - stand.headerHeight
    const avaialableSpace = Math.floor(merchspace / pitch) * pitch
    const unusableSpace = merchspace - avaialableSpace

    //here we need to check if the merch overlaps the header - this is a hack that was created when the header was added to the merch space - and avoided costly changes to the backend
    //check stand height values match - seems to be some discrepancy between the height and the header/footer/merch heights.
    //here we need to overlap the header with the merch space - so minus the header by the overlap amount.
    // let headerSubtract = 0;
    // if (stand.headerHeight + stand.merchHeight + stand.footerHeight > stand.height) {
    //   headerSubtract = ((stand.headerHeight + stand.merchHeight + stand.footerHeight) - stand.height);
    // }

    const newPitch = stand.shelfIncrement == 0 ? 1 : stand.shelfIncrement //needs to come from standInfo -- don't think we're using this
    //taking into account header height and unusable space doesn't work for wall pitch layout
    const nearestPitch = Math.ceil(realPosition / pitch) * pitch
    //workout if we are snapping on 5 or 10 rounded numbers for the dots
    // const is5or10 = (Math.round((merchspace + stand.headerHeight)/5) * 5) % 10;

    if (
      stand.layoutStyle == StandLayoutEnum.ColumnPitch &&
      (stand.parentStandTypeId == 1 || stand.parentStandTypeId == 7)
    ) {
      // not working for this one.
      // if (headerSubtract > 0 && pitch == 10) { //if the header overlaps the merch space - assuming the pitch is 10mm
      //   var nearestPitchMultiple = (Math.round((nearestPitch)/5) * 5) % 10;
      //   if (nearestPitchMultiple !== is5or10 && is5or10 > 0) {
      //     nearestPitch = nearestPitch + is5or10; //add the 5 to the pitch if the merch height is a multiple of 5
      //   }

      //   if (nearestPitch - realPosition >= pitch) {
      //     nearestPitch = nearestPitch - pitch; //if the pitch is above the real position, then we need to snap down
      //   }
      // }
      // else {
      //     //nearestPitch = (Math.floor(realPosition / pitch) * pitch) + unusableSpace;
      //     nearestPitch = Math.floor((realPosition / pitch) * pitch)
      // }

      const uprStartYPos = this.getStartYPos(stand)
      const pitchArray = this.getPitchArray(
        merchspace + uprStartYPos + unusableSpace,
        pitch,
        uprStartYPos + unusableSpace,
      )

      const newNearestPitch = this.findClosestIndex(pitchArray, realPosition) //add the unusable space to the pitch
      //return nearestPitch - shelfHeight;
      const foundPitch = pitchArray[newNearestPitch] ?? 0
      return foundPitch - shelfHeight
    }
    if (
      stand.layoutStyle == StandLayoutEnum.Pitch &&
      (stand.parentStandTypeId == 1 || stand.parentStandTypeId == 7)
    ) {
      // if (headerSubtract > 0) {
      //   if (partType == PartTypes.BaseShelf) {
      //     nearestPitch = (Math.floor(realPosition / pitch) * pitch) + stand.headerHeight + unusableSpace - headerSubtract;
      //     return nearestPitch - shelfHeight;
      //   }
      //   else {
      //     nearestPitch = (Math.floor(realPosition / pitch) * pitch) + unusableSpace;
      //     return nearestPitch - shelfHeight;
      //   }
      // }
      // else {
      //   nearestPitch = (Math.floor(realPosition / pitch) * pitch) + unusableSpace;
      //   return nearestPitch - shelfHeight;
      // }

      const uprStartYPos = this.getStartYPos(stand)
      const pitchArray = this.getPitchArray(
        merchspace + uprStartYPos + unusableSpace,
        newPitch,
        uprStartYPos + unusableSpace,
      )

      const newNearestPitch = this.findClosestIndex(pitchArray, realPosition) //add the unusable space to the pitch
      //return nearestPitch - shelfHeight;
      return (pitchArray[newNearestPitch] ?? 0) - shelfHeight
    } else {
      const newRealPosition = (nearestPitch - shelfHeight) / newPitch
      return Math.ceil(newRealPosition * newPitch)
    }
  }

  getNearestRow(headerHeight: number, position: number, rowList: Row[]) {
    let rowPosition = headerHeight

    for (let i = 0; i < rowList.length; i++) {
      const rowHeight = rowList[i]?.height ?? 0
      rowPosition = rowPosition + rowHeight
      if (rowPosition > position) break
    }
    return rowPosition
  }

  // Get the sku list
  async exportSku(planogramId: number) {
    // const params: GetMenuParams = new GetMenuParams();
    // params.planogramId = planogramId;

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }

    const response = await apiClient
      .get('/GetSkuList', { params: { id: planogramId } })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  }

  // unlock the planogram
  async unlock(planogramId: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }

    const response = await apiClient
      .get('/UnLock', { params: { id: planogramId } })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  }

  // Save the planogram cassettes
  async saveCassettes(planogramInfo: PlanogramInfo) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }

    const response = await apiClient
      .post('/SavePlanogramCassettes', planogramInfo)
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  }

  // Get the data to create the menu
  async savePlanogram(planogramInfo: PlanogramInfo) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms/edit'
    const response = await apiClient
      .post('/SavePlanogram', planogramInfo)
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  }

  // // Get the data to create the menu
  // savePlanogramV2(planogramInfo: PlanogramInfo) {
  // if (token.value) {
  //   apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
  // }

  // const response = await apiClient
  //   .post('/SavePlanogram', planogramInfo)
  //   .then((res) => {
  //     return res.data
  //   })
  //   .catch((error) => {
  //     throw error
  //   })
  // return response

  //   return $.ajax({
  //       type: "POST",
  //       url: '/umbraco/api/planmatrapi/SavePlanogramV2',
  //       data: JSON.stringify(data),
  //       contentType: "application/json"
  //     }).done((data: unknown) => data)
  //     .fail((data: unknown) => data);

  // }

  // Get the data to create the menu
  async saveCluster(planogramInfo: PlanogramInfo) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/clusters/'
    const response = await apiClient
      .post('/SaveCluster', planogramInfo)
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
    // return $.ajax({
    //     type: "POST",
    //     url: '/api/v2/planmatr/cluster/save-cluster',
    //     data: JSON.stringify(data),
    //     contentType: "application/json"
    //   }).done((data: unknown) => data)
    //   .fail((data: unknown) => data);
  }

  // Check if the planogram is locked
  async getPlanogramLock(id: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    // apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/clusters/edit';
    apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms'
    const response = await apiClient
      .get('/getPlanoLock', { params: { planogramId: id } })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        ;``
        throw error
      })
    return response
  }

  // Get the data to create the menu
  async getPlanogramComCount(planogramId: number, brandId: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    // apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/clusters/edit';
    apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms'
    const response = await apiClient
      .get('/getCommentCount', { params: { planogramId: planogramId, brandId: brandId } })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response

    // return $.ajax({
    //   type: "POST",
    //   url: '/umbraco/api/planmatrapi/getPlanoComCount',
    //   data: JSON.stringify(params),
    //   contentType: "application/json"
    // }).done((data: unknown) => data)
    //   .fail((data: unknown) => data);
  }

  // Save the pdf image of the planogram
  async exportPDF(svg: string, planogramId: number) {
    const planoSvg = new PlanogramSvg()
    planoSvg.planogramId = planogramId
    planoSvg.image = svg

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    // apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/clusters/edit';
    apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms/edit'
    const response = await apiClient
      .post('/getPlanoPDF', planoSvg)
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  }

  // Save the svg image of the planogram
  // async savePlanogramSvg(svg: string, planogramId: number) {
  //     const planoSvg = new PlanogramSvg();
  //     planoSvg.planogramId = planogramId;
  //     planoSvg.image = svg;

  //     if (token.value) {
  //         apiClient.defaults.headers.Authorization = `Bearer ${token.value}`;
  //     }
  //     apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms/edit';

  //     const response = await apiClient
  //         .post('/savePlanogramSvg', planoSvg)
  //         .then((res) => {
  //             return res.data;
  //         })
  //         .catch((error) => {
  //             throw error;
  //         });
  //     return response;
  // }

  // Save the svg image of the planogram
  async savePlanogramSnapshot(planogramSvg: PlanogramSvg) {
    const planoJpeg = planogramSvg
    var formData = new FormData()
    formData.append('planogramId', planoJpeg.planogramId.toString())
    formData.append('image', planoJpeg.image)
    // formData.append('userId', planoJpeg.userId.toString());
    // formData.append('userName', planoJpeg.userName);

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    // apiClient.defaults.headers['Content-Type'] = 'multipart/form-data';

    apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms/edit'
    const response = await apiClient
      .post('/savePlanogramJpeg', formData)
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  }

  async getPlanogramPreview(planogramId: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms'
    const response = await apiClient
      .get('/getPlanogramPreview', { params: { planogramId: planogramId } })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  }

  // Get the data to create the menu
  async getScratchPad(planogramId: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms/edit'

    const params = { planogramId: planogramId }
    const response = await apiClient
      .get('/GetPlanogramScratchPad', { params })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  }

  toggleScratchPad(
    paper: joint.dia.Paper,
    graph: joint.dia.Graph,
    hide: boolean,
    currentView: CurrentView,
  ) {
    const allElems = graph.getElements()
    const scratchParts = allElems.filter(function (el: joint.dia.Element) {
      if (el.attributes.planogramInfo != null) {
        return (
          el.attributes.planogramInfo.scratchPadId !== null &&
          el.attributes.planogramInfo.scratchPadId != 0
        )
      } else {
        return null
      }
    })

    for (let i = 0; i < scratchParts.length; i++) {
      if (scratchParts[i] != null) {
        if (hide) {
          scratchParts[i]?.attr('./display', 'none', { ignoreCommandManager: true })
        } else {
          scratchParts[i]?.attr('./display', 'block', { ignoreCommandManager: true })
        }
      }
    }

    if (!hide) {
      if (currentView == CurrentView.shade) {
        this.planogramRenderService.renderShadeView(scratchParts, paper)
      }

      if (currentView == CurrentView.render) {
        this.planogramRenderService.renderRenderView(scratchParts, paper)
      }
    }
  }

  // Get the data to create the menu
  async saveScratchPad(shelfInfoList: ShelfInfoList) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    apiClient.defaults.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms/edit'

    const response = await apiClient
      .post('/SaveScratchpad', shelfInfoList)
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  }

  setModelsUnderneath(cell: joint.dia.Element): void {
    const self = this
    self.modelsUnderneath = self.graph.findModelsUnderElement(cell)
  }

  isOnCarcass(cell: joint.dia.Element, bbox: g.Rect): ElementPosition {
    //Not sure why the partial is allowed here (need to investigate further - possibly it's to allow shelves to placed on the edge exactly?
    const self = this
    const carcass = this.graph.findElementsInArea(bbox).filter(function (el) {
      return el.attributes.type == 'planmatr.Carcass'
    })

    if (carcass.length > 0) {
      const thisCarcass = carcass[0] as joint.dia.Element
      if (self.isInCarcassSpace(thisCarcass, cell)) {
        return ElementPosition.Inside
      } else {
        return ElementPosition.Partial
      }
    } else {
      return ElementPosition.Outside
    }
  }

  isOnCarcassNew(cell: joint.dia.Element): ElementPosition {
    const self = this
    const carcass = self.modelsUnderneath.filter(function (el) {
      return (el as joint.dia.Element).attributes.type == 'planmatr.Carcass'
    })

    if (carcass.length > 0) {
      const thisCarcass = carcass[0] as joint.dia.Element
      if (self.isInCarcassSpace(thisCarcass, cell)) {
        return ElementPosition.Inside
      } else {
        return ElementPosition.Partial
      }
    } else {
      return ElementPosition.Outside
    }
  }

  isInCarcassSpace(carcass: joint.dia.Element, cell: joint.dia.Element): boolean {
    //var position = cell.attributes.position;

    const merchbbox = carcass.getBBox()

    //if (!cell.getBBox().intersect(merchbbox)) {
    if (!merchbbox.containsRect(cell.getBBox())) {
      //the cell is in the header or the footer
      return false
    } else {
      return true
    }
  }

  isInMerchSpace(cell: joint.dia.Element, bbox: joint.dia.BBox): boolean {
    const carcass = this.modelsUnderneath.filter(function (el) {
      return (el as joint.dia.Element).attributes.type == 'planmatr.Carcass'
    })
    if (carcass.length > 0) {
      const merchbbox = (carcass[0] as joint.dia.Element).getBBox()
      const carcassAttrs = (carcass[0] as joint.dia.Element).attributes
      if (carcassAttrs?.size) {
        merchbbox.height = carcassAttrs.merchHeight
        merchbbox.width = carcassAttrs.merchWidth
        merchbbox.y =
          0 + carcassAttrs.size.height - (carcassAttrs.footerHeight + carcassAttrs.merchHeight)
        merchbbox.x = 0 + (carcassAttrs.size.width - carcassAttrs.merchWidth) / 2
      }

      //if (!cell.getBBox().intersect(merchbbox)) {
      if (!merchbbox.containsRect(cell.getBBox())) {
        //the cell is in the header or the footer
        return false
      } else {
        return true
      }
    } else {
      return false
    }
  }

  // isInMerchSpaceNew(cell: joint.dia.Element, bbox: joint.dia.BBox): boolean {
  //   const carcass = this.graph.findElementsInArea(bbox).filter(function (el) {
  //     return (el.attributes.type == 'planmatr.Carcass');
  //   });
  //   if (carcass.length > 0) {

  //     const merchbbox = (carcass[0] as joint.dia.Element).getBBox();
  //     const carcassAttrs = (carcass[0] as joint.dia.Element).attributes;
  //     if (carcassAttrs?.size) {
  //       merchbbox.height = carcassAttrs.merchHeight;
  //       merchbbox.width = carcassAttrs.merchWidth;
  //       merchbbox.y = 0 + carcassAttrs.size.height - (carcassAttrs.footerHeight + carcassAttrs.merchHeight);
  //       merchbbox.x = 0 + ((carcassAttrs.size.width - carcassAttrs.merchWidth) / 2);
  //     }

  //     //if (!cell.getBBox().intersect(merchbbox)) {
  //     if (!merchbbox.containsRect(cell.getBBox())) {
  //       //the cell is in the header or the footer
  //       return false;
  //     }
  //     else {
  //       return true;
  //     }

  //   }
  //   else {
  //     return false;
  //   }

  // }

  remmoveAssociatedElements(
    elementsUnderneath: joint.dia.Element[],
    cell: joint.dia.Element,
  ): joint.dia.Element[] {
    //remove any elements underneath the shelf that are associated with the shelf
    if (elementsUnderneath.length > 0) {
      {
        //check to see if the elements underneath are parts that belong to the shelf
        const result = elementsUnderneath.find(function (el) {
          if (el.attributes.type == 'planmatr.Part.Shelf') {
            if (cell.attributes.type == 'planmatr.Part.Shelf') {
              return el
            } else {
              return (
                el.attributes.shelfInfo.planogramShelfId !=
                cell.attributes.shelfInfo.planogramShelfId
              )
            }
          } else if (el.attributes.type == 'planmatr.Part.Cassette') {
            return (
              el.attributes.partInfo.planogramShelfId != cell.attributes.shelfInfo.planogramShelfId
            )
          } else {
            return []
          }
        })
        if (!result) {
          elementsUnderneath = []
        }
      }
    }
    return elementsUnderneath
  }

  renderShadeViewSingle(cassette: joint.dia.Element) {
    if (cassette.attributes.partInfo.statusId == 0) {
      const statusColour = ShadeStatusColourEnum[cassette.attributes.partInfo.statusId]
      cassette.attr('.body/fill-opacity', 0)
      cassette.attr('.body/stroke', statusColour)
    } else {
      cassette.attr('.body/fill-opacity', 0.2)
      const statusColour = ShadeStatusColourEnum[cassette.attributes.partInfo.statusId]
      cassette.attr('.body/stroke', statusColour)
      //cassettes[i].attr(".body/fill", statusColour);
    }
    cassette.attr('image/xlink:href', '')
    if (
      cassette.attributes.partInfo.partTypeId != PartTypes.Blanking &&
      cassette.attributes.partInfo.partTypeId != PartTypes.FasciaPlate
    ) {
      const facingHtml = this.planogramRenderService.generateFacingHtml(cassette)
      cassette.attr('.facings/html', facingHtml)
    }

    if (cassette.attributes.partInfo.svgLineGraphic != null) {
      if (cassette.attributes.partInfo.partTypeId == PartTypes.FasciaPlate) {
        const graphicSvg = cassette.attributes.partInfo.svgLineGraphic.substring(
          cassette.attributes.partInfo.svgLineGraphic.indexOf('<svg'),
        )
        const markup = cassette.attributes.markup?.toString()
        const startString = markup?.substring(0, markup.indexOf('<svg class="cSvg'))
        const endString = markup?.substring(markup.indexOf('<rect class="body'))
        const cassetteMarkup = startString + graphicSvg + endString
        cassette.attributes.markup = cassetteMarkup
      } else {
        const cassetteImageAttr = cassette.attributes.attrs?.image
        if (cassetteImageAttr != null) {
          cassetteImageAttr['xlink:href'] =
            'data:image/svg+xml;utf8,' +
            encodeURIComponent(cassette.attributes.partInfo.svgLineGraphic)
        }
      }
      const cellView = cassette.findView(this.paper)
      cellView.render()
    }
  }

  renderRenderViewSingle(cassette: joint.dia.Element) {
    if (cassette.attributes.partInfo.statusId == 0) {
      const statusColour = ShadeStatusColourEnum[cassette.attributes.partInfo.statusId]
      cassette.attr('.body/fill-opacity', 0)
      cassette.attr('.body/stroke', statusColour)
    } else {
      cassette.attr('.body/fill-opacity', 0.2)
      const statusColour = ShadeStatusColourEnum[cassette.attributes.partInfo.statusId]
      cassette.attr('.body/stroke', statusColour)
      //cassettes[i].attr(".body/fill", statusColour);
    }
    cassette.attr('image/xlink:href', '')
    if (
      cassette.attributes.partInfo.partTypeId != PartTypes.Blanking &&
      cassette.attributes.partInfo.partInfo.partTypeId != PartTypes.FasciaPlate
    ) {
      const facingHtml = this.planogramRenderService.generateFacingHtml(cassette)
      cassette.attr('.facings/html', facingHtml)
    }

    if (cassette.attributes.partInfo.svgLineGraphic != null) {
      if (cassette.attributes.partInfo.partTypeId == PartTypes.FasciaPlate) {
        const graphicSvg = cassette.attributes.partInfo.svgLineGraphic.substring(
          cassette.attributes.partInfo.svgLineGraphic.indexOf('<svg'),
        )
        const markup = cassette.attributes.markup?.toString()
        const startString = markup?.substring(0, markup.indexOf('<svg class="cSvg'))
        const endString = markup?.substring(markup.indexOf('<rect class="body'))
        const cassetteMarkup = startString + graphicSvg + endString
        cassette.attributes.markup = cassetteMarkup
      } else {
        const cassetteImageAttr = cassette.attributes.attrs?.image
        if (cassetteImageAttr != null) {
          cassetteImageAttr['xlink:href'] =
            'data:image/svg+xml;utf8,' +
            encodeURIComponent(cassette.attributes.partInfo.svgLineGraphic)
        }
      }
      const cellView = cassette.findView(this.paper)
      cellView.render()
    }
  }

  setProperty(
    path: string,
    value: g.Point,
    opt: { rewrite?: boolean },
    model: joint.dia.Element,
  ): void {
    opt = opt || {}

    // The model doesn't have to be a JointJS cell necessarily. It could be
    // an ordinary Backbone.Model and such would have no method 'prop'.
    const prop = joint.dia.Cell.prototype.prop
    //var model = this.cassette;
    const overwrite = opt.rewrite || false

    if (value === undefined) {
      // Method prop can't handle undefined values in right way.
      // The model attributes would stay untouched if try to
      // set a nested property to undefined.
      joint.dia.Cell.prototype.removeProp.call(model, path, opt)
    } else {
      let updated

      if (joint.util.isObject(value) && !overwrite) {
        const current = prop.call(model, path, opt)
        const targetType = Array.isArray(value) ? [] : {}
        updated = joint.util.merge(targetType, current, value)
      } else {
        updated = joint.util.clone(value)
      }

      //if (overwrite) opt.rewrite = true;
      prop.call(model, path, updated, opt)
    }
  }

  orderOverlays(graph: joint.dia.Graph, partTypeId: number) {
    const allElems = graph.getElements()
    const overlayParts = allElems.filter(function (el: joint.dia.Element) {
      if (el.attributes.partInfo != null) {
        return el.attributes.partInfo.partTypeId == partTypeId
      } else {
        return null
      }
    })

    for (let i = 0; i < overlayParts.length; i++) {
      overlayParts[i]?.toFront({ ignoreCommandManager: true })
    }
    // for (var i = 0; i < overlayParts.length; i++) {
    //   if (overlayParts[i].attributes.partInfo.partTypeId == PartTypes.Blanking) {
    //     overlayParts[i].toFront();
    //   }
    // }
  }

  orderAllOverlays(graph: joint.dia.Graph) {
    const allElems = graph.getElements()
    const overlayParts = allElems.filter(function (el: joint.dia.Element) {
      if (el.attributes.partInfo != null) {
        return (
          el.attributes.partInfo.partTypeId == PartTypes.Blanking ||
          el.attributes.partInfo.partTypeId == PartTypes.Glorifier ||
          el.attributes.partInfo.partTypeId == PartTypes.FasciaPlate
        )
      } else {
        return null
      }
    })

    for (let i = 0; i < overlayParts.length; i++) {
      if (overlayParts[i]?.attributes.partInfo.partTypeId == PartTypes.Glorifier) {
        overlayParts[i]?.toFront({ ignoreCommandManager: true })
      }
    }
    for (let i = 0; i < overlayParts.length; i++) {
      if (overlayParts[i]?.attributes.partInfo.partTypeId == PartTypes.FasciaPlate) {
        overlayParts[i]?.toFront({ ignoreCommandManager: true })
      }
    }
    for (let i = 0; i < overlayParts.length; i++) {
      if (overlayParts[i]?.attributes.partInfo.partTypeId == PartTypes.Blanking) {
        overlayParts[i]?.toFront({ ignoreCommandManager: true })
      }
    }
  }

  orderCellOverlays(cell: joint.dia.Cell) {
    const self = this
    cell.toFront({ deep: true, ignoreCommandManager: true })
    let fpOverlays: joint.dia.Element[] = []
    const overlays = this.graph.findElementsInArea(cell.getBBox()).filter(function (
      el: joint.dia.Element,
    ) {
      if (el.attributes.type == 'planmatr.Part.Cassette') {
        return (
          el.attributes.partInfo.partTypeId == PartTypes.Blanking ||
          el.attributes.partInfo.partTypeId == PartTypes.Glorifier ||
          el.attributes.partInfo.partTypeId == PartTypes.FasciaPlate
        )
      } else {
        return null
      }
    })
    for (let i = 0; i < overlays.length; i++) {
      const overlay = overlays[i]
      if (overlay != null) {
        if (overlay?.attributes.partInfo.partTypeId == PartTypes.FasciaPlate) {
          fpOverlays = this.graph.findElementsInArea(overlay?.getBBox()).filter(function (
            el: joint.dia.Element,
          ) {
            if (el.attributes.type == 'planmatr.Part.Cassette') {
              return el.attributes.partInfo.partTypeId == PartTypes.Blanking
            } else {
              return null
            }
          })
        }
        overlay.toFront({ ignoreCommandManager: true })
        self.blankingGraphicsToFront(fpOverlays)
      }
      self.blankingGraphicsToFront(overlays)
    }
  }

  blankingGraphicsToFront(overlays: joint.dia.Element[]) {
    for (let i = 0; i < overlays.length; i++) {
      const overlay = overlays[i]
      if (overlay != null) {
        if (overlay.attributes.partInfo.partTypeId === PartTypes.Blanking) {
          overlay.toFront({ ignoreCommandManager: true })
        }
      }
    }
  }

  async initialise() {
    const authStore = useAuthStore()
    if (!authStore.initialized) {
      await authStore.initialize()
    }
    const t = await Auth.getToken()
    token.value = t
    const idT = await Auth.getIdToken()
    idToken.value = idT
    initialized.value = true
  }
}
