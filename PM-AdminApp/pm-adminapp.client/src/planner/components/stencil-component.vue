<script lang="ts" setup>
// import { stencilGroups, stencilShapes } from '@/config/stencil.ts';
import { Menu } from '@/planner/models/Menu'
import { StencilGroups } from '@/planner/models/shapes/menu-shapes'
import * as pmshapes from '@/planner/models/shapes/planmatr-shapes'

import type { Category } from '@/planner/models/Category'
import type { MenuPart } from '@/planner/models/UserInfo'

import { MenuService } from '@/planner/services/menu.service'
// import { UtilitiesService } from '@/planner/services/UnusedServices/utilities.service';

import { dia, ui, util } from '@joint/plus'
import { onMounted, ref } from 'vue'

const menuService = ref<MenuService | null>(null)

const menuCategories = ref<Menu | null>(null)
const stencilGroups = ref<StencilGroups | null>(null)
const stencil = ref<ui.Stencil | null>(null)
// const extendedStencil = ref<ui.Stencil | null>(null);
const enteredGraph = ref(false)
const tooltipGraph = ref<dia.Graph | null>(null)
const tooltipPaper = ref<dia.Paper | null>(null)
const tooltip = ref<ui.Tooltip | null>(null)

const stencilContainer = ref<HTMLElement | null>(null)

const emit = defineEmits(['stencilLoaded'])
defineExpose({
  stencil,
  tooltipGraph,
  tooltipPaper,
  tooltip,
  loadSearchGraph,
})

const props = defineProps({
  paperScroller: {
    type: Object as () => ui.PaperScroller,
    required: true,
  },
  snaplines: {
    type: Object as () => ui.Snaplines,
    required: true,
  },
  selection: {
    type: Object as () => ui.Selection,
    required: true,
  },
  planogramId: {
    type: Number,
    required: true,
  },
  isCluster: {
    type: Boolean,
    required: true,
  },
  clusterId: {
    type: Number,
    required: true,
  },
  searchGraph: {
    type: Object as () => joint.dia.Graph,
    required: true,
  },
})

const HIGHLIGHT_COLOR = 'transparent'
//const HIGHLIGHT_COLOR = '#F4F7FB'; // Light blue color for the stencil hover effect
// Define a custom highlighter for the stencil hover effect
const StencilBackground = dia.HighlighterView.extend({
  tagName: 'rect',

  attributes: {
    stroke: 'none',
    fill: 'transparent',
    'pointer-events': 'none',
    rx: 4,
    ry: 4,
  },

  style: {
    transition: 'fill 400ms',
  },

  options: {
    padding: 4,
    color: 'black',
    width: 100,
    height: 100,
    layer: dia.Paper.Layers.BACK,
  },

  // Method called to highlight a CellView
  // eslint-disable-next-line
  highlight(cellView: dia.CellView, _node: Node) {
    const { padding, width, height } = this.options
    const bbox = cellView.model.getBBox()
    // Highlighter is always rendered relatively to the CellView origin
    bbox.x = bbox.y = 0
    // Custom width and height can be set
    if (Number.isFinite(width)) {
      bbox.x = (bbox.width - width) / 2
      bbox.x = 100
      bbox.width = width
      bbox.width = 100
    }
    if (Number.isFinite(height)) {
      bbox.y = (bbox.height - height) / 2
      bbox.y = 100
      bbox.height = height
      bbox.height = 100
    }
    // Increase the size of the highlighter
    bbox.inflate(padding)
    this.vel.attr(bbox.toJSON())
    // Change the color of the highlighter (allow transition)
    util.nextFrame(() => this.vel.attr('fill', this.options.color))
  },
})

