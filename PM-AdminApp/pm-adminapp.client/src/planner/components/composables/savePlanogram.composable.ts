import * as appShapes from '@/planner/models/shapes/planmatr-shapes'
import { PlanogramService } from '@/planner/services/planogram-service'
import * as joint from '@joint/plus'

import { AppMode, CurrentView } from '@/planner/models/Enumerations'
import { PartFacing } from '@/planner/models/PartFacing'
import { PartInfo } from '@/planner/models/PartInfo'
import { PlanogramInfo } from '@/planner/models/PlanogramInfo'
import { PlanogramSvg } from '@/planner/models/PlanogramSvg'
import { ShelfInfoList } from '@/planner/models/ShelfInfoList'
import type { Stand } from '@/planner/models/Stand'
import { MenuService } from '@/planner/services/menu.service'
import { ref } from 'vue'

// const planogramService = ref<PlanogramService | null>(null);
// const menuService = ref<MenuService | null>(null);
// const commandManager = ref<joint.dia.CommandManager | null>(null);
const graph = ref<joint.dia.Graph | null>(null)
// const paper = ref<joint.dia.Paper | null>(null);
// const cassette = ref<joint.dia.Cell | null>(null);
const planogramShelves = ref<PartInfo[] | null>(null)
const planogramParts = ref<PartInfo[] | null>(null)
const scratchPad = ref<PartInfo[] | null>(null)

const scratchPadHidden = ref<boolean>(false)
const currentView = ref<number>()
const saving = ref<boolean>(false)
const finishedSave = ref<{ planogramId: number; clusterId: number } | null>(null)
const showScratchPad = ref<boolean>(false)
const isCluster = ref<boolean>(false)
// const planogramId = ref<number>(0);
// const clusterId = ref<number>(0);

