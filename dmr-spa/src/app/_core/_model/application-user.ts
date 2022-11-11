export interface ApplicationUser {
  username: string;
  id: number;
  role: string;
  originalUserName: string;
}
export interface FunctionSystem {
  name: string;
  url: string;
  functionCode: string;
  childrens: Action[];
}
export interface Action {
  id: number;
  url: string;
  code: string;
}