onMounted(async () => {
  menuService.value = new MenuService()
  await menuService.value.initialise()
  if (!props.isCluster) {
    menuCategories.value = await menuService.value.loadMenuCategories(props.planogramId)
  } else {
    menuCategories.value = await menuService.value.loadClusterMenuCategories(props.clusterId)
  }

  stencilGroups.value = new StencilGroups()

  menuCategories.value?.categories.forEach((category) => {
    let i = menuCategories.value?.categories.indexOf(category)
    //let item = {category.CategoryName: { index: 1, label: category.CategoryName }};
    //Stencil.groups.push(item);
    let item = JSON.stringify(
      '{"index": ' +
        i +
        ', "label": "' +
        category.name +
        '", "categoryId": ' +
        category.categoryId +
        ', "closed": "true"}',
    )
    let itemJson = JSON.parse(item)
    stencilGroups.value![category.name] = itemJson
  })

  let extendedStencil = ui.Stencil.extend({
    toggleGroup: function (name: any) {
      this.trigger(
        'group:toggle',
        name,
        function done(this: any) {
          ui.Stencil.prototype.toggleGroup.call(this, name)
        }.bind(this),
      )
    },
    openGroups: function () {
      this.trigger(
        'groups:toggle',
        function done(this: any) {
          ui.Stencil.prototype.openGroups.call(this)
        }.bind(this),
      )
    },
    filter: function (keyword: any, cellAttributesMap: any) {
      this.trigger(
        'filter:graph',
        keyword,
        cellAttributesMap,
        function done(this: any) {
          ui.Stencil.prototype.filter.call(this, keyword, cellAttributesMap)
        }.bind(this),
      )
    },
  })

  stencil.value = new extendedStencil({
    label: 'Menu',
    paper: props.paperScroller,
    snaplines: props.snaplines,
    stencilService: self,
    scaleClones: true,
    width: 260,
    groups: stencilGroups.value,
    searchGraph: props.searchGraph,
    dropAnimation: true,
    groupsToggleButtons: true,

    search: {
      '*': [
        'partInfo/category',
        'partInfo/name',
        'partInfo/partNumber',
        'partInfo/altPartNumber',
        'partInfo/customerRefNo',
        'shelfInfo/category',
        'shelfInfo/name',
        'shelfInfo/partNumber',
        'shelfInfo/altPartNumber',
        'shelfInfo/customerRefNo',
      ],
    },

    layout: {
      columnWidth: 110,
      columns: 2,
      marginY: 10,
      rowHeight: 160,
      rowGap: 10,
      centre: false,
      horizontalAlign: 'left',
      verticalAlign: 'top',
      dx: 0,
      // resizeToFit: true
    },
    // height: '2200px',
    // Remove tooltip definition from clone
    dragStartClone: function (cell: any) {
      //NEED TO CHECK IF CASSETTE OR SHELF
      const clone = createFromStencilElement(cell)
      clone.attr({
        label: {
          text: cell.get('name'),
        },
      })
      clone.unset('name')
      return clone
    },
    el: stencilContainer.value,
  })

  //Handle scrolling the paper while dragging a stencil element

  stencil.value?.on('element:dragstart', (elementView: any, e: any, evt: any) => {
    enteredGraph.value = false
  })

  stencil.value?.on('element:drag', (elementView: any, evt: any, cloneArea: any) => {
    const { x, y } = cloneArea.center()
    props.paperScroller.scrollWhileDragging(evt, x, y, {
      padding: -20,
      scrollingFunction: (distance) => {
        if (distance < 5) enteredGraph.value = true
        if (!enteredGraph) return 0
        return distance < 20 ? 5 : 20
      },
    })
  })

  stencil.value?.on('element:dragend', (elementView: any, evt: any) => {
    props.paperScroller.stopScrollWhileDragging(evt)
  })

  // End of scrolling the paper while dragging a stencil element

  stencil.value?.on('groups:toggle', async function (this: any, done: any, self: joint.ui.Stencil) {
    let isCluster = this.options.stencilService.isCluster
    // let utilitiesService = new UtilitiesService(
    //   isCluster,
    //   this.partOverlap,
    //   this.partOverlapAmount,
    //   this.graph,
    //   this.paper,
    // )

    for (let item in this.papers) {
      let populated = this.papers[item].model.attributes.cells.length
      let matchedCells = this.options.searchGraph.get('cells').filter(function (cell: any) {
        return cell.attributes.groupName == item
      })
      if (populated < matchedCells.length) {
        this.trigger(
          'group:toggle',
          item,
          function done(this: any) {
            ui.Stencil.prototype.toggleGroup.call(this, item)
          }.bind(this),
        )
        // utilitiesService.toggleSpinner(false)
      }
    }
    // this.stencil.render();
    done()
  })

  stencil.value?.on(
    'group:toggle',
    async function (this: any, name: any, done: any, self: joint.ui.Stencil) {
      let isCluster = this.options.stencilService.isCluster
      // let utilitiesService = new UtilitiesService(
      //   isCluster,
      //   this.partOverlap,
      //   this.partOverlapAmount,
      //   this.graph,
      //   this.paper,
      // )
      let populated = this.papers[name].model.attributes.cells.length

      let matchedCells = this.options.searchGraph.get('cells').filter(function (cell: any) {
        return cell.attributes.groupName == name
      })

      if (populated == 0) {
        //$('#preloader').attr('class', 'preloader-show');
        // utilitiesService.toggleSpinner(true)
        // const urlParams = new URLSearchParams(window.location.search);
        // let planogramId = +urlParams.get('planogramId');
        // let clusterId = +urlParams.get('clusterId');
        // let menuList: MenuPart[];
        // let menuService = new MenuService;
        // let menuData: any;
        //let stencilService = new StencilService(this.stencilContainer);
        if (!isCluster) {
          populateStencilSearchResults(matchedCells, this)
        } else {
          populateStencilSearchResults(matchedCells, this)
        }
        // utilitiesService.toggleSpinner(false)
        //$('#preloader').attr('class', 'preloader-hide');
      }
      done()
    },
  )

  stencil.value?.on(
    'filter:graph',
    async function (
      this: any,
      keyword: any,
      cellAttributesMap: any,
      done: any,
      self: joint.ui.Stencil,
    ) {
      // a searching mode when the keyword consists of lowercase only
      // e.g 'keyword' matches 'Keyword' but not other way round
      let lowerCaseOnly = keyword.toLowerCase() == keyword

      // an array of cells that matches a search criteria
      let matchedCells = this.options.searchGraph.get('cells').filter(function (cell: any) {
        //let cellView = this.options.searchGraph.findViewByModel(cell);

        // check whether the currect cell matches a search criteria
        let cellMatch =
          !keyword ||
          Object.keys(cellAttributesMap).some(function (type) {
            let paths = cellAttributesMap[type]

            if (type != '*' && cell.get('type') != type) {
              // type is not universal and doesn't match the current cell
              return false
            }

            // find out if any of specific cell attributes matches a search criteria
            let attributeMatch = paths.some(function (path: any) {
              let value = util.getByPath(cell.attributes, path, '/')

              if (value === undefined || value === null) {
                // if value undefined than current attribute doesn't match
                return false
              }

              // convert values to string first (e.g value could be a number)
              value = value.toString()

              if (lowerCaseOnly) {
                value = value.toLowerCase()
              }

              return value.indexOf(keyword) >= 0
            })

            return attributeMatch
          })

        // each element that does not match a search has 'unmatched' css class
        //joint.V(cellView.el).toggleClass('unmatched', !cellMatch);

        return cellMatch
      }, this)

      //let stencilService = new StencilService(stencilContainer.value);
      //stencilService.populateStencilSearchResults(matchedCells, this);
      populateStencilSearchResults(matchedCells, this)
      done()
    },
  )

  stencil.value?.render()

  stencil.value?.on({
    'element:dragstart': () => tooltip.value?.disable(),
    'element:dragend': () => tooltip.value?.enable(),
  })

  // We create a single tooltip paper that will be reused for all tooltips
  tooltipGraph.value = new dia.Graph({}, { cellNamespace: pmshapes })
  tooltipPaper.value = new dia.Paper({
    model: tooltipGraph.value as dia.Graph,
    cellViewNamespace: pmshapes,
    width: 140,
    height: 120,
    async: true,
    autoFreeze: true,
    overflow: true,
    sorting: dia.Paper.sorting.NONE,
  })

  initializeStencilTooltip.call(this)
  startHoverListener()
  loadSearchGraph()
  stencil.value?.closeGroups()
  emit('stencilLoaded')
})

