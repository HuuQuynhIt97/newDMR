export class HierarchyNode<T> {
    childNodes: Array<HierarchyNode<T>>;
    depth: number;
    hasChildren: boolean;
    parent: T;
    constructor() {
        this.childNodes = new Array<HierarchyNode<T>>();
    }
    any(): boolean {
        return this.childNodes.length > 0;
    }
}
export interface IBuilding {
    id: number;
    level: number;
    name: string;
    parentID?: number | null;
    plans: any;
    lunchTime: string;
    kindName: string;
    kindID?: number | null;
    lunchTimeID?: number | null;
    buildingTypeID?: number | null;
    buildingType?: any | null;
    settings: any;
}
export interface ILunchTime {
    startTime: Date;
    endTime: Date;
    content: string;
}
// export class LunchTime {
//     data: ILunchTime[];
//     loadData() {
//         return this.data = [
//             {
//                 startTime: null,
//                 endTime: null,
//                 content: 'N/A'
//             },
//             {
//                 startTime: new Date(2021, 1, 11, 11, 0, 0),
//                 endTime: new Date(2021, 1, 11, 12, 0, 0),
//                 content: '11:00 - 12:00'
//             },
//             {
//                 startTime: new Date(2021, 1, 11, 11, 30, 0),
//                 endTime: new Date(2021, 1, 11, 12, 30, 0),
//                 content: '11:30 - 12:30'
//             },
//             {
//                 startTime: new Date(2021, 1, 11, 12, 0, 0),
//                 endTime: new Date(2021, 1, 11, 13, 0, 0),
//                 content: '12:00 - 13:00'
//             },
//             {
//                 startTime: new Date(2021, 1, 11, 12, 30, 0),
//                 endTime: new Date(2021, 1, 11, 13, 30, 0),
//                 content: '12:30 - 13:30'
//             }
//         ];
//     }
//     constructor() {
//         this.loadData();
//     }
// }
