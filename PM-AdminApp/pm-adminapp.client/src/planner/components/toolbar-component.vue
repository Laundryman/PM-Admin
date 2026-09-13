<script lang="ts" setup>
/*! JointJS+ v4.2.2 (2026-01-22) - HTML5 Diagramming Framework

Copyright (c) 2025 client IO

This Source Code Form is subject to the terms of the JointJS+
License, v. 2.0. If a copy of the JointJS+ License was not
distributed with this file, You can obtain one at
https://www.jointjs.com/license or from the JointJS+ archive as was
distributed by client IO. See the LICENSE file.
*/

import type { PlanogramNote } from '@/models/Planograms/note.model';
import { DirectedGraph } from '@joint/layout-directed-graph';
import * as joint from '@joint/plus';
import { onMounted, ref } from 'vue';
import { fontsStyleSheet } from '../config/font-style-sheet';

const toolbarContainer = ref<HTMLElement | null>(null);
const toolbar = ref<joint.ui.Toolbar | null>(null);
const fileTools = ref<{ action: string; content: string }[]>([]);
const layoutTools = ref<{ action: string; content: string }[]>([]);
const shareTools = ref<{ action: string; content: string }[]>([]);
const settingsTools = ref<{ action: string; content: string }[]>([]);
const toolbarLoaded = ref(false);

const emit = defineEmits(['toolbarLoaded', 'toggleScratchpad', 'toggleSnaplines', 'savePlanogram', 'showIconView', 'showShadeView', 'showRenderView', 'showCommentsDialog']);
const props = defineProps<{
    commandManager: joint.dia.CommandManager;
    paperScroller: joint.ui.PaperScroller;
    graph: joint.dia.Graph;
    paper: joint.dia.Paper;
    notes: PlanogramNote[];
}>();

onMounted(() => {
    create();
    toolbarLoaded.value = true;
    emit('toolbarLoaded');
});
// Keep track of open dropdowns
const openDropdowns = ref({
    file: false,
    layout: false,
    settings: false,
    share: false
});

function create() {
    const { tools, groups } = getToolbarConfig();

    toolbar.value = new joint.ui.Toolbar({
        groups,
        tools,
        autoToggle: true,
        references: {
            paperScroller: props.paperScroller,
            commandManager: props.commandManager
        },
        el: toolbarContainer.value!
    });

    toolbar.value.render();

    fileTools.value = [
        // {
        //     action: 'new',
        //     content: 'New file'
        // },
        // {
        //     action: 'load',
        //     content: 'Load file'
        // },
        {
            action: 'download',
            content: 'Download planogram'
        },
        {
            action: 'save',
            content: 'Save Planogram'
        }
    ];

    shareTools.value = [
        {
            action: 'exportPNG',
            content: 'Export as PNG'
        },
        {
            action: 'exportSVG',
            content: 'Export as SVG'
        },
        {
            action: 'print',
            content: 'Print'
        }
    ];

    // layoutTools.value = [
    //     {
    //         action: 'layout-tb',
    //         content: 'Top to bottom'
    //     },
    //     {
    //         action: 'layout-bt',
    //         content: 'Bottom to top'
    //     },
    //     {
    //         action: 'layout-lr',
    //         content: 'Left to right'
    //     },
    //     {
    //         action: 'layout-rl',
    //         content: 'Right to left'
    //     }
    // ];

    layoutTools.value = [
        {
            action: 'icon-view',
            content: 'Icon View'
        },
        {
            action: 'shade-view',
            content: 'Shade View'
        },
        {
            action: 'render-view',
            content: 'Render View'
        }
    ];

    settingsTools.value = [
        {
            action: 'toggle-snaplines',
            content: 'Snaplines'
        },
        {
            action: 'toggle-scratchpad',
            content: 'Scratchpad'
        }
    ];

    toolbar.value.on({
        'select-file:pointerclick': () => openFileSelect(props.commandManager, props.graph),
        'select-share:pointerclick': () => openShareSelect(props.paper),
        'select-layout:pointerclick': () => openLayoutSelect(props.graph, props.paperScroller),
        'select-settings:pointerclick': () => openSettingsSelect(props.graph, props.paper),
        'select-canvas-settings:pointerclick': () => openSettingsPopup(props.graph),
        'toggle-snaplines:pointerclick': (event: Event) => checkSnaplines(event),
        // 'toggle-scratchpad:pointerclick': () => emit('toggleScratchpad'),
        'toggle-scratchpad:pointerclick': (event: Event) => checkScratchpad(event),
        'comments:pointerclick': () => emit('showCommentsDialog')
    });
}