function initializeStencilTooltip() {
  tooltip.value = new ui.Tooltip({
    target: '[model-id]',
    rootTarget: stencil.value?.el,
    // Tooltip container denotes the area where the tooltip can be shown
    // It's adding a padding on the top and the bottom of the paper area.
    container: stencilContainer.value,
    content: (el: HTMLElement): DocumentFragment | false => {
      const groups = stencilGroups.value ? Object.keys(stencilGroups.value) : []

      const graphs = groups.map((group) => stencil.value?.getGraph(group))
      let stencilElement = null

      for (const graph of graphs) {
        const foundElement = graph?.getCell(el.getAttribute('model-id') as string)
        if (!foundElement) continue

        stencilElement = foundElement
      }

      if (!stencilElement) {
        // The element should be always found
        return false
      }

      return buildTooltipContent(createFromStencilElement(stencilElement))
    },
    position: ui.Tooltip.TooltipPosition.Left,
    positionSelector: '.stencil-container',
    padding: 10,
    animation: {
      duration: '250ms',
    },
  })
}
function buildTooltipContent(cell: dia.Cell) {
  // const { tooltipGraph, tooltipPaper } = this;
  // Add a copy of the cell to the tooltip graph
  // Note: We don't have to care about the position of the cell
  // because the tooltip paper will be transformed to fit the cell
  tooltipGraph.value?.resetCells([cell.clone()])

  const shapeNameEl = document.createElement('span')
  var name =
    cell.get('name') ||
    cell.attributes.partInfo?.name ||
    cell.attributes.shelfInfo?.name ||
    'No name'
  shapeNameEl.append(document.createTextNode(name))

  const shapeDetailEl = document.createElement('div')
  shapeDetailEl.setAttribute('class', 'tooltip-details')
  const width = cell.attributes.partInfo?.width || cell.attributes.shelfInfo?.width || 'N/A'
  const facings = cell.attributes.partInfo?.facings || cell.attributes.shelfInfo?.facings || 'N/A'
  const stock = cell.attributes.partInfo?.stock || cell.attributes.shelfInfo?.stock || 'N/A'

  const detailWidthEl = document.createElement('div')
  detailWidthEl.append(document.createTextNode(`Width: ${width}`))

  const detailFacingsEl = document.createElement('div')
  detailFacingsEl.append(document.createTextNode(`Facings: ${facings}`))

  const detailStockEl = document.createElement('div')
  detailStockEl.append(document.createTextNode(`Stock: ${stock}`))
  shapeDetailEl.append(detailWidthEl, detailFacingsEl, detailStockEl)

  const documentFragment = document.createDocumentFragment()
  documentFragment.append(tooltipPaper.value?.el as HTMLElement, shapeNameEl, shapeDetailEl)

  tooltipPaper.value?.transformToFitContent({
    padding: 5,
    contentArea: cell.getBBox(),
    verticalAlign: 'middle',
    horizontalAlign: 'middle',
  })

  return documentFragment
}

