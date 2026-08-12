<script setup lang="ts">
import * as appShapes from '@/planner/shapesDELETE/app-shapes'
import * as joint from '@joint/plus'

import { onMounted, onUnmounted, ref, useTemplateRef } from 'vue'

// import rappid servicesimport inspectorComponent from '@/planner/components/inspector-component.vue';
import { ClusterFilter } from '@/models/Clusters/clusterFilter.model'
import { PlanogramNote } from '@/models/Planograms/note.model'
import { PlanogramFilter } from '@/models/Planograms/planogramFilter.model'
import { default as noteService } from '@/services/Planograms/NotesService'

import inspectorComponent from '@/planner/components/inspector-component.vue'
import navigatorComponent from '@/planner/components/navigator-component.vue'
import stencilComponent from '@/planner/components/stencil-component.vue'
import ToolbarComponent from '@/planner/components/toolbar-component.vue'
import { StandColumn } from '@/planner/models/Column'
import { StandColUpright } from '@/planner/models/ColumnUpright'
import {
  AppMode,
  CurrentView,
  PartTypes,
  ShadeStatusColourEnum,
  StandLayoutEnum,
  StatusColourEnum,
} from '@/planner/models/Enumerations'

import { useSavePlanogram } from '@/planner/components/composables/savePlanogram.composable'
import { Menu } from '@/planner/models/Menu'
import { PartInfo } from '@/planner/models/PartInfo'
import { Planogram } from '@/planner/models/Planogram'
import { planmatr } from '@/planner/models/shapes/planmatr-shapes'
import type { Stand } from '@/planner/models/Stand'
import { CassetteService } from '@/planner/services/cassette-service'
import { HaloService } from '@/planner/services/halo-service'
import { KeyboardService } from '@/planner/services/keyboard-service'
import { MenuService } from '@/planner/services/menu.service'
import { PlanogramRenderService } from '@/planner/services/planogram-render-service'
import { PlanogramService } from '@/planner/services/planogram-service'
// import { ToolbarService } from '@/planner/services/toolbar-service';
// import { UtilitiesService } from '@/planner/services/UnusedServices/utilities.service'
import { ValidationService } from '@/planner/services/validation.service'
import { useClusterStore } from '@/stores/clusterStore'
import { usePlanogramStore } from '@/stores/planogramStore'
import { useDialog } from 'primevue/usedialog'
import { useRouter } from 'vue-router'
const router = useRouter()
const app = useTemplateRef<HTMLElement>('app')
const props = defineProps<{
  planogramId: number
  clusterId: number
  appMode: AppMode
}>()
const planogramStore = usePlanogramStore()
const clusterStore = useClusterStore()
const dialog = useDialog()

const pageBreakSettings: { color: string; width: number; height: number } = {
  color: '#353535',
  width: 1000,
  height: 1000,
}
const defaultPaperSize: { width: number; height: number } = { width: 1000, height: 1000 }
//const planner = ref<Element | null>(null);
const paperContainer = ref<HTMLElement | null>(null)
const stencil = ref<InstanceType<typeof stencilComponent> | null>(null)
const inspector = ref<InstanceType<typeof inspectorComponent> | null>(null)
const navigator = ref<InstanceType<typeof navigatorComponent> | null>(null)

const graph = ref<joint.dia.Graph | null>(null)
const paper = ref<joint.dia.Paper | null>(null)
const commandManager = ref<joint.dia.CommandManager | null>(null)
const paperScroller = ref<joint.ui.PaperScroller | null>(null)
const paperReady = ref(false)
const planLoading = ref(true)
const showSpinner = ref(true)
const searchGraph = ref<joint.dia.Graph>(new joint.dia.Graph())
const haloService = new HaloService()
const keyboardService = new KeyboardService()
// const toolbarService = ref<ToolbarService | null>(null);

const clipboard = ref<joint.ui.Clipboard | null>(null)
const keyboard = ref<joint.ui.Keyboard | null>(null)
const selection = ref<joint.ui.Selection | null>(null)
const snaplines = ref<joint.ui.Snaplines | null>(null)
const snaplinesEnabled = ref<boolean>(true)
const tooltip = ref<joint.ui.Tooltip | null>(null)
// const utilitiesService = ref<UtilitiesService | null>(null)
const validator = ref<joint.dia.Validator | null>(null)
const planogramService = ref<PlanogramService | null>(null)
const cassetteService = ref<CassetteService | null>(null)
const planogramRenderService = ref<PlanogramRenderService | null>(null)
const selectedCell = ref<joint.dia.Cell | null>(null)
const currSelection = ref<joint.ui.Selection | null>(null)
const cellSelected = ref(false)
const ZOOM_SETTINGS = {
  min: 0.2,
  max: 2,
}

const menuCategories = ref<Menu | null>(null)
const planogramShelves = ref<PartInfo[]>([])
const planogramParts = ref<PartInfo[]>([])
const carcass = ref<planmatr.Carcass | null>(null)
const stand = ref<Stand>({} as Stand)
const standLayoutType = ref<number | null>(null)
const scratchPad = ref<PartInfo[]>([])
const standId = ref<number | null>(null)
const planogram = ref<Planogram | null>(null)
// const standShape = ref<planmatr.Carcass>();
const planogramName = ref<string | null>(null)
const planogramId = ref<number | null>(null)
const clusterId = ref<number | null>(null)
const currentInspector = ref<joint.ui.Inspector | null>(null)
const scratchPadHidden = ref<boolean>(false)
const currentView = ref<number>(0)
const skuCount = ref<number>(0)
const showNotesDialog = ref(false)
const planogramNotes = ref<PlanogramNote[]>([])
// const comCount = ref<number>(0);
// const catalogueWindowRef = ref<Window | null>(null);
// const selectionScrollerId = ref<number | null>(null);

const isCluster = ref<boolean>(false)
const partOverlap = ref<boolean>(false)
const partOverlapAmount = ref<number>(0)
const Cassette = planmatr.Part.Cassette
const Shelf = planmatr.Part.Shelf

const shapenamespace = {
  ...joint.shapes,
  planmatr: {
    Part: { Cassette, Shelf },
  },
}

// const { saving, showScratchPad, finishedSave } = useSavePlanogram(props.appMode, planogramStore.planogram.brandId, planogramStore.planogram.countryId);
const planogramImageFile = ref<File | null>(null)

// watch(
//     () => saving.value,
//     (newVal) => {
//         showSpinner.value = newVal;
//     }
// );

