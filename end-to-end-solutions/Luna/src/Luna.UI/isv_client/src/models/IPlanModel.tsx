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
  planIdList?: string,
  planNameList?: string  
}

export interface IRestrictedUsersModel extends IBaseModel {
  tenantId: string,
  description: string
}