import { IBaseModel } from "./IBaseModel";

export interface IARMTemplateModel extends IBaseModel {
    templateName: string,
    templateFilePath?: string,
    templateContent?: string
}