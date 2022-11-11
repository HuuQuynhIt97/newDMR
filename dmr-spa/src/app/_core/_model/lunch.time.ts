export interface LunchTime {
    iD: number;
    startTime: Date;
    endTime: Date;
    createdBy: number;
    deletedBy: number;
    updatedBy: number;
    isDelete: number;
    createdTime: string;
    deletedTime: string | null;
    updatedTime: string | null;
}