// watch(
//     () => showScratchPad.value,
//     (newVal) => {
//         scratchPadHidden.value = newVal;
//         toggleScratchPad();
//     }
// );
onUnmounted(() => {
  document.getElementsByClassName('layout-main')[0]?.classList.remove('planner-container')
})
onMounted(async () => {
  document.getElementsByClassName('layout-main')[0]?.classList.add('planner-container')

  // apply current joint js theme
  joint.setTheme('light')
  if (props.appMode === AppMode.Cluster) {
    isCluster.value = true
  } else {
    isCluster.value = false
  }
  if (props.appMode === AppMode.Cluster) {
    let clusterFilter = new ClusterFilter()
    clusterFilter.id = props.clusterId
    await clusterStore.initialize(clusterFilter)
  } else {
    let planogramFilter = new PlanogramFilter()
    planogramFilter.id = props.planogramId
    await planogramStore.initialize(planogramFilter)
  }

  graph.value = new joint.dia.Graph(
    {},
    {
      cellNamespace: shapenamespace,
    },
  )

  commandManager.value = new joint.dia.CommandManager({
    graph: graph.value as joint.dia.Graph,
    // cmdBeforeAdd: (cmdName: string, _cellView, _value, { ignoreUndoRedo } = { ignoreUndoRedo: false }) => {
    //     const [, property] = cmdName.split(':');
    //     const ignoredChanges = ['infinitePaper', 'dotGrid', 'snaplines', 'gridSize'];
    //     return !ignoreUndoRedo && !ignoredChanges.some((change) => change === property);
    // }
    cmdBeforeAdd: function (
      cmdName: any,
      cell: any,
      graph: any,
      options: { ignoreCommandManager?: any },
    ) {
      options = options || {}
      return !options.ignoreCommandManager
    },
  })
  commandManager.value.on('stack:undo', function (opt: any) {
    if (selection.value?.collection.length != null) {
      for (var i = 0; i < selection.value?.collection.length; i++) {
        selection.value?.collection.remove(selection.value?.collection.models[i])
      }
    }
    for (var i = 0; i < opt.length; i++) {
      if (opt[i].action === 'change:partInfo') {
        if (
          opt[i].data.type == 'planmatr.Part.Cassette' ||
          opt[i].data.type == 'planmatr.Part.Shelf'
        ) {
          if (opt[i].options.propertyPath === 'partInfo/notes') {
            undoNotesIndicator(opt[i], true)
          }
        }
      }
    }
  })

  commandManager.value.on('stack:redo', function (opt: any) {
    if (selection.value?.collection.length != null) {
      for (var i = 0; i < selection.value?.collection.length; i++) {
        selection.value?.collection.remove(selection.value?.collection.models[i])
      }
    }

    for (var i = 0; i < opt.length; i++) {
      if (opt[i].action === 'add') {
        if (
          opt[i].data.attributes.type == 'planmatr.Part.Cassette' ||
          opt[i].data.attributes.type == 'planmatr.Part.Shelf'
        ) {
          updateItemStatus(opt[i].data)
        }
      }
      if (opt[i].action === 'change:partInfo') {
        if (
          opt[i].data.type == 'planmatr.Part.Cassette' ||
          opt[i].data.type == 'planmatr.Part.Shelf'
        ) {
          if (opt[i].options.propertyPath === 'partInfo/notes') {
            undoNotesIndicator(opt[i], false)
          }
        }
      }
    }
  })

  commandManager.value.on('stack:push', function (opt: any) {
    for (var i = 0; i < opt.length; i++) {
      if (opt[i].action === 'add' && opt[i].batch === true) {
        if (opt[i].data.attributes.type === 'planmatr.Part.Cassette') {
          //then we need to ensure that the data reflects the fact it's a new part (not a copy)
          opt[i].data.attributes.partInfo.planogramPartId = 0
        } else if (opt[i].data.attributes.type === 'planmatr.Part.Shelf') {
          opt[i].data.attributes.label = ''
          opt[i].data.attributes.attrs['label'] = ''
          opt[i].data.attributes.shelfInfo.label = ''

          opt[i].data.attributes.shelfInfo.planogramShelfId = 0
          if (isCluster) {
            opt[i].data.attributes.shelfInfo.clusterShelfId = 0
          }
        }
      }
    }
    planogramStore.dirty = true
  })

  validator.value = new joint.dia.Validator({
    commandManager: commandManager.value as joint.dia.CommandManager,
  })

  /////////////////////////////////
  // graph event listeners
  /////////////////////////////////

  graph.value.on('change:partInfo', function (cell: joint.dia.Cell, partInfo: any, opt: any) {
    //var thisThing = 'thatThing';
  })

  graph.value.on('add', async (cell: joint.dia.Cell, collection: any, opt: any) => {
    if (opt.stencil) {
      var cassetteService = new CassetteService()
      //now we must generate the product types.
      //generate product types using api

      if (cell.attributes.type === 'planmatr.Part.Cassette') {
        var partId = cell.attributes.partInfo.partId
        if (cell.attributes.partInfo.facings > 0) {
          await cassetteService.initialise()
          await cassetteService
            .getPartProducts(partId, planogramStore.planogram.id as number)
            .then(function (data) {
              gotPartProductData(cell, data)
              selectedCell.value = cell
              // var inspectorEl = currentInspector.value?.$el;

              //maxlength value to be derived by shelflength (if it's a shelf) shelf length / 4.5mm ? From Diam

              //self.setInspectorProps(inspectorEl, cell);
            })
        }
      } else {
        selectedCell.value = cell
        // currentInspector.value.on('render', function () {
        //     cell.findView(self.paper).render();
        // });
      }
    }
  })

  graph.value.on(
    'change:partInfo',
    function (this: any, cell: joint.dia.Cell, partInfo: any, opt: any) {
      //this.modelChanged();
      var self = this
      if (cell.attributes.type === 'planmatr.Part.Cassette') {
        // var cassetteService = new CassetteService;
        if (opt.propertyPath) {
          console.log(opt.propertyPath + ' attribute value changed to "' + opt.propertyValue + '"')
          if (opt.propertyPath.includes('Product')) {
            // var inspector = this.currentInspector;
            //get Shades from product
            var product = cell.attributes.partInfo.products.find(
              (p: any) => p.productId === opt.propertyValue,
            )
            if (product != null) {
              // var shades = product.shades;
              // let shadePropertyPath = opt.propertyPath.replace(new RegExp('Product', 'g'), 'Shade');
            }
            cell.findView(paper.value as joint.dia.Paper).render()
          }

          if (opt.propertyPath.includes('partInfo/scratchPadId')) {
            //check here if the instruction is coming from commandManager
            if (opt.commandManager == null) {
              if (opt.propertyValue == null || opt.propertyValue == 0) {
                cell.attributes.planogramInfo.scratchPadId = 0
                cell.attributes.partInfo.scratchPadId = 0
              } else {
                if (opt.propertyValue != 0) {
                  cell.attributes.planogramInfo.scratchPadId = opt.propertyValue
                  cell.attributes.partInfo.scratchPadId = opt.propertyValue
                  cell.attributes.partInfo.planogramShelfId = 0
                  cell.attributes.partInfo.planxShelfId = 0
                }
              }
            } else {
              //commandManager is remembering the original set value - so we need to set it reverse
              if (opt.propertyValue == null || opt.propertyValue == 0) {
                cell.attributes.planogramInfo.scratchPadId = self.planogram.scratchPadId
                cell.attributes.partInfo.scratchPadId = self.planogram.scratchPadId
                cell.attributes.partInfo.planogramShelfId = 0
                cell.attributes.partInfo.planxShelfId = 0
              } else {
                if (opt.propertyValue != 0) {
                  cell.attributes.planogramInfo.scratchPadId = 0
                  cell.attributes.partInfo.scratchPadId = 0
                }
              }
            }
          }

          if (opt.propertyPath.includes('partInfo/notes')) {
            if (opt.propertyValue != null) {
              if (opt.propertyValue != '') {
                cell.attributes.partInfo.hasNotes = true
                if (
                  cell.attributes.attrs != undefined &&
                  cell.attributes.attrs['.comment-indi'] != undefined
                ) {
                  if (cell.attributes.attrs['.comment-indi']['visibility'] == 'hidden') {
                    cell.attributes.attrs['.comment-indi']['visibility'] = 'visible'
                    cell.attributes.attrs['.comment-indi']['display'] = 'block'
                    cell.findView(paper.value as joint.dia.Paper).render()
                  }
                } else {
                  cell.attributes.partInfo.hasNotes = false
                  if (
                    cell.attributes.attrs != undefined &&
                    cell.attributes.attrs['.comment-indi'] != undefined
                  ) {
                    cell.attributes.attrs['.comment-indi']['visibility'] = 'hidden'
                    cell.findView(paper.value as joint.dia.Paper).render()

                    document.querySelectorAll('body').forEach((body) => {
                      body
                        .querySelector("[model-id='" + cell.cid + "']")
                        ?.querySelector('circle')
                        ?.setAttribute('display', 'none')
                    })
                  }
                }
              }
            }
          }

          if (opt.propertyPath.includes('partInfo/selectedShade')) {
            var statusColour = ShadeStatusColourEnum[opt.propertyValue]
            var propertySplit = opt.propertyPath.split('-')
            var facingNo = propertySplit[propertySplit.length - 1]

            if (currentView.value == CurrentView.shade) {
              cell.findView(paper.value as joint.dia.Paper).render()
            }
          }

          if (opt.propertyPath.includes('partInfo/statusId')) {
            var statusColour = StatusColourEnum[partInfo.statusId]
            planogramRenderService.value?.SetStatusColour(
              cell as joint.dia.Element,
              statusColour,
              currentView.value,
            )
            cell.findView(paper.value as joint.dia.Paper).render()
          }
          if (opt.propertyPath.includes('partInfo/selectedStatus')) {
            var statusColour = ShadeStatusColourEnum[opt.propertyValue]
            var propertySplit = opt.propertyPath.split('-')
            var facingNo = propertySplit[propertySplit.length - 1]

            if (currentView.value == CurrentView.shade) {
              if (
                cell.attributes.attrs != undefined &&
                cell.attributes.attrs['facings'] != undefined
              ) {
                const cassetteFacings = cell.attributes.attrs['facings']['html'] as string
                const cassetteFacingsHtml = new DOMParser().parseFromString(
                  cassetteFacings,
                  'text/html',
                )

                var facingElem = cassetteFacingsHtml?.querySelector(
                  'td[data-facingNo="' + facingNo + '"]',
                ) as HTMLElement
                facingElem.style.color = statusColour
                cell.attributes.attrs['facings']['html'] =
                  cassetteFacingsHtml.documentElement.outerHTML
                cell.findView(paper.value as joint.dia.Paper).render()
              }
            }
          }
        } // endif opt.propertyPath

        if (currentView.value === CurrentView.shade) {
          planogramRenderService.value?.updateShadeView(
            paper.value as joint.dia.Paper,
            cell as joint.dia.Element,
          )
        }
      }
    },
    this,
  )

  graph.value.on(
    'change:attrs',
    function (this: any, cell: joint.dia.Cell, partInfo: any, opt: any) {
      if (cell.attributes.type === 'planmatr.Part.Shelf') {
        // var cassetteService = new CassetteService;
        console.log(opt.propertyPath + ' attribute value changed to "' + opt.propertyValue + '"')
      }
      if (
        opt.propertyPath.includes('attrs/text/text') ||
        opt.propertyPath.includes('attrs/#label/text')
      ) {
        // var labelVal = opt.propertyValue;
        if (cell.attributes.type === 'planmatr.Part.Shelf') {
          cell.attributes.shelfInfo.label = opt.propertyValue
        } else {
          cell.attributes.partInfo.label = opt.propertyValue
        }
        document.querySelectorAll('facing-object').forEach((elem) => {
          ;(elem as HTMLElement).style.opacity = '0.8'
        })
        cell.findView(paper.value as joint.dia.Paper).render()
      }
    },
    this,
  )

  graph.value.on(
    'change:shelfInfo',
    function (this: any, cell: joint.dia.Cell, shelfInfo: any, opt: any) {
      try {
        if (cell.attributes.type === 'planmatr.Part.Shelf') {
          // var cassetteService = new CassetteService;
          console.log(opt.propertyPath + ' attribute value changed to "' + opt.propertyValue + '"')

          if (
            opt.propertyPath.includes('shelfInfo/scratchPadId') ||
            opt.propertyPath.includes('planogramInfo/scratchPadId')
          ) {
            if (opt.commandManager == null) {
              if (opt.propertyValue == null || opt.propertyValue == 0) {
                cell.attributes.planogramInfo.scratchPadId = 0
                cell.attributes.shelfInfo.scratchPadId = 0
              } else {
                if (opt.propertyValue != 0) {
                  cell.attributes.planogramInfo.scratchPadId = opt.propertyValue
                  cell.attributes.shelfInfo.scratchPadId = opt.propertyValue
                  //cell.attributes.shelfInfo.planogramShelfId = 0;
                  //cell.attributes.shelfInfo.planxShelfId = 0;
                }
              }
            } else {
              //commandManager is remembering the original set value - so we need to set it reverse
              if (opt.propertyValue == null || opt.propertyValue == 0) {
                cell.attributes.planogramInfo.scratchPadId = planogramStore.planogram.scratchPadId
                cell.attributes.shelfInfo.scratchPadId = planogramStore.planogram.scratchPadId
                //cell.attributes.shelfInfo.planogramShelfId = 0;
                //cell.attributes.shelfInfo.planxShelfId = 0;
              } else {
                if (opt.propertyValue != 0) {
                  cell.attributes.planogramInfo.scratchPadId = 0
                  cell.attributes.shelfInfo.scratchPadId = 0
                }
              }
            }
          }

          if (opt.propertyPath.includes('shelfInfo/status')) {
            var statusColour = StatusColourEnum[shelfInfo.statusId]
            //if (self.currentView != CurrentView.render) {
            if (cell.attributes.attrs != undefined && cell.attributes.attrs['.body'] != undefined) {
              if (shelfInfo.statusId == 0 || shelfInfo.statusId == null) {
                cell.attributes.attrs['.body']['stroke'] = '#000000'
              } else {
                cell.attributes.attrs['.body']['stroke'] = statusColour
              }
              cell.attributes.attrs['.body']['fill'] = statusColour
              cell.attributes.attrs['.body']['fill-opacity'] = 0.4
              cell.findView(paper.value as joint.dia.Paper).render()
            }
          }
          if (
            opt.propertyPath.includes('attrs/#label/save') ||
            opt.propertyPath.includes('attrs/#label/text') ||
            opt.propertyPath.includes('attrs/label/text')
          ) {
            // var labelText = opt.propertyValue;
            setProperty('/label/text', opt.propertyValue, {}, cell)
            cell.attributes.shelfInfo.label = opt.propertyValue
            cell.findView(paper.value as joint.dia.Paper).render()
          }

          if (opt.propertyPath.includes('shelfInfo/label')) {
            // cell.attributes.shelfInfo.attrs['.label'].text = opt.propertyValue
            cell.attributes.shelfInfo.label = opt.propertyValue

            cell.findView(paper.value as joint.dia.Paper).render()
          }
        }
      } catch (e) {
        var result = 'error'
        if (typeof e === 'string') {
          result = e
        } else if (e instanceof Error) {
          result = e.message
        }

        console.log(result)
      }
    },
    this,
  )

  paper.value = new joint.dia.Paper({
    width: 1000,
    height: 1000,
    gridSize: 1,
    drawGrid: true,
    model: graph.value as joint.dia.Graph,
    cellViewNamespace: shapenamespace,
    defaultLink: <joint.dia.Link>new appShapes.app.Link(),
    defaultConnectionPoint: appShapes.app.Link.connectionPoint,
    routerNamespace: {
      normal: joint.routers.normal,
      orthogonal: joint.routers.orthogonal,
      // Redefine the rightAngle router to use vertices.
      rightAngle: function (
        vertices: joint.g.Point[],
        opt: Record<string, unknown>,
        linkView: joint.dia.LinkView,
      ) {
        opt.useVertices = true
        return joint.routers.rightAngle.call(this, vertices, opt, linkView)
      },
    },
    interactive: { linkMove: false },
    async: true,
    sorting: joint.dia.Paper.sorting.APPROX,
  })

  paper.value.on('blank:contextmenu', ({ clientX, clientY }, x, y) => {
    const selectionBBox = selection.value
      ? graph.value?.getCellsBBox(selection.value.collection.toArray())
      : undefined

    const selectedCells = selectionBBox?.containsPoint({ x, y })
      ? selection.value!.collection.toArray()
      : []

    renderContextToolbar({ x: clientX ?? 0, y: clientY ?? 0 }, selectedCells)
  })

  paper.value.on('cell:contextmenu', (cellView, evt) => {
    renderContextToolbar({ x: evt.clientX ?? 0, y: evt.clientY ?? 0 }, [cellView.model])
  })

  paper.value.on('cell:mouseover', (cellView, evt) => {
    //console.log(cellView.getBBox());
    //going to use this to display a highlight around the cell when hovering over it.
  })

  //check the snaplines ui for more settings (can exclude some shapes from the snapping if required)
  snaplines.value = new joint.ui.Snaplines({ paper: paper.value as joint.dia.Paper, distance: 2 })

  paperScroller.value = new joint.ui.PaperScroller({
    paper: paper.value as joint.dia.Paper,
    autoResizePaper: true,
    scrollWhileDragging: true,
    borderless: true,
    cursor: 'grab',
  })

  let removePageBreaks: (() => void) | null = null

  graph.value?.on({
    'change:paperColor': (_, color: string) => paper.value!.drawBackground({ color }),
    'change:infinitePaper': (_, borderless: boolean) => {
      const { options } = paperScroller.value!

      if (borderless) {
        options.borderless = true
        options.baseWidth = 100
        options.baseHeight = 100

        if (removePageBreaks) removePageBreaks()

        paperContainer.value?.classList.remove('bordered')
      } else {
        const { width: paperWidth, height: paperHeight } = defaultPaperSize

        options.borderless = false
        options.baseWidth = paperWidth
        options.baseHeight = paperHeight

        removePageBreaks = addPageBreaks()

        paperContainer.value?.classList.add('bordered')
      }

      paperScroller.value?.adjustPaper()
    },
    'change:dotGrid': (_, showDotGrid: boolean) => paper.value!.setGrid(showDotGrid),
    'change:snaplines': (_, allowSnaplines: boolean) => changeSnapLines(allowSnaplines),
    'change:gridSize': (_, gridSize: number) => paper.value!.setGridSize(gridSize),
  })

  paperContainer.value?.appendChild(paperScroller.value!.el)
  paperScroller.value!.center()

  paper.value!.on('paper:pan', (evt, tx, ty) => {
    evt.preventDefault()
    paperScroller.value!.el.scrollLeft += tx
    paperScroller.value!.el.scrollTop += ty
  })

  paper.value!.on('paper:pinch', (_evt, ox, oy, scale) => {
    // the default is already prevented
    const zoom = paperScroller.value!.zoom()
    paperScroller.value!.zoom(zoom * scale, {
      min: ZOOM_SETTINGS.min,
      max: ZOOM_SETTINGS.max,
      ox,
      oy,
      absolute: true,
    })
  })

  // plannerService.startPlanMatr();
  //graph.value!.fromJSON(JSON.parse(sampleGraphs.emergencyProcedure), { ignoreUndoRedo: true });
  await loadPlanogram()
  await noteService.initialise()
  if (props.appMode === AppMode.Cluster) {
    //planogramNotes.value = await noteService.getNotes(clusterStore.cluster.id as number)
  } else {
    planogramNotes.value = await noteService.getNotes(planogramStore.planogram.id as number)
  }

  initializeSelection()
  initializeToolsAndInspector()
  initializeNavigator()
  // initializeToolbar();
  initializeElementDrop()
  initializeKeyboardShortcuts()
  initializeTooltips()
  paperReady.value = true
})

