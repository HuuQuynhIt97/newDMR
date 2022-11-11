export interface IRole {
    id: number;
    name: string;
}
export interface IUserRole {
    roleID: number;
    userID: any;
    isLock: boolean;
}
