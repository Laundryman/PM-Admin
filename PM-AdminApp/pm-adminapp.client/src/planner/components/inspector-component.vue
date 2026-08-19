<script lang="ts" setup>
import { Shade } from '@/models/Products/shade.model'
import { usePartStore } from '@/stores/partStore'
import * as joint from '@joint/plus'
import { defineAsyncComponent, onMounted, ref, watch } from 'vue'
const HIDDEN_CLASS_NAME = 'hidden'
const ShadeEditorAsyncComponent = defineAsyncComponent(
  () => import('@/components/planner/editShades.vue'),
)

const inspector = ref<joint.ui.Inspector | null>(null)
const container = ref<HTMLElement | null>(null)
const content = ref<HTMLElement | null>(null)
const header = ref<HTMLDivElement | null>(null)
const openGroupsButton = ref<HTMLButtonElement | null>(null)
const closeGroupsButton = ref<HTMLButtonElement | null>(null)
const inspectorLoaded = ref(false)
const inspectCell = ref<joint.dia.Cell | null>(null)
const shadeDialog = ref(false)
const shade = ref<Shade | null>(null)
const partStore = usePartStore()
const emit = defineEmits([
  'inspectorLoaded',
  'shadeUpdated',
  'statusUpdated',
  'labelUpdated',
  'copiedToClipBoard',
])

const props = defineProps({
  cell: {
    type: Object as () => joint.dia.Cell,
    required: true,
  },
  selection: {
    type: Object as () => joint.ui.Selection,
    required: true,
  },
})
const localCell = ref(props.cell)
const tooltip = new joint.ui.Tooltip({
  target: '[data-tooltip]',
  direction: joint.ui.Tooltip.TooltipArrowPosition.Auto,
  padding: 20,
})
onMounted(async () => {
  inspectCell.value = props.cell
  inspector.value = await createDynamic(inspectCell.value as joint.dia.Cell)
  inspectorLoaded.value = true
  emit('inspectorLoaded')
})

watch(
  () => props.cell,
  async (newCell: joint.dia.Cell) => {
    const { collection } = props.selection
    if (newCell && collection.length === 1) {
      inspectCell.value = newCell
      await createDynamic(inspectCell.value as joint.dia.Cell)
    }
  },
)

const create = (cell: joint.dia.Cell): joint.ui.Inspector => {
  header.value?.classList.remove(HIDDEN_CLASS_NAME)

  // const { groups, inputs } = inspectorDefinitions[cell.get('type')] || {};
  const { groups, inputs } = getInspectorConfig()[cell.get('type')] || {}
  const inspectorInstance = joint.ui.Inspector.create('.inspector-container', {
    cell,
    groups,
    inputs,
    container: container.value!,
  } as joint.ui.Inspector.Options)

  // const inspectorInstance = joint.ui.Inspector.create(content.value!, {
  //     cell,
  //     groups,
  //     inputs,
  //     container: container.value!,
  //     renderFieldContent: (options, path, _value, inspector) => {
  //         if (options.type === 'image-picker') {
  //             const label = document.createElement('label');
  //             label.textContent = options.label;

  //             const input = document.createElement('input');

  //             input.type = 'file';
  //             input.accept = 'image/x-png,image/gif,image/jpeg';

  //             const field = document.createElement('div');
  //             field.appendChild(label);
  //             field.appendChild(input);

  //             input.addEventListener('change', function () {
  //                 inspector.updateCell(field, path, options);
  //             });

  //             return field;
  //         }

  //         // Use the default field renderer.
  //         return null;
  //     },
  //     getFieldValue: (field, type) => {
  //         if (type === 'image-picker') {
  //             const file = field?.querySelector<HTMLInputElement>('input')?.files?.item(0);
  //             return { value: file ? URL.createObjectURL(file) : '' };
  //         }

  //         // Use the default field value getter.
  //         return null;
  //     }
  // } as joint.ui.Inspector.Options);

  if (inspector.value !== inspectorInstance) {
    inspectorInstance.on('close', () => {
      header.value?.classList.add(HIDDEN_CLASS_NAME)
    })
    inspector.value = inspectorInstance
  }

  return inspector.value as joint.ui.Inspector
}