function getToolbarConfig() {
    return {
        groups: {
            left: {
                index: 1,
                align: joint.ui.Toolbar.Align.Left
            },
            right: {
                index: 2,
                align: joint.ui.Toolbar.Align.Right
            }
        },

        tools: [
            {
                type: 'button',
                name: 'select-file',
                group: 'left',
                attrs: {
                    button: {
                        'data-tooltip': 'File',
                        'data-tooltip-position': 'top',
                        'data-tooltip-position-selector': '.toolbar-container'
                    }
                }
            },
            {
                type: 'separator',
                group: 'left'
            },
            {
                type: 'undo',
                name: 'undo',
                group: 'left',
                attrs: {
                    button: {
                        'data-tooltip': 'Undo',
                        'data-tooltip-position': 'top',
                        'data-tooltip-position-selector': '.toolbar-container'
                    }
                }
            },
            {
                type: 'redo',
                name: 'redo',
                group: 'left',
                attrs: {
                    button: {
                        'data-tooltip': 'Redo',
                        'data-tooltip-position': 'top',
                        'data-tooltip-position-selector': '.toolbar-container'
                    }
                }
            },

            {
                type: 'separator',
                group: 'left'
            },
            {
                type: 'button',
                name: 'select-layout',
                group: 'left',
                text: 'Layout'
            },
            {
                type: 'separator',
                group: 'left'
            },

            {
                type: 'button',
                text: 'Settings',
                name: 'select-settings',
                group: 'left'
            },
            {
                type: 'checkbox',
                name: 'toggle-snaplines',
                group: 'layout',
                label: 'Snaplines',
                value: true,
                attrs: {
                    label: {
                        'data-tooltip': 'Toggle Snaplines',
                        'data-tooltip-position': 'top',
                        'data-tooltip-position-selector': '.toolbar-container'
                    }
                }
            },
            {
                type: 'separator',
                group: 'layout'
            },
            {
                type: 'checkbox',
                name: 'toggle-scratchpad',
                group: 'layout',
                label: 'Scratchpad',
                value: true,
                attrs: {
                    label: {
                        'data-tooltip': 'Toggle Scratchpad',
                        'data-tooltip-position': 'top',
                        'data-tooltip-position-selector': '.toolbar-container'
                    }
                }
            },
            {
                type: 'separator',
                group: 'layout'
            },

            {
                type: 'button',
                name: 'comments',
                text: 'Comments',
                group: 'comments',
                attrs: {
                    button: {
                        id: 'planm-show-comments',
                        'data-tooltip': 'Comments',
                        'data-tooltip-position': 'top',
                        'data-tooltip-position-selector': '.toolbar-container'
                    }
                }
            },
            {
                type: 'separator',
                group: 'comments'
            },
            // {
            //     type: 'button',
            //     text: 'Canvas settings',
            //     name: 'select-canvas-settings',
            //     group: 'left'
            // },

            // {
            //     type: 'separator',
            //     group: 'right'
            // },
            {
                type: 'button',
                name: 'select-share',
                group: 'right',
                attrs: {
                    button: {
                        'data-tooltip': 'Share',
                        'data-tooltip-position': 'top',
                        'data-tooltip-position-selector': '.toolbar-container'
                    }
                }
            }
        ]
    };
}

function getSettingsInspectorConfig() {
    return {
        inputs: {
            snaplines: {
                type: 'toggle',
                label: 'Snaplines'
            },
            scratchpad: {
                type: 'toggle',
                label: 'Scratchpad'
            }
        }
    };
}

function checkScratchpad(event: Event) {
    //need to handle the fact we get two click events, one for the checkbox and one for the label. We only want to emit the event when the checkbox is clicked.
    if (event.target instanceof HTMLInputElement) {
        const isChecked = event.target.checked;
        emit('toggleScratchpad', isChecked);
    }
}
function checkSnaplines(event: Event) {
    //need to handle the fact we get two click events, one for the checkbox and one for the label. We only want to emit the event when the checkbox is clicked.
    if (event.target instanceof HTMLInputElement) {
        const isChecked = event.target.checked;
        emit('toggleSnaplines', isChecked);
    }
}

