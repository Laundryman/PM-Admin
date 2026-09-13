<script setup lang="ts">
import { Product } from '@/models/Products/product.model';
import { Shade } from '@/models/Products/shade.model';
import { ShadeStatusColourEnum, StatusEnum } from '@/planner/models/Enumerations';
import { usePartStore } from '@/stores/partStore';
import { useDialog } from 'primevue/usedialog';
import { inject, onMounted, ref } from 'vue';

const dialogRef = inject('dialogRef');
const dialog = useDialog();
const props = defineProps<{
    partId: number;
    cassette: joint.dia.Cell;
}>();

const emit = defineEmits<{
    (e: 'shadeUpdate', updatedCell: joint.dia.Cell): void;
    (e: 'close'): void;
}>();

const currentCell = ref(props.cassette);
const partStore = usePartStore();
// const currentCell = localCassette.value;
// const part = partStore.part;
const updatedCell = ref<joint.dia.Cell>(currentCell.value.clone() as joint.dia.Cell);
const partInfo = updatedCell.value.attributes.partInfo;
const facings = partInfo.facings || [];
const facingProducts = partInfo.facingProducts || [];
const currentSvg = ref<Document | null>(null);
const svgContainer = ref<HTMLElement | null>(null);

const currentSelectedProductId = ref<number | null>(null);
const currentSelectedProduct = ref<Product | null>(null);
const currentSelectedShadeId = ref<number | null>(null);
const currentSelectedShade = ref<Shade | null>(null);
const currentSelectedFacings = ref<HTMLElement[]>([]);
// watch(
//     () => currentSelectedProductId.value,
//     (newProductId) => {
//         const newProduct = partInfo.products.find((product: any) => product.id === newProductId);
//         currentSelectedProduct.value = newProduct || null;
//     }
// );
onMounted(async () => {
    if (currentCell?.value.attributes.partInfo.svgLineGraphic != null) {
        if (typeof currentCell.value?.attributes.partInfo.svgLineGraphic != 'undefined') {
            currentSvg.value = new DOMParser().parseFromString(currentCell.value.attributes.partInfo.svgLineGraphic, 'image/svg+xml');
            let svgCassette: any;
            if (currentSvg.value.childNodes.length > 1) {
                svgCassette = currentSvg.value.childNodes[1];
            } else {
                svgCassette = currentSvg.value.childNodes[0];
            }
            svgCassette.setAttribute('height', '150');

            //svgCassette.setAttribute('viewBox', '0 0 ' + width + ' ' + height);
            svgContainer.value?.appendChild(svgCassette);
        }

        //svgImage = 'data:image/svg+xml;utf8,' + encodeURIComponent(this.cassette.attributes.partInfo.svgLineGraphic);
    }

    // if (currentCell?.attributes.partInfo.facings != null) {
    //     facings.value = currentCell.attributes.partInfo.facings;
    // }
});

function getSelectedProduct(facingNo: number) {
    let productId = partInfo['selectedProduct-facing-' + facingNo];
    return partInfo.products.find((product: any) => product.id == productId);
}

function getSelectedShade(facingNo: number) {
    let productId = partInfo['selectedProduct-facing-' + facingNo];
    let product = partInfo.products.find((product: any) => product.id == productId);
    let shadeId = partInfo['selectedShade-facing-' + facingNo];
    return product?.shades.find((shade: any) => shade.id == shadeId);
}

function getSelectedShadeStatus(facingNo: number) {
    const selectedShadeStatus = partInfo['selectedStatus-facing-' + facingNo];
    const selectColour = ShadeStatusColourEnum[selectedShadeStatus];
    return selectColour;
}