async function getStencilGroups(planogramId: number, isCluster: boolean, clusterId: number) {
  let data: any
  if (!isCluster) {
    data = await menuService.value?.loadMenuCategories(planogramId)
  } else {
    data = await menuService.value?.loadClusterMenuCategories(clusterId)
  }
  //.done(function (data) {
  //alert('got the data');
  let menuCategories = data as Menu
  //menu = data;
  let stencilGroups = new StencilGroups()

  menuCategories.categories.forEach(function (category: Category, i: number) {
    //let item = {category.CategoryName: { index: 1, label: category.CategoryName }};
    //Stencil.groups.push(item);
    let item = JSON.stringify(
      '{"index": ' +
        i +
        ', " label:"' +
        category.name +
        '," categoryId: "' +
        category.categoryId +
        ', " closed: true"}}',
    )
    let itemJson = JSON.parse(item)
    stencilGroups[category.name] = itemJson
  })
  return stencilGroups
}

function getCentreY(size: any): number {
  let boxHeight = 90
  let boxWidth = 60
  let ratio = 1

  let heightRatio = boxHeight / size.height
  let widthRatio = boxWidth / size.width

  if (heightRatio > widthRatio) {
    ratio = 1
  } else {
    ratio = heightRatio
  }
  return ratio
}