async function createDynamic(cell: joint.dia.Cell): Promise<joint.ui.Inspector> {
  const inspectCell = ref(cell)
  let { groups, inputs, renderFieldContent } =
    getInspectorConfig()[inspectCell.value.get('type')] || {}
  // await partStore.setCurrentCell(cell);
  if (cell.get('type') == 'planmatr.Part.Cassette') {
    const products = cell.attributes.partInfo.products

    if (cell.attributes.partInfo.partType == 'Accessory') {
      //we need to add the label attribute.
      inputs.attrs = {
        text: {
          text: {
            type: 'pm-textarea',
            label: 'Label',
            group: 'partInfo',
            max: 47,
            index: 1,
          },
          save: {
            type: 'pm-textarea',
            label: 'save',
            group: 'partInfo',
            index: 2,
          },
        },
      }
    }
    //var prodInputs: any;
    if (products != '' && products != null) {
      //  try {
      //let productList = [];
      //for (var j = 0; j < products.length; j++) {
      //  let prod = { value: products[j].productId, content: products[j].name };
      //  productList.push(prod);
      //};

      //Add button to launch modal shade editor
      inputs.partInfo['selectShades'] = {
        type: 'pm-editShades',
        options: [],
        label: 'Edit Shades',
        group: 'Edit Shades',
        productId: 0,
        index: 0,
      }
    }
  }
  //var prodInputs = inputs;
  return joint.ui.Inspector.create('.inspector-container', {
    cell,
    groups,
    inputs,
    renderFieldContent,
  })
}