function gotPartProductData(cell: joint.dia.Cell, partProducts: any) {
  cell.attributes.partInfo.products = partProducts.products
}

async function setupStencil() {
  // renderPlugin('.stencil-container', stencilService.value?.stencil);
  // closeGroups();
  //this.loadMenu();
  document.querySelectorAll('input.search').forEach((element) => {
    ;(element as HTMLInputElement).setAttribute('disabled', 'disabled')
  })

  //await stencil.value?.loadSearchGraph();
  // stencil.value?.closeGroups();
  //stencilComponent.populateSearchGraph(searchGraph, searchData);

  stencil.value?.stencil?.on({
    'element:drop': (elementView: joint.dia.ElementView) =>
      selection.value?.collection.reset([elementView.model]),
    // We need to track the dragging state to prevent showing the tooltip when dragging an element
    'element:dragstart': () => stencil.value?.tooltip?.disable(),
    'element:dragend': () => stencil.value?.tooltip?.enable(),
  })
}
async function loadPlanogram() {
  const urlParams = new URLSearchParams(window.location.search)
  // planogramId.value = parseInt(urlParams.get('planogramId') ?? '0', 10);
  clusterId.value = parseInt(urlParams.get('clusterId') ?? '0', 10)
  const menuService = new MenuService()
  await menuService.initialise()
  const planogramService = new PlanogramService(
    isCluster.value,
    partOverlap.value,
    partOverlapAmount.value,
    graph.value as joint.dia.Graph,
    paper.value as joint.dia.Paper,
  )
  await planogramService.initialise()
  // const planogramRenderService = new PlanogramRenderService();
  if (!isCluster.value) {
    await Promise.resolve(
      planogramService.getPlanogramLock(planogramStore.planogram.id).then(
        async function () {
          standId.value = planogramStore.planogram.standId ?? null
          planogramName.value = planogramStore.planogram.name ?? ''
          const appTitle = document.getElementsByClassName('app-title')[0] as HTMLElement
          if (appTitle) {
            const h2 = appTitle.querySelector('h2')
            if (h2) {
              h2.textContent = planogramName.value
            }
          }
          const planxTitleBar = document.getElementsByClassName('planx-title-bar')[0] as HTMLElement
          if (planxTitleBar) {
            planxTitleBar.classList.remove('text-hide')
          }
          await initializeStand()

          // register validation functions
        },
        function () {
          new joint.ui.FlashMessage({
            title: 'Message',
            type: 'alert',
            content: 'This planogram is locked by another user',
          }).open()
        },
      ),
    )
  } else {
    //we're dealing with a layout (cluster) so we don't need to check for locks
    // const planogram = await menuService.loadClusterData(planogramStore.planogram.id)

    standId.value = clusterStore.cluster.standId ?? null
    planogramName.value = clusterStore.cluster.name ?? ''
    const appTitle = document.getElementsByClassName('app-title')[0] as HTMLElement
    if (appTitle) {
      const h2 = appTitle.querySelector('h2')
      if (h2) {
        h2.textContent = planogramName.value
      }
    }
    const planxTitleBar = document.getElementsByClassName('planx-title-bar')[0] as HTMLElement
    if (planxTitleBar) {
      planxTitleBar.classList.remove('text-hide')
    }
    await initializeStand()
    // // register validation functions
    // validator.validate('change:position', validatePlacement.bind(this), display.bind(this))
    // validator.validate('add', validatePlacement.bind(this), display.bind(this))
  }
  initializePlanoTitle()
  initializeCassetteEditor()
}

function initializePlanoTitle() {
  //const self = this;
  document.querySelector('.app-title')!.addEventListener('click', function () {
    const dialogHtml =
      '<div data-field="name" class="field text-field">' +
      '<label>Name</label>' +
      '<div class="input-wrapper"><input style="width:100%;" type="text" class="text" data-type="text" data-attribute="name" value="' +
      planogramName.value +
      '"></div>' +
      '</div>'

    const dialog = new joint.ui.Dialog({
      type: 'inspector-dialog',
      width: 700,
      title: 'Edit Planogram Name',
      closeButton: false,
      content: dialogHtml,
      buttons: [
        {
          content: 'Cancel',
          action: 'cancel',
        },
        {
          content: 'Apply',
          action: 'apply',
        },
      ],
    })

    dialog.on({
      'action:cancel': function () {
        //inspector.remove();
        dialog.close()
      },
      'action:apply': function () {
        //inspector.updateCell();
        //inspector.remove();

        planogramName.value = dialog.el.querySelector('input')?.value ?? planogramName.value
        if (planogramStore.planogram) {
          planogramStore.planogram.name =
            dialog.el.querySelector('input')?.value ?? planogramStore.planogram.name
        }
        document.querySelector('.app-title h1')!.textContent = planogramName.value
        dialog.close()
      },
    })
    dialog.open()
  })
}

function initializeCassetteEditor() {}

async function initializeStand() {
  const menuService = new MenuService()
  await menuService.initialise()
  let currentStand: Stand
  try {
    await menuService.loadStandData(standId.value ?? 0, isCluster.value).then(async (response) => {
      //return response;
      const data = response.data
      //alert('got the stand');
      stand.value = data as Stand
      if (stand.value) {
        document
          .querySelector('.app-title .plan-x-type .stand-type')
          ?.prepend(stand.value.standTypeName)
        currentStand = stand.value
        initializeScratchPad()
        standLayoutType.value = stand.value?.layoutStyle ?? null

        const baseStandImageUrl = '/server_files/standImages/' + stand.value?.standId
        const footerYpos = currentStand.height - currentStand.footerHeight
        carcass.value = new planmatr.Carcass({
          attrs: {
            '.': { magnet: false },
            header: {
              fill: '#f0f0f0',
              stroke: '#000',
              'stroke-width': 1,
              width: stand.value?.width ?? 0,
              height: stand.value?.headerHeight ?? 0,
            },
            footer: {
              fill: '#f0f0f0',
              width: stand.value?.width ?? 0,
              height: stand.value?.footerHeight ?? 0,
              y: footerYpos,
            },
            headerGraphic: {},
            body: {
              fill: '#fff',
              height: stand.value?.merchHeight ?? 0,
              width: stand.value?.merchWidth ?? 0,
            },
          },
        })
        const header = new planmatr.Header({
          headerGraphic: {},
          body: {
            fill: '#f0f0f0',
            stroke: '#000',
            'stroke-width': 1,
            width: stand.value?.width ?? 0,
            height: stand.value?.headerHeight ?? 0,
          },
        })

        header.position(0, 0)
        header.resize(stand.value?.width ?? 0, stand.value?.headerHeight ?? 0)
        header.addTo(graph.value as joint.dia.Graph, {
          ignoreMove: true,
          ignoreCommandManager: true,
        })

        carcass.value.toBack()
        if (stand.value?.headerGraphic != null && stand.value?.headerGraphic != '') {
          header.attributes['.header-graphic']['xlink:href'] =
            baseStandImageUrl + '/' + stand.value.headerGraphic
          header.attributes['.header-graphic'].width = stand.value.width
          header.attributes['.header-graphic'].height = stand.value.headerHeight
        }
        carcass.value.position(0, 0)
        carcass.value.attributes.merchHeight = stand.value?.merchHeight ?? 0
        carcass.value.attributes.merchWidth = stand.value?.merchWidth ?? 0
        carcass.value.attributes.headerHeight = stand.value?.headerHeight ?? 0
        carcass.value.attributes.headerWidth = stand.value?.headerWidth ?? 0
        carcass.value.attributes.footerHeight = stand.value?.footerHeight ?? 0
        carcass.value.attributes.footerWidth = stand.value?.footerWidth ?? 0
        carcass.value.attributes.size = {
          width: stand.value?.width ?? 0,
          height: stand.value?.height ?? 0,
        }
        // carcass.attributes.width = stand.value?.width ?? 0, height: stand.value?.height ?? 0 };
        //carcass..attributes.disableMove = false;
        // carcass.resize(stand.value?.width ?? 0, stand.value?.height ?? 0, { ignoreMove: true });
        carcass.value.attr({
          text: {
            text: 'Stand',
          },
        })

        carcass.value.addTo(graph.value as joint.dia.Graph, {
          ignoreMove: true,
          ignoreCommandManager: true,
        })
        // standShape.value = carcass;
        // paper.value?.setDimensions(stand.value?.width ?? 0, (stand.value?.height ?? 0) - 20);
        paper.value?.fitToContent({
          padding: 0,
          gridWidth: 1,
          gridHeight: 1,
        })

        carcass.value.embed(header, { deep: true, ignoreCommandManager: true })

        //here we need to check if the merch overlaps the header
        //check stand height values match - seems to be some discrepancy between the height and the header/footer/merch heights.
        //here we need to overlap the header with the merch space - so minus the header by the overlap amount.
        // let headerSubtract = 0;
        // if (stand.value?.headerHeight + stand.value?.merchHeight + stand.value?.footerHeight > stand.value?.height) {

        //   //self.stand.height = self.stand.headerHeight + self.stand.merchHeight + self.stand.footerHeight;
        //   headerSubtract = ((stand.value?.headerHeight + stand.value?.merchHeight + stand.value?.footerHeight) - stand.value?.height);
        // }
        let startXPos = Math.round((stand.value?.width - stand.value?.merchWidth) / 2)
        let startYPos = (stand.value?.height - stand.value?.merchHeight) / 2
        let uprStartYPos = startYPos
        if (stand.value?.headerHeight + stand.value?.footerHeight > 0) {
          // const startYPos = Math.round(stand.value?.headerHeight - headerSubtract);
          uprStartYPos = Math.round(stand.value?.headerHeight)
          //let startYPos = Math.round((stand.value?.height - stand.value?.headerHeight) / 2);
        }

        const cols = stand.value?.columnList
        if (
          stand.value?.columnList.length == 0 &&
          stand.value?.horizontalPitchCount > 0 &&
          stand.value?.horizontalPitchSize != 1
        ) {
          //generate columnList
          for (let c = 1; c <= stand.value?.horizontalPitchCount; c++) {
            //let col = new StandColumn(c, stand.value?.standId, c, (stand.value?.width / stand.value?.horizontalPitchCount)); //this is wrong for layout type = pitch
            const col = new StandColumn(
              c,
              stand.value?.standId,
              c,
              stand.value?.horizontalPitchSize,
            )
            col.columnUprightList = []
            if (c == 1) {
              //we need to add an upright at the start of the column as well
              const upR = new StandColUpright(
                c,
                c,
                stand.value?.standId,
                c,
                0,
                stand.value?.merchHeight - stand.value?.headerHeight,
              )
              col.columnUprightList.push(upR)
            }
            const upR = new StandColUpright(
              c,
              c,
              stand.value?.standId,
              c,
              col.width,
              stand.value?.merchHeight - stand.value?.headerHeight,
            )
            col.columnUprightList.push(upR)
            stand.value?.columnList.push(col)
          }
        } else if (
          stand.value?.layoutStyle == StandLayoutEnum.Column &&
          stand.value?.columnList.length == 0
        ) {
          // can't have a column layout without columms - so add 1 column full height and width
          const col = new StandColumn(1, stand.value?.standId, 1, stand.value?.merchWidth)
          col.columnUprightList = []
          stand.value?.columnList.push(col)
        }
        const rows = stand.value?.rowList
        let posx = startXPos
        //We need to check in the case of pitch layout (2) whether the width of the specified pitches == merch width if not we offset
        if (stand.value?.layoutStyle == StandLayoutEnum.Pitch) {
          posx =
            (stand.value?.width -
              stand.value?.horizontalPitchSize * stand.value?.horizontalPitchCount) /
            2
        }

        let posy = startYPos

        if (stand.value?.columnList.length > 0) {
          cols.forEach(function (col) {
            const column = new planmatr.Column({
              attrs: {
                label: {
                  text: col.columnId.toString(),
                  fill: 'yellow',
                },
                body: {
                  fill: 'none',
                },
              },
            })

            if (stand.value?.layoutStyle == StandLayoutEnum.Pitch) {
              if (column.attributes?.attrs?.['.body']) {
                column.attributes.attrs['.body']['stroke'] = 'none'
              }
            }
            column.resize(col.width, currentStand.merchHeight, { ignoreCommandManager: true })
            column.addTo(graph.value as joint.dia.Graph, { ignoreCommandManager: true })
            column.uprights = col.columnUprightList
            column.attributes.disableMove = true
            carcass.value?.embed(column, { deep: true, ignoreCommandManager: true })
            const uprights = col.columnUprightList
            let uprPosx = posx
            //discover the unusable space
            const merchspace =
              currentStand.height - currentStand.footerHeight - currentStand.headerHeight
            const avaialableSpace =
              Math.floor(merchspace / currentStand.shelfIncrement) * currentStand.shelfIncrement
            const unusableSpace = merchspace - avaialableSpace
            const dashArrayIncrement = currentStand.shelfIncrement - 1
            uprights.forEach(function (upright: any) {
              const upr = new planmatr.Upright({
                attrs: {
                  line: {
                    //y2: (stand.value?.height - (stand.value?.headerHeight + stand.value?.footerHeight)),
                    //y2: (stand.value?.height - stand.value?.headerHeight - unusableSpace),
                    y2: merchspace - unusableSpace,
                    'stroke-dasharray': '1,' + dashArrayIncrement.toString(),
                    'stroke-dashoffset': '0',
                  },
                },
              })
              upr.resize(1, merchspace - unusableSpace, { ignoreCommandManager: true })
              upr.attributes.disableMove = true
              upr.addTo(graph.value as joint.dia.Graph, { ignoreCommandManager: true })
              column.embed(upr, { deep: true, ignoreCommandManager: true })
              //upr.attributes.attrs.line.y2 = self.stand.height - (self.stand.headerHeight + self.stand.footerHeight);
              upr.position(upright.width + uprPosx, uprStartYPos + unusableSpace, {
                parentRelative: true,
                ignoreCommandManager: true,
              })
              uprPosx = upright.width + uprPosx
            })

            column.position(posx, posy, { parentRelative: true, ignoreCommandManager: true })
            posx = posx + col.width
          })
        }

        if (stand.value?.layoutStyle == StandLayoutEnum.Column) {
          //we need to implement rows as lines
          //reset start positions for rows - and use those positions as snap points

          startXPos = Math.round((stand.value?.width - stand.value?.merchWidth) / 2)
          startYPos = Math.round((stand.value?.height - stand.value?.merchHeight) / 2)
          if (stand.value?.headerHeight + stand.value?.footerHeight > 0) {
            //let startYPos = Math.round(stand.value?.merchHeight + stand.value?.headerHeight + stand.value?.footerHeight);
            startYPos = Math.round(stand.value?.headerHeight)
          }

          posx = startXPos
          posy = startYPos //Math.round(stand.value?.height - stand.value?.footerHeight);
          if (rows.length > 0) {
            rows.forEach(function (row) {
              const standRow = new planmatr.Row({
                attrs: {
                  label: {
                    text: row.rowId.toString(),
                    fill: 'yellow',
                  },
                  body: {
                    fill: 'none',
                  },
                },
              })
              standRow.resize(stand.value?.merchWidth ?? 0, 1)
              standRow.addTo(graph.value as joint.dia.Graph, {
                ignoreMove: true,
                ignoreCommandManager: true,
              })
              carcass.value?.embed(standRow, { deep: true, ignoreCommandManager: true })
              standRow.toFront({ ignoreCommandManager: true })

              standRow.position(posx, posy + row.height, {
                parentRelative: true,
                ignoreCommandManager: true,
              })
              posy = posy + row.height
            })
          }
        }

        carcass.value?.embed(header, { deep: true, ignoreCommandManager: true })
        header.toFront({ ignoreCommandManager: true })

        await displayPlanogram(planogramId.value ?? 0, clusterId.value ?? 0).then(async () => {
          //add validation rules after we load the planogram - to avoid validating the initial placement of the parts
          validator.value?.addRule('change:position', validatePlacement, display)
          validator.value?.addRule('add', validatePlacement, display)
        })
      } else {
        throw new Error('Failed to load stand data')
        return
      }
    })

    // let colCells = carcass.getEmbeddedCells();
  } catch (error) {
    console.log('Error loading stand data:', error)
    new joint.ui.FlashMessage({
      title: 'Error',
      type: 'alert',
      content: 'Failed to load stand data',
    }).open()
  }
}