export function useSavePlanogram(
  appMode: AppMode,
  brandId: number,
  countryId: number,
  // planogramService: PlanogramService,
  // graph: joint.dia.Graph,
  // paper: joint.dia.Paper,
  // isCluster: boolean
  // stand: Stand,
  // standShape: appShapes.planmatr.Carcass,
) {
  async function savePlanogram(
    planogramName: string,
    planogramId: number,
    clusterId: number,
    currentViewParam: number,
    commandManager: joint.dia.CommandManager,
    paper: joint.dia.Paper,
    graph: joint.dia.Graph,
    standShape: appShapes.planmatr.Carcass,
    stand: Stand,
    partOverlap: boolean,
    partOverlapAmount: number,
  ) {
    isCluster.value = appMode === AppMode.Cluster
    currentView.value = currentViewParam
    const planogramService = new PlanogramService(
      isCluster.value,
      partOverlap,
      partOverlapAmount,
      graph,
      paper,
    )
    planogramService.initialise()
    planogramService.toggleScratchPad(paper, graph, true, currentView.value)

    //show spinner
    saving.value = true

    //first hide the scratchpad
    const wasAlreadyHidden = scratchPadHidden.value
    showScratchPad.value = !scratchPadHidden.value

    const allElems = graph.getElements()
    //allElems[0].attributes.type
    // var carcass = _.filter(allElems, function(el) { return el.attributes.type === 'planmatr.Carcass'; });
    // var planoParts = carcass[0].getEmbeddedCells();
    const cassettes = allElems.filter(function (el) {
      return (
        (el.attributes.type === 'planmatr.Part.Cassette' &&
          el.attributes.planogramInfo.scratchPadId === 0) ||
        (el.attributes.type === 'planmatr.Part.Cassette' &&
          el.attributes.planogramInfo.scratchPadId === null)
      )
    })

    const shelves = allElems.filter(function (el) {
      return (
        (el.attributes.type === 'planmatr.Part.Shelf' &&
          el.attributes.planogramInfo.scratchPadId === 0) ||
        (el.attributes.type === 'planmatr.Part.Shelf' &&
          el.attributes.planogramInfo.scratchPadId === null)
      )
    })

    const scratchParts = allElems.filter(function (el) {
      if (el.attributes.planogramInfo != null) {
        return (
          el.attributes.type === 'planmatr.Part.Cassette' &&
          el.attributes.planogramInfo.scratchPadId !== 0 &&
          el.attributes.type === 'planmatr.Part.Cassette' &&
          el.attributes.planogramInfo.scratchPadId !== null
        ) // || el.attributes.type === 'planmatr.Part.Cassette' && el.attributes.planogramInfo.scratchPadId !== null && el.attributes.planogramInfo.scratchPadId !== 0;
      } else {
        return null
      }
    })

    const scratchShelves = allElems.filter(function (el) {
      if (el.attributes.planogramInfo != null) {
        return (
          el.attributes.type === 'planmatr.Part.Shelf' &&
          el.attributes.planogramInfo.scratchPadId !== 0 &&
          el.attributes.type === 'planmatr.Part.Shelf' &&
          el.attributes.planogramInfo.scratchPadId !== 0
        ) // || el.attributes.type === 'planmatr.Part.Shelf' && el.attributes.planogramInfo.scratchPadId !== null && el.attributes.planogramInfo.scratchPadId !== 0;
      } else {
        return null
      }
    })

    //find any removed items in the undo stack
    // commandManager = commandManager;
    let undoStack = commandManager.undoStack
    let deleteList: any[] = []
    let updateList: any[] = []
    let updateShelfList: any[] = []

    //need to handle remove items separately
    deleteList = getDeleteList(undoStack)

    for (let i = 0; i < undoStack.length; i++) {
      let item = undoStack[i]
      if (Array.isArray(item)) {
        for (let j = 0; j < item.length; j++) {
          if (item[j].action == 'add') {
            if (item[j].data.attributes.type == 'planmatr.Part.Cassette') {
              //Check the item hasn't already been added
              let alreadyAdded = false
              let foundItem = cassettes?.find(function (o: any) {
                return o.id == item[j].data.attributes.id
              })
              if (foundItem != null && foundItem != undefined) {
                if (
                  foundItem.attributes.partInfo.planogramPartId != 0 ||
                  foundItem.attributes.partInfo.clusterPartId != null
                ) {
                  alreadyAdded = true
                }

                let existingUpdateItem = updateList.find(function (o: any) {
                  return o.id == item[j].data.attributes.id
                })
                if (existingUpdateItem != null && existingUpdateItem != undefined) {
                  alreadyAdded = true
                }

                if (!alreadyAdded) {
                  let addedPart = foundItem
                  updateList.push(addedPart)
                }
              }
            } else if (item[j].data.attributes.type == 'planmatr.Part.Shelf') {
              //Check the item hasn't already been added
              let alreadyAdded = false
              let foundItem = shelves.find(function (o: any) {
                return o.id == item[j].data.attributes.id
              })
              if (foundItem != null && foundItem != undefined) {
                if (
                  foundItem.attributes.shelfInfo.planogramShelfId != 0 ||
                  foundItem.attributes.shelfInfo.clusterPartId != null
                ) {
                  alreadyAdded = true
                }

                let existingUpdateItem = updateShelfList.find(function (o: any) {
                  return o.id == item[j].data.attributes.id
                })
                if (existingUpdateItem != null && existingUpdateItem != undefined) {
                  alreadyAdded = true
                }

                if (!alreadyAdded) {
                  let changedShelf = foundItem
                  updateShelfList.push(changedShelf)
                }
              }
            }
          }
          if (
            item[j].action == 'change:partInfo' ||
            item[j].action == 'change:position' ||
            item[j].action == 'change:attrs' ||
            item[j].action == 'change:shelfInfo'
          ) {
            if (item[j].options.propertyPath != 'partInfo/save') {
              if (item[j].data.type == 'planmatr.Part.Cassette') {
                let foundItem = cassettes.find(function (o: any) {
                  return o.id == item[j].data.id
                })
                if (foundItem != null) {
                  let changedPart = foundItem
                  let existingUpdateItem = updateList.find(function (o: any) {
                    return o.id == item[j].data.id
                  })
                  if (existingUpdateItem == null || existingUpdateItem == undefined) {
                    updateList.push(changedPart)
                  }
                }
              } else if (item[j].data.type == 'planmatr.Part.Shelf') {
                let foundItem = shelves.find(function (o: any) {
                  return o.id == item[j].data.id
                })
                if (foundItem != null) {
                  let changedShelf = foundItem
                  let existingUpdateItem = updateShelfList.find(function (o: any) {
                    return o.id == item[j].data.id
                  })
                  if (existingUpdateItem == null || existingUpdateItem == undefined) {
                    updateShelfList.push(changedShelf)
                  }
                }
              }
            }
          }
        }
      } else {
        if (item.action == 'add') {
          if (item.data.attributes.type == 'planmatr.Part.Cassette') {
            //Check the item hasn't already been added
            let alreadyAdded = false
            let foundItem = cassettes.find(function (o: any) {
              return o.id == item.data.attributes.id
            })
            if (foundItem != null && foundItem != undefined) {
              if (
                foundItem.attributes.shelfInfo.planogramPartId != 0 ||
                foundItem.attributes.shelfInfo.clusterPartId != null
              ) {
                alreadyAdded = true
              }

              let existingUpdateItem = updateList.find(function (o: any) {
                return o.id == item.data.attributes.id
              })
              if (existingUpdateItem != null && existingUpdateItem != undefined) {
                alreadyAdded = true
              }

              if (!alreadyAdded) {
                if (item.data.attributes.type == 'planmatr.Part.Cassette') {
                  let addedPart = foundItem
                  updateList.push(addedPart)
                } else {
                  let addedPart = foundItem
                  updateList.push(addedPart)
                }
              }
            }
          } else if (item.data.attributes.type == 'planmatr.Part.Shelf') {
            //Check the item hasn't already been added
            let alreadyAdded = false
            let foundItem = shelves.find(function (o: any) {
              return o.id == item.data.attributes.id
            })
            if (foundItem != null && foundItem != undefined) {
              if (
                foundItem.attributes.partInfo.planogramShelfId != 0 ||
                foundItem.attributes.partInfo.clusterShelfId != null
              ) {
                alreadyAdded = true
              }

              let existingUpdateItem = updateList.find(function (o: any) {
                return o.id == item.data.attributes.id
              })
              if (existingUpdateItem != null && existingUpdateItem != undefined) {
                alreadyAdded = true
              }

              if (!alreadyAdded) {
                let changedShelf = foundItem
                updateShelfList.push(changedShelf)
              }
            }
          }
          continue
        }
        if (
          item.action == 'change:partInfo' ||
          item.action == 'change:position' ||
          item.action == 'change:attrs' ||
          item.action == 'change:shelfInfo'
        ) {
          if (item.options.propertyPath != 'partInfo/save') {
            if (item.data.type == 'planmatr.Part.Cassette') {
              let foundItem = cassettes.find(function (o: any) {
                return o.id == item.data.id
              })
              if (foundItem != null) {
                let changedPart = foundItem
                let existingUpdateItem = updateList.find(function (o: any) {
                  return o.id == item.data.id
                })
                if (existingUpdateItem == null) {
                  updateList.push(changedPart)
                }
              }
            } else if (item.data.type == 'planmatr.Part.Shelf') {
              let foundItem = shelves.find(function (o: any) {
                return o.id == item.data.id
              })
              if (foundItem != null) {
                let changedShelf = foundItem
                let existingUpdateItem = updateShelfList.find(function (o: any) {
                  return o.id == item.data.id
                })
                if (existingUpdateItem == null || existingUpdateItem == undefined) {
                  updateShelfList.push(changedShelf)
                }
              }
            }
          }
        }
        continue
      }
    }

    /// NEED to add non shelf parts to plano update
    let cassetteUpdateList: PartInfo[] = createCassetteInfo(updateList)

    /////////////////////////////////
    //organise shelves from command List
    let shelfUpdateList: any[] = []
    for (let k = 0; k < updateShelfList.length; k++) {
      let shelf = updateShelfList[k].attributes.shelfInfo
      shelf.planmatrShelfId = updateShelfList[k].attributes.id
      shelf.position = updateShelfList[k].attributes.position
      if (updateShelfList[k].attributes.attrs['#label'].text !== null)
        shelf.label = updateShelfList[k].attributes.attrs['#label'].text

      shelfUpdateList.push(shelf)
    }
    /////////////////////////////////

    let shelvesList = new ShelfInfoList(planogramId, shelfUpdateList)

    //now check that we have added all the parts that have been added to the shelves to the shelfUpdateList
    for (let l = 0; l < shelvesList.shelfInfos.length; l++) {
      let shelf = shelvesList.shelfInfos[l]
      //let shelfParts: PartInfo[] = [];
      let shelfParts = cassetteUpdateList.filter(function (o: any) {
        return o.planmatrShelfId == shelf.planmatrShelfId
      })
      shelf.parts = shelfParts
      //_.pullAll(cassetteUpdateList, shelfParts);
      for (let m = 0; m < shelfParts.length; m++) {
        let part = shelfParts[m]
        let foundItem = cassetteUpdateList.find(function (o: any) {
          return o.partId == part.partId && o.id == part.id
        })
        if (foundItem != null && foundItem != undefined) {
          cassetteUpdateList.splice(cassetteUpdateList.indexOf(foundItem), 1)
        }
      }
    }

    let planogramInfo = new PlanogramInfo(
      planogramId,
      planogramName,
      shelvesList,
      cassetteUpdateList,
    )

    if (!isCluster.value) {
      planogramInfo.clusterId = planogramId
    }
    planogramInfo.countryId = countryId
    planogramInfo.brandId = brandId

    if (isCluster.value) {
      planogramInfo.clusterId = clusterId
      shelvesList.clusterId = clusterId
    }
    //set scratchpad
    let scratchPadInfo = new ShelfInfoList(planogramId, [])
    let scratchShelvesList: any[] = []

    for (let l = 0; l < scratchShelves.length; l++) {
      let shelf = scratchShelves[l].attributes.shelfInfo
      shelf.planmatrShelfId = scratchShelves[l].attributes.id
      shelf.position = scratchShelves[l].attributes.position
      scratchShelvesList.push(shelf)
    }
    scratchPadInfo.shelfInfos = scratchShelvesList
    planogramInfo.scratchPadInfo = scratchPadInfo
    scratchPadInfo.partInfos = createCassetteInfo(scratchParts)

    //set deleted parts
    let deletedPartInfo = new ShelfInfoList(planogramId, [])
    deletedPartInfo.partInfos = createPartInfoList(deleteList)
    // now add any deleted shelves
    const deleteShelvesList: any[] = []
    for (let m = 0; m < deleteList.length; m++) {
      if (deleteList[m].type == 'planmatr.Part.Shelf') {
        let shelf = deleteList[m].shelfInfo
        shelf.planmatrShelfId = deleteList[m].id
        shelf.position = deleteList[m].position
        deleteShelvesList.push(shelf)
      }
    }
    deletedPartInfo.shelfInfos = deleteShelvesList
    planogramInfo.deletedInfo = deletedPartInfo
    let dialogMessage = '<b>Are you sure you want to save the cluster?</b>'
    if (!isCluster.value) {
      dialogMessage = '<b>Are you sure you want to save the planogram?</b>'
    }

    let saveDialog = new joint.ui.Dialog({
      theme: 'material',
      width: 400,
      title: 'Confirm',
      content: dialogMessage,
      buttons: [
        { action: 'yes', content: 'Yes' },
        { action: 'no', content: 'No' },
      ],
    })
    saveDialog.$el
    saveDialog.on(
      'action:yes',
      async function () {
        if (!isCluster.value) {
          new joint.ui.FlashMessage({
            title: 'Message',
            type: 'message',
            content: 'saving planogram',
          })
          await planogramService.savePlanogram(planogramInfo).then(
            async function (data) {
              new joint.ui.FlashMessage({
                theme: 'material',
                title: 'Message',
                type: 'success',
                content: 'planogram saved ok',
              }).open()

              //save snapshot jpeg
              await saveSnapshot(
                planogramId,
                currentView.value as number,
                stand,
                partOverlap,
                partOverlapAmount,
                paper,
                graph,
              ).then(() => {
                planogramService.toggleScratchPad(paper, graph, false, currentViewParam)
              })

              // //getSVG to save
              // var exportStylesheet = [
              //   '.facing-object.show-facing-object { opacity: 0.8; }',
              //   '.facing-object body { background-color: white; }',
              //   '.facing-table { background-color: #fff; }',
              //   '​.facing-object .facing-table .facing-item-name span { width: 1em; overflow: hidden; overflow-wrap: break-word; }'
              // ].join('');

              // await Promise.resolve(self.paper.toSVG((svg: string) => {
              //   self.planogramService.savePlanogramSvg(encodeURIComponent(svg), self.planogramId);
              // }, {
              //     preserveDimensions: true,
              //     convertImagesToDataUris: true,
              //     useComputedStyles: false,
              //     stylesheet: exportStylesheet
              //   }));

              //show scratchpad again if necessary
              // if (!wasAlreadyHidden) {
              //     scratchPadHidden.value = utilitiesService.toggleScratchPad(scratchPadHidden.value, selection, currentInspector, currentView.value as number);
              // }
              // showScratchPad.value = false;
              //check all parts are set correctly since last save
              await Promise.resolve(
                updatePlanogramParts(
                  planogramId,
                  clusterId,
                  standShape,
                  stand,
                  partOverlap,
                  partOverlapAmount,
                  paper,
                  graph,
                ),
              )

              finishedSave.value = { planogramId: planogramId, clusterId: clusterId }
              saving.value = false
            },
            async function (data) {
              new joint.ui.FlashMessage({
                theme: 'material',
                title: 'Message',
                type: 'alert',
                content: 'There was a problem saving this planogram',
              }).open()
              saving.value = false
              //show scratchpad again if necessary
              // if (!wasAlreadyHidden) scratchPadHidden.value = utilitiesService.toggleScratchPad(scratchPadHidden.value, selection, currentInspector, currentView.value as number);
              saving.value = false
              //check all parts are set correctly since last save
              await Promise.resolve(
                updatePlanogramParts(
                  planogramId,
                  clusterId,
                  standShape,
                  stand,
                  partOverlap,
                  partOverlapAmount,
                  paper,
                  graph,
                ),
              )
            },
          )
        } else {
          new joint.ui.FlashMessage({
            theme: 'material',
            title: 'Message',
            type: 'message',
            content: 'saving cluster',
          }).open()
          await planogramService.saveCluster(planogramInfo).then(
            async function (data) {
              new joint.ui.FlashMessage({
                theme: 'material',
                title: 'Message',
                type: 'success',
                content: 'cluster saved ok',
              }).open()
              await Promise.resolve(
                updatePlanogramParts(
                  planogramId,
                  clusterId,
                  standShape,
                  stand,
                  partOverlap,
                  partOverlapAmount,
                  paper,
                  graph,
                ),
              )
              saving.value = false
              //show scratchpad again if necessary
              // if (!wasAlreadyHidden) scratchPadHidden.value = utilitiesService.toggleScratchPad(scratchPadHidden.value, selection, currentInspector, currentView.value as number);
              planogramService.toggleScratchPad(paper, graph, true, currentViewParam)

              //check all parts are set correctly since last save
              await Promise.resolve(
                updatePlanogramParts(
                  planogramId,
                  clusterId,
                  standShape,
                  stand,
                  partOverlap,
                  partOverlapAmount,
                  paper,
                  graph,
                ),
              )
            },
            async function (data) {
              new joint.ui.FlashMessage({
                theme: 'material',
                title: 'Message',
                type: 'alert',
                content: 'There was a problem saving this cluster',
              }).open()
              saving.value = false
              //show scratchpad again if necessary
              // if (!wasAlreadyHidden) scratchPadHidden.value = utilitiesService.toggleScratchPad(scratchPadHidden.value, selection, currentInspector, currentView.value as number);
              saving.value = false
              //check all parts are set correctly since last save
              await Promise.resolve(
                updatePlanogramParts(
                  planogramId,
                  clusterId,
                  standShape,
                  stand,
                  partOverlap,
                  partOverlapAmount,
                  paper,
                  graph,
                ),
              )
            },
          )
        }
        saveDialog.close()
      },
      saveDialog,
    )
    saveDialog.on(
      'action:no',
      async function () {
        saveDialog.close()
        // utilitiesService.toggleSpinner(false);
        saving.value = false
        //show scratchpad again if necessary
        // if (!wasAlreadyHidden) scratchPadHidden.value = utilitiesService.toggleScratchPad(scratchPadHidden.value, selection, currentInspector, currentView.value as number);
        showScratchPad.value = true
      },
      saveDialog,
    )
    saveDialog.on(
      'action:close',
      function () {
        //saveDialog.close();
        saving.value = false
        //show scratchpad again if necessary
        // if (!wasAlreadyHidden) scratchPadHidden.value = utilitiesService.toggleScratchPad(scratchPadHidden.value, selection, currentInspector, currentView.value as number);
        showScratchPad.value = true
      },
      saveDialog,
    )
    saveDialog.open()
  }

  function getDeleteList(undoStack: any[]): any[] {
    var deleteList: any[] = []

    for (var i = 0; i < undoStack.length; i++) {
      var item = undoStack[i]
      if (Array.isArray(item)) {
        for (var j = 0; j < item.length; j++) {
          if (item[j].action == 'remove') {
            if (
              item[j].data.attributes.type == 'planmatr.Part.Cassette' ||
              item[j].data.attributes.type == 'planmatr.Part.Shelf'
            ) {
              if (item[j].data.attributes.type == 'planmatr.Part.Cassette') {
                var deletedPart = item[j].data.attributes
                deleteList.push(deletedPart)
              } else {
                var deletedPart = item[j].data.attributes
                deleteList.push(deletedPart)
              }
            }
          }
        }
      } else {
        if (item.action == 'remove') {
          if (
            item.data.attributes.type == 'planmatr.Part.Cassette' ||
            item.data.attributes.type == 'planmatr.Part.Shelf'
          ) {
            if (item.data.attributes.type == 'planmatr.Part.Cassette') {
              const deletedPart = item.data.attributes
              deleteList.push(deletedPart)
            } else {
              const deletedPart = item.data.attributes
              deleteList.push(deletedPart)
            }
          }
        }
      }
    }
    return deleteList
  }

  async function updatePlanogramParts(
    planogramId: number,
    clusterId: number,
    standShape: appShapes.planmatr.Carcass,
    stand: Stand,
    partOverlap: boolean,
    partOverlapAmount: number,
    paper: joint.dia.Paper,
    graph: joint.dia.Graph,
  ) {
    let menuService = new MenuService()
    let planogramService = new PlanogramService(
      isCluster.value,
      partOverlap,
      partOverlapAmount,
      graph,
      paper,
    )
    //var menu = new Menu;
    let result: any
    if (!isCluster.value) {
      result = await Promise.resolve(menuService.loadPlanogramShelves(planogramId))
    } else {
      // result = await Promise.resolve(menuService.loadClusterShelves(clusterId));
    }

    planogramShelves.value = result as any
    if (planogramShelves.value != null && planogramShelves.value != undefined) {
      if (planogramShelves.value?.length > 0) {
        planogramService.rePopulatePlanogram(
          graph,
          planogramShelves.value,
          standShape,
          stand,
          appMode,
          0,
        )
      }
    }

    let parts: any
    let scratchParts: any
    if (!isCluster.value) {
      parts = await Promise.resolve(menuService.loadNewPlanogramParts(planogramId))
    } else {
      // parts = await Promise.resolve(menuService.loadClusterParts(clusterId));
    }
    planogramParts.value = parts as any
    if (planogramParts.value != null && planogramParts.value != undefined) {
      if (planogramParts.value.length > 0) {
        planogramService.rePopulatePlanogram(
          graph,
          planogramParts.value,
          standShape,
          stand,
          appMode,
          0,
        )
      }
    }
    if (!isCluster.value) {
      scratchParts = await Promise.resolve(planogramService.getScratchPad(planogramId))
      // } else {
      //     scratchParts = await Promise.resolve(planogramService.getScratchPad(clusterId));
      scratchPad.value = scratchParts as any
      if (scratchPad.value != null && scratchPad.value != undefined) {
        if (scratchPad.value.length > 0) {
          planogramService.rePopulateScratchPad(graph, scratchPad.value, standShape, stand)
        }
      }
    }
  }

  function createCassetteInfo(cassettes: joint.dia.Element[]): PartInfo[] {
    let cassetteUpdateList: PartInfo[] = []
    for (let i = 0; i < cassettes.length; i++) {
      let part: any
      if (cassettes[i].attributes.type == 'planmatr.Part.Cassette') {
        part = cassettes[i].attributes.partInfo
        //}
        //else {
        //  part = cassettes[i].attributes.shelfInfo;
        //}
        let cassettePosition = cassettes[i].attributes.position
        if (cassettePosition != undefined && cassettePosition != null) {
          if (!Number.isInteger(cassettePosition.x)) {
            cassettePosition.x = Math.round(cassettePosition.x)
          }

          if (!Number.isInteger(cassettePosition.y)) {
            cassettePosition.y = Math.round(cassettePosition.y)
          }
        }
        cassettes[i].attributes.position = cassettePosition
        if (part.position != undefined) {
          part.position = cassettes[i].attributes.position as joint.g.PlainPoint
        }
        //need to make the product list from the partInfo properties
        let partFacings: any[] = []
        for (let j = 1; j < part.facings + 1; j++) {
          let prodFacing = new PartFacing(j)

          if (part['selectedProduct-facing-' + j] != null) {
            prodFacing.productId = part['selectedProduct-facing-' + j]
            if (part['selectedShade-facing-' + j] != null) {
              prodFacing.shadeId = part['selectedShade-facing-' + j]
            }
          }
          if (part['selectedStatus-facing-' + j] != null)
            prodFacing.facingStatus = part['selectedStatus-facing-' + j]

          partFacings.push(prodFacing)
        }

        part.facingProducts = partFacings
        cassetteUpdateList.push(part)
      }
    }
    return cassetteUpdateList
  }

  function createPartInfoList(cassettes: any) {
    let cassetteUpdateList: any[] = []
    for (let i = 0; i < cassettes.length; i++) {
      if (cassettes[i].type == 'planmatr.Part.Cassette') {
        let part = cassettes[i].partInfo
        part.position = cassettes[i].position

        cassetteUpdateList.push(part)
      }
    }
    return cassetteUpdateList
  }

  async function saveSnapshot(
    planogramId: number,
    currentView: number,
    stand: Stand,
    partOverlap: boolean,
    partOverlapAmount: number,
    paper: joint.dia.Paper,
    graph: joint.dia.Graph,
  ) {
    let exportStylesheet = [
      'body {position: relative; width: 100 %; height: 100 %; box-sizing: border-box; margin: 0; padding: 0;}',
      '.facing-object {float:left;}',
      '.facing-object.show-facing-object { opacity: 0.8; }',
      '.facing-object body { margin:0; }',
      '.facing-table { font-family: sans-serif; width: 100%; height: 100%;}',
      '​.facing-object .facing-table .facing-item-name span { width: 1em; overflow: hidden; overflow-wrap: break-word; }',
    ].join('')

    //if this is a maybelline
    if (stand.brandId == 9 || stand.brandId == 1 || stand.brandId == 5) {
      exportStylesheet =
        exportStylesheet +
        '.joint-type-planmatr-carcass.joint-theme-planmatr-dark.render-view .body, .joint-type-planmatr-column.joint-theme-planmatr-dark.render-view .body {background-color: black; fill: black}'
    }

    // let paper = paper;
    let imageSize = '1x'
    if (currentView == CurrentView.render) {
      imageSize = '1x'
    }
    // if (!scratchPadHidden) {
    //     scratchPadHidden = utilitiesService.toggleScratchPad(scratchPadHidden, selection, currentInspector, currentView);
    // }
    // showScratchPad.value = false;

    let planogramService = new PlanogramService(
      isCluster.value,
      partOverlap,
      partOverlapAmount,
      graph,
      paper,
    )

    paper.hideTools()
    joint.format.toJPEG(
      paper,
      async (dataURL: string) => {
        let postData = new PlanogramSvg()
        postData.planogramId = planogramId
        postData.image = encodeURIComponent(dataURL)
        await planogramService.savePlanogramSnapshot(postData)
        paper.showTools()
      },
      {
        padding: 0,
        size: imageSize,
        useComputedStyles: false,
        stylesheet: exportStylesheet,
      },
    )

    //show scratchpad
    // if (!scratchPadHidden) {
    //     scratchPadHidden = utilitiesService.toggleScratchPad(scratchPadHidden, selection, currentInspector, currentView);
    // }
    showScratchPad.value = true
  }

  return {
    savePlanogram,
    updatePlanogramParts,
    createCassetteInfo,
    createPartInfoList,
    saveSnapshot,
    currentView,
    saving,
    finishedSave,
    showScratchPad,
  }
}