function fitRatio(size: any): number {
  let boxHeight = 90
  let boxWidth = 60
  let ratio = 1

  let heightRatio = boxHeight / size.height
  let widthRatio = boxWidth / size.width

  if (heightRatio > widthRatio) {
    ratio = widthRatio
  } else {
    ratio = heightRatio
  }

  return ratio
}

async function populateSearchGraph(searchGraph: joint.dia.Graph, menuData: MenuPart[]) {
  menuData.forEach(function (part: MenuPart) {
    //        $.each(menuData, function (i, part) {
    try {
      // let shape: string;
      let jointShape
      let ratio = fitRatio({ width: part.width, height: part.height })
      let centreX = (100 - part.width * ratio) / 2
      let centreY = 100 - part.height * ratio

      if (part.partTypeId == 4 || part.partTypeId == 10) {
        let tableHtml = [
          '<body xmlns="http://www.w3.org/1999/xhtml">',
          '<div class="menu-info">',
          '<span class="partname">' + part.name + '</span>',
          '<br/>',
          '</div>',
          '</body>',
        ].join('')

        if (part.svgLineGraphic != null) {
          jointShape = new pmshapes.planmatr.Part.MenuShelf({
            size: { width: part.width * ratio, height: part.height * ratio },
            height: part.height,
            width: part.width,
            attrs: {
              shelf: {
                'xlink:href': 'data:image/svg+xml;utf8,' + encodeURIComponent(part.svgLineGraphic),
              },
              menutable: { html: tableHtml },

              //text: { text: part.Name, "fill": "#000", "font-size": 10, "stroke": "#000", "stroke-width": 0 },
            },
          })
        } else {
          jointShape = new pmshapes.planmatr.Part.MenuShelf({
            size: { width: part.width, height: part.height },
            height: part.height,
            width: part.width,
            y: 0,
            text: {
              text: part.name,
              fill: '#000',
              'font-size': 10,
              stroke: '#000',
              'stroke-width': 0,
            },
            attrs: {
              menutable: { html: tableHtml },
              //text: { text: part.Name, "fill": "#000", "font-size": 10, "stroke": "#000", "stroke-width": 0 },
            },
          })
        }

        if (
          jointShape.attributes.attrs != null &&
          jointShape.attributes.attrs['shelfbody'] != null
        ) {
          // jointShape.attributes.attrs['shelfbody'].width = part.width * ratio;
          // jointShape.attributes.attrs['shelfbody'].height = part.height * ratio;
          // jointShape.attributes.attrs['shelfbody'].x = centreX;
          // jointShape.attributes.attrs['shelfbody'].y = centreY;
        }
        if (jointShape.attributes.attrs != null && jointShape.attributes.attrs['fobj'] != null) {
          jointShape.attributes.attrs['fobj'].y = 100
          jointShape.attributes.attrs['fobj'].x = centreX * 0.75
        }
        if (jointShape.attributes.attrs != null && jointShape.attributes.attrs['shelf'] != null) {
          // jointShape.attributes.attrs['shelf'].width = part.width * ratio;
          // jointShape.attributes.attrs['shelf'].height = part.height * ratio;
          // jointShape.attributes.attrs['shelf'].x = centreX;
          // jointShape.attributes.attrs['shelf'].y = centreY;
        }
        if (props.isCluster) {
          jointShape.attributes.shelfInfo.planogramId = props.clusterId
        } else {
          jointShape.attributes.shelfInfo.planogramId = props.planogramId
        }

        jointShape.attributes.shelfInfo.name = part.name
        jointShape.attributes.shelfInfo.partId = part.id
        jointShape.attributes.shelfInfo.shelfTypeId = part.partTypeId
        jointShape.attributes.shelfInfo.shelfType = part.partType
        jointShape.attributes.shelfInfo.partNumber = part.partNumber
        jointShape.attributes.shelfInfo.altPartNumber = part.altPartNumber
        jointShape.attributes.shelfInfo.height = part.height
        jointShape.attributes.shelfInfo.width = part.width
        jointShape.attributes.groupName = part.categoryName
        jointShape.attributes.shelfInfo.svgLineGraphic = part.svgLineGraphic
      } else {
        //let vel = V(part.SvgLineGraphic);
        let tableHtml = [
          '<body xmlns="http://www.w3.org/1999/xhtml">',
          '<div class="menu-info">',
          '<span class="partname">' + part.name + '</span>',
          '<br/>',
          '</div>',
          '</body>',
        ].join('')

        let ratio = fitRatio({ width: part.width, height: part.height })
        let centreX = (100 - part.width * ratio) / 2
        let centreY = 100 - part.height * ratio
        if (part.svgLineGraphic != null) {
          jointShape = new pmshapes.planmatr.Part.MenuCassette({
            //size: { width: resizedWidth, height: resizedHeight },
            size: { width: part.width * ratio, height: part.height * ratio },
            height: part.height,
            width: part.width,
            attrs: {
              cassetteImage: {
                'xlink:href': 'data:image/svg+xml;utf8,' + encodeURIComponent(part.svgLineGraphic),
              },
              menutable: { html: tableHtml },

              //text: { text: part.Name, "fill": "#000", "font-size": 10, "stroke": "#000", "stroke-width": 0 },
            },
          })
        } else {
          jointShape = new pmshapes.planmatr.Part.MenuCassette({
            //size: { width: resizedWidth, height: resizedHeight },
            size: { width: part.width, height: part.height },
            height: part.height,
            width: part.width,
            text: {
              text: part.name,
              fill: '#000',
              'font-size': 10,
              stroke: '#000',
              'stroke-width': 0,
            },
            attrs: {
              menutable: { html: tableHtml },
            },
          })
        }
        if (
          jointShape.attributes.attrs != null &&
          jointShape.attributes.attrs['menu-container'] != null
        ) {
          jointShape.attributes.attrs['menu-container'].y = 0
        }
        if (
          jointShape.attributes.attrs != null &&
          jointShape.attributes.attrs['cassetteBody'] != null
        ) {
          // jointShape.attributes.attrs['cassetteBody'].width = part.width * ratio;
          // jointShape.attributes.attrs['cassetteBody'].height = part.height * ratio;
          // jointShape.attributes.attrs['cassetteBody'].fill = 'transparent';
          // jointShape.attributes.attrs['cassetteBody'].x = centreX;
          // jointShape.attributes.attrs['cassetteBody'].y = centreY;
        }
        if (jointShape.attributes.attrs != null && jointShape.attributes.attrs['fobj'] != null) {
          jointShape.attributes.attrs['fobj'].y = 100
          jointShape.attributes.attrs['fobj'].x = centreX * 0.75
        }
        if (
          jointShape.attributes.attrs != null &&
          jointShape.attributes.attrs['menu-item'] != null
        ) {
          // jointShape.attributes.attrs['menu-item'].width = 'calc(w)'; //part.width * ratio;
          // jointShape.attributes.attrs['menu-item'].height = 'calc(h)'; //part.height * ratio;
          // jointShape.attributes.attrs['menu-item'].x = 'calc(w)';
          // jointShape.attributes.attrs['menu-item'].y = 'calc(h) + ';
        }
        if (
          jointShape.attributes.attrs != null &&
          jointShape.attributes.attrs['cassetteImage'] != null
        ) {
          // jointShape.attributes.attrs['cassetteImage'].width = 'calc(w)';
          // jointShape.attributes.attrs['cassetteImage'].height = 'calc(h)';
          // jointShape.attributes.attrs['cassetteImage'].x = 'calc(w)';
          // jointShape.attributes.attrs['cassetteImage'].y = 100 * ratio;
        }
        if (props.isCluster) {
          jointShape.attributes.partInfo.planogramId = props.clusterId
        } else {
          jointShape.attributes.partInfo.planogramId = props.planogramId
        }

        jointShape.attributes.partInfo.name = part.name
        jointShape.attributes.partInfo.facings = part.facings
        jointShape.attributes.partInfo.stock = part.stock
        jointShape.attributes.partInfo.partId = part.id
        jointShape.attributes.partInfo.partTypeId = part.partTypeId
        jointShape.attributes.partInfo.partType = part.partType
        jointShape.attributes.partInfo.partNumber = part.partNumber
        jointShape.attributes.partInfo.altPartNumber = part.altPartNumber
        jointShape.attributes.partInfo.customerRefNo = part.customerRefNo
        jointShape.attributes.partInfo.svgLineGraphic = part.svgLineGraphic
        jointShape.attributes.partInfo.render2dImage = part.render2dImage

        //jointShape.attributes.partInfo.category = part.categoryName;
        jointShape.attributes.groupName = part.categoryName
      }

      //stencilShapes[part.categoryName].push(jointShape);
      jointShape.addTo(searchGraph)
    } catch (e) {
      //alert(e);
      console.log('error in populateSearchGraph', e)
    } finally {
      document.querySelectorAll('input.search').forEach((element) => {
        ;(element as HTMLInputElement).removeAttribute('disabled')
      })
    }
  })

  //setTimeout(function () {
  // $("input.search").removeAttr("disabled");
  //}, 5000);
}