async function initializeScratchPad() {
  // const self = this;
  const planoService = new PlanogramService(
    isCluster.value,
    partOverlap.value,
    partOverlapAmount.value,
    graph.value as joint.dia.Graph,
    paper.value as joint.dia.Paper,
  )
  //let standId = self.standId;
  //const planogramId = planogramId.value;
  if (!isCluster.value) {
    scratchPad.value = await planoService.getScratchPad(planogramStore.planogram.id ?? 0)

    planoService.populateScratchPad(
      graph.value as joint.dia.Graph,
      scratchPad.value,
      stand.value,
      planogramStore.planogram.countryId ?? 0,
    )
    paper.value?.fitToContent({
      padding: 0,
    })
  }
}

async function displayPlanogram(planogramId: number, clusterId: number) {
  // const self = this;

  const menuService = new MenuService()
  await menuService.initialise()
  planogramService.value = new PlanogramService(
    isCluster.value,
    partOverlap.value,
    partOverlapAmount.value,
    graph.value as joint.dia.Graph,
    paper.value as joint.dia.Paper,
  )
  await planogramService.value.initialise()
  // let menu = new Menu;
  let result: PartInfo[] = []
  if (!isCluster.value) {
    result = await Promise.resolve(menuService.loadPlanogramShelves(planogramStore.planogram.id))
  } else {
    result = await Promise.resolve(menuService.loadClusterShelves(clusterStore.cluster.id))
  }
  paperScroller.value?.zoomToFit({})

  planogramShelves.value = result as PartInfo[]
  planogramService.value.populatePlanogram(
    graph.value as joint.dia.Graph,
    planogramShelves.value,
    carcass.value as planmatr.Carcass,
    stand.value,
    planogramStore.planogram.countryId ?? 0,
  )
  let parts: PartInfo[] = []
  if (!isCluster.value) {
    parts = await Promise.resolve(menuService.loadPlanogramParts(planogramStore.planogram.id))
  } else {
    parts = await menuService.loadClusterParts(clusterStore.cluster.id)
  }
  planogramParts.value = parts as PartInfo[]

  //let cassetteService = new CassetteService;
  //let nonMarketParts = await Promise.resolve(cassetteService.getNonMarketParts(planogramStore.planogram.id));

  const countObj = planogramService.value.getSkuCount(parts)
  skuCount.value = countObj.skuCount
  const shelfCountObj = planogramService.value.getSkuCount(planogramShelves.value)
  document.querySelector('.app-header .sku-count span.count')!.textContent =
    countObj.skuCount.toString()
  document.querySelector('.app-header .shelf-count span.count')!.textContent =
    shelfCountObj.shelfCount.toString()

  planogramService.value.populatePlanogram(
    graph.value as joint.dia.Graph,
    planogramParts.value,
    carcass.value as planmatr.Carcass,
    stand.value,
    planogramStore.planogram.countryId ?? 0,
  )

  currentView.value = CurrentView.shade
  //we need to render all the part statuses
  // this.renderPartStatuses();
  showShadeView()

  document.querySelectorAll('.facing-object').forEach((element) => {
    ;(element as HTMLElement).style.opacity = '0.8'
  })

  // utilitiesService.value?.toggleSpinner(false)
  document.querySelectorAll('input.search').forEach((element) => {
    ;(element as HTMLInputElement).removeAttribute('disabled')
  })

  commandManager.value?.reset() //clear undo and redo stack before editing.
  //set comments indicators
  planogramService.value.displayComCount(planogramNotes.value)
  if (stencil.value?.stencil != null) {
    await stencil.value.stencil.openGroups()
    stencil.value.stencil.closeGroups()
  }

  paperScroller.value?.zoomToFit({})
  showSpinner.value = false
}

function initializeSelection() {
  clipboard.value = new joint.ui.Clipboard()
  selection.value = new joint.ui.Selection({
    boxContent: null,
    paper: paperScroller.value as joint.ui.PaperScroller,
    useModelGeometry: true,
    translateConnectedLinks: joint.ui.Selection.ConnectedLinksTranslation.SUBGRAPH,
    handles: [
      {
        ...joint.ui.Selection.getDefaultHandle('remove'),
        position: joint.ui.Selection.HandlePosition.NW,
      },
      // {
      //     ...joint.ui.Selection.getDefaultHandle('resize'),
      //     position: joint.ui.Selection.HandlePosition.SE
      // }
    ],
    frames: new joint.ui.HTMLSelectionFrameList({
      rotate: true,
    }),
  })

  selection.value?.collection.on('reset add remove', () => onSelectionChange())

  const keyboard = keyboardService.keyboard

  // Initiate selecting when the user grabs the blank area of the paper while the Shift key is pressed.
  // Otherwise, initiate paper pan.
  paper.value?.on('blank:pointerdown', (evt: joint.dia.Event, _x: number, _y: number) => {
    if (keyboard!.isActive('shift', evt)) {
      selection.value?.startSelecting(evt)
    } else {
      selection.value?.collection.reset([])
      paperScroller.value?.startPanning(evt)
      paper.value?.removeTools()
    }
  })

  // Initiate selecting when the user grabs a cell while shift is pressed.
  paper.value?.on(
    'cell:pointerdown element:magnet:pointerdown',
    (cellView: joint.dia.CellView, evt: joint.dia.Event) => {
      if (keyboard!.isActive('shift', evt)) {
        cellView.preventDefaultInteraction(evt)
        selection.value?.startSelecting(evt)
      }
    },
  )

  paper.value?.on(
    'element:pointerdown',
    (elementView: joint.dia.ElementView, evt: joint.dia.Event) => {
      // Select an element if CTRL/Meta key is pressed while the element is clicked.
      if (
        elementView.attributes.type == 'planmatr.Carcass' ||
        elementView.attributes.type == 'planmatr.Header' ||
        elementView.attributes.type == 'planmatr.Column' ||
        elementView.attributes.type == 'planmatr.Row' ||
        elementView.attributes.type == 'planmatr.Upright'
      ) {
        elementView.preventDefaultInteraction(evt)
        return
      }
      if (keyboard!.isActive('ctrl meta', evt)) {
        selection.value?.collection.add(elementView.model)
      }
    },
  )

  graph.value?.on('remove', (cell: joint.dia.Cell) => {
    // If element is removed from the graph, remove from the selection too.
    if (selection.value?.collection.has(cell)) {
      selection.value?.collection.reset(
        selection.value?.collection.models.filter((c) => c !== cell),
      )
    }
  })

  selection.value?.on(
    'selection-box:pointerdown',
    (elementView: joint.dia.ElementView, evt: joint.dia.Event) => {
      // Unselect an element if the CTRL/Meta key is pressed while a selected element is clicked.
      if (keyboard!.isActive('ctrl meta', evt)) {
        selection.value?.collection.remove(elementView.model)
      }
    },
  )

  selection.value?.on('selection-box:pointerup', (_, evt: joint.dia.Event) => {
    if (evt.button === 2) {
      evt.stopPropagation()
      renderContextToolbar(
        { x: evt.clientX ?? 0, y: evt.clientY ?? 0 },
        selection.value?.collection.toArray() || [],
      )
    }
  })
}

