export interface IMailing {
    id: number ;
    email: string;
    userID: number;
    userName: string;
    frequency: string;
    userNames: string[];
    userIDList: number[];
    timeSend: Date;
}
