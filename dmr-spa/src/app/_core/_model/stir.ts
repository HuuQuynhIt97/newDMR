import { IGlueType } from './glue-type';

export interface IStir {
    id: number;
    glueName: string;
    glueType: IGlueType;
    settingID: number;
    actualDuration: number;
    standardDuration: number;
    status: boolean;
    mixingInfoID: number;
    startTime: Date;
    endTime: Date;
    createdTime: Date;
    startScanTime: Date;
    startStiringTime: Date;
    finishStiringTime: Date;
}


export interface IStirForAdd {
    id: number;
    glueName: string;
    settingID: number;
    buildingID: number;
    mixingInfoID: number;
    startScanTime: any;
    startStiringTime: any;
}

export interface IStirForUpdate {
    id: number;
    glueName: string;
    settingID: number;
    mixingInfoID: number;
    glueType: IGlueType;
    startScanTime: any;
    buildingID: number;
    finishStiringTime: any;
}
