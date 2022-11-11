export interface IStation {
    id: number;
    amount: number;
    planID: number;
    glueID: number;
    glueName: string;
    isDelete: number;
    createBy: number;
    deleteBy: number;
    createTime: Date;
    deleteTime: Date;
    modifyTime: Date;
}
