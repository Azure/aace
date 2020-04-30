import { IDropdownOption } from "office-ui-fabric-react";

export interface IParameterModel {
  name: string,
  type: string,
  value: string
}

export interface ISubscriptionsModel {

  subscriptionId: string,
  name: string,
  offerName: string,
  planName: string,
  owner: string,
  quantity: number,
  beneficiaryTenantId: string,
  purchaserTenantId: string,
  subscribeWebhookName: string,
  unsubscribeWebhookName: string,
  suspendWebhookName: string,
  deleteDataWebhookName: string,
  priceModel: string,
  monthlyBase: number,
  privatePlan: boolean,
  inputParameters: IParameterModel[],
  provisioningStatus: string,

  publisherId: string,
  status: string,
  isTest: boolean,
  allowedCustomerOperationsMask: number,
  sessionMode: string,
  sandboxType: string,
  isFreeTrial: boolean,
  createdTime: string,
  activatedTime: string,
  lastUpdatedTime: string,
  lastSuspendedTime: string,
  unsubscribedTime: string,
  dataDeletedTime: string,
  operationId: string,
  deploymentName: string,
  deploymentId: string,
  resourceGroup: string,
  activatedBy: string,
}

export interface ISubscriptionsPostModel {

  subscriptionId: string,
  subscriptionName: string,
  name: string,
  offerName: string,
  planName: string,
  availablePlanName:string,
  owner: string,
  quantity: number,
  beneficiaryTenantId: string,
  purchaserTenantId: string,
  subscribeWebhookName: string,
  unsubscribeWebhookName: string,
  suspendWebhookName: string,
  deleteDataWebhookName: string,
  priceModel: string,
  monthlyBase: number,
  privatePlan: boolean,
  inputParameters: IParameterModel[],
  planlist: IDropdownOption[]
}

export interface IOperationHistoryModel {
  timeStamp: string,
  status: number,
  action: string
  activityId: string,
  id: string,
  offerId: string,
  operationRequestSource: string,
  planId: string,
  publisherId: string,
  quantity?: number,
  resourceLocation: string,
  subscriptionId: string,  
  requestId: string,
  statusCode: number,
  success: boolean,
  isunsubscribe:boolean
}

export interface ISubscriptionsWarnings {
  subscriptionId: string,
  warningMessage: string,
  details: string
}

export interface ISubscriptionsV2Model {
  subscriptionId: string,
  name: string,
  userId: string,
  productId: string,
  deploymentId: string,
  status: string,
  baseUrl: string,
  primaryKey: string,
  secondaryKey: string

}