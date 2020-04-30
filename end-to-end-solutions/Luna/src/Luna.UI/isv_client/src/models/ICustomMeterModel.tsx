import { IBaseModel } from "./IBaseModel";

export interface ICustomMeterModel extends IBaseModel {
    offerName:string
    meterName:string
    telemetryDataConnectorName:string
    telemetryQuery:string
}