function layoutDirectedGraph(rankDir: 'TB' | 'BT' | 'LR' | 'RL', graph: joint.dia.Graph, paperScroller: joint.ui.PaperScroller) {
    DirectedGraph.layout(graph, {
        setVertices: true,
        rankDir,
        marginX: 100,
        marginY: 100
    });

    paperScroller.centerContent({ useModelGeometry: true });
}

async function exportPNG(paper: joint.dia.Paper) {
    paper.hideTools();
    joint.format.toPNG(
        paper,
        (dataURL: string) => {
            new joint.ui.Lightbox({
                image: dataURL,
                downloadable: true,
                fileName: 'joint-plus'
            }).open();
            paper.showTools();
        },
        {
            padding: 10,
            useComputedStyles: false,
            grid: true,
            stylesheet: await fontsStyleSheet()
        }
    );
}

async function exportSVG(paper: joint.dia.Paper) {
    paper.hideTools();
    joint.format.toSVG(
        paper,
        (svg: string) => {
            new joint.ui.Lightbox({
                image: 'data:image/svg+xml,' + encodeURIComponent(svg),
                downloadable: true,
                fileName: 'joint-plus'
            }).open();
            paper.showTools();
        },
        {
            preserveDimensions: true,
            convertImagesToDataUris: true,
            useComputedStyles: false,
            grid: true,
            stylesheet: await fontsStyleSheet()
        }
    );
}

function toggleSnaplines(snaplines: joint.ui.Snaplines) {
    // snaplines.isEnabled() ? snaplines.disable() : snaplines.enable();
}

function toggleScratchpad(paper: joint.dia.Paper, scratchPadHidden: boolean) {
    let hide = !scratchPadHidden;
    // need to emit an event to the parent component to toggle the scratchpad visibility, and also
    //deselect any selected items.
}

function openFileSelect(commandManager: joint.dia.CommandManager, graph: joint.dia.Graph) {
    if (openDropdowns.value.file) {
        joint.ui.ContextToolbar.close();
        return;
    }

    openDropdowns.value.file = true;

    const contextToolbar = new joint.ui.ContextToolbar({
        target: toolbar.value?.getWidgetByName('select-file').el,
        root: toolbarContainer.value as HTMLElement,
        padding: 0,
        vertical: true,
        position: 'bottom-left',
        anchor: 'top-left',
        tools: fileTools.value
    });

    contextToolbar.on('action:load', () => {
        joint.ui.ContextToolbar.close();

        const fileInput = document.createElement('input');
        fileInput.setAttribute('type', 'file');
        fileInput.setAttribute('accept', '.json');

        fileInput.click();

        fileInput.onchange = () => {
            if (!fileInput.files || fileInput.files.length === 0) {
                return;
            }
            const file = fileInput.files[0];
            const reader = new FileReader();

            reader.onload = (evt: ProgressEvent<FileReader>) => {
                const str = evt.target!.result as string;
                graph.fromJSON(JSON.parse(str));
                commandManager.reset();
            };

            reader.readAsText(file!);
        };
    });

    contextToolbar.on('action:save', () => {
        joint.ui.ContextToolbar.close();
        emit('savePlanogram', commandManager, graph);
    });

    contextToolbar.on('action:download', () => {
        joint.ui.ContextToolbar.close();
        const str = JSON.stringify(graph.toJSON());
        const bytes = new TextEncoder().encode(str);
        const blob = new Blob([bytes], { type: 'application/json' });
        const el = window.document.createElement('a');
        el.href = window.URL.createObjectURL(blob);
        el.download = 'kitchensink.json';
        document.body.appendChild(el);
        el.click();
        document.body.removeChild(el);
    });

    contextToolbar.on('action:new', () => {
        joint.ui.ContextToolbar.close();
        graph.resetCells([]);
        commandManager.reset();
    });

    // Update openDropdowns state when the contextToolbar is closed
    contextToolbar.once('close', () => (openDropdowns.value.file = false));

    contextToolbar.render();
}

