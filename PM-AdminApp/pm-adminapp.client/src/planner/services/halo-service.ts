/*! JointJS+ v4.2.2 (2026-01-22) - HTML5 Diagramming Framework

Copyright (c) 2025 client IO

This Source Code Form is subject to the terms of the JointJS+
License, v. 2.0. If a copy of the JointJS+ License was not
distributed with this file, You can obtain one at
https://www.jointjs.com/license or from the JointJS+ archive as was
distributed by client IO. See the LICENSE file.
*/

import { ui } from '@joint/plus'
const Position = ui.Halo.HandlePosition

export class HaloService {
  create(cellView: joint.dia.CellView) {
    const halo = new ui.Halo({
      cellView,
      boxContent: null,
      handles: this.getHaloConfig(),
      type: 'overlay',
      useModelGeometry: true,
    })
    halo.removeHandle('remove')
    halo.addHandle({
      ...ui.Halo.getDefaultHandle('remove'),
      position: Position.NW,
      events: {
        pointerdown: function (this: any, evt: any) {
          evt.stopPropagation()
          var self = this
          var msgContent = '<b>Are you sure you want to remove this item?</b>'
          if (self.options.cellView.model.attributes.shapeType == 'Shelf') {
            msgContent =
              "<b>Are you sure you want to remove this shelf? Clicking remove will remove the shelf and it's contents.</b>"
          }
          var dialog = new ui.Dialog({
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
        },
      },
    })
    halo.render()
    // halo.on('action:remove:pointerdown', () => {
    //   if (confirm('Are you sure you want to delete this element?')) {
    //     cellView.model.remove()
    //   }
    // })
    return halo
  }
  createNoRem(cellView: joint.dia.CellView) {
    const halo = new ui.Halo({
      cellView,
      boxContent: null,
      handles: this.getHaloNoRemConfig(),
      type: 'overlay',
      useModelGeometry: true,
    })
    halo.render()
    // halo.on('action:deleteElement:pointerdown', () => {
    //     if (confirm('Are you sure you want to delete this element?')) {
    //         cellView.model.remove();
    //     }
    // });
    return halo
  }

  getHaloConfig() {
    return [
      {
        ...ui.Halo.getDefaultHandle('remove'),
        position: Position.NW,
        // events: { pointerdown: 'deleteElement' },
        attrs: {
          '.handle': {
            'data-tooltip-class-name': 'small',
            'data-tooltip': 'Click to remove the object',
            'data-tooltip-position': 'right',
            'data-tooltip-padding': 15,
          },
        },
      },
      // {
      //     ...ui.Halo.getDefaultHandle('unlink'),
      //     position: Position.W,
      //     attrs: {
      //         '.handle': {
      //             'data-tooltip-class-name': 'small',
      //             'data-tooltip': 'Click to break all connections to other objects',
      //             'data-tooltip-position': 'right',
      //             'data-tooltip-padding': 15
      //         }
      //     }
      // },
      // {
      //     ...ui.Halo.getDefaultHandle('rotate'),
      //     position: Position.SW,
      //     attrs: {
      //         '.handle': {
      //             'data-tooltip-class-name': 'small',
      //             'data-tooltip': 'Click and drag to rotate the object',
      //             'data-tooltip-position': 'right',
      //             'data-tooltip-padding': 15
      //         }
      //     }
      // },
      // {
      //     ...ui.Halo.getDefaultHandle('fork'),
      //     position: Position.NE,
      //     attrs: {
      //         '.handle': {
      //             'data-tooltip-class-name': 'small',
      //             'data-tooltip': 'Click and drag to clone and connect the object in one go',
      //             'data-tooltip-position': 'left',
      //             'data-tooltip-padding': 15
      //         }
      //     }
      // },
      // {
      //     ...ui.Halo.getDefaultHandle('link'),
      //     position: Position.E,
      //     attrs: {
      //         '.handle': {
      //             'data-tooltip-class-name': 'small',
      //             'data-tooltip': 'Click and drag to connect the object',
      //             'data-tooltip-position': 'left',
      //             'data-tooltip-padding': 15
      //         }
      //     }
      // },
      {
        ...ui.Halo.getDefaultHandle('clone'),
        position: Position.SE,
        events: { pointerdown: 'startCloning', pointermove: 'doClone', pointerup: 'stopCloning' },
        attrs: {
          '.handle': {
            'data-tooltip-class-name': 'small',
            'data-tooltip': 'Click and drag to clone the object',
            'data-tooltip-position': 'left',
            'data-tooltip-padding': 15,
          },
        },
      },
    ]
  }
  getHaloNoRemConfig() {
    return [
      {
        ...ui.Halo.getDefaultHandle('clone'),
        position: Position.SE,
        events: { pointerdown: 'startCloning', pointermove: 'doClone', pointerup: 'stopCloning' },
        attrs: {
          '.handle': {
            'data-tooltip-class-name': 'small',
            'data-tooltip': 'Click and drag to clone the object',
            'data-tooltip-position': 'left',
            'data-tooltip-padding': 15,
          },
        },
      },
    ]
  }
}
