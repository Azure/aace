import { IBaseModel } from "./IBaseModel";

export interface IWebHookParameterModel extends IBaseModel {
    name:string
    value:string
}