function renderContextToolbar(point: joint.dia.Point, selectedCells: joint.dia.Cell[] = []) {
  selection.value?.collection.reset(selectedCells)
  const isSelectionEmpty = selectedCells.length === 0

  const contextToolbar = new joint.ui.ContextToolbar({
    target: point,
    root: paper.value?.el,
    padding: 0,
    vertical: true,
    anchor: 'top-left',
    tools: [
      {
        action: 'delete',
        content: 'Delete',
        attrs: {
          disabled: isSelectionEmpty,
        },
      },
      {
        action: 'copy',
        content: 'Copy',
        attrs: {
          disabled: isSelectionEmpty,
        },
      },
      {
        action: 'paste',
        content: 'Paste',
        attrs: {
          disabled: clipboard.value?.isEmpty(),
        },
      },
      {
        action: 'send-to-front',
        content: 'Send to front',
        attrs: {
          disabled: isSelectionEmpty,
        },
      },
      {
        action: 'send-to-back',
        content: 'Send to back',
        attrs: {
          disabled: isSelectionEmpty,
        },
      },
    ],
  })

  contextToolbar.on('action:delete', () => {
    contextToolbar.remove()
    graph.value?.removeCells(selectedCells)
  })

  contextToolbar.on('action:copy', () => {
    contextToolbar.remove()

    clipboard.value?.copyElements(selectedCells, graph.value as joint.dia.Graph)
  })

  contextToolbar.on('action:paste', () => {
    contextToolbar.remove()
    const pastedCells =
      clipboard.value?.pasteCellsAtPoint(
        graph.value as joint.dia.Graph,
        paper.value?.clientToLocalPoint(point) as joint.dia.Point,
      ) || []

    const elements = pastedCells.filter((cell) => cell.isElement())

    // Make sure pasted elements get selected immediately. This makes the UX better as
    // the user can immediately manipulate the pasted elements.
    selection.value?.collection.reset(elements)
  })

  contextToolbar.on('action:send-to-front', () => {
    contextToolbar.remove()
    selectedCells.forEach((cell) => cell.toFront())
  })

  contextToolbar.on('action:send-to-back', () => {
    contextToolbar.remove()
    selectedCells.forEach((cell) => cell.toBack())
  })

  contextToolbar.render()
}

function onSelectionChange() {
  // const { paper, selection } = this;
  const { collection } = selection.value as joint.ui.Selection
  paper.value?.removeTools()
  joint.ui.Halo.clear(paper.value as joint.dia.Paper)
  joint.ui.FreeTransform.clear(paper.value as joint.dia.Paper)
  joint.ui.Inspector.close()
  if (!collection.length) {
    selectedCell.value = null
    cellSelected.value = false
    joint.ui.Inspector.close()
  }
  if (collection.length === 1) {
    const primaryCell: joint.dia.Cell = collection.first()
    const primaryCellView = paper.value?.findViewByModel(primaryCell)
    selection.value?.destroySelectionBox(primaryCell)
    selectPrimaryCell(primaryCellView as joint.dia.CellView)
  } else if (collection.length === 2) {
    collection.each(function (cell: joint.dia.Cell) {
      selection.value?.createSelectionBox(cell)
    })
  }
}

function selectPrimaryCell(cellView: joint.dia.CellView) {
  const cell = cellView.model
  if (cell.isElement()) {
    selectPrimaryElement(<joint.dia.ElementView>cellView)
  } else {
    selectPrimaryLink(<joint.dia.LinkView>cellView)
  }

  //inspectorComponent.value.create(cell);
  selectedCell.value = cell
  cellSelected.value = true
}

function deleteElement() {
  alert('Are you sure you want to delete this element?')
}

function selectPrimaryElement(elementView: joint.dia.ElementView) {
  const element = elementView.model
  // new joint.ui.FreeTransform({
  //     cellView: elementView,
  //     allowRotation: false,
  //     preserveAspectRatio: !!element.get('preserveAspectRatio'),
  //     allowOrthogonalResize: element.get('allowOrthogonalResize') !== false,
  //     useBordersToResize: true
  // }).render();

  // haloService.create(elementView);

  // if (!selection.value?.collection.models.includes(element)) {
  if (
    element.attributes.type !== 'planmatr.Carcass' &&
    element.attributes.type !== 'planmatr.Column' &&
    element.attributes.type !== 'planmatr.Row' &&
    element.attributes.type !== 'standard.Rectangle' &&
    element.attributes.type !== 'planmatr.Upright' &&
    element.attributes.type !== 'planmatr.Header'
  ) {
    if (element.isElement()) {
      //determine stand type and brand - cannot delete
      let halo
      if (element.attributes.disableMove) {
        halo = haloService.createNoRem(elementView)
      } else {
        halo = haloService.create(elementView)
      }

      if (stand.value.shelfLock && element.attributes.type == 'planx.Part.Shelf') {
        halo.removeHandle('clone')
      }

      halo.on('action:clone:pointerdown', function (clone: any) {
        //we need to release any selection that might be happening here.
        // selection.value?.collection.reset([]);
      })

      halo.on('action:clone:pointerup', function (clone: any) {
        var model = clone.data[Object.keys(clone.data)[0]].delegatedView.model
        if (model.isElement()) {
          if (model.attributes.type == 'planmatr.Part.Cassette') {
            model.attributes.partInfo.planogramPartId = 0
            model.attributes.partInfo.notes = ''
            model.attributes.partInfo.hasNotes = false
            // let inspectorEl = currentInspector.value?.el
            // if (inspectorEl) {
            //   let notesVal = inspectorEl.querySelector('[data-attribute="partInfo/notes"]')
            //   if (notesVal) {
            //     notesVal.textContent = ''
            //   }
            // }

            model.attributes.hasNotes = false
            model.attributes.attrs.commentIndicator.visibility = 'hidden'
            //need to make updates that will persist with redo/undo
            var statusColour = StatusColourEnum[model.attributes.partInfo.statusId]

            if (currentView.value != CurrentView.render) {
              model.attributes.attrs.body.fill = statusColour
            }

            let modelView = model.findView(paper.value as joint.dia.Paper)
            if (modelView) {
              modelView.render()
            }
            //need to remove shades from any cloned part.
            for (var i = 1; i <= model.attributes.partInfo.facings; i++) {
              if (model.attributes.partInfo['selectedShade-facing-' + i] != null) {
                model.attributes.partInfo['selectedShade-facing-' + i] = null
              }
            }
            let currElement = new joint.dia.Element()
            Object.assign(currElement, model)
            displayItemPlanoView(currElement, '', currentView.value)
          }
          if (model.attributes.type == 'planmatr.Part.Shelf') {
            //Need to remove label
            commandManager.value?.initBatchCommand()
            model.attributes.shelfInfo.planogramShelfId = 0
            if (isCluster) {
              model.attributes.shelfInfo.clusterShelfId = 0
            }
            model.attributes.attrs['#label'].text = ''
            var view = model.findView(paper.value as joint.dia.Paper)
            if (view) {
              view.render()
            }
            // let inspectorEl = currentInspector.value?.el
            // if (inspectorEl) {
            //   let label = inspectorEl?.querySelector(
            //     '[data-attribute="attrs/#label/text"]',
            //   ) as HTMLInputElement
            //   label.value = ''
            // }
            commandManager.value?.storeBatchCommand()
          }
        }
      })
      halo.on('action:remove:pointerdown', function (this: any, evt: any) {
        evt.stopPropagation()
        var self = this
        var msgContent = '<b>Are you sure you want to remove this item?</b>'
        if (self.options.cellView.model.attributes.shapeType == 'Shelf') {
          msgContent =
            "<b>Are you sure you want to remove this shelf? Clicking remove will remove the shelf and it's contents.</b>"
        }
        var dialog = new joint.ui.Dialog({
          width: 400,
          title: 'Confirm',
          content: msgContent,
          buttons: [
            { action: 'yes', content: 'Yes' },
            { action: 'no', content: 'No' },
          ],
        })

        dialog.on(
          'action:yes',
          function (event: any) {
            self.options.cellView.model.remove()
            dialog.close()
          },
          dialog,
        )
        dialog.on(
          'action:no',
          function (event: any) {
            dialog.close()
          },
          dialog,
        )
        dialog.open()
      })

      // selection.value?.collection.add(element, { silent: true });
      planogramService.value?.orderCellOverlays(element)
    }

    //disbale the name field for editing

    // const newInspector = await currentInspector.value?.el;
    // const inspectorEl = currentInspector.value?.el as HTMLElement;

    // //maxlength value to be derived by shelflength (if it's a shelf) shelf length / 4.5mm ? From Diam

    // setInspectorProps(inspectorEl as HTMLElement, element);
  }
  // }
}

function selectPrimaryLink(linkView: joint.dia.LinkView) {
  const ns = joint.linkTools
  const tools = [
    new ns.Vertices(),
    new ns.SourceAnchor(),
    new ns.TargetAnchor(),
    new ns.SourceArrowhead(),
    new ns.TargetArrowhead(),
    new ns.Segments({
      visibility: function (linkView) {
        return linkView.model.router()?.name === 'normal'
      },
    }),
    new ns.Boundary({ padding: 15 }),
    new ns.Remove({ offset: -20, distance: 40 }),
  ]

  const toolsView = new joint.dia.ToolsView({
    name: 'link-pointerdown',
    tools,
  })

  linkView.addTools(toolsView)
}

function initializeElementDrop() {
  paper.value?.on(
    'element:pointerup link:options',
    (cellView: joint.dia.CellView, evt: joint.dia.Event) => {
      // need to ensure we don't do anything on clicking shape types other than shelf or cassette - like carcass for example
      if (cellView.model.attributes.type == 'planx.Part.Shelf') {
        checkIsOnScratchPad(cellView.model)
      } else if (cellView.model.attributes.type == 'planx.Part.Cassette') {
        if (
          cellView.model.attributes.partInfo.partTypeId == PartTypes.Glorifier ||
          cellView.model.attributes.partInfo.partTypeId == PartTypes.Blanking ||
          cellView.model.attributes.partInfo.partTypeId == PartTypes.FasciaPlate
        ) {
          planogramService.value?.orderCellOverlays(cellView.model)
        }
        checkIsOnScratchPad(cellView.model)
      }
    },
  )
  selection.value?.on(
    'selection-box:pointerdown',
    (elementView: joint.dia.ElementView, evt: joint.dia.Event, x: number, y: number) => {
      commandManager.value?.initBatchCommand()
    },
  )
  selection.value?.on(
    'selection-box:pointerup',
    (elementView: joint.dia.ElementView, evt: joint.dia.Event, x: number, y: number) => {
      //this.commandManager.initBatchCommand();
      if (!selection.value?.collection?.length || selection.value.collection.length == 0) {
        return
      } else {
        for (let i = 0; i < selection.value.collection.length; i++) {
          checkIsOnScratchPad(selection.value?.collection.models[i])
          if (selection.value?.collection.models[i].attributes.shapeType == 'Shelf') {
            //then we need to ensure all items on the shelf are on the scratchpad or not
            const cassettes = selection.value?.collection.models[i].attributes.embeds
            if (cassettes) {
              for (var c = 0; c < cassettes.length; c++) {
                var cassette = graph.value?.getCell(cassettes[c])
                if (cassette && cassette.isElement()) {
                  checkIsOnScratchPad(cassette)
                }
              }
            }
            //don't forget to check the shelf
            //self.checkIsOnScratchPad(self.selection.collection.models[i]);
            //we can now exit this function as we will have checked all items now.
          }
        }
        selection.value?.collection.reset([])

        commandManager.value?.storeBatchCommand()
      }
    },
  )
}