function populateStencilSearchResults(searchResults: dia.Cell[], stencil: any) {
  //stencil is type ui.Stencil, but ui.Stencil does not have graphs property, so we need to use any type here

  searchResults.forEach(function (part: dia.Cell) {
    try {
      //if (part.categoryName === category.name) {
      //let graph = self.getGraph(part.attributes.categoryName);
      // let exists = _.find(stencil.graphs[part.attributes.groupName].attributes.cells.models, function (e: any) {
      let exists = stencil.graphs[part.attributes.groupName].attributes.cells.models.find(function (
        e: any,
      ) {
        if (part.attributes.partInfo) {
          if (e.attributes.partInfo) {
            return e.attributes.partInfo.partId == part.attributes.partInfo.partId
          } else {
            return e.attributes.shelfInfo.partId == part.attributes.partInfo.partId
          }
        } else {
          //if we have a shelf, then the match list may not be shelves, so check each time to determine if we can find a match.
          if (e.attributes.shelfInfo) {
            return e.attributes.shelfInfo.partId == part.attributes.shelfInfo.partId
          } else {
            return e.attributes.partInfo.partId == part.attributes.shelfInfo.partId
          }
        }
      })
      if (!exists) {
        part.clone().addTo(stencil.graphs[part.attributes.groupName])
      }
      //}
    } catch (e) {
      //alert(e);
    }
  })
  //self.stencil.load(stencilShapes);
  //$.each(stencil.$groups, function (i, group) {
  // stencil.$groups.forEach(function (group: any) {

  for (const group of Object.entries(stencil.$groups)) {
    let groupName = group[0]
    stencil.layoutGroup(stencil.graphs[groupName], stencil.getGroup(groupName))
    let graph: dia.Graph = stencil.graphs[groupName]

    let paper: dia.Paper = stencil.papers[groupName]

    paper.fitToContent({
      gridWidth: 1,
      gridHeight: 1,
      padding: 10,
    })
  }
  //});
  stencil.value?.render()
  stencil.value?.closeGroups()
}