function openLayoutSelect(graph: joint.dia.Graph, paperScroller: joint.ui.PaperScroller) {
    if (openDropdowns.value.layout) {
        joint.ui.ContextToolbar.close();
        return;
    }

    openDropdowns.value.layout = true;

    const contextToolbar = new joint.ui.ContextToolbar({
        target: toolbar.value?.getWidgetByName('select-layout').el,
        root: toolbarContainer.value as HTMLElement,
        padding: 0,
        vertical: true,
        position: 'bottom-left',
        anchor: 'top-left',
        tools: layoutTools.value
    });

    // contextToolbar.on({
    //     'action:layout-tb': () => {
    //         layoutDirectedGraph('TB', graph, paperScroller);
    //         joint.ui.ContextToolbar.close();
    //     },
    //     'action:layout-bt': () => {
    //         layoutDirectedGraph('BT', graph, paperScroller);
    //         joint.ui.ContextToolbar.close();
    //     },
    //     'action:layout-lr': () => {
    //         layoutDirectedGraph('LR', graph, paperScroller);
    //         joint.ui.ContextToolbar.close();
    //     },
    //     'action:layout-rl': () => {
    //         layoutDirectedGraph('RL', graph, paperScroller);
    //         joint.ui.ContextToolbar.close();
    //     }
    // });
    contextToolbar.on({
        'action:icon-view': () => {
            emit('showIconView');
            joint.ui.ContextToolbar.close();
        },
        'action:shade-view': () => {
            emit('showShadeView');
            joint.ui.ContextToolbar.close();
        },
        'action:render-view': () => {
            emit('showRenderView');
            joint.ui.ContextToolbar.close();
        }
    });

    // Update openDropdowns state when the contextToolbar is closed
    contextToolbar.once('close', () => (openDropdowns.value.layout = false));

    contextToolbar.render();
}

function openSettingsPopup(graph: joint.dia.Graph) {
    const { inputs } = getSettingsInspectorConfig();

    if (openDropdowns.value.settings) {
        joint.ui.Popup.close();
        return;
    }

    openDropdowns.value.settings = true;

    const settingsInspector = new joint.ui.Inspector({
        cell: graph,
        className: 'settings-inspector',
        inputs
    }).render();

    const settingsPopup = new joint.ui.Popup({
        content: settingsInspector.el,
        target: toolbar.value?.getWidgetByName('select-canvas-settings').el,
        root: toolbarContainer.value as HTMLElement,
        padding: 0,
        position: 'bottom-right',
        anchor: 'top-right',
        autoResize: true,
        arrowPosition: 'none'
    }).render();

    settingsPopup.once('close', () => {
        settingsInspector.updateCell();
        // Update openDropdowns state when the settingsPopup is closed
        openDropdowns.value.settings = false;
    });
}

function openShareSelect(paper: joint.dia.Paper) {
    if (openDropdowns.value.share) {
        joint.ui.ContextToolbar.close();
        return;
    }

    openDropdowns.value.share = true;

    const contextToolbar = new joint.ui.ContextToolbar({
        target: toolbar.value?.getWidgetByName('select-share').el,
        root: toolbarContainer.value as HTMLElement,
        padding: 0,
        vertical: true,
        position: 'bottom-right',
        anchor: 'top-right',
        tools: shareTools.value
    });

    contextToolbar.on({
        'action:exportPNG': () => {
            exportPNG(paper);
            joint.ui.ContextToolbar.close();
        },
        'action:exportSVG': () => {
            exportSVG(paper);
            joint.ui.ContextToolbar.close();
        },
        'action:print': () => {
            joint.format.print(paper, { grid: true });
            joint.ui.ContextToolbar.close();
        }
    });

    // Update openDropdowns state when the contextToolbar is closed
    contextToolbar.once('close', () => (openDropdowns.value.share = false));

    // contextToolbar.on('action:');

    contextToolbar.render();
}

function openSettingsSelect(graph: joint.dia.Graph, paper: joint.dia.Paper) {
    if (openDropdowns.value.settings) {
        joint.ui.ContextToolbar.close();
        return;
    }

    openDropdowns.value.settings = true;

    const contextToolbar = new joint.ui.ContextToolbar({
        target: toolbar.value?.getWidgetByName('select-settings').el,
        root: toolbarContainer.value as HTMLElement,
        padding: 0,
        vertical: true,
        position: 'bottom-left',
        anchor: 'top-left',
        tools: settingsTools.value
    });

    contextToolbar.on({
        'action:toggle-snaplines': () => {
            emit('toggleSnaplines');
            joint.ui.ContextToolbar.close();
        },
        'action:toggle-scratchpad': () => {
            emit('toggleScratchpad');
            joint.ui.ContextToolbar.close();
        }
    });

    // Update openDropdowns state when the contextToolbar is closed
    contextToolbar.once('close', () => (openDropdowns.value.settings = false));

    contextToolbar.render();
}
</script>
<template>
    <div ref="toolbarContainer" class="toolbar-container"></div>
</template>
