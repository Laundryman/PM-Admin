import * as joint from '@joint/plus';

export class StencilShapes {
    //   key: string]: object;
    [groupName: string]: Array<joint.dia.Cell | joint.mvc.ObjectHash>;
}
export class StencilGroups {
    [key: string]: joint.ui.Stencil.Group;
}
