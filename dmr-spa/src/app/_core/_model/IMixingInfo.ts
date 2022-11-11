export interface IMixingInfo {
    id: number;
    glueID: number;
    glueName: string;
    buildingID: number;
    code: string;
    mixBy: number;
    expiredTime: Date;
    createdTime: Date;
    status: boolean;
    estimatedTime: Date;
    startTime: Date;
    endTime: Date;
    printTime: Date;
    estimatedStartTime: Date;
    estimatedFinishTime: Date;
    mixingInfoDetails: IMixingInfoDetails[];
}
export interface IMixingInfoDetails {
    id: number;
    batch: string;
    position: string;
    code: string;
    amount: number;
    ingredientID: number;
    mixingInfoID: number;
}

export interface IMixingDetailForResponse {
    mixedConsumption: string;
    deliveryConsumption: string;
    mixedTime: number;
    deliveryTime: number;
}
