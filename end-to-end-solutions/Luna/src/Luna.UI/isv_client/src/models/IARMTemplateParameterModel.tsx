import { IBaseModel } from "./IBaseModel";

export interface IARMTemplateParameterModel extends IBaseModel {
    name: string;
    type: string
    value: string
}