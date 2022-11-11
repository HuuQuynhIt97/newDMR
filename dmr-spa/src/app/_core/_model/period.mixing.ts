import { PeriodDispatch } from "./period.dispatch";

export interface PeriodMixing {
    id: number;
    buildingID: number;
    isOvertime: boolean;
    startTime: any;
    endTime: any;
    createdTime: Date;
    updatedTime: Date | null;
    deletedTime: Date | null;
    isDelete: number;
    createdBy: number;
    deletedBy: number;
    updatedBy: number;
    periodDispatchList: PeriodDispatch[];
}
