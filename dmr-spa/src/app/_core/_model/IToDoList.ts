import { IIngredient } from './summary';

export interface IToDoList {
    id: number;
    planID: number;
    mixingInfoID: number;
    glueID: number;
    jobType: number;
    buildingID: number;
    lineID: number;
    lineName: string;
    lineNames: string[];
    glueName: string;
    supplier: string;
    status: boolean;
    startMixingTime: Date;
    finishMixingTime: Date;
    startStirTime: Date;
    finishStirTime: Date;
    startDispatchingTime: Date;
    finishDispatchingTime: Date;
    printTime: Date;
    dispatchTime: Date;
    standardConsumption: number;
    mixedConsumption: number;
    deliveredConsumption: number;
    estimatedStartTime: Date;
    estimatedFinishTime: Date;
    abnormalStatus: boolean;
    glueNameID: number;
}
export interface IToDoListForCancel {
    id: number;
    lineNames: string[];
}
export interface IScanner {
    QRCode: string;
    ingredient: IIngredient;
}
export interface IToDoListForReturn {
    total: number;
    doneTotal: number;
    todoTotal: number;
    delayTotal: number;
    percentageOfDone: number;

    dispatchTotal: number;
    todoDispatchTotal: number;
    doneDispatchTotal: number;
    delayDispatchTotal: number;
    percentageOfDoneDispatch: number;
    data: IToDoList[];
}

export interface IDispatchListForUpdate {
    id: number;
    glueNameID: number;
    amount: number;
    estimatedStartTime: any;
    estimatedFinishTime: any;
}
export interface IDispatchListDetail {
    createdTime: any;
    id: number;
    planID: number;
    mixingInfoID: number;
    glueID: number;
    buildingID: number;
    lineID: number;
    lineName: string;
    lineNames: string[];
    glueName: string;
    supplier: string;
    status: boolean;
    startDispatchingTime: Date;
    finishDispatchingTime: Date;
    printTime: Date;
    deliveredAmount: number;
    estimatedStartTime: Date;
    estimatedFinishTime: Date;
    abnormalStatus: boolean;
    glueNameID: number;
}