function selectFacing(facingNo: number, event: Event) {
    event.stopPropagation();
    const tableCell = event.currentTarget as HTMLElement;
    if (tableCell) {
        if (tableCell.classList.contains('selected-facing')) {
            tableCell.classList.remove('selected-facing');
            const index = currentSelectedFacings.value.indexOf(tableCell);
            if (index > -1) {
                currentSelectedFacings.value.splice(index, 1);
            }
        } else {
            // const allCells = document.querySelectorAll('.edit-facing-table-cell');
            // allCells.forEach((cell) => cell.classList.remove('selected-facing'));
            // tableCell.classList.add('selected-facing');
            tableCell.classList.add('selected-facing');
            currentSelectedFacings.value.push(tableCell);
        }
        if (currentSelectedFacings.value.length > 0) {
            const firstSelectedFacing = currentSelectedFacings.value[0];
            const facingNo = parseInt(firstSelectedFacing.dataset.facingno || '0');
            currentSelectedProductId.value = partInfo['selectedProduct-facing-' + facingNo] || null;
            currentSelectedProduct.value = partInfo.products.find((product: any) => product.id == currentSelectedProductId.value) || null;
            currentSelectedShadeId.value = partInfo['selectedShade-facing-' + facingNo] || null;
        } else {
            currentSelectedProductId.value = null;
            currentSelectedShadeId.value = null;
        }
    }
    // tableCell.style.setProperty('background-color', '#d3d3d3');
}
function handleProductChange(event: any) {
    // currentSelectedProductId.value = selectedProductId;
    const selectedProductId = event.value;
    currentSelectedProductId.value = selectedProductId;
    currentSelectedProduct.value = partInfo.products.find((product: any) => product.id == currentSelectedProductId.value) || null;

    // Update the partInfo for all selected facings
    currentSelectedFacings.value.forEach((facingCell) => {
        const facingNo = parseInt(facingCell.dataset.facingno || '0');
        partInfo['selectedProduct-facing-' + facingNo] = currentSelectedProductId.value;
        partInfo['selectedShade-facing-' + facingNo] = null; // Reset shade selection when product changes
        partInfo['selectedStatus-facing-' + facingNo] = 1; // Assuming 1 is the status for "New Shade"

        emit('shadeUpdate', updatedCell.value as joint.dia.Cell); // Emit the shadeUpdate event for each facing
    });

    // Reset the selected shade since the product has changed
    currentSelectedShadeId.value = null;
    currentSelectedShade.value = null;
}

function handleShadeChange(event: any) {
    const selectedShadeId = event.value;
    currentSelectedShadeId.value = selectedShadeId;
    currentSelectedShade.value = currentSelectedProduct.value?.shades.find((shade: any) => shade.id == selectedShadeId) || null;

    // Update the partInfo for all selected facings
    currentSelectedFacings.value.forEach((facingCell) => {
        const facingNo = parseInt(facingCell.dataset.facingno || '0');
        updatedCell.value.attributes.partInfo['selectedProduct-facing-' + facingNo] = currentSelectedProductId.value;
        partInfo['selectedShade-facing-' + facingNo] = currentSelectedShadeId.value;
        partInfo['selectedStatus-facing-' + facingNo] = 1; // Assuming 1 is the status for "New Shade"
    });
    emit('shadeUpdate', updatedCell.value as joint.dia.Cell); // Emit the shadeUpdate event for each facing
}

function handleSave() {
    emit('shadeUpdate', updatedCell.value as joint.dia.Cell);
    emit('close');
}

function statusChange(status: StatusEnum) {
    currentSelectedFacings.value.forEach((facingCell) => {
        const facingNo = parseInt(facingCell.dataset.facingno || '0');
        // partInfo['selectedStatus-facing-' + facingNo] = status;
        partInfo['selectedStatus-facing-' + facingNo] = status;
    });
}
</script>