async function initializeToolsAndInspector() {
  paper.value?.on('cell:pointerup', async (cellView: joint.dia.CellView) => {
    const cell = cellView.model as joint.dia.Cell
    const { collection } = selection.value as joint.ui.Selection
    for (const c of collection.models) {
      if (c.id === cell.id) {
        return
      }
    }

    collection.reset([cell])
  })

  paper.value?.on('link:mouseenter', (linkView: joint.dia.LinkView) => {
    // Open tool only if there is none yet
    if (linkView.hasTools()) {
      return
    }

    const ns = joint.linkTools
    const toolsView = new joint.dia.ToolsView({
      name: 'link-hover',
      tools: [
        new ns.Vertices({ vertexAdding: false }),
        new ns.SourceArrowhead(),
        new ns.TargetArrowhead(),
      ],
    })

    linkView.addTools(toolsView)
  })

  paper.value?.on('link:mouseleave', (linkView: joint.dia.LinkView) => {
    // Remove only the hover tool, not the pointerdown tool
    if (linkView.hasTools('link-hover')) {
      linkView.removeTools()
    }
  })

  graph.value?.on('change', (cell: joint.dia.Cell, opt: joint.dia.Cell.Options) => {
    if (cell instanceof joint.dia.Graph || !cell.isLink() || !opt.inspector) {
      return
    }

    const ns = joint.linkTools
    const toolsView = new joint.dia.ToolsView({
      name: 'link-inspected',
      tools: [new ns.Boundary({ padding: 15 })],
    })

    cell.findView(paper.value as joint.dia.Paper).addTools(toolsView)
  })
}

async function initializeCassetteOperations() {
  paper.value?.on('blank:pointerdown', function (evt: any, x: any, y: any) {
    //alert('paperClicked');
  })

  paper.value?.on('cell:mouseenter', function (cellView: joint.dia.CellView, evt: any) {
    const cell = cellView.model
    if (cell.attributes.disableMove) {
      cellView.el.style.cursor = 'pointer'
    }
  })
  paper.value?.on('element:pointerdown', (cellView: joint.dia.CellView) => {
    const cell = cellView.model

    if (
      cell.isElement() &&
      cell.attributes.type == 'planmatr.Part.Cassette' &&
      cell.attributes.partInfo.partType != PartTypes.Blanking
    ) {
    } else if (cell.isElement() && cell.attributes.type == 'planmatr.Part.Shelf') {
      //select all child cassettes of shelf and bring them forward
      //var cellView = this.findViewByModel(cell);
      var cassettes = cellView.model.attributes.embeds
      cell.toFront({ ignoreCommandManager: true })
      if (cassettes) {
        for (var i = 0; i < cassettes.length; i++) {
          var cassette = graph.value?.getCell(cassettes[i]) as joint.dia.Cell
          if (cassette.isElement()) {
            cassette.toFront({ ignoreCommandManager: true })
          }
        }
        for (var i = 0; i < cassettes.length; i++) {
          var cassette = graph.value?.getCell(cassettes[i]) as joint.dia.Cell
          if (cassette.isElement()) {
            if (cassette.attributes.partInfo.partTypeId === PartTypes.FasciaPlate) {
              cassette.toFront({ ignoreCommandManager: true })
            }
          }
        }
        for (var i = 0; i < cassettes.length; i++) {
          //ensure blanking graphics are on top
          const cassette = graph.value?.getCell(cassettes[i]) as joint.dia.Cell
          if (cassette.isElement()) {
            if (cassette.attributes.partInfo.partTypeId === PartTypes.Blanking) {
              cassette.toFront({ ignoreCommandManager: true })
            }
          }
        }
      }
    }
    //}
  })

  paper.value?.on('element:pointerup', (cellView: any, options: any) => {
    const cell = cellView.model
    const { collection } = selection.value as joint.ui.Selection
    if (collection.includes(cellView)) {
      var cassetteService = new CassetteService()
      //now we must generate the product types.
      //generate product types using api
      if (cell.attributes.type === 'planmatr.Part.Cassette') {
        // if (typeof (cell.attributes.partInfo.products) === 'string') {
        //   if (cell.attributes.partInfo.facings > 0) {

        var partId = cell.attributes.partInfo.partid
        cassetteService
          .getPartProducts(partId, planogramStore.planogram.id as number)
          .then(function (data) {
            gotPartProductData(cell, data)
            //self.planogramService.orderAllOverlays(self.graph);
          })
        //   }
        // }
      }
    }
  })

  paper.value?.on('element:pointermove', (cellView: joint.dia.CellView, options: any) => {
    if (
      cellView.model.attributes.type === 'planmatr.Part.Cassette' ||
      cellView.model.attributes.type === 'planmatr.Part.Shelf'
    ) {
      cellView.model.toFront()
    }
  })

  graph.value?.on('add', function (cell: joint.dia.Cell, collection: any, opt: any) {
    // The stencil adds the `stencil` property to the option object with value
    // set to a client id (`cid`) of the stencil view.
    if (opt.stencil) {
      console.log('A cell with id', cell.id, 'was just added to the paper from the stencil.')
      var halo
      var cellView = cell.findView(paper.value as joint.dia.Paper)
      halo = haloService.create(cellView)

      //(hide the halo label box with the position info in it)
      // halo?.$box.toggle();
      //need to convert this cell to a different cell.
      if (
        cell.isElement() &&
        cell.attributes.type == 'planmatr.Part.Cassette' &&
        cell.attributes.partInfo.partType == 'Cassette'
      ) {
        planogramService.value?.snapToShelf(
          paper.value as joint.dia.Paper,
          graph.value as joint.dia.Graph,
          stand.value,
          cell,
          planogramStore.planogram.scratchPadId as number,
        )
        var cassetteService = new CassetteService()
        //now we must generate the product types.
        //generate product types using api
        if (typeof cell.attributes.partInfo.products === 'string') {
          var partId = cell.attributes.partInfo.partid
          if (cell.attributes.partInfo.facings > 0) {
            cassetteService
              .getPartProducts(partId, planogramStore.planogram.id as number)
              .then(function (data) {
                gotPartProductData(cell, data)
              })
          }
        }
        //finally change view to be same as currently selected plano view.
        let currElement = new joint.dia.Element()
        let element = graph.value?.getCell(cell.attributes.id)
        Object.assign(currElement, element)
        displayItemPlanoView(currElement, '', currentView.value as CurrentView)
      } else if (cell.isElement() && cell.attributes.type == 'planmatr.Part.Shelf') {
        planogramService.value?.SnapShelfToStand(
          cell,
          graph.value as joint.dia.Graph,
          stand.value,
          planogramStore.planogram.scratchPadId as number,
        )
        //finally change view to be same as currently selected plano view.
        let currElement = new joint.dia.Element()
        let element = graph.value?.getCell(cell.attributes.id)
        Object.assign(currElement, element)
        planogramRenderService.value?.displayItemShelf(paper.value as joint.dia.Paper, currElement)
      } else if (
        (cell.isElement() && cell.attributes.partInfo.partTypeId == PartTypes.Blanking) ||
        cell.attributes.partInfo.partTypeId == PartTypes.FasciaPlate
      ) {
        let currElement = new joint.dia.Element()
        let element = graph.value?.getCell(cell.attributes.id)
        Object.assign(currElement, element)
        displayItemPlanoView(currElement, '', currentView.value as CurrentView)
      }
    }
  })
}

function initializeNavigator() {
  // navigator.value?.create();
}

// function initializeToolbar() {
//     toolbarService.value?.create(commandManager.value as joint.dia.CommandManager, paperScroller.value as joint.ui.PaperScroller, graph.value as joint.dia.Graph, paper.value as joint.dia.Paper);
// }

async function savePlanogram() {
  // if (planogramService.value) {
  // const savePlanogramService = new SaveService(
  //     planogramService.value as PlanogramService,
  //     utilitiesService.value as UtilitiesService,
  //     commandManager.value as joint.dia.CommandManager,
  //     graph.value as joint.dia.Graph,
  //     paper.value as joint.dia.Paper,
  //     stand.value,
  //     carcass.value as planmatr.Carcass,
  //     isCluster.value,
  //     partOverlap.value,
  //     partOverlapAmount.value,
  //     planogram.value?.id as number,
  //     planogram.value?.name as string,
  //     props.clusterId as number,
  //     props.appMode
  // );
  //     await savePlanogramService.savePlanogram(scratchPadHidden.value as boolean, selection.value as joint.ui.Selection, currentInspector.value as joint.ui.Inspector, currentView.value as CurrentView);
  // }
  toggleScratchPad()
  if (props.appMode === AppMode.Cluster) {
    await useSavePlanogram(
      props.appMode,
      clusterStore.cluster.brandId,
      0,
      // clusterStore.cluster.countryId,0
    ).savePlanogram(
      clusterStore.cluster.name as string,
      clusterStore.cluster.id as number,
      props.clusterId as number,
      currentView.value as CurrentView,
      commandManager.value as joint.dia.CommandManager,
      paper.value as joint.dia.Paper,
      graph.value as joint.dia.Graph,
      carcass.value as planmatr.Carcass,
      stand.value,
      partOverlap.value,
      partOverlapAmount.value,
    )
  } else {
    await useSavePlanogram(
      props.appMode,
      planogramStore.planogram.brandId,
      planogramStore.planogram.countryId,
    ).savePlanogram(
      planogramStore.planogram.name as string,
      planogramStore.planogram.id as number,
      props.clusterId as number,
      currentView.value as CurrentView,
      commandManager.value as joint.dia.CommandManager,
      paper.value as joint.dia.Paper,
      graph.value as joint.dia.Graph,
      carcass.value as planmatr.Carcass,
      stand.value,
      partOverlap.value,
      partOverlapAmount.value,
    )
  }
  // toggleScratchPad();
}

function renderPlugin(selector: string, plugin: any): void {
  ;(paperContainer.value?.querySelector(selector) as HTMLElement).appendChild(plugin.el)
  plugin.render()
}

function changeSnapLines(checked: boolean) {
  if (checked) {
    snaplines.value?.enable()
  } else {
    snaplines.value?.disable()
  }
}

function addPageBreaks() {
  // const { paper, pageBreakSettings } = this;
  //const { color, width, height } = pageBreakSettings;

  const pageBreaksVEl = joint.V('path', {
    stroke: pageBreakSettings.color,
    fill: 'none',
    strokeDasharray: '5,5',
  })

  paper.value?.layers.prepend(pageBreaksVEl.node)

  let lastArea: joint.g.Rect | null = null

  function updatePageBreaks() {
    const area = paper.value?.getArea()
    if (!area) return
    // Do not update if the area is the same
    if (lastArea && area.equals(lastArea)) return
    lastArea = area
    let d = ''
    // Draw vertical lines
    // Do not draw the first and last lines
    for (let x = pageBreakSettings.width; x < area.width; x += pageBreakSettings.width) {
      d += `M ${area.x + x} ${area.y} v ${area.height}`
    }
    // Draw horizontal lines
    // Do not draw the first and last lines
    for (let y = pageBreakSettings.height; y < area.height; y += pageBreakSettings.height) {
      d += ` M ${area.x} ${area.y + y} h ${area.width}`
    }
    pageBreaksVEl.attr('d', d || null)
  }

  updatePageBreaks()

  paper.value?.on('translate resize', updatePageBreaks)

  return () => {
    paper.value?.off('translate resize', updatePageBreaks)
    pageBreaksVEl.remove()
  }
}

function initializeKeyboardShortcuts() {
  keyboardService.create(
    graph.value as joint.dia.Graph,
    clipboard.value as joint.ui.Clipboard,
    selection.value as joint.ui.Selection,
    paperScroller.value as joint.ui.PaperScroller,
    commandManager.value as joint.dia.CommandManager,
  )
}

function initializeTooltips() {
  tooltip.value = new joint.ui.Tooltip({
    rootTarget: document.body,
    target: '[data-tooltip]',
    direction: joint.ui.Tooltip.TooltipArrowPosition.Auto,
    padding: 12,
    animation: {
      delay: '250ms',
    },
  })
}

async function showCommentsDialog() {
  // Implement the logic to show the comments dialog
  // await noteService.initialise();
  // planogramNotes.value = await noteService.getNotes(planogram.id);
  showNotesDialog.value = true
}

function showRenderView() {
  // const self = this;
  //let planogramService = new PlanogramService(this.isCluster, this.partOverlap, this.partOverlapAmount, this.graph, this.paper);
  planogramRenderService.value = new PlanogramRenderService()
  planogramRenderService.value.displayRenderView(
    paper.value as joint.dia.Paper,
    graph.value as joint.dia.Graph,
  )
  if (paperContainer.value) {
    const facingElements = paperContainer.value.querySelectorAll('[data-name^=facing]')
    facingElements.forEach(function (facingEl) {
      ;(facingEl as HTMLElement).style.display = 'none'
    })
  }
  if (currentInspector.value != null) {
    const inspectorEl = currentInspector.value.el
    const facingButton = inspectorEl.querySelector('[data-attribute="partInfo/selectShades"]')
    if (facingButton) {
      ;(facingButton.querySelector('button') as HTMLElement).setAttribute('disabled', 'disabled')
    }
  }
  currentView.value = CurrentView.render
}

