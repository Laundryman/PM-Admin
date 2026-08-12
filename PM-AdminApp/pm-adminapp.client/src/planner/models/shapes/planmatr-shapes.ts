import { dia, g, util } from '@joint/plus';
import { type ColumnUpright } from '../ColumnUpright';
// eslint-disable-next-line @typescript-eslint/no-namespace
export namespace planmatr {
    // <g class="rotatable">
    //     <rect @selector="body" class="body"/>
    //     <rect @selector="footer" class="footer"/>
    //     <text @selector="label" class="label"/>
    //     <rect @selector="header" class="header"/>
    //     <image @selector="headerGraphic" class="header-graphic"/>
    // </g>

    const carcassMarkup = util.svg /* xml */ `
                <rect @selector="body" class="body"/>
        `;
    export class Carcass extends dia.Element {
        preinitialize() {
            this.markup = carcassMarkup;
        }
        defaults() {
            return {
                type: 'planmatr.Carcass',
                shapeType: 'Carcass',
                columns: [],
                rows: [],
                disableMove: true,
                merchHeight: 0,
                merchWidth: 0,
                headerHeight: 0,
                headerWidth: 0,
                footerHeight: 0,
                footerWidth: 0,
                size: {
                    width: 80,
                    height: 80
                },
                attrs: {
                    root: {
                        magnet: false
                    },
                    label: {
                        text: 'Model',
                        'ref-x': 0.5,
                        'ref-y': 10,
                        'font-size': 18,
                        'text-anchor': 'middle',
                        fill: '#fff'
                    },
                    body: {
                        stroke: '#000',
                        fill: '#fff',
                        height: 'calc(h)',
                        width: 'calc(w)'
                    },
                    header: {
                        stroke: '#000',
                        fill: '#f0f0f0'
                    },
                    headerGraphic: {},
                    footer: {
                        stroke: '#000',
                        fill: '#f0f0f0'
                    }
                }
            };
        }
    }
    const headerMarkup = util.svg /* xml */ `
            <rect @selector="body" class="body"/>
            <text @selector="label" class="label"/>
            <image @selector="headerGraphic" class="header-graphic"/>
        `;
    export class Header extends dia.Element {
        preinitialize() {
            this.markup = headerMarkup;
        }
        defaults() {
            return {
                type: 'planmatr.Header',
                shapeType: 'Header',
                size: {
                    width: 80,
                    height: 80
                },
                disableMove: true,
                attrs: {
                    '.': {
                        magnet: false
                    },
                    label: {
                        text: '',
                        'ref-x': 0.5,
                        'ref-y': 10,
                        'font-size': 18,
                        'text-anchor': 'middle',
                        fill: '#000'
                    },
                    body: {
                        height: 'calc(h)',
                        width: 'calc(w)',
                        stroke: '#000',
                        fill: '#fff'
                    }
                }
            };
        }
    }
    const columnMarkup = util.svg /* xml */ `
            <rect @selector="body" class="body"/>
            <text @selector="label" class="label"/>
        `;
    export class Column extends dia.Element {
        uprights!: ColumnUpright[];
        preinitialize() {
            this.markup = columnMarkup;
        }
        defaults() {
            return {
                type: 'planmatr.Column',
                shapeType: 'Column',
                size: {
                    width: 80,
                    height: 80
                },
                disableMove: true,
                attrs: {
                    '.': {
                        magnet: false
                    },
                    '.label': {
                        text: '',
                        'ref-x': 0.5,
                        'ref-y': 10,
                        'font-size': 18,
                        'text-anchor': 'middle',
                        fill: '#000'
                    },
                    body: {
                        height: 'calc(h)',
                        width: 'calc(w)',
                        stroke: '#000',
                        fill: '#fff'
                    }
                }
            };
        }
    }

    const rowMarkup = util.svg /* xml */ `
            <rect @selector="body" class="body"/>
            <text @selector="label" class="label"/>
        `;

    export class Row extends dia.Element {
        preinitialize() {
            this.markup = rowMarkup;
        }
        defaults() {
            return {
                type: 'planmatr.Row',
                shapeType: 'Row',
                size: {
                    width: 80,
                    height: 80
                },
                disableMove: true,
                attrs: {
                    '.': {
                        magnet: false
                    },
                    label: {
                        text: '',
                        'ref-x': 0.5,
                        'ref-y': 10,
                        'font-size': 18,
                        'text-anchor': 'middle',
                        fill: '#000'
                    },
                    body: {
                        height: 'calc(h)',
                        width: 'calc(w)',
                        stroke: '#000',

                        fill: '#fff'
                    }
                }
            };
        }
    }
    const uprightMarkup = util.svg /* xml */ `
            <rect @selector="body" class="body"/>
            <line @selector="upright" class="upright"/>
        `;