function createFromStencilElement(el: dia.Cell) {
  // let clone = el.clone();
  // clone.prop(clone.get('targetAttributes'));
  // clone.removeProp('targetAttributes');

  // return clone;
  //NEED TO CHECK IF CASSETTE OR SHELF
  if (el.attributes.type == 'planmatr.Part.MenuShelf') {
    let element = new pmshapes.planmatr.Part.Shelf({
      size: { width: el.attributes.width, height: el.attributes.height },
      height: el.attributes.height,
      width: el.attributes.width,
      attrs: {
        shelf: {
          'xlink:href':
            'data:image/svg+xml;utf8,' + encodeURIComponent(el.attributes.shelfInfo.svgLineGraphic),
        },
      },
    })

    //clone.attributes.partInfo.planogramShelfId = el.attributes.partInfo.PlanogramShelfId;
    //clone.attributes.shelfInfo.facings = el.attributes.partInfo.facings;
    //clone.attributes.shelfInfo.stock = el.attributes.partInfo.stock;
    element.attributes.shelfInfo.partId = el.attributes.shelfInfo.partId
    element.attributes.shelfInfo.planogramId = el.attributes.shelfInfo.planogramId
    element.attributes.shelfInfo.shelfid = el.attributes.shelfInfo.shelfId
    element.attributes.shelfInfo.shelfTypeId = el.attributes.shelfInfo.shelfTypeId
    element.attributes.shelfInfo.shelfType = el.attributes.shelfInfo.shelfType
    element.attributes.shelfInfo.partNumber = el.attributes.shelfInfo.partNumber
    element.attributes.shelfInfo.name = el.attributes.shelfInfo.name
    element.attributes.shelfInfo.render2dImage = el.attributes.shelfInfo.render2dImage
    element.attributes.shelfInfo.height = el.attributes.shelfInfo.height
    element.attributes.shelfInfo.width = el.attributes.shelfInfo.width
    element.attributes.shelfInfo.svgLineGraphic = el.attributes.shelfInfo.svgLineGraphic
    element.attributes.shelfInfo.statusId = 0
    //element.attributes.shelfInfo.notes = "";
    let clone = element.clone()
    clone.prop(element.get('targetAttributes'))
    clone.removeProp('targetAttributes')

    return clone
  } else {
    let element = new pmshapes.planmatr.Part.Cassette({
      size: { width: el.attributes.width, height: el.attributes.height },
      height: el.attributes.height,
      width: el.attributes.width,
      attrs: {
        cassette: {
          'xlink:href':
            'data:image/svg+xml;utf8,' + encodeURIComponent(el.attributes.partInfo.svgLineGraphic),
          // width: el.attributes.width,
          // height: el.attributes.height
        },
      },
    })

    //clone.attributes.partInfo.planogramShelfId = el.attributes.partInfo.PlanogramShelfId;
    element.attributes.partInfo.planogramId = el.attributes.partInfo.planogramId
    element.attributes.partInfo.facings = el.attributes.partInfo.facings
    element.attributes.partInfo.stock = el.attributes.partInfo.stock
    element.attributes.partInfo.partId = el.attributes.partInfo.partId
    element.attributes.partInfo.partTypeId = el.attributes.partInfo.partTypeId
    element.attributes.partInfo.partType = el.attributes.partInfo.partType
    element.attributes.partInfo.partNumber = el.attributes.partInfo.partNumber
    element.attributes.partInfo.name = el.attributes.partInfo.name
    element.attributes.partInfo.render2dImage = el.attributes.partInfo.render2dImage
    element.attributes.partInfo.svgLineGraphic = el.attributes.partInfo.svgLineGraphic
    element.attributes.partInfo.statusId = 0
    element.attributes.partInfo.status = 'Not Changed'
    element.attributes.partInfo.notes = ''
    let clone = element.clone()
    clone.prop(clone.get('targetAttributes'))
    clone.removeProp('targetAttributes')

    return clone
  }
}