function showCassetteView() {
  // const self = this;
  //let planogramService = new PlanogramService(this.isCluster, this.partOverlap, this.partOverlapAmount, this.graph, this.paper);
  paperContainer.value?.querySelectorAll('[data-name^=facing]').forEach((facingEl) => {
    ;(facingEl as HTMLElement).style.display = 'none'
  })
  planogramRenderService.value?.displayCassetteView(
    paper.value as joint.dia.Paper,
    graph.value as joint.dia.Graph,
  )
  if (currentInspector.value != null) {
    const inspectorEl = currentInspector.value?.el
    //  if (this.currentView !== CurrentView.shade) {
    const facingButton = inspectorEl?.querySelector('[data-attribute="partInfo/selectShades"]')
    if (facingButton) {
      ;(facingButton.querySelector('button') as HTMLElement).setAttribute('disabled', 'disabled')
    }
    //  }
  }
  currentView.value = CurrentView.cassette
}

function showShadeView() {
  // const self = this;
  //let planogramService = new PlanogramService(this.isCluster, this.partOverlap, this.partOverlapAmount, this.graph, this.paper);
  planogramRenderService.value = new PlanogramRenderService()
  paperContainer.value?.querySelectorAll('[data-name^=facing]').forEach((facingEl) => {
    ;(facingEl as HTMLElement).style.display = 'block'
  })
  planogramRenderService.value.displayShadeView(
    paper.value as joint.dia.Paper,
    graph.value as joint.dia.Graph,
  )
  if (currentInspector.value != null) {
    const inspectorEl = currentInspector.value?.el

    if (currentView.value !== CurrentView.shade) {
      inspectorEl
        ?.querySelector('[data-attribute="partInfo/selectShades"]')
        ?.querySelector('button')
        ?.removeAttribute('disabled')
    }
  }
  currentView.value = CurrentView.shade
}

function toggleScratchPad() {
  // const self = this;
  let hide = true
  if (scratchPadHidden.value == true) {
    hide = false
    scratchPadHidden.value = false
  } else {
    hide = true
    scratchPadHidden.value = true
  }
  //deselect any selected items.
  selection.value?.cancelSelection()
  if (currentInspector.value) {
    currentInspector.value.remove()
  }
  selection.value?.collection.reset([])
  joint.ui.Halo.clear(paper.value as joint.dia.Paper)

  const planogramService = new PlanogramService(
    isCluster.value,
    partOverlap.value,
    partOverlapAmount.value,
    graph.value as joint.dia.Graph,
    paper.value as joint.dia.Paper,
  )
  planogramService.toggleScratchPad(
    paper.value as joint.dia.Paper,
    graph.value as joint.dia.Graph,
    hide,
    currentView.value,
  )
  scratchPadHidden.value = hide
}

function toggleSnaplines() {
  if (snaplinesEnabled.value) {
    changeSnapLines(false)
    snaplinesEnabled.value = false
  } else {
    changeSnapLines(true)
    snaplinesEnabled.value = true
  }
}

function validatePlacement(
  err: Error,
  command: joint.dia.CommandManager.Command,
  next: (err: Error | null) => void,
) {
  try {
    planogramService.value = new PlanogramService(
      isCluster.value,
      partOverlap.value,
      partOverlapAmount.value,
      graph.value as joint.dia.Graph,
      paper.value as joint.dia.Paper,
    )
    // Parameter "command" contains all cells attributes (command.data.attributes) in case
    // it was added or removed to/from graph.
    // Otherwise (in case an attribute was changed) it keeps only changed attribute
    // (command.data.previous, command.data.next) - in order to know rest of the attributes
    // you have to get them from the graph.
    const cell = graph.value?.getCell(command.data.id)
    const currElement = new joint.dia.Element()
    if (
      cell?.isElement() &&
      cell.attributes.type == 'planmatr.Part.Cassette' &&
      cell.attributes.partInfo.partType == 'Cassette' &&
      !(cell.attributes.partInfo.partTypeId == PartTypes.Blanking)
    ) {
      Object.assign(currElement, cell)
      // let cellView = this.paper.findViewByModel(cell);
      const validationService = new ValidationService(
        planogramService.value as PlanogramService,
        graph.value as joint.dia.Graph,
        paper.value as joint.dia.Paper,
        cell,
        isCluster.value,
      )
      if (
        validationService.validateCassettePosition(
          currElement,
          command,
          stand.value,
          planogramStore.planogram as Planogram,
        )
      ) {
        return next(err)
      } else {
        return next(new Error('Cannot place cassette here!'))
      }
    } else if (
      cell?.isElement() &&
      cell.attributes.type == 'planmatr.Part.Cassette' &&
      (cell.attributes.partInfo.partTypeId == PartTypes.Accessory ||
        cell.attributes.partInfo.partTypeId == PartTypes.Blanking ||
        cell.attributes.partInfo.partTypeId == PartTypes.Glorifier ||
        cell.attributes.partInfo.partTypeId == PartTypes.FasciaPlate)
    ) {
      Object.assign(currElement, cell)
      // let cellView = this.paper.findViewByModel(cell);
      const validationService = new ValidationService(
        planogramService.value as PlanogramService,
        graph.value as joint.dia.Graph,
        paper.value as joint.dia.Paper,
        cell,
        isCluster.value,
      )
      if (
        validationService.validateAccessoryPosition(
          currElement,
          command,
          stand.value,
          planogramStore.planogram as Planogram,
        )
      ) {
        return next(err)
      } else {
        return next(new Error('Cannot place accessory here!'))
      }
    } else if (
      (cell?.isElement() && cell.attributes.type == 'planmatr.Column') ||
      cell?.attributes.type == 'planmatr.Carcass'
    ) {
      if (!command.options.ignoreMove) {
        return next(new Error('Cannot move carcass'))
      } else return next(err)
    } else if (
      cell?.isElement() &&
      cell.attributes.type == 'planmatr.Part.Shelf' &&
      (cell.attributes.shelfInfo.shelfType == 'Shelf' ||
        cell.attributes.shelfInfo.shelfType == 'Base Shelf')
    ) {
      // let cellView = this.paper.findViewByModel(cell);
      Object.assign(currElement, cell)

      const validationService = new ValidationService(
        planogramService.value as PlanogramService,
        graph.value as joint.dia.Graph,
        paper.value as joint.dia.Paper,
        cell,
        isCluster.value,
      )
      if (
        validationService.validateShelfPosition(
          currElement,
          stand.value,
          planogramStore.planogram.scratchPadId ?? 0,
        )
      ) {
        //return false;
      } else {
        return next(new Error('Cannot place shelf here'))
      }
    } else return next(err)
  } catch (error) {
    console.log(error)
  }
}

// alert an error
function display(
  err: Error,
  command: joint.dia.CommandManager.Command,
  next: joint.dia.Validator.NextRule,
) {
  if (err) {
    new joint.ui.FlashMessage({
      title: 'Message',
      type: 'alert',
      content: err.toString(),
    }).open()
  }
  return next(err)
}

async function checkIsOnScratchPad(cell: joint.dia.Cell) {
  var opt = { overwrite: false }
  // var currElement = new joint.dia.Element;
  //const cell = cellView.model;

  var bbox = cell.getBBox()

  //implement overlap of cassettes here if needed
  if (planogramService.value?.partOverlap) {
    bbox.x = bbox.x + planogramService.value.partOverlapAmount
    bbox.width = bbox.width - planogramService.value.partOverlapAmount * 2
  }

  //need to handle updating element based on position here

  const isOnCarcass = graph.value?.findModelsInArea(bbox).filter(function (el: any) {
    return el.attributes.type == 'planx.Carcass'
  })

  if (isOnCarcass?.length == 0) {
    //added to scratchpad
    //some strange decimal position value is breaking the api
    if (!Number.isInteger(cell.attributes.position.x)) {
      cell.attributes.position.x = Math.round(cell.attributes.position.x)
    }
    if (!Number.isInteger(cell.attributes.position.y)) {
      cell.attributes.position.y = Math.round(cell.attributes.position.y)
    }

    if (cell.attributes.shapeType == 'Shelf') {
      setProperty('shelfInfo/scratchPadId', planogramStore.planogram?.scratchPadId ?? 0, opt, cell)
      setProperty(
        'planogramInfo/scratchPadId',
        planogramStore.planogram?.scratchPadId ?? 0,
        opt,
        cell,
      )

      //unembed the embeded cells from the shelf
      const cassettes = cell.attributes.embeds
      if (cassettes) {
        for (let i = 0; i < cassettes.length; i++) {
          const cassette = graph.value?.getCell(cassettes[i])
          setProperty(
            'partInfo/scratchPadId',
            planogramStore.planogram.scratchPadId ?? 0,
            opt,
            cassette,
          )
          if (cassette?.isElement()) {
            cell.unembed(cassette)
          }
        }
      }
    } else {
      setProperty('partInfo/scratchPadId', planogramStore.planogram.scratchPadId ?? 0, opt, cell)
      //unembed the cell from the shelf or stand before adding to the scratch pad.
      var parent = cell.getParentCell()
      if (parent != null) {
        parent.unembed(cell)
      }
      setInspectorProps(currentInspector.value?.el as HTMLElement, cell)
    }
  } else {
    if (cell.attributes.shapeType == 'Shelf') {
      if (
        cell.attributes.shelfInfo.scratchPadId != null &&
        cell.attributes.shelfInfo.scratchPadId != 0
      ) {
        setProperty('shelfInfo/scratchPadId', 0, opt, cell)
        //disbale the name field for editing
        const inspectorEl = currentInspector.value?.el

        //maxlength value to be derived by shelflength (if it's a shelf) shelf length / 4.5mm ? From Diam

        setInspectorProps(inspectorEl as HTMLElement, cell)
      }
      planogramService.value?.SnapShelfToStand(
        cell,
        graph.value as joint.dia.Graph,
        stand.value,
        planogramStore.planogram.scratchPadId ?? 0,
      )
    } else if (cell.attributes.shapeType == 'Cassette') {
      if (cell.attributes.partInfo.partTypeId == PartTypes.Cassette) {
        if (
          cell.attributes.partInfo.scratchPadId != null &&
          cell.attributes.partInfo.scratchPadId != 0
        ) {
          setProperty('partInfo/scratchPadId', 0, opt, cell)
          //disbale the name field for editing
          // const newInspector = await inspector.value?.createDynamic(cell);
          const inspectorEl = currentInspector.value?.el as HTMLElement

          //maxlength value to be derived by shelflength (if it's a shelf) shelf length / 4.5mm ? From Diam

          setInspectorProps(inspectorEl as HTMLElement, cell)
        }

        planogramService.value?.snapToShelf(
          paper.value as joint.dia.Paper,
          graph.value as joint.dia.Graph,
          stand.value,
          cell,
          planogramStore.planogram.scratchPadId ?? 0,
        )
      }
    } else if (
      cell.attributes.partInfo.partTypeId == PartTypes.Blanking ||
      cell.attributes.partInfo.partTypeId == PartTypes.FasciaPlate
    ) {
      if (
        cell.attributes.partInfo.scratchPadId != null &&
        cell.attributes.partInfo.scratchPadId != 0
      ) {
        setProperty('partInfo/scratchPadId', 0, opt, cell)
        //disbale the name field for editing
        // const newInspector = await inspector.value?.createDynamic(cell);
        const inspectorEl = currentInspector.value?.el as HTMLElement

        //maxlength value to be derived by shelflength (if it's a shelf) shelf length / 4.5mm ? From Diam

        setInspectorProps(inspectorEl, cell)
      }

      planogramService.value?.snapToShelf(
        paper.value as joint.dia.Paper,
        graph.value as joint.dia.Graph,
        stand.value,
        cell,
        planogramStore.planogram.scratchPadId ?? 0,
      )
    }
  }
}

