export interface IScanParams {
    partNO: string;
    glueID: number;
    glueName: string;
    glueNameID: number;
    mixingInfoID: number;
    buildingID: number;
    estimatedStartTime: any;
    estimatedFinishTime: any;
    batchNO: string;
}

export interface IGenerateSubpackageParams {
    amountOfChemical: number;
    buildingID: number;
    mixingInfoID: number;
    glueName: string;
    glueNameID: number;
    can: number;
}
