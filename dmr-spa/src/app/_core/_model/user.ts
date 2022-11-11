export interface User {
  ID: number;
  Username: string;
  Alias: string;
  image: string;
  IsLeader: boolean;
  OcLevel: number;
  Role: number;
  ListOcs: [];
}
export interface UserDRM {
  iD: number;
  username: string;
  password: string;
  employeeID: string;
  email: string;
  status: boolean;
  systemID: number;
  userRoleID: number;
  buildingUserID: number;
  role: string;
  building: string;
}
export interface UserForLogin {
  username: string;
  password: string;
  systemCode: number;

}
export interface UserGetAll {
   ID: number;
   Username: string;
   OCID: number ;
   LevelOC: number ;
   Email: string ;
   RoleID: number ;
   ImageURL: string ;
  ImageBase64: string   ;
  isLeader: boolean ;
   Role: any ;
}

export interface IUserCreate {
  id: number;
  username: string;
  password: string;
  email: string;
  roleid: number;
  employeeID: string;
  systemCode: number;
  isLeader: boolean;
}
export interface IUserUpdate {
  id: number;
  username: string;
  password: string;
  email: string;
  roleid: number;
  employeeID: string;
  isLeader: boolean;

}