<template>
    <div class="edit-shades">
        <div class="editor-header">
            <h5>{{ partInfo.name }}</h5>
        </div>
        <div class="editor-container container">
            <div class="cassette-area row">
                <div class="col-sm">
                    <div class="svg-container" ref="svgContainer">
                        <!-- <img v-if="partStore.part?.svgLineGraphic" :src="partStore.part.svgLineGraphic" alt="Cassette SVG" />
                        <p v-else>No cassette SVG available</p> -->
                    </div>
                </div>
                <div class="col-sm">
                    <div class="facing-container" ref="facingContainer">
                        <table class="edit-facing-table" width="100%" height="260px" style="font-size: 8px; table-layout: fixed">
                            <tbody>
                                <tr height="80%">
                                    <td
                                        v-for="id in partInfo.facings"
                                        :key="id"
                                        class="facing-item-name vertical-text edit-facing-table-cell"
                                        valign="top"
                                        :data-facingno="id"
                                        style="padding: 4px; font-weight: bold"
                                        :width="Math.floor(400 / partInfo.facings)"
                                        height="100%"
                                        :style="{ color: getSelectedShadeStatus(id) }"
                                        v-tooltip="getSelectedProduct(id)?.name + ' - ' + getSelectedShade(id)?.shadeNumber"
                                        @click="selectFacing(id, $event)"
                                    >
                                        <div class="product-name facing-text" style="writing-mode: vertical-rl" height="90%">
                                            {{ getSelectedProduct(id)?.name }}
                                        </div>
                                        <div class="shade-name facing-text" style="writing-mode: vertical-rl">{{ getSelectedShade(id)?.shadeNumber }}</div>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
            <div class="edit-area flex grow gap-6 mt-20">
                <div class="flex-1">
                    <div class="facing-status-container grid grid-cols-1 gap-4">
                        <h4>Status</h4>
                        <div class="row flex">
                            <!-- <div class="flex-1 status-name">New Shade</div> -->
                            <div class="flex-1">
                                <Button v-slot="slotProps" asChild>
                                    <button
                                        v-bind="slotProps.a11yAttrs"
                                        @click="statusChange(StatusEnum['New Module'])"
                                        class="w-50 bg-(--status-new) rounded-lg text-white border-none px-6 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all"
                                    >
                                        New Shade
                                    </button>
                                </Button>
                            </div>
                        </div>

                        <div class="row flex">
                            <!-- <div class="flex-1 status-name">Moved Shade</div> -->
                            <div class="flex-1 w-full">
                                <Button v-slot="slotProps" asChild>
                                    <button
                                        v-bind="slotProps.a11yAttrs"
                                        @click="statusChange(StatusEnum['Moved Module'])"
                                        class="w-50 bg-(--status-moved) rounded-lg text-white border-none px-6 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary transition-all"
                                    >
                                        Moved Shade
                                    </button>
                                </Button>
                            </div>
                        </div>

                        <div class="row flex">
                            <!-- <div class="flex-1 status-name">No Change</div> -->
                            <div class="flex-1 w-full">
                                <Button v-slot="slotProps" asChild>
                                    <button
                                        v-bind="slotProps.a11yAttrs"
                                        @click="statusChange(StatusEnum['Not Changed'])"
                                        class="w-50 bg-(--status-none) rounded-lg text-white border-none px-6 py-3 font-bold hover:ring-2 cursor-pointer ring-offset-2 ring-offset-surface-0 dark:ring-offset-surface-900 ring-primary active:outline-offset-2 active:outline-black transition-all"
                                    >
                                        No Change
                                    </button>
                                </Button>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="flex-1">
                    <div class="grid grid-cols-1 gap-4">
                        <h4>Products and Shades</h4>
                        <div class="selector-container product-selector-container">
                            <Select v-model="currentSelectedProductId" :options="partInfo.products" option-label="name" option-value="id" placeholder="Select product" @change="handleProductChange" class="w-full" />
                        </div>
                        <div class="selector-container shade-selector-container">
                            <Select v-model="currentSelectedShadeId" :options="currentSelectedProduct?.shades" @change="handleShadeChange" option-label="shadeNumber" option-value="id" placeholder="Select shade" class="w-full" />
                        </div>
                        <!-- <div id="default-suggestions" class="selector-container typeahead-container">
                        <input class="typeahead" type="text" placeholder="Select shade" />
                    </div> -->
                    </div>
                    <div class="flex justify-content-end mt-4">
                        <Button label="Save" icon="pi pi-check" @click="handleSave" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
