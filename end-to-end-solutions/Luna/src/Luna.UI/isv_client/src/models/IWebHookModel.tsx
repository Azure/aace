import { IBaseModel } from "./IBaseModel";

export interface IWebHookModel extends IBaseModel {
    webhookName:string
    webhookUrl:string
}