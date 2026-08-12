import * as joint from '@joint/plus';
// import * as _ from 'lodash';
import * as appShapes from '@/planner/models/shapes/planmatr-shapes';
import { CurrentView, PartTypes, ShadeStatusColourEnum, StatusColourEnum } from '../models/Enumerations';
// import type { PartInfo } from '@/planner/models/PartInfo';
import type { Product } from '@/planner/models/Product';
import type { Shade } from '@/planner/models/Shade';

//VITE_APP_CASSETTERENDER_URL
const renderGraphicUrl = import.meta.env.VITE_APP_CASSETTERENDER_URL;

export class PlanogramRenderService {
    async displayRenderView(paper: joint.dia.Paper, graph: joint.dia.Graph) {
        const allElems = graph.getElements();

        const carcass = allElems.filter(function (el) {
            return el.attributes.type === 'planmatr.Carcass';
        });
        const columns = allElems.filter(function (el) {
            return el.attributes.type === 'planmatr.Column';
        });
        const carcassView = paper.findViewByModel(carcass[0] as joint.dia.Element);
        for (let i = 0; i < columns.length; i++) {
            const column = paper.findViewByModel(columns[i] as joint.dia.Element);
            column.el.classList.add('render-view');
        }

        carcassView.el.classList.add('render-view');

        await this.renderRenderView(allElems, paper);
    }

