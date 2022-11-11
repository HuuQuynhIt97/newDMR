export interface Plan {
    id: number;
    buildingID: number;
    hourlyOutput: number;
    workingHour: number;
    BPFCEstablishID: number;
    BPFCName: string;
    dueDate: any;
    startWorkingTime: any;
    finishWorkingTime: any;
    startTime: ITime;
    endTime: ITime;
}
export interface ITime {
    hour: number;
    minute: number;
}
export interface BPFC {
    id: number;
    artProcess: string;
    glues: string[];
    kinds: number[];
    name: string;
}
export interface Consumtion {
    id: number;
    modelName: string;
    modelNo: string;
    articleNo: string;
    process: string;
    glue: string;
    std: number;
    qty: number;
    line: string;
    totalConsumption: number;
    realConsumption: number;
    diff: number;
    percentage: number;
    dueDate: Date;
    mixingDate: Date;
}
export interface IDispatch {
    id: number;
    lineID: number;
    line: string;
    standardAmount: number;
    mixedConsumption: number;
    mixingInfoID: number;
    glue: string;
    real: number;
    stationID: number;
    warningStatus: boolean;
    scanStatus: boolean;
    isLock: boolean;
    isNew: boolean;
    deliveryTime: Date;
    createdTime: Date;
}
export interface IDispatchForCreate {
    id: number;
    lineID: number;
    amount: number;
    mixingInfoID: number;
    createdTime: Date;
    stationID: number;
    estimatedTime: Date;
    standardAmount: number;
    startDispatchingTime: any;
    finishDispatchingTime: any;
}
export interface DispatchParams {
    id: number;
    lines: string[];
    glue: string;
    estimatedTime: Date;
    mixingInfoID: number;
    estimatedStartTime: Date;
    estimatedFinishTime: Date;
}
export interface IMixingInfo {
    id: number;
    glueID: number;
    glueName: string;
    chemicalA: string;
    chemicalB: string;
    chemicalC: string;
    chemicalD: string;
    chemicalE: string;
    batchA: string;
    batchB: string;
    batchC: string;
    batchD: string;
    batchE: string;
    mixBy: number;
    buildingID: number;
    estimatedTime: any;
    estimatedStartTime: any;
    estimatedFinishTime: any;
    startTime: any;
    endTime: any;
    createdTime: any;
}


export interface IPlan {
    id: number;
    bpfcName: string;
    dueDate: Date;
    createdDate: Date;
    workingHour: number;
    hourlyOutput: number;
    buildingName: string;
    buildingID: number;
    isOvertime: boolean;
    startWorkingTime: Date;
    isShowOvertimeOption: boolean;
    finishWorkingTime: Date;
    bpfcEstablishID: number;
    glues: string[];
}