    export class Upright extends dia.Element {
        preinitialize() {
            this.markup = uprightMarkup;
        }
        defaults() {
            return {
                type: 'planmatr.Upright',
                shapeType: 'Upright',
                size: {
                    width: 2,
                    height: 80
                },
                disableMove: true,
                attrs: {
                    '.': {
                        magnet: false
                    },
                    line: {
                        fill: 'none',
                        stroke: '#000',
                        'stroke-width': 1,
                        'stroke-dasharray': '1,10',
                        'stroke-dashoffset': 0.76246919,
                        'stroke-opacity': 1,
                        y2: 100,
                        y1: 0,
                        x1: 0,
                        x2: 0
                    },
                    body: {
                        height: 'calc(h)',
                        width: 'calc(w)',
                        stroke: 'none',
                        fill: 'none'
                    }
                }
            };
        }
    }
}
// <g class="rotatable" @selector="menu-cassette-container">
//     <svg @selector="menu-item" class="menu-item" viewbox="5 0 100 90">
//         <rect class="body"/>
//         <image @selector="cassetteImage" class="cassette"/>
//         <foreignObject @selector="fobj" class="fobj" width="100" height="80">
//         <body xmlns="http://www.w3.org/1999/xhtml">
//         <div @selector="menutable" class="menutable"/>
//         </body>
//         </foreignObject>
//     </svg>

// </g>

// eslint-disable-next-line @typescript-eslint/no-namespace
export namespace planmatr.Part {
    const menuCassetteMarkup = util.svg /* xml */ `
                        <rect @selector="cassetteBody" class="cassettebody" width="100px" height="160px"/>
                        <image @selector="cassetteImage" class="cassette" width="100px" height="100px"/>
                    <foreignObject @selector="fobj" class="fobj" width="80" height="60">
                        <body xmlns="http://www.w3.org/1999/xhtml">
                            <div @selector="menutable" class="menutable"/>
                        </body>
                    </foreignObject>
        `;

    //                 <g class="rotatable">
    //                 <rect class="menu-container" @selector="menu-container"/>
    //                 <svg @selector="menu-item" class="menu-item" viewBox="5 0 100 90" width="100" height="100" preserveAspectRatio="xMidYMid meet">
    //                     <rect @selector="cassetteBody" class="cassettebody" width="100%" height="100%"/>
    //                     <image @selector="cassetteImage" class="cassette" width="100px" height="100%"/>
    //                 </svg>
    //                 <foreignObject @selector="fobj" class="fobj" width="80" height="60">
    //                     <body xmlns="http://www.w3.org/1999/xhtml">
    //                         <div @selector="menutable" class="menutable"/>
    //                     </body>
    //                 </foreignObject>

    // <rect class="body"/>
    // </g>

    export class MenuCassette extends dia.Element {
        preinitialize() {
            this.markup = menuCassetteMarkup;
        }
        defaults() {
            return {
                ...super.defaults,
                type: 'planmatr.Part.MenuCassette',
                shapeType: 'MenuCassette',
                size: {
                    width: 40,
                    height: 40
                },
                disableMove: false,
                partInfo: {
                    partId: 0,
                    planogramPartId: 0,
                    parentPlanogramPartId: 0,
                    planogramShelfId: 0,
                    partTypeId: 1,
                    partType: '',
                    name: '',
                    category: '',
                    partNumber: '',
                    position: new g.Point(),
                    facings: 0,
                    stock: 0,
                    height: 0,
                    width: 0,
                    notes: '',
                    status: '',
                    packShotImageSrc: '',
                    svgLineGraphic: '',
                    products: '',
                    facingProducts: []
                },
                planogramInfo: {
                    x: 0,
                    y: 0,
                    planogramId: 0,
                    scratchPadId: 0
                },
                attrs: {
                    '.': {
                        magnet: false
                    },
                    body: {
                        // width: '100%',
                        // height: '100%',
                        stroke: '#000'
                    },
                    // image: {},
                    cassetteImage: {
                        // width: '100%',
                        // height: '100%'
                        // x: '0'
                    },
                    'menu-container': {
                        fill: '#f1f5f9',
                        // fill: '#fff',
                        width: '100px',
                        height: '160px'
                    },
                    'menu-item': {},
                    cassetteBody: {
                        fill: '#f1f5f9'
                    },
                    fobj: {
                        y: '100px'
                    }
                }
                // {
                //     markup: [
                //         '<g class="rotatable">',
                //         '<rect class="menu-container"/>',
                //         '<foreignObject class="fobj" width="100" height="80">',
                //         '<body xmlns="http://www.w3.org/1999/xhtml">',
                //         '<div class="menutable"/>',
                //         '</body>',
                //         '</foreignObject>',
                //         '<svg class="menu-item" viewbox="5 0 100 90">',
                //         '<rect class="body"/>',
                //         '<image class="cassette"/>',
                //         '</svg>',
                //         '</g>'
                //     ].join('')
                // },
                // dia.Element.prototype.defaults
            };
        }
    }