    async renderRenderView(allElems: joint.dia.Element[], paper: joint.dia.Paper) {
        const self = this;
        //remove status colour
        const cassettes = allElems.filter(function (el) {
            return el.attributes.type === 'planmatr.Part.Cassette' && el.attributes.planogramInfo.scratchPadId === 0;
        });

        cassettes.forEach(function (cassette: joint.dia.Element) {
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['body'] != null) {
                cassette.attributes.attrs['body']['fill-opacity'] = 0;
            }
            //cassette.attributes.attrs[".cSvg"].visibility = "hidden";

            self.clearShadeView(cassette);
            self.displayItemRenderView(paper, cassette);
            self.hideSvgGraphic(cassette);
        });
    }

    displayItemRenderView(paper: joint.dia.Paper, cassette: joint.dia.Element) {
        if (cassette.attributes.attrs != null && cassette.attributes.attrs['body'] != null) {
            cassette.attributes.attrs['body']['fill-opacity'] = 0;
        }
        this.clearShadeView(cassette);
        if (cassette.attributes.partInfo.partTypeId == PartTypes.Blanking || cassette.attributes.partInfo.partTypeId == PartTypes.FasciaPlate) {
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['cassette'] != null) {
                cassette.attributes.attrs['cassette']['opacity'] = 0.5;
            }
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['body'] != null) {
                if (cassette.attributes.attrs['body']['fill'] == '#8d8c8c') {
                    cassette.attributes.attrs['body']['fill'] = '#fff';
                }
            }
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['icon'] != null) {
                cassette.attributes.attrs['icon']['fill-opacity'] = 0.3;
            }
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['facingObject'] != null) {
                cassette.attributes.attrs['facingObject']['fill-opacity'] = 0.3;
            }
        } else {
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['facingObject'] != null) {
                cassette.attributes.attrs['facingObject']['fill-opacity'] = 0.6;
            }
        }

        if (cassette.attributes.partInfo.svgLineGraphic != null) {
            const parser = new DOMParser();
            const graphicSvg = cassette.attributes.partInfo.svgLineGraphic.substring(cassette.attributes.partInfo.svgLineGraphic.indexOf('<svg'));
            const partGraphic = parser.parseFromString(graphicSvg, 'image/svg+xml');

            if (partGraphic.querySelector('#STATUS') != null) {
                this.renderSvgGraphic(graphicSvg, cassette);

                this.hideSvgGraphic(cassette);
                if (cassette.attributes.attrs != null && cassette.attributes.attrs['cassette'] != null) {
                    cassette.attributes.attrs['cassette']['opacity'] = 1;
                }
            }
        }
        if (cassette.attributes.partInfo.render2dImage != null) {
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['cassette'] != null) {
                cassette.attributes.attrs['cassette']['xlink:href'] = renderGraphicUrl + encodeURIComponent(cassette.attributes.partInfo.render2dImage);
            }

            const cellView = cassette.findView(paper);
            cellView.render();
        } else {
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['body'] != null) {
                cassette.attributes.attrs['body']['fill-opacity'] = 1;
                cassette.attributes.attrs['body']['fill'] = '#8d8c8c';
            }
            const cellView = cassette.findView(paper);
            cellView.render();
        }
    }

    ///display a single item in the cassette view
    displayItemCassetteView(paper: joint.dia.Paper, cassette: joint.dia.Element) {
        if (cassette.attributes.attrs != null && cassette.attributes.attrs['body'] != null) {
            cassette.attributes.attrs['body']['fill-opacity'] = 0.2;
            cassette.attributes.attrs['body']['stroke'] = '#fff';
        }
        if (cassette.attributes.attrs != null && cassette.attributes.attrs['facingObject'] != null) {
            cassette.attributes.attrs['facingObject']['opacity'] = 0.6;
        }
        if (cassette.attributes.attrs != null && cassette.attributes.attrs['icon'] != null) {
            //need to remove border in cassette view
            cassette.attributes.attrs['icon']['stroke-width'] = '0';
        }

        if (cassette.attributes.partInfo.partTypeId == PartTypes.Blanking) {
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['body'] != null) {
                cassette.attributes.attrs['body']['fill-opacity'] = 0.2;
            }
        }
        if (cassette.attributes.partInfo.svgLineGraphic != null) {
            const parser = new DOMParser();
            const graphicSvg = cassette.attributes.partInfo.svgLineGraphic.substring(cassette.attributes.partInfo.svgLineGraphic.indexOf('<svg'));
            const partGraphic = parser.parseFromString(graphicSvg, 'image/svg+xml');

            if (partGraphic.querySelector('#STATUS') != null) {
                this.renderSvgGraphic(graphicSvg, cassette);
            } else {
                if (cassette.attributes.attrs != null && cassette.attributes.attrs['cassette'] != null) {
                    cassette.attributes.attrs['cassette']['xlink:href'] = 'data:image/svg+xml;utf8,' + encodeURIComponent(cassette.attributes.partInfo.svgLineGraphic);
                }
            }
            this.SetStatusColour(cassette, StatusColourEnum[cassette.attributes.partInfo.statusId] as string, CurrentView.cassette);

            const cellView = cassette.findView(paper);
            cellView.render();
        } else {
            this.SetStatusColour(cassette, StatusColourEnum[cassette.attributes.partInfo.statusId] as string, CurrentView.cassette);
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['cassette'] != null) {
                cassette.attributes.attrs['cassette']['xlink:href'] = '';
            }
            const cellView = cassette.findView(paper);

            if (cellView != undefined) {
                cellView.render();
            }
        }
        //planoParts[i].attributes.
    }

    ///display all items in the cassette view
    displayCassetteView(paper: joint.dia.Paper, graph: joint.dia.Graph) {
        const allElems = graph.getElements();
        const carcass = allElems.filter(function (el: joint.dia.Element) {
            return el.attributes.type === 'planmatr.Carcass';
        });
        const columns = allElems.filter(function (el: joint.dia.Element) {
            return el.attributes.type === 'planmatr.Column';
        });
        const carcassView = paper.findViewByModel(carcass[0] as joint.dia.Element);
        for (let i = 0; i < columns.length; i++) {
            const column = paper.findViewByModel(columns[i] as joint.dia.Element);
            column.el.classList.remove('render-view');
        }

        carcassView.el.classList.remove('render-view');

        const cassettes = allElems.filter(function (el: joint.dia.Element) {
            return el.attributes.type === 'planmatr.Part.Cassette' && el.attributes.planogramInfo.scratchPadId === 0;
        });

        for (let i = 0; i < cassettes.length; i++) {
            this.clearShadeView(cassettes[i] as joint.dia.Element);
            this.showSvgGraphic(cassettes[i] as joint.dia.Element);
            this.displayItemCassetteView(paper, cassettes[i] as joint.dia.Element);
        }
    }

    clearShadeView(cassette: joint.dia.Element) {
        if (cassette.attributes.attrs != null && cassette.attributes.attrs['facings'] != null) {
            cassette.attributes.attrs['facings']['html'] = '';
        }
        if (cassette.attributes.attrs != null && cassette.attributes.attrs['facingObject'] != null) {
            cassette.attributes.attrs['facingObject']['opacity'] = 0;
        }
        if (cassette.attributes.attrs != null && cassette.attributes.attrs['body'] != null) {
            cassette.attributes.attrs['body']['stroke'] = '#000'; //black
            cassette.attributes.attrs['body']['fill'] = '#fff';
        }
    }

    ///display shade view for all items
    displayShadeView(paper: joint.dia.Paper, graph: joint.dia.Graph) {
        const allElems = graph.getElements();
        //allElems[0].attributes.type
        const carcass = allElems.filter(function (el: joint.dia.Element) {
            return el.attributes.type === 'planmatr.Carcass';
        });
        const columns = allElems.filter(function (el: joint.dia.Element) {
            return el.attributes.type === 'planmatr.Column';
        });
        const carcassView = paper.findViewByModel(carcass[0] as joint.dia.Element);
        for (let i = 0; i < columns.length; i++) {
            const column = paper.findViewByModel(columns[i] as joint.dia.Element);
            column.el.classList.remove('render-view');
        }

        carcassView.el.classList.remove('render-view');
        this.renderShadeView(allElems, paper);
    }

    ///display single item in shade view
    displayItemShadeView(paper: joint.dia.Paper, cassette: joint.dia.Element) {
        if (cassette.attributes.attrs != null && cassette.attributes.attrs['icon'] != null) {
            cassette.attributes.attrs['icon']['stroke-width'] = '1';
        }
        const statusColour = StatusColourEnum[cassette.attributes.partInfo.statusId];
        if (cassette.attributes.partInfo.statusId == 0 || cassette.attributes.partInfo.statusId == null) {
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['body'] != null) {
                cassette.attributes.attrs['body']['fill-opacity'] = 0.5;
                if (cassette.attributes.attrs['body']['fill'] == '#8d8c8c') {
                    cassette.attributes.attrs['body']['fill'] = '#fff';
                }
                cassette.attributes.attrs['body']['stroke'] = '#000'; //black
            }
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['facingObject'] != null) {
                cassette.attributes.attrs['facingObject']['opacity'] = 0.6;
            }
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['icon'] != null) {
                cassette.attributes.attrs['icon']['stroke'] = '#000'; //black
            }
        } else {
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['body'] != null) {
                cassette.attributes.attrs['body']['fill-opacity'] = 0.3;
                cassette.attributes.attrs['body']['stroke'] = statusColour;
                cassette.attributes.attrs['body']['fill'] = statusColour;
                if (cassette.attributes.attrs['body']['fill'] == '#8d8c8c') cassette.attributes.attrs['body']['fill'] = '#fff';
            }
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['facingObject'] != null) {
                cassette.attributes.attrs['facingObject']['opacity'] = 0.6;
            }
            if (cassette.attributes.attrs != null && cassette.attributes.attrs['icon'] != null) {
                cassette.attributes.attrs['icon']['stroke'] = statusColour;
            }
        }
        if (cassette.attributes.attrs != null && cassette.attributes.attrs['cassette'] != null) {
            cassette.attributes.attrs['cassette']['xlink:href'] = '';
        }
        if (cassette.attributes.partInfo.partTypeId != PartTypes.Blanking && cassette.attributes.partInfo.partTypeId != PartTypes.FasciaPlate) {
            const facingHtml = this.generateFacingHtml(cassette);

            if (cassette.attributes.attrs != null && cassette.attributes.attrs['facings'] != null) {
                cassette.attributes.attrs['facings']['html'] = facingHtml;
            }
        }

        //this section was causing some cassettes to fail to render - so I've commented it out for now. We need to check if the svgLineGraphic is valid before trying to render it.
        if (cassette.attributes.partInfo.svgLineGraphic != null) {
            const parser = new DOMParser();
            const graphicSvg = cassette.attributes.partInfo.svgLineGraphic.substring(cassette.attributes.partInfo.svgLineGraphic.indexOf('<svg'));
            const partGraphic = parser.parseFromString(graphicSvg, 'image/svg+xml');

            if (partGraphic.querySelector('#STATUS') != null) {
                this.renderSvgGraphic(graphicSvg, cassette);
            } else {
                if (cassette.attributes.attrs != null && cassette.attributes.attrs['cassette'] != null) {
                    cassette.attributes.attrs['cassette']['xlink:href'] = 'data:image/svg+xml;utf8,' + encodeURIComponent(cassette.attributes.partInfo.svgLineGraphic);
                }
            }
            this.SetStatusColour(cassette, StatusColourEnum[cassette.attributes.partInfo.statusId] as string, CurrentView.cassette);
        }

        const cellView = cassette.findView(paper);
        if (cellView != undefined) {
            cellView.render();
        }
    }
    renderShadeView(allElems: joint.dia.Element[], paper: joint.dia.Paper) {
        const cassettes = allElems.filter(function (el: joint.dia.Element) {
            return el.attributes.type === 'planmatr.Part.Cassette'; //&& el.attributes.planogramInfo.scratchPadId === 0;
        });

        for (let i = 0; i < cassettes.length; i++) {
            this.displayItemShadeView(paper, cassettes[i] as joint.dia.Element);
            //This method is failing - needs to be rewritten when we get to showing views.
            //this.showSvgGraphic(cassettes[i] as joint.dia.Element);
        }
    }

    updateShadeView(paper: joint.dia.Paper, el: joint.dia.Element) {
        if (el.attributes.attrs != null && el.attributes.attrs.image != null) {
            el.attributes.attrs.image['xlink:href'] = '';
        }
        if (el.attributes.partInfo.partTypeId != PartTypes.Blanking && el.attributes.partInfo.partTypeId != PartTypes.FasciaPlate) {
            const facingHtml = this.generateFacingHtml(el as appShapes.planmatr.Part.Cassette);
            if (el.attributes.attrs != null && el.attributes.attrs['facings'] != null) {
                el.attributes.attrs['facings']['html'] = facingHtml;
            }
        }

        if (el.attributes.partInfo.svgLineGraphic != null) {
            const parser = new DOMParser();
            //var graphicSvg = el.attributes.partInfo.svgLineGraphic.substring(el.attributes.partInfo.svgLineGraphic.indexOf('<svg'))
            const markup = el.attributes.markup?.toString();
            const graphicSvg = markup?.substring(markup.indexOf('<svg'), markup.indexOf('</svg>') + 6);
            const partGraphic = parser.parseFromString(graphicSvg ?? '', 'image/svg+xml');

            if (partGraphic.querySelector('#STATUS') != null) {
                this.renderSvgGraphic(graphicSvg ?? '', el);
            } else {
                if (el.attributes.attrs != null && el.attributes.attrs['cassette'] != null) {
                    el.attributes.attrs['cassette']['xlink:href'] = 'data:image/svg+xml;utf8,' + encodeURIComponent(el.attributes.partInfo.svgLineGraphic);
                }
            }
        }
        const cellView = el.findView(paper);
        if (cellView != undefined) {
            cellView.render();
        }
        document.querySelector('.facing-object')!.setAttribute('opacity', '0.8');
        // $('.facing-object').attr("opacity", "0.8");
    }

    ///display a single item in the cassette view
    displayItemShelf(paper: joint.dia.Paper, cassette: joint.dia.Element) {
        if (cassette.attributes.attrs != null && cassette.attributes.attrs['body'] != null) {
            cassette.attributes.attrs['body']['fill-opacity'] = 0.4;
        }
    }

    generateFacingHtml(part: joint.dia.Element) {
        const partInfo = part.attributes.partInfo;
        const shadeWidth = Math.floor(part.attributes.width / partInfo.facings);
        let fontSize = '8';
        const shadeHeight = part.attributes.height * 0.8;
        let facingRowHtml = '';
        const paddingRL = (shadeWidth - 8) / 2;
        let padding = '10px 0px';

        if (paddingRL > 1) {
            padding = '10px ' + paddingRL + 'px';
        }
        if (partInfo.facings == 1) {
            padding = '5% 24% 5% 35%';
        }

        if (shadeWidth < 10) {
            fontSize = '6';
        }
        //if (partInfo.facings > 5)

        for (let i = 1; i <= partInfo.facings; i++) {
            const selectedProduct = partInfo['selectedProduct-facing-' + i];
            const selectedShade = partInfo['selectedShade-facing-' + i];
            const selectedShadeStatus = partInfo['selectedStatus-facing-' + i];
            const selectColour = ShadeStatusColourEnum[selectedShadeStatus];
            if (partInfo['selectedProduct-facing-' + i] != null) {
                //var shadeList = [];
                if (selectedProduct != null) {
                    // const selProduct = <any>_.find(partInfo.products, ['productId', selectedProduct]);
                    const selProduct = partInfo.products.find((p: Product) => p.id === selectedProduct);
                    //sometimes the product is not in the list - if it's region/country has been altered after being saved to the facing
                    if (selectedShade != null && selProduct != null) {
                        // const selShade = <any>_.find(selProduct.shades, ['shadeId', selectedShade]);
                        const selShade = selProduct.shades.find((s: Shade) => s.id === selectedShade);
                        if (selShade != null && selShade.published == true) {
                            facingRowHtml =
                                facingRowHtml +
                                `<td class=\"facing-item-name vertical-text\" valign=\"top\" data-facingNo=${i} style="padding: ${padding}; color: ${selectColour}; font-weight: bold;" width=\"${shadeWidth}\" height=\"${
                                    shadeHeight
                                }\"><span style="writing-mode: vertical-rl; overflow: hidden; overflow-wrap: break-word;height: ${shadeHeight + 50}px">${selShade.shadeNumber}</span></td>`;
                        } else {
                            facingRowHtml =
                                facingRowHtml +
                                `<td class=\"facing-item-name vertical-text\" valign=\"top\" data-facingNo=${i} style="padding: ${padding}; color: ${selectColour}; font-weight: bold;" width=\"${shadeWidth}\" height=\"${
                                    shadeHeight
                                }\"><span style="writing-mode: vertical-rl; overflow: hidden; overflow-wrap: break-word;height: ${shadeHeight + 50}px"></span></td>`;
                        }
                    } else {
                        facingRowHtml =
                            facingRowHtml +
                            `<td class=\"facing-item-name vertical-text\" valign=\"top\" data-facingNo=${i} style="padding: ${padding}; color: ${selectColour}; font-weight: bold;" width=\"${shadeWidth}\" height=\"${
                                shadeHeight
                            }\"><span style="writing-mode: vertical-rl; overflow: hidden; overflow-wrap: break-word;height: ${shadeHeight + 50}px"></span></td>`;
                    }
                }
            } else {
                facingRowHtml =
                    facingRowHtml +
                    `<td class=\"facing-item-name vertical-text\" valign=\"top\" data-facingNo=${i} style="padding: ${padding}; color: ${selectColour}; font-weight: bold;" width=\"${shadeWidth}\" height=\"${
                        shadeHeight
                    }\"><span style="writing-mode: vertical-rl; overflow: hidden; overflow-wrap: break-word;height: ${shadeHeight + 50}px"></span></td>`;
            }
        }
        let facingTableHtml = '';

        let partName = partInfo.name;
        if (part.attributes.partInfo.partTypeId == PartTypes.Blanking) {
            partName = '';
        }
        facingTableHtml =
            facingTableHtml +
            [
                '<body xmlns="http://www.w3.org/1999/xhtml">',
                '<div class="facing-info">',
                `<table class=\"facing-table\" width=\"${part.attributes.width}\" height=\"${part.attributes.height}\" style=\"font-size: ${fontSize}px;table-layout: fixed;\">`,
                '<tbody>',
                `<tr colspan=${partInfo.facings}  height=\"20%\">`,
                `<td colspan=${partInfo.facings} class=\"facing-info-title\" style=\"overflow-wrap: break-word; font-weight: bold; color: black;\">` + partName + '</td>',
                '</tr>',
                '<tr height="80%">'
            ].join('');

        const facingTableEndHtml = ['</tr>', '</tbody>', '</table>', '<span></span>', '<br/>', '</div>', '</body>'].join('');

        facingTableHtml = facingTableHtml + facingRowHtml + facingTableEndHtml;
        return facingTableHtml;
    }

    generateShadeSVG(shape: joint.dia.Element) {
        const x = 0;
        const y = 0;
        const partInfo = shape.attributes.partInfo;
        const shadeWidth = Math.floor(shape.attributes.width / partInfo.facings);
        let rawFacingSvg = '';
        for (let i = 0; i < partInfo.facings; i++) {
            rawFacingSvg = rawFacingSvg + `<rect style = \"fill:none;fill-opacity:1;stroke:#000000;stroke-width:1;stroke-miterlimit:2.85714293;stroke-opacity:1\" width=\"${shadeWidth}\" height=\"${shape.attributes.height}\" x=\"${x}\" y=\"${y}\" />`;
            rawFacingSvg =
                rawFacingSvg +
                `<text style=\"font-style:normal;font-weight:normal;font-size:12px;line-height:1.25;font-family:sans-serif;letter-spacing:0px;word-spacing:0px;fill:#000000;fill-opacity:1;stroke:none\" x=\"${x}\" y=\"${y + (shape.attributes.height - 50)}\">${partInfo.name}</text>`;
            rawFacingSvg =
                rawFacingSvg +
                `<text style=\"font-style:normal;font-weight:normal;font-size:12px;line-height:1.25;font-family:sans-serif;letter-spacing:0px;word-spacing:0px;fill:#000000;fill-opacity:1;stroke:none\" x=\"${x}\" y=\"10\" transform=\"rotate(-90)">Shade Name</text>`;

            //d3.textWrap()
        }

        //var svg = `<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"${shape.attributes.width}\" height=\"${shape.attributes.height}\"/>` + rawFacingSvg + '</svg>';
        const svg = `<svg xmlns=\"http://www.w3.org/2000/svg\" height=\"${shape.attributes.height}\" width=\"${shape.attributes.width}\">` + rawFacingSvg + '</svg>';
        return svg;
    }

    renderSvgGraphic(svg: string, part: joint.dia.Element) {
        const parser = new DOMParser();
        const partGraphic = parser.parseFromString(svg, 'image/svg+xml');
        partGraphic.documentElement.classList.add('cSvg');
        const viewbox = partGraphic.documentElement.getAttribute('viewBox');
        // const markup = part.attributes.markup?.toString();
        // const elementSvg = parser.parseFromString(markup ?? '', 'image/svg+xml');

        // const partGraphicSvgs = elementSvg.querySelectorAll('svg');
        // for (let i = 0; i < partGraphicSvgs.length; i++) {
        //     if (partGraphicSvgs[i]?.classList.contains('cSvg')) {
        //         if (partGraphicSvgs[i] != null) {
        //             elementSvg.documentElement.children[0]?.removeChild(partGraphicSvgs[i] as ChildNode);
        //             partGraphicSvgs[i]?.setAttribute('pointer-events', 'visible');
        //         }
        //     }
        // }
        // if (part.attributes.attrs != null && part.attributes.attrs['cSvg'] != null) {
        //     part.attributes.attrs['cSvg'].setAttribute('pointer-events', 'visible');
        // }
        partGraphic.documentElement.setAttribute('pointer-events', 'visible');

        if (part.attributes.attrs != null && part.attributes.attrs['icon'] != null) {
            part.attributes.attrs['icon']['pointer-events'] = 'none';
        }
        if (part.attributes.attrs != null && part.attributes.attrs['cassette'] != null) {
            part.attributes.attrs['cassette']['pointer-events'] = 'none';
        }
        if (part.attributes.attrs != null && part.attributes.attrs['fObj'] != null) {
            part.attributes.attrs['fObj']['pointer-events'] = 'none';
        }

        if (part.attributes.attrs != null && part.attributes.attrs['cassette-container'] != null) {
            //part.attributes.attrs['cassette-container']['xlink:href'] = 'data:image/svg+xml;utf8,' + encodeURIComponent(partGraphic.documentElement.outerHTML);
            // part.attributes.attrs['cassette-container']['viewBox'] = viewbox;
            // part.attributes.attrs['cassette-container']['width'] = part.attributes.width;
            // part.attributes.attrs['cassette-container']['height'] = part.attributes.height;
            partGraphic.documentElement.setAttribute('class', 'cSvg');
            partGraphic.documentElement.setAttribute('viewBox', viewbox ?? '');
            partGraphic.documentElement.setAttribute('width', part.attributes.width.toString());
            partGraphic.documentElement.setAttribute('height', part.attributes.height.toString());
            part.attr('cassette-container/shape', partGraphic.documentElement, { ignoreCommandManager: true });
            // part.attributes.attrs['cassette-container'] = partGraphic.documentElement;
            // part.attributes.attrs['cassette-container']['xlink:href'] = 'data:image/svg+xml;utf8,' + encodeURIComponent(partGraphic.documentElement.outerHTML);
            // part.markup.push(partGraphic.documentElement.outerHTML);
        }
    }

    hideSvgGraphic(part: joint.dia.Element) {
        const parser = new DOMParser();
        const graphicSvg = part.attributes.partInfo.svgLineGraphic.substring(part.attributes.partInfo.svgLineGraphic.indexOf('<svg'));
        const partGraphic = parser.parseFromString(graphicSvg, 'image/svg+xml');

        if (partGraphic.querySelector('#STATUS') != null) {
            partGraphic.querySelector('#STATUS')?.setAttribute('fill-opacity', '0');
            partGraphic.querySelector('#STATUS')?.children[0]?.setAttribute('fill-opacity', '0');
            partGraphic.querySelectorAll('image').forEach(function (image) {
                image.setAttribute('opacity', '1');
            });
        }
        if (part.attributes.attrs != null && part.attributes.attrs['cassette'] != null) {
            part.attributes.attrs['cassette']['xlink:href'] = 'data:image/svg+xml;utf8,' + encodeURIComponent(partGraphic.documentElement.outerHTML);
        }
    }
    showSvgGraphic(part: joint.dia.Element) {
        // const parser = new DOMParser();
        // const markup = part.attributes.markup?.toString();
        // const elementSvg = parser.parseFromString(markup ?? '', 'image/svg+xml');
        // if (elementSvg.querySelector('#STATUS') != null) {
        //     if (elementSvg != null) {
        //         //part.attributes.attrs[".cSvg"].visibility = "visible";
        //         elementSvg.querySelector('#STATUS')?.setAttribute('fill-opacity', '0.4');
        //         elementSvg.querySelector('#STATUS')?.children[0]?.setAttribute('fill-opacity', '0.4');
        //     }
        //     elementSvg.querySelectorAll('image').forEach(function (image) {
        //         image.setAttribute('opacity', '0.5');
        //     });
        // }
        // part.attributes.markup = elementSvg.documentElement.outerHTML;
        const parser = new DOMParser();
        const graphicSvg = part.attributes.partInfo.svgLineGraphic.substring(part.attributes.partInfo.svgLineGraphic.indexOf('<svg'));
        const partGraphic = parser.parseFromString(graphicSvg, 'image/svg+xml');
        if (partGraphic.querySelector('#STATUS') != null) {
            partGraphic.querySelector('#STATUS')?.setAttribute('fill-opacity', '0.4');
            partGraphic.querySelector('#STATUS')?.children[0]?.setAttribute('fill-opacity', '0.4');
            partGraphic.querySelectorAll('image').forEach(function (image) {
                image.setAttribute('opacity', '0.5');
            });
        }
        if (part.attributes.attrs != null && part.attributes.attrs['cassette'] != null) {
            part.attributes.attrs['cassette']['xlink:href'] = 'data:image/svg+xml;utf8,' + encodeURIComponent(partGraphic.documentElement.outerHTML);
        }
    }

    SetStatusColour(cell: joint.dia.Element, statusColour: string, currentView: number) {
        const parser = new DOMParser();
        const partInfo = cell.attributes.partInfo;
        // const graphicSvg = cell.attributes.partInfo.svgLineGraphic.substring(cell.attributes.partInfo.svgLineGraphic.indexOf('<svg'));

        // let svgMarkup: Document | null = null;
        // svgMarkup = parser.parseFromString(graphicSvg, 'image/svg+xml');
        // if (cell.attributes.partInfo.statusId === 0) {
        //     statusColour = '';
        // }

        let hasSVG = false;

        if (cell.attributes.attrs != null && cell.attributes.attrs['cSvg'] != null) {
            if (cell.attributes.attrs['cSvg']['xlink:href'] != null) {
                hasSVG = true;
            }
        }
        if (hasSVG) {
            if (cell.attributes.attrs != null && cell.attributes.attrs['cSvg'] != null) {
                cell.attributes.attrs['cSvg'].visibility = 'visible';
            }
            // if (svgMarkup.querySelector('#STATUS') != null) {
            //     svgMarkup.querySelector('#STATUS')?.setAttribute('fill', statusColour);
            //     svgMarkup.querySelector('#STATUS')?.setAttribute('fill-opacity', '0.4');
            //     if (svgMarkup.querySelector('#BACKGROUND') != null) svgMarkup.querySelector('#BACKGROUND')?.setAttribute('fill', 'none');
            //     svgMarkup.querySelector('#STATUS')?.children[0]?.setAttribute('fill', statusColour);
            //     svgMarkup.querySelector('#STATUS')?.children[0]?.setAttribute('style', 'fill: ' + statusColour + ';');
            //     cell.attributes['markup'] = new XMLSerializer().serializeToString(svgMarkup);
            // } else {
            //     svgMarkup = null;
            // }
            if (cell.attributes.attrs != null && cell.attributes.attrs['#STATUS'] != null) {
                cell.attributes.attrs['#STATUS']['fill'] = statusColour;
            }
            if (cell.attributes.attrs != null && cell.attributes.attrs['body'] != null) {
                cell.attributes.attrs.body['fill-opacity'] = 0.4;
                cell.attributes.attrs.body.fill = statusColour;
                cell.attributes.attrs.body.stroke = 'none';
            }
            if (cell.attributes.attrs != null && cell.attributes.attrs['icon'] != null) {
                cell.attributes.attrs['icon']['fill-opacity'] = 0;
                cell.attributes.attrs['icon']['fill'] = 'none';
                cell.attributes.attrs['icon']['stroke'] = 'none';
            }
        }
        if (!hasSVG) {
            if (cell.attributes.partInfo.partTypeId == PartTypes.Blanking || cell.attributes.partInfo.partTypeId == PartTypes.FasciaPlate) {
                if (cell.attributes.attrs != null && cell.attributes.attrs['body'] != null) {
                    cell.attributes.attrs['body']['fill-opacity'] = 0.3;
                    if (cell.attributes.attrs['body']['fill'] == '#8d8c8c') {
                        cell.attributes.attrs['body']['fill'] = '#fff';
                    }
                    cell.attributes.attrs['body']['fill'] = statusColour;
                }
                if (cell.attributes.attrs != null && cell.attributes.attrs['icon'] != null) {
                    cell.attributes.attrs['icon']['fill-opacity'] = 0.3;
                }
                if (cell.attributes.attrs != null && cell.attributes.attrs['facingObject'] != null) {
                    cell.attributes.attrs['facingObject']['fill-opacity'] = 0.3;
                }
                if (cell.attributes.attrs != null && cell.attributes.attrs['cassette'] != null) {
                    cell.attributes.attrs['cassette']['opacity'] = 0.5;
                }
            } else {
                if (cell.attributes.attrs != null && cell.attributes.attrs['facingObject'] != null) {
                    cell.attributes.attrs['facingObject']['fill-opacity'] = 0.6;
                }
            }

            if (currentView != CurrentView.render) {
                if (partInfo.statusId == 0 || partInfo.statusId == null) {
                    if (cell.attributes.attrs != null && cell.attributes.attrs['body'] != null) {
                        cell.attributes.attrs['body']['stroke'] = '#000';
                    }
                } else {
                    if (cell.attributes.attrs != null && cell.attributes.attrs['body'] != null) {
                        cell.attributes.attrs['body']['stroke'] = statusColour;
                    }
                    if (cell.attributes.attrs != null && cell.attributes.attrs['icon'] != null) {
                        cell.attributes.attrs['icon']['stroke'] = statusColour;
                    }
                }
                if (cell.attributes.attrs != null && cell.attributes.attrs['body'] != null) {
                    cell.attributes.attrs['body']['fill'] = statusColour;
                    if (statusColour != ShadeStatusColourEnum[0]) {
                        cell.attributes.attrs['body']['fill-opacity'] = 0.3;
                    }
                }
            }
        }

        if (cell.attributes.attrs != null && cell.attributes.attrs['facingObject'] != null) {
            cell.attributes.attrs['facingObject']['opacity'] = 0.6;
        }

        // ShadeView
        // if (cassette.attributes.attrs[".body"]["fill"] == "#8d8c8c")
        //   cassette.attributes.attrs[".body"]["fill"] = "#fff";

        // CassetteView
        //cassette.attributes.attrs[".body"]["fill-opacity"] = 0.2;
    }

    SetCommentIndicator(cellMarkup: Document) {
        if (cellMarkup.querySelector('#STATUS') != null) {
            //move the circle to the end of the svg group
            const commentIndicator = cellMarkup.querySelector('.comment-indi');
            if (commentIndicator != null) {
                cellMarkup.documentElement.children[0]?.removeChild(commentIndicator);
                cellMarkup.documentElement.children[0]?.appendChild(commentIndicator);
            }
        }
        return cellMarkup;
    }
}
