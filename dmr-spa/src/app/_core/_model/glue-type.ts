export class HierarchyNode<T> {
    childNodes: Array<HierarchyNode<T>>;
    depth: number;
    hasChildren: boolean;
    parent: T;
    entity: T;
    constructor() {
        this.childNodes = new Array<HierarchyNode<T>>();
    }
    any(): boolean {
        return this.childNodes.length > 0;
    }
}
export interface IGlueType {
    id: number;
    level: number;
    title: string;
    method: string;
    parentID: number;
    rpm: number;
    minutes: number;
}