    const cassetteMarkup = util.svg /* xml */ `

                        <rect @selector="icon" class="icon"/>
                        <image @selector="cassette" class="cassette"/>
                        <g @selector="cassette-container" class="cassette-container">
                        <svg class="cSvg" @selector="cSvg" preserveAspectRatio="none" viewbox="0 0 100 100"></svg>
                        </g>
                        <rect @selector="body" id="body" class="body"/>
                        <foreignObject @selector="facingObject" class="facing-object">
                        <body xmlns="http://www.w3.org/1999/xhtml">
                        <div class="facings" @selector="facings"/>
                        </body>
                        </foreignObject>
                        <circle @selector="commentIndicator" class="comment-indi" cx="5" cy="5" r="5"/>
                        <text @selector="label" id="label"/>
        `;
    export class Cassette extends dia.Element {
        preinitialize() {
            this.markup = cassetteMarkup;
        }
        defaults() {
            return {
                ...super.defaults,
                type: 'planmatr.Part.Cassette',
                shapeType: 'Cassette',
                size: {
                    width: 40,
                    height: 40
                },
                disableMove: false,
                partInfo: {
                    partId: 0,
                    planogramPartId: 0,
                    parentPlanogramPartId: 0,
                    planogramShelfId: 0,
                    partTypeId: 1,
                    partType: '',
                    name: '',
                    category: '',
                    partNumber: '',
                    position: new g.Point(),
                    facings: 0,
                    stock: 0,
                    height: 0,
                    width: 0,
                    notes: '',
                    hasNotes: true,
                    status: '',
                    packShotImageSrc: '',
                    render2dImage: '',
                    svgLineGraphic: '',
                    products: [],
                    facingProducts: [],
                    title: ''
                },
                planogramInfo: {
                    x: 0,
                    y: 0,
                    planogramId: 0,
                    scratchPadId: 0
                },
                attrs: {
                    '.': {
                        magnet: false
                    },
                    label: {
                        text: '',
                        'text-anchor': 'middle',
                        // fill: '#000',
                        'font-size': 10,
                        'max-length': 28,
                        x: 0.5,
                        y: 0.5,
                        'y-alignment': 'middle',
                        'font-family': 'Arial, helvetica, sans-serif'
                    },
                    body: {
                        stroke: '#000',
                        fill: '#000',
                        width: 'calc(w)',
                        height: 'calc(h)',
                        'fill-opacity': 0
                    },
                    facings: {
                        width: 'calc(w)',
                        height: 'calc(h)',
                        stroke: '#000',
                        fill: '#fff',
                        'fill-opacity': '0.4',
                        'font-size': '8px'
                    },
                    'cassette-container': {
                        shape: document.createElementNS('http://www.w3.org/2000/svg', 'svg')
                    },
                    cSvg: {
                        'font-size': '8px'
                        // '#BACKGROUND': {
                        //     fill: 'NONE'
                        // },
                        // '#STATUS': {
                        //     fill: 'NONE'
                        // }
                    },
                    '#BACKGROUND': {
                        fill: 'NONE'
                    },
                    '#STATUS': {
                        fill: 'NONE'
                    },
                    title: {
                        fill: '#000',
                        'font-size': 18,
                        'font-family': 'Arial, helvetica, sans-serif',
                        display: 'block'
                    },
                    // 'tspan':  {
                    //   fill: '#000',
                    //   'font-size': 18,
                    //   'fill-opacity': 1,
                    //   'stroke-opacity': 1,
                    //   style: 'fill-opacity: 1; stroke-opacity: 1;',
                    // },

                    // image: {
                    //     width: '100',
                    //     height: '100'
                    // },
                    icon: {
                        // width: 'calc(w)',
                        // height: 'calc(h)'
                    },
                    commentIndicator: {
                        fill: '#ff0000',
                        visibility: 'hidden',
                        cx: '5',
                        cy: '5',
                        r: '5'
                    },
                    cassette: {
                        width: 'calc(w)',
                        height: 'calc(h)'
                    },
                    facingObject: {
                        width: 'calc(w)',
                        height: 'calc(h)'
                        // opacity: 0
                    },
                    'facing-item-name': {
                        transform: 'rotate(90deg)',
                        'transform-origin': 'left top 0'
                    }
                }
            };
        }

