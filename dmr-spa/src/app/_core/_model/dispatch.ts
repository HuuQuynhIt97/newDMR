export interface IAddDispatchParams {
    mixingInfoID: number;
    glueNameID: number;
    buildingID: number;
    lineName: string;
    option: string;
    estimatedStartTime: any;
    estimatedFinishTime: any;
}

export interface IDispatchParams {
    mixingInfoID: number;
    glueNameID: number;
    buildingID: number;
}
export interface IUpdateDispatchParams {
    iD: number;
    remaningAmount: number;
}
