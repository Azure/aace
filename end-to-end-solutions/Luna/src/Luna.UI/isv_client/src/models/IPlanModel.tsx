import {IBaseModel} from "./IBaseModel";

export interface IPlanModel extends IBaseModel {
  planName: string,
  dataRetentionInDays: number,
  subscribeArmTemplateName: string | null,
  unsubscribeArmTemplateName: string | null,
  suspendArmTemplateName: string | null,
  deleteDataArmTemplateName: string | null,
  subscribeWebhookName: string | null,
  unsubscribeWebhookName: string | null,
  suspendWebhookName: string | null,
  deleteDataWebhookName: string | null,
  priceModel: string,
  monthlyBase: number,
  annualBase: number,
  privatePlan: boolean
  restrictedUsers: IRestrictedUsersModel[];
  customMeterDimensions: ICustomMeterDimensionsModel[];
  planIdList?: string,
  planNameList?: string  
}

export interface IRestrictedUsersModel extends IBaseModel {
  tenantId: string,
  description: string
}

export interface ICustomMeterDimensionsModel extends IBaseModel {
  meterName: string,
  planName: string,
  monthlyUnlimited: boolean,
  annualUnlimited: boolean,
  monthlyQuantityIncludedInBase: number,
  annualQuantityIncludedInBase: number
}