        // {
        //     markup: [
        //         '<g class="rotatable">',
        //         '<g class="scalable">',
        //         '<rect class="icon"/>',
        //         '<image class="cassette"/>',
        //         '<svg class="cSvg" preserveAspectRatio="none"></svg>',
        //         '<rect id="body" class="body"/>',
        //         '<foreignObject class="facing-object">',
        //         '<body xmlns="http://www.w3.org/1999/xhtml">',
        //         '<div class="facings"/>',
        //         '</body>',
        //         '</foreignObject>',
        //         '<circle class="comment-indi" cx="5" cy="5" r="5"/>',
        //         '</g><text id="label"/></g>'
        //     ].join('')
        // },
        // dia.Element.prototype.defaults
    }

    Cassette.attributes = {
        shape: {
            set: function (value: any, refBBox: any, _node: any, attrs: any, _cellView: any) {
                if (value != null) {
                    _node.removeChild(_node.querySelector('svg'));
                    _node.appendChild(value);
                    return { shape: '' };
                }
            }
        }
    };

    const shelfMarkup = util.svg /* xml */ `
                    <rect @selector="bgfill" class="bgfill"/>
                    <image @selector="shelf" class="shelf"/>
                    <rect @selector="body" class="body"/>
                    <text @selector="label" id="label"/>
        `;
    export class Shelf extends dia.Element {
        preinitialize() {
            this.markup = shelfMarkup;
        }
        defaults() {
            return {
                ...super.defaults,
                type: 'planmatr.Part.Shelf',
                shapeType: 'Shelf',
                size: {
                    width: 80,
                    height: 80
                },
                disableMove: false,
                shelfInfo: {
                    id: 0,
                    planogramId: 0,
                    partId: 0,
                    planogramShelfId: 0,
                    clusterShelfId: 0,
                    shelfTypeId: 0,
                    partTypeId: 0,
                    partNumber: '',
                    height: 0,
                    width: 0,
                    label: '',
                    status: 0,
                    column: 0,
                    notes: '',
                    svgLineGraphic: '',
                    packShotImageSrc: '',
                    render2dImage: ''
                },
                planogramInfo: {
                    x: 0,
                    y: 0,
                    planogramId: 0,
                    scratchPadId: 0
                },

                attrs: {
                    '.': {
                        magnet: false
                    },
                    rect: {
                        fill: '#ffffff',
                        stroke: '#fff',
                        width: 100,
                        height: 60
                    },
                    '#label': {
                        fill: '#000',
                        'font-size': 8,
                        'max-length': 28,
                        'ref-x': 0.5,
                        'ref-y': 0.5,
                        'text-anchor': 'middle',
                        'y-alignment': 'middle',
                        'font-family': 'Arial, helvetica, sans-serif'
                    },
                    bgfill: {
                        'ref-width': '100%',
                        'ref-height': '100%',
                        stroke: '#000',
                        fill: '#fff',
                        'fill-opacity': '1'
                    },
                    body: {
                        'ref-width': '100%',
                        'ref-height': '100%',
                        stroke: '#000',
                        fill: '#fff',
                        'fill-opacity': 0
                    },
                    shelf: {
                        'ref-width': '100%',
                        'ref-height': '100%'
                    },
                    image: {
                        'ref-width': '100%',
                        'ref-height': '100%'
                    }
                    // '`shelf-image': {
                    //     'ref-width': '100%',
                    //     'ref-height': '100%'
                    // }
                }
            };
            // {
            //     markup: '<g><rect class="bgfill"/><image class="shelf"/><rect class="body"/><text id="label"/></g>'
            // },
        }
    }
    const menuShelfMarkup = util.svg /* xml */ `
                    <rect @selector="shelfbody" class="shelfbody" width="100px" height="160px"/>

                    <image @selector="shelf" class="shelf" width="100" height="90"/>
                    <foreignObject @selector="fobj" class="fobj" width="80" height="60">
                        <body xmlns="http://www.w3.org/1999/xhtml">
                            <div @selector="menutable" class="menutable"/>
                        </body>
                    </foreignObject>

        `;
    export class MenuShelf extends dia.Element {
        preinitialize() {
            this.markup = menuShelfMarkup;
        }
        defaults() {
            return {
                ...super.defaults,
                type: 'planmatr.Part.MenuShelf',
                shapeType: 'MenuShelf',
                size: {
                    width: 80,
                    height: 80
                },
                disableMove: false,
                shelfInfo: {
                    id: 0,
                    planogramId: 0,
                    partId: 0,
                    planogramShelfId: 0,
                    clusterShelfId: 0,
                    shelfTypeId: 0,
                    partTypeId: 0,
                    partNumber: '',
                    height: 0,
                    width: 0,
                    label: '',
                    status: '',
                    column: 0,
                    notes: '',
                    svgLineGraphic: '',
                    packShotImageSrc: '',
                    render2dImage: ''
                },
                planogramInfo: {
                    // x: 0,
                    // y: 0,
                    // planogramId: 0,
                    // scratchPadId: 0
                },

                attrs: {
                    '.': {
                        magnet: false
                    },
                    text: {
                        // fill: '#000',
                        // 'font-size': 6,
                        // 'ref-x': 0.5,
                        // 'ref-y': 0.5,
                        // 'text-anchor': 'middle',
                        // 'y-alignment': 'middle',
                        // 'font-family': 'Arial, helvetica, sans-serif'
                    },
                    '.label': {
                        // text: '',
                        // 'ref-x': 0.5,
                        // 'ref-y': 0.5,
                        // yAlignment: 'middle',
                        // textAnchor: 'middle',
                        // 'font-size': 6,
                        // 'text-anchor': 'middle',
                        // fill: '#000'
                    },
                    'menu-container': {
                        // fill: '#5d5d5d',
                        // height: '180px',
                        // width: '100px'
                    },
                    'svg-body': {
                        stroke: '#000'
                    },
                    shelfbody: {
                        // width: '100px',
                        // // 'height': '140px',
                        // stroke: '#fff'
                        // //fill: '#fff'
                        fill: '#f1f5f9'
                        // stroke: '#000'
                    },
                    shelf: {
                        // 'ref-width': '50%',
                        // 'ref-height': '50%'
                        'max-width': '90px',
                        stroke: '#fff'
                    },
                    fobj: {
                        y: '100px'
                    }
                }
            };
            // {
            //     markup: [
            //         '<g class="rotatable">',
            //         '<rect class="menu-container"/>',
            //         '<foreignObject class="fobj" width="100" height="80">',
            //         '<body xmlns="http://www.w3.org/1999/xhtml">',
            //         '<div class="menutable"/>',
            //         '</body>',
            //         '</foreignObject>',
            //         '<svg class="menu-item" viewbox="5 0 100 90">',
            //         '<rect class="body"/>',
            //         '<image class="shelf"/>',
            //         '</svg>',
            //         // '<g class=""></g>',
            //         '</g>'
            //     ].join('')
            // },
            // dia.Element.prototype.defaults);
        }
    }