async function loadSearchGraph() {
  const urlParams = new URLSearchParams(window.location.search)
  let planogramId = props.planogramId
  let searchData: MenuPart[]
  let menuService = new MenuService()
  await menuService.initialise()
  let menuData: any
  if (!props.isCluster) {
    menuData = await menuService.loadMenuData(planogramId).catch((error) => {
      console.log('Error loading menu data:', error)
      return null
    })
  } else {
    menuData = await Promise.resolve(menuService.loadClusterMenuData(props.clusterId))
  }
  searchData = menuData as any

  await populateSearchGraph(props.searchGraph as joint.dia.Graph, searchData)
}

function startHoverListener() {
  stencil.value?.on({
    'group:element:mouseenter': (_, elementView) => {
      StencilBackground.add(elementView, 'root', 'stencil-highlight', {
        padding: 4,
        width: 36,
        height: 36,
        color: HIGHLIGHT_COLOR,
      })
    },
    'group:element:mouseleave': (groupPaper) => {
      StencilBackground.removeAll(groupPaper)
    },
    // Remove all highlights when the user starts dragging an element
    'group:element:pointerdown': (groupPaper) => {
      StencilBackground.removeAll(groupPaper)
    },
  })
}
</script>

<template>
  <div class="stencil-container" ref="stencilContainer"></div>
</template>
