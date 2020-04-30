import { IBaseModel } from "./IBaseModel";

export interface ITelemetryDataConnectorModel extends IBaseModel {
    name:string
    type:string
    configuration:string
}