    export const NavigatorElementView = dia.ElementView.extend({
        body: null,

        markup: [
            {
                tagName: 'rect',
                selector: 'body',
                attributes: {
                    stroke: '#000'
                }
            }
        ],

        initFlag: ['RENDER', 'UPDATE', 'TRANSFORM'],

        presentationAttributes: {
            size: ['UPDATE'],
            position: ['TRANSFORM'],
            angle: ['TRANSFORM'],
            fill: ['UPDATE']
        },

        confirmUpdate: function (flags: number) {
            if (this.hasFlag(flags, 'RENDER')) {
                this.render();
            }
            if (this.hasFlag(flags, 'UPDATE')) {
                this.update();
            }
            if (this.hasFlag(flags, 'TRANSFORM')) {
                this.updateTransformation();
            }
        },

        render: function () {
            const {
                fragment,
                selectors: { body }
            } = util.parseDOMJSON(this.markup);
            this.body = body;
            this.el.appendChild(fragment);
        },

        update: function () {
            const { model, body } = this;
            const { width, height } = model.size();
            const { shapeType } = model.attributes;
            const { fill } = model.attr('.body');

            body.setAttribute('width', width);
            body.setAttribute('height', height);
            body.setAttribute('fill', fill);
            // if (model.attr[".body"]) {
            //   const { fill } = model.attr(".body");
            //   body.setAttribute('fill', fill);
            // }
            if (shapeType == 'Cassette' || shapeType == 'Shelf') {
                body.setAttribute('fill-opacity', '0.4');
            } else {
                body.setAttribute('fill-opacity', '1');
            }
            //body.set
        }
    });
}
