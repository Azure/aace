import {IBaseModel} from "./IBaseModel";

export interface IOfferParameterModel extends IBaseModel {
  parameterName: string;
  displayName: string;
  description: string;
  valueType: string;
  fromList: boolean;
  valueList: string;
  maximum: number | null;
  minimum: number | null;
}