function getInspectorConfig(): { [key: string]: any } {
  const options = {
    partStatus: [
      { value: 1, content: 'New Module' },
      { value: 2, content: 'Moved Module' },
      { value: 3, content: 'New Graphic' },
      { value: 4, content: 'Other' },
      { value: 0, content: 'Not Changed' },
    ],

    partStatus2: [
      {
        value: 1,
        content: 'New Module',
        // buttonWidth: 200,
        attrs: {
          '.select-button-group-button': {
            class:
              'w-40 bg-(--status-new) rounded-lg text-white border-none m-10 px-2 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all',
          },
        },
      },
      {
        value: 2,
        content: 'Moved Module',
        // buttonWidth: 200,
        attrs: {
          '.select-button-group-button': {
            class:
              'w-40 bg-(--status-moved) rounded-lg text-white border-none m-10 px-2 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all',
          },
        },
      },
      {
        value: 3,
        content: 'New Graphic',
        // buttonWidth: 200,
        attrs: {
          '.select-button-group-button': {
            class:
              'w-40 bg-(--status-new-graphic) rounded-lg text-black border-none m-10 px-2 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all',
          },
        },
      },
      {
        value: 4,
        content: 'Other',
        // buttonWidth: 200,
        attrs: {
          '.select-button-group-button': {
            class:
              'w-40 bg-(--status-other) rounded-lg text-black border-none m-10 px-2 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all',
          },
        },
      },
      {
        value: 0,
        content: 'Not Changed',
        // buttonWidth: 200,
        attrs: {
          '.select-button-group-button': {
            class:
              'w-40 bg-(--status-none) rounded-lg text-white border-none m-10 px-2 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all',
          },
        },
      },
    ],
  }

  return <{ [index: string]: any }>{
    'planmatr.Part.Cassette': {
      inputs: {
        partInfo: {
          name: {
            type: 'non-editable',
            label: 'Name',
            group: 'partInfo',
            index: 3,
          },
          partNumber: {
            type: 'non-editable',
            label: 'PartNumber',
            group: 'partInfo',
            index: 4,
          },
          statusId: {
            type: 'select-button-group',
            // type: 'pm-statusButtonSet',
            options: options.partStatus2,
            label: 'status',
            group: 'partInfo',
            index: 5,
          },
          notes: {
            type: 'textarea',
            label: 'Notes',
            group: 'partInfo',
            index: 6,
          },
          save: {
            type: 'pm-saveNote',
            label: 'save',
            group: 'partInfo',
            index: 7,
          },
        },
      },
      groups: {
        partInfo: {
          label: 'Part',
          index: 1,
        },
        facings: {
          label: 'facings',
          index: 2,
        },
        editShades: {
          label: 'edit shades',
          index: 2,
        },
      },

      renderFieldContent: function (options: any, path: any, value: any, inspector: any) {
        switch (options.type) {
          case 'pm-textarea':
            let buttonSet = document.createElement('div')
            buttonSet.style.margin = '20px 0 20px 20px'
            buttonSet.style.textAlign = 'center'
            buttonSet.style.overflow = 'auto'

            let addLabel = document.createElement('label')
            addLabel.className = 'bg-blue-500 text-white font-bold py-2 px-4 rounded'
            addLabel.textContent = 'Add Label'
            addLabel.style.margin = 'auto'
            addLabel.style.fontSize = '0.75rem'
            buttonSet.appendChild(addLabel)

            return buttonSet

          case 'pm-saveNote':
            let noteButtonSet = document.createElement('div')
            noteButtonSet.style.margin = '20px 0 20px 20px'
            noteButtonSet.style.textAlign = 'center'
            noteButtonSet.style.overflow = 'auto'

            let addNote = document.createElement('button')
            addNote.className = 'bg-blue-500 text-white font-bold py-2 px-4 rounded'
            addNote.textContent = 'Add Note'
            addNote.style.margin = 'auto'
            addNote.style.fontSize = '0.75rem'
            noteButtonSet.appendChild(addNote)

            return noteButtonSet

          case 'pm-statusButtonSet':
            let containerDiv = document.createElement('div')
            let statusLabel = document.createElement('label')
            statusLabel.textContent = options.label
            let statusSelectGroupSet = document.createElement('div')
            statusSelectGroupSet.classList.add('joint-select-button-group')
            let newModuleButton = document.createElement('button')
            newModuleButton.className =
              'select-button-group-button w-40 bg-(--status-new) rounded-lg text-white border-none m-10 px-2 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all'
            newModuleButton.innerHTML = 'New Module'
            statusSelectGroupSet.appendChild(newModuleButton)

            let movedModuleButton = document.createElement('button')
            movedModuleButton.className =
              'select-button-group-button w-40 bg-(--status-moved) rounded-lg text-white border-none m-10 px-2 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all'
            movedModuleButton.innerHTML = 'Moved Module'
            statusSelectGroupSet.appendChild(movedModuleButton)

            let newGraphicButton = document.createElement('button')
            newGraphicButton.className =
              'select-button-group-button w-40 bg-(--status-new-graphic) rounded-lg text-white border-none m-10 px-2 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all'
            newGraphicButton.innerHTML = 'New Graphic'
            statusSelectGroupSet.appendChild(newGraphicButton)

            let OtherButton = document.createElement('button')
            OtherButton.className =
              'select-button-group-button w-40 bg-(--status-other) rounded-lg text-white border-none m-10 px-2 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all'
            OtherButton.innerHTML = 'Other'
            statusSelectGroupSet.appendChild(OtherButton)

            let NoChangeButton = document.createElement('button')
            NoChangeButton.className =
              'select-button-group-button w-40 bg-(--status-no-change) rounded-lg text-white border-none m-10 px-2 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all'
            NoChangeButton.innerHTML = 'No Change'
            statusSelectGroupSet.appendChild(NoChangeButton)

            newModuleButton.addEventListener('click', function () {
              inspector.updateCell(newModuleButton, path, { value: 1 })
            })
            movedModuleButton.addEventListener('click', function () {
              inspector.updateCell(movedModuleButton, path, { value: 2 })
            })
            newGraphicButton.addEventListener('click', function () {
              inspector.updateCell(newGraphicButton, path, { value: 3 })
            })
            OtherButton.addEventListener('click', function () {
              inspector.updateCell(OtherButton, path, { value: 4 })
            })
            NoChangeButton.addEventListener('click', function () {
              inspector.updateCell(NoChangeButton, path, { value: 0 })
            })

            containerDiv.appendChild(statusLabel)
            containerDiv.appendChild(statusSelectGroupSet)
            return statusSelectGroupSet

          case 'pm-editShades':
            let editShadesButtonSet = document.createElement('div')
            editShadesButtonSet.style.margin = '20px 0 20px 20px'
            editShadesButtonSet.style.overflow = 'auto'
            editShadesButtonSet.style.textAlign = 'center'
            //var content = [$label, $('<div/>').addClass('input-wrapper').append($input)];
            let editShades = document.createElement('button')
            editShades.className = 'bg-blue-500 text-black font-bold py-2 px-4 rounded'
            editShades.textContent = 'Edit Shades'
            editShades.style.margin = 'auto'
            editShades.style.fontSize = '0.75rem'
            editShadesButtonSet.appendChild(editShades)
            let cell = inspector.options.cell

            editShades.addEventListener('click', function () {
              //let shadesService = new ShadesService(cell, inspector);
              //shadesService.displayEditor();
              editShade(cell.attributes.partInfo.shade)
            })

            return editShadesButtonSet
          case 'non-editable':
            let nonEditable = document.createElement('div')
            nonEditable.style.overflow = 'auto'
            nonEditable.setAttribute('data-tooltip', 'click to copy')
            let addALabel = document.createElement('label')
            addALabel.textContent = options.label

            let display = document.createElement('div')
            display.style.overflow = 'auto'
            display.style.border = '1px solid black'
            display.style.padding = '5px 10px'
            display.style.color = 'black'
            display.style.fontWeight = '600'
            display.classList.add('non-editable-display')
            display.textContent = value
            nonEditable.appendChild(addALabel)
            nonEditable.appendChild(display)

            nonEditable.addEventListener('click', function () {
              //let cell = inspector.options.cell
              navigator.clipboard.writeText(value)
              emit('copiedToClipBoard', { message: 'Copied to clipboard', severity: 'success' })
            })
            return nonEditable
          default:
            return undefined
        }
      },
    },
    'planmatr.Part.Shelf': {
      inputs: {
        shelfInfo: {
          name: {
            type: 'non-editable',
            label: 'Name',
            group: 'shelfInfo',
            index: 3,
          },
          partNumber: {
            type: 'non-editable',
            label: 'PartNumber',
            group: 'shelfInfo',
            index: 3,
          },
          label: {
            type: 'textarea',
            label: 'Label',
            group: 'shelfInfo',
            index: 4,
          },
          save: {
            type: 'pm-textarea',
            label: 'save',
            group: 'shelfInfo',
            index: 5,
          },

          // attrs: {
          //   '#label': {
          //     text: {
          //       type: 'textarea',
          //       name: 'shelf-label',
          //       id: 'shelfLabel',
          //       label: 'Label',
          //       group: 'shelfInfo',
          //       max: 47,
          //       index: 1,
          //     },
          //     save: {
          //       type: 'pm-textarea',
          //       label: 'save',
          //       group: 'shelfInfo',
          //       index: 2,
          //     },
          //   },
          // },

          //statusId: {
          //  type: 'select-box',
          //  options: options.partStatus,
          //  label: 'status',
          //  group: 'shelfInfo',
          //  index: 4
          //},
          statusId: {
            type: 'select-button-group',
            options: options.partStatus2,
            label: 'status',
            group: 'partInfo',
            index: 5,
          },
        },
      },

      groups: {
        shelfInfo: {
          label: 'Shelf',
          index: 1,
        },
      },
      renderFieldContent: function (options: any, path: any, value: any, inspector: any) {
        switch (options.type) {
          case 'pm-textarea':
            let buttonSet = document.createElement('div')
            buttonSet.style.margin = '20px 0 20px 20px'
            buttonSet.style.overflow = 'auto'

            let textAreaLabel = document.createElement('button')
            textAreaLabel.type = 'button'
            textAreaLabel.className = 'p-button p-component mr-2'
            textAreaLabel.textContent = 'Add Label'
            textAreaLabel.style.float = 'right'
            textAreaLabel.style.fontSize = '0.75rem'
            buttonSet.appendChild(textAreaLabel)

            textAreaLabel.addEventListener('click', function () {
              //let cell = inspector.options.cell
              inspector.options.cell.attributes.shelfInfo.label = value
              inspector.options.cell.prop('shelfInfo/label', value)
            })

            return buttonSet
          case 'lm-saveNote':
          case 'non-editable':
            let nonEditable = document.createElement('div')
            nonEditable.style.overflow = 'auto'
            nonEditable.setAttribute('data-tooltip', 'click to copy')
            let addLabel = document.createElement('label')
            addLabel.textContent = options.label

            let display = document.createElement('div')
            display.style.overflow = 'auto'
            display.style.border = '1px solid black'
            display.style.padding = '5px 10px'
            display.style.color = 'black'
            display.style.fontWeight = '600'
            display.classList.add('non-editable-display')
            display.textContent = value
            nonEditable.appendChild(addLabel)
            nonEditable.appendChild(display)

            nonEditable.addEventListener('click', function () {
              //let cell = inspector.options.cell
              navigator.clipboard.writeText(value)
              emit('copiedToClipBoard', { message: 'Copied to clipboard', severity: 'success' })
            })
            return nonEditable
          // case 'select2':
          //     var $select = $('').width(170).hide();

          //     // select2 requires the element to be in the live DOM.
          //     // Therefore, postpone the select2 initialization for after we
          //     // add the Inspector container to the live DOM (see below).
          //     setTimeout(function () {
          //         //$select.show().select2({ data: options.options }).val(value || 'none').trigger('change');
          //         $select.data('select2').$container.css('margin', 20);
          //         $select.on('change', function () {
          //             inspector.updateCell($select, path, options);
          //         });
          //     }, 0);

          //     return $select;
          default:
            return undefined
        }
      },
    },
  }
}

