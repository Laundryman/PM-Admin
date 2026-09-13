<script lang="ts" setup>
import { dia, ui, util } from '@joint/plus';
import { onMounted, ref } from 'vue';

const navigatorContainer = ref<HTMLElement | null>(null);
const toolbar = ref<ui.Toolbar | null>(null);
const navigator = ref<ui.Navigator | null>(null);
const transitionCanceled = ref(false);

const ZOOM_SETTINGS = {
    min: 0.2,
    max: 2
};

const baseUrl = '/public/assets/navigator';

const IconButton = ui.widgets.button.extend({
    render: function () {
        const size = this.options.size || 20;
        const imageEl = document.createElement('img');
        imageEl.style.width = `${size}px`;
        imageEl.style.height = `${size}px`;
        this.el.appendChild(imageEl);
        this.setIcon(this.options.icon);
        this.setTooltip(this.options.tooltip);
        return this;
    },
    setIcon: function (icon = '') {
        this.el.querySelector('img').src = icon;
    },
    setTooltip: function (tooltip = '') {
        this.el.dataset.tooltip = tooltip;
    }
});

// Simplified navigator element view

const UpdateFlags = {
    Render: '@render',
    Update: '@update',
    Transform: '@transform'
};

const NavigatorElementView = dia.ElementView.extend({
    body: null,
    markup: util.svg /* xml */ `<path @selector="body" opacity="0.4" />`,
    // updates run on view initialization
    initFlag: [UpdateFlags.Render, UpdateFlags.Update, UpdateFlags.Transform],
    // updates run when the model attribute changes
    presentationAttributes: {
        position: [UpdateFlags.Transform],
        angle: [UpdateFlags.Transform],
        size: [UpdateFlags.Update] // shape
    },
    // calls in an animation frame after a multiple changes
    // has been made to the model
    confirmUpdate: function (flags: number) {
        if (this.hasFlag(flags, UpdateFlags.Render)) this.render();
        if (this.hasFlag(flags, UpdateFlags.Update)) this.update();
        // using the original `updateTransformation()` method
        if (this.hasFlag(flags, UpdateFlags.Transform)) this.updateTransformation();
    },
    render: function () {
        const doc = util.parseDOMJSON(this.markup);
        this.body = doc.selectors.body;
        this.body.classList.add(this.model.get('group'));
        this.el.appendChild(doc.fragment);
    },
    update: function () {
        const { model, body } = this;
        // shape
        const { width, height } = model.size();
        const d = `M 0 0 H ${width} V ${height} H 0 Z`;
        body.setAttribute('d', d);
    }
});

const emit = defineEmits(['navigatorLoaded']);
const props = defineProps<{
    paperScroller: ui.PaperScroller;
}>();

defineExpose({
    create
});

onMounted(() => {
    emit('navigatorLoaded', {
        NavigatorElementView,
        IconButton,
        ZOOM_SETTINGS
    });
    create();
});

function create() {
    toolbar.value = new ui.Toolbar({
        autoToggle: true,
        references: {
            paperScroller: props.paperScroller
        },
        tools: [
            {
                type: 'icon-button',
                name: 'fullscreen'
                /* icon and tooltip are set in updateToolbarButtons() */
            },
            {
                type: 'icon-button',
                name: 'fit-to-screen',
                icon: `${baseUrl}/fit-to-screen.svg`,
                tooltip: 'Fit to screen'
            },
            {
                type: 'zoom-slider',
                min: ZOOM_SETTINGS.min * 100,
                max: ZOOM_SETTINGS.max * 100,
                step: 10,
                attrs: { input: { 'data-tooltip': 'Slide to zoom' } }
            },
            { type: 'separator' },
            {
                type: 'icon-button',
                name: 'minimap',
                icon: `${baseUrl}/minimap.svg`
            }
        ],
        widgetNamespace: {
            ...ui.widgets,
            iconButton: IconButton
        } as { [name: string]: typeof ui.Widget }
    });

    toolbar.value?.render();
    updateToolbarButtons();
    navigatorContainer.value?.appendChild(toolbar.value?.el);

    toolbar.value?.on('fit-to-screen:pointerclick', () => fitToScreen());
    toolbar.value?.on('fullscreen:pointerclick', () => toggleFullscreen());
    toolbar.value?.on('minimap:pointerclick', () => toggleMinimap());

    document.addEventListener('fullscreenchange', () => updateToolbarButtons());

    navigator.value = new ui.Navigator({
        paperScroller: props.paperScroller,
        width: 340,
        height: 130,
        padding: 10,
        zoom: false,
        useContentBBox: true,
        paperOptions: {
            async: true,
            autoFreeze: true,
            sorting: dia.Paper.sorting.APPROX,
            elementView: NavigatorElementView,
            cellViewNamespace: {},
            viewManagement: {
                disposeHidden: true
            },
            // Don't render links in the navigator
            cellVisibility: (cell) => !cell.isLink(),
            background: {
                color: 'transparent'
            }
        }
    });

    navigator.value?.el.addEventListener('transitionend', () => {
        if (transitionCanceled.value) return;
        navigator.value?.updateCurrentView();
    });

    navigator.value?.el.addEventListener('transitioncancel', () => {
        transitionCanceled.value = true;
    });

    navigatorContainer.value?.prepend(navigator.value?.el);
    navigator.value?.render();
    toolbar.value?.getWidgetByName('minimap').el.classList.add('active');
}

function isMinimapVisible() {
    return !navigator.value?.el.classList.contains('hidden');
}

function fitToScreen() {
    props.paperScroller.zoomToFit({ useModelGeometry: true, padding: 20 });
}

function toggleFullscreen() {
    const fullScreenEl = toolbar.value?.getWidgetByName('fullscreen').el;

    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen();
        fullScreenEl?.classList.add('active');
    } else if (document.exitFullscreen) {
        document.exitFullscreen();
        fullScreenEl?.classList.remove('active');
    }
}

function showMinimap() {
    navigator.value?.el.classList.remove('hidden');
    transitionCanceled.value = false;
}

function hideMinimap() {
    navigator.value?.el.classList.add('hidden');
}

function toggleMinimap() {
    const minimapEl = toolbar.value?.getWidgetByName('minimap').el;

    if (isMinimapVisible()) {
        hideMinimap();
        minimapEl?.classList.remove('active');
    } else {
        showMinimap();
        minimapEl?.classList.add('active');
    }
    updateToolbarButtons();
}

function updateToolbarButtons() {
    // Minimap
    const minimapButton = toolbar.value?.getWidgetByName('minimap') as typeof IconButton;
    if (isMinimapVisible()) {
        minimapButton.setTooltip('Hide minimap');
    } else {
        minimapButton.setTooltip('Show minimap');
    }
    // Full screen
    const fullscreenButton = toolbar.value?.getWidgetByName('fullscreen') as typeof IconButton;
    if (document.fullscreenElement) {
        fullscreenButton.setIcon(`${baseUrl}/exit-fullscreen.svg`);
        fullscreenButton.setTooltip('Exit full screen');
    } else {
        fullscreenButton.setIcon(`${baseUrl}/request-fullscreen.svg`);
        fullscreenButton.setTooltip('Toggle full screen');
    }
}
</script>

<template>
    <div ref="navigatorContainer" class="navigator-container"></div>
</template>