function undoNotesIndicator(opt: any, isUndo: boolean) {
  var data = opt.data
  let cellId = data.id
  let cell = graph.value?.getCell(cellId) as joint.dia.Element

  if (data.type === 'planmatr.Part.Cassette') {
    if (cell.attributes.partInfo.notes == '' || cell.attributes.partInfo.notes == null) {
      cell.attributes.hasNotes = false
      if (
        cell.attributes.attrs != undefined &&
        cell.attributes.attrs['.comment-indi'] != undefined
      ) {
        cell.attributes.attrs['.comment-indi']['visibility'] = 'hidden'
      }
    } else {
      cell.attributes.hasNotes = true
      if (
        cell.attributes.attrs != undefined &&
        cell.attributes.attrs['.comment-indi'] != undefined
      ) {
        cell.attributes.attrs['.comment-indi']['visibility'] = 'visible'
        cell.attributes.attrs['.comment-indi']['display'] = 'block'
      }
    }
  }

  cell.findView(paper.value as joint.dia.Paper).render()

  if (data.type === 'planmatr.Part.Cassette') {
    //deal with the notes indicator
    if (cell.attributes.partInfo.notes === '' || cell.attributes.partInfo.notes === null) {
      document.querySelectorAll("[model-id='" + cellId + "']").forEach((el) => {
        el.querySelectorAll('circle').forEach((circle) => {
          circle.style.display = 'none'
        })
      })
    } else {
      document.querySelectorAll("[model-id='" + cellId + "']").forEach((el) => {
        el.querySelectorAll('circle').forEach((circle) => {
          circle.style.display = 'block'
        })
      })
    }
  }
}

function updateItemStatus(data: any) {
  // var self = this;
  var partInfo
  var currElement = new joint.dia.Element()

  let cellId = data.id
  let cell = graph.value?.getCell(cellId) as joint.dia.Element
  Object.assign(currElement, cell)

  if (data.type === 'planmatr.Part.Cassette') {
    if (data.attributes) {
      partInfo = data.attributes.partInfo
    } else {
      partInfo = cell.attributes.partInfo
    }
  } else if (data.type === 'planmatr.Part.Shelf') {
    partInfo = data.attributes.shelfInfo
  }
  if (currentView.value != CurrentView.render) {
    if (data.type === 'planmatr.Part.Shelf') {
      //ensure label is removed (this would only be here on an add if it were a clone)
      if (cell.attributes.attrs != undefined) {
        if (cell.attributes.attrs['text']) {
          cell.attributes.attrs['text']['text'] = ''
          cell.attributes.shelfInfo.label = ''
        }

        planogramService.value?.SnapShelfToStand(
          cell,
          graph.value as any,
          stand.value,
          planogramStore.planogram.scratchPadId as number,
        )
      }
    }
    if (data.type === 'planmatr.Part.Cassette') {
      if (partInfo.planogramPartId == 0) {
        cell.attributes.partInfo.notes = ''
        cell.attributes.partInfo.hasNotes = false
        var inspectorEl = currentInspector.value?.el
        if (inspectorEl) {
          ;(
            inspectorEl.querySelector('[data-attribute="partInfo/notes"]') as HTMLInputElement
          ).value = ''
        }

        cell.attributes.hasNotes = false
        if (
          cell.attributes.attrs != undefined &&
          cell.attributes.attrs['.comment-indicator'] != undefined
        ) {
          cell.attributes.attrs['.comment-indicator']['visibility'] = 'hidden'
        }
      }
    }

    document.querySelectorAll("[model-id='" + cellId + "']").forEach((el) => {
      el.querySelectorAll('.scalable').forEach((scalable) => {
        scalable.setAttribute('transform', 'scale(1,1)')
      })
    })

    cell.findView(paper.value as joint.dia.Paper).render()

    if (data.type === 'planmatr.Part.Cassette') {
      //deal with the notes indicatorf
      if (cell.attributes.partInfo.notes === '') {
        document.querySelectorAll("[model-id='" + cellId + "']").forEach((el) => {
          el.querySelectorAll('circle').forEach((circle) => {
            circle.style.display = 'none'
          })
        })
      } else {
        document.querySelectorAll("[model-id='" + cellId + "']").forEach((el) => {
          el.querySelectorAll('circle').forEach((circle) => {
            circle.style.display = 'block'
          })
        })
      }
    }

    if (cell.attributes.attrs != undefined) {
      if (cell.attributes.attrs['.body']) {
        if (partInfo.statusId == 0) {
          cell.attributes.attrs['.body']['fill-opacity'] = 1
        } else {
          cell.attributes.attrs['.body']['fill-opacity'] = 0.4
        }
      }
    } else {
      document.querySelectorAll("[model-id='" + cellId + "']").forEach((el) => {
        el.querySelectorAll('.body').forEach((body) => {
          body.setAttribute('stroke', '#000')
          body.setAttribute('fill', '#fff')
          body.setAttribute('fill-opacity', '1')
        })
      })
    }
  }
}

function setProperty(path: string, value: any, opt: any, model: any) {
  opt = opt || {}

  // The model doesn't have to be a JointJS cell necessarily. It could be
  // an ordinary Backbone.Model and such would have no method 'prop'.
  const prop = joint.dia.Cell.prototype.prop
  //var model = this.cassette;
  let overwrite = opt.overwrite || false

  if (value === undefined) {
    // Method prop can't handle undefined values in right way.
    // The model attributes would stay untouched if try to
    // set a nested property to undefined.
    joint.dia.Cell.prototype.removeProp.call(model, path, opt)
  } else {
    let updated

    if (joint.util.isObject(value) && !overwrite) {
      let current = prop.call(model, path, undefined)
      let targetType = Array.isArray(value) ? [] : {}
      updated = joint.util.merge(targetType, current, value)
    } else {
      updated = joint.util.clone(value)
    }

    if (overwrite) opt.rewrite = true
    prop.call(model, path, updated, opt)
  }
}

function setInspectorProps(inspectorEl: HTMLElement, cell: joint.dia.Cell) {
  if (cell.attributes.type == 'planx.Part.Shelf') {
    let maxLen = cell.attributes.shelfInfo.width / 4.85
    ;(
      inspectorEl.querySelector('[data-attribute="attrs/text/text"]') as HTMLInputElement
    ).maxLength = maxLen
    ;(inspectorEl.querySelector('[data-attribute="shelfInfo/name"]') as HTMLInputElement).disabled =
      true
    ;(
      inspectorEl.querySelector('[data-attribute="shelfInfo/partNumber"]') as HTMLInputElement
    ).disabled = true
  } else {
    ;(
      inspectorEl.querySelector('[data-attribute="attrs/text/text"]') as HTMLInputElement
    ).maxLength = 67
    ;(inspectorEl.querySelector('[data-attribute="partInfo/name"]') as HTMLInputElement).disabled =
      true
    ;(
      inspectorEl.querySelector('[data-attribute="partInfo/partNumber"]') as HTMLInputElement
    ).disabled = true
    ;(
      inspectorEl.querySelector('[data-attribute="partInfo/name"]') as HTMLInputElement
    ).setAttribute('disabled', 'disabled')
    ;(
      inspectorEl.querySelector('[data-attribute="partInfo/partNumber"]') as HTMLInputElement
    ).setAttribute('disabled', 'disabled')
    if (currentView.value !== CurrentView.shade) {
      ;(
        inspectorEl.querySelector(
          '[data-attribute="partInfo/selectShades"] button',
        ) as HTMLButtonElement
      ).setAttribute('disabled', 'disabled')
    }
  }
}

function displayPlanoView(option: string, index: number) {
  //console.log('option', option, 'at index', index, 'selected');
  //var self = this;

  switch (index) {
    case 0:
      showCassetteView()
      break
    case 1:
      showShadeView() //.bind(this);
      break
    case 2:
      showRenderView() //.bind(this);
      break
    default:
      showCassetteView()
      break
  }
}
function displayItemPlanoView(cell: joint.dia.Element, option: string, index: number) {
  switch (index) {
    case 0:
      planogramRenderService.value?.displayItemCassetteView(paper.value as joint.dia.Paper, cell)
      break
    case 1:
      planogramRenderService.value?.displayItemShadeView(paper.value as joint.dia.Paper, cell)
      break
    case 2:
      planogramRenderService.value?.displayItemRenderView(paper.value as joint.dia.Paper, cell)
      break
    default:
      planogramRenderService.value?.displayItemCassetteView(paper.value as joint.dia.Paper, cell)
      break
  }
}

function updateShades(updatedCell: joint.dia.Cell) {
  if (updatedCell.attributes.type == 'planmatr.Part.Cassette') {
    for (var i = 0; i < updatedCell.attributes.partInfo.facings; i++) {
      let facingProduct = updatedCell.attributes.partInfo['selectedProduct-facing-' + i]
      let facingShade = updatedCell.attributes.partInfo['selectedShade-facing-' + i]
      let facingStatus = updatedCell.attributes.partInfo['selectedStatus-facing-' + i]
      if (facingProduct != null && facingShade != null && facingStatus != null) {
        if (selectedCell.value != null) {
          selectedCell.value.prop('partInfo/selectedProduct-facing-' + i, facingProduct, {
            overwrite: true,
          })
          selectedCell.value.prop('partInfo/selectedShade-facing-' + i, facingShade, {
            overwrite: true,
          })
          selectedCell.value.prop('partInfo/selectedStatus-facing-' + i, facingStatus, {
            overwrite: true,
          })
        }
        // let statusColour = ShadeStatusColourEnum[facingStatus as keyof typeof ShadeStatusColourEnum];
        // let facingNo = i;
      }
    }
    if (currentView.value == CurrentView.shade) {
      let planogramRenderService = new PlanogramRenderService()
      planogramRenderService?.updateShadeView(
        paper.value as joint.dia.Paper,
        selectedCell.value as joint.dia.Element,
      )
      // selectedCell.value.findView(paper.value as joint.dia.Paper).render();
    }
  }
}
</script>

<template>
  <div id="app" ref="app" class="joint-app joint-theme-light">
    <div class="app-header">
      <!-- <img src="@/planner/assets/icons/joint-js.svg" alt="JointJS" /> -->
      <div class="app-title flex-1 app-header-text"><h2></h2></div>
      <div class="planmatr-type flex-1 app-header-text">
        <div class="stand-type"></div>
        <div class="sku-count">
          Sku <span class="count"></span>
          <div class="joint-widget joint-theme-planx-dark" data-type="divider"></div>
        </div>
        <div class="shelf-count">
          Shelf <span class="count"></span>
          <div class="joint-widget joint-theme-planx-dark" data-type="divider"></div>
        </div>
      </div>
    </div>
    <toolbar-component
      v-if="paperReady"
      :commandManager="commandManager as joint.dia.CommandManager"
      :paper="paper as joint.dia.Paper"
      :paperScroller="paperScroller as joint.ui.PaperScroller"
      :graph="graph as joint.dia.Graph"
      :notes="planogramNotes"
      @toolbarLoaded="initializeTooltips"
      @toggleScratchpad="toggleScratchPad"
      @toggleSnaplines="toggleSnaplines"
      @save-planogram="savePlanogram"
      @show-icon-view="showCassetteView"
      @show-shade-view="showShadeView"
      @show-render-view="showRenderView"
      @show-comments-dialog="showCommentsDialog"
      ref="toolbar"
    />
    <div class="app-body">
      <stencil-component
        v-if="paperReady"
        :paperScroller="paperScroller as joint.ui.PaperScroller"
        :selection="selection as joint.ui.Selection"
        :planogramId="planogramStore.planogram.id"
        :isCluster="isCluster"
        :clusterId="clusterStore.cluster.id"
        :searchGraph="searchGraph as joint.dia.Graph"
        :snaplines="snaplines as joint.ui.Snaplines"
        ref="stencil"
        @stencilLoaded="setupStencil"
      />
      <div ref="paperContainer" class="paper-container"></div>
      <inspector-component
        v-if="cellSelected"
        ref="inspector"
        :cell="selectedCell as joint.dia.Cell"
        :selection="selection as joint.ui.Selection"
        @shade-updated="updateShades"
      />
      <navigator-component
        v-if="paperReady"
        :paperScroller="paperScroller as joint.ui.PaperScroller"
        ref="navigator"
      />
    </div>
  </div>

  <div id="preloader" class="preloader-hide">
    <div id="loader"></div>
  </div>
  <Dialog
    v-model:visible="showSpinner"
    modal
    :closable="false"
    :dismissable-mask="false"
    class="plan-loading-dialog"
  >
    <ProgressSpinner />
  </Dialog>
</template>

<style lang="scss">
// import rappid styles
@import '@joint/plus/joint-plus.css';
// import custom styles
@import '@/planner/css/styles';
</style>