function editShade(sh: Shade) {
  shade.value = sh

  shadeDialog.value = true
}

function hideDialog() {
  shadeDialog.value = false
}

function updateShades(updatedCell: joint.dia.Cell) {
  // for (var i = 0; i < updatedCell.attributes.partInfo.facings; i++) {
  //     props.cell.attributes.partInfo['selectedProduct-facing-' + i] = updatedCell.attributes.partInfo['selectedProduct-facing-' + i];
  //     props.cell.attributes.partInfo['selectedShade-facing-' + i] = updatedCell.attributes.partInfo['selectedShade-facing-' + i];
  //     props.cell.attributes.partInfo['selectedStatus-facing-' + i] = updatedCell.attributes.partInfo['selectedStatus-facing-' + i];
  // }
  emit('shadeUpdated', updatedCell)
}

function updateLabel(cell: joint.dia.Cell) {
  // emit('labelUpdated', cell)
  cell.attributes.shelfInfo.label = cell.attributes.attrs['#label'].text
  let thisvalue = cell.attributes.shelfInfo.label
}

defineExpose({
  create,
  createDynamic,
  inspectCell,
})
</script>

<template>
  <div class="inspector-container" ref="container">
    <div ref="header" class="inspector-header hidden">
      <button ref="openGroupsButton" class="open-groups-btn"></button>
      <button ref="closeGroupsButton" class="close-groups-btn"></button>
      <span class="inspector-header-text">Properties</span>``
    </div>
    <div ref="content" class="inspector-content"></div>
  </div>
  <Dialog
    v-model:visible="shadeDialog"
    :style="{ width: '650px' }"
    header="Shade Details"
    :modal="true"
  >
    <div class="flex flex-col gap-6">
      <ShadeEditorAsyncComponent
        :partId="props.cell.attributes.partInfo.partId"
        :cassette="props.cell"
        @close="hideDialog"
        @shadeUpdate="updateShades"
      />
    </div>

    <!-- <template #footer>
            <Button label="Cancel" icon="pi pi-times" text />
            <Button label="Save" icon="pi pi-check" />
        </template> -->
  </Dialog>
</template>
