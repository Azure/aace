import { IBaseModel } from "./IBaseModel";
import { IDropdownOption } from "office-ui-fabric-react";

export interface IParamModel {
  name: string,
  type: string,
  value: any
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
  inputParameters: IParamModel[],
  provisioningStatus: string,
  entryPointUrl: string,

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

export interface ICreateSubscriptionModel {
  SubscriptionId: string
  Name: string
  OfferName: string
  PlanName: string
  Owner: string
  Quantity: number,
  BeneficiaryTenantId: string,
  PurchaserTenantId: string
  InputParameters: IParamModel[];
}

export interface ISubscriptionFormModel {
  subscription: IUpdateSubscriptionModel
}

export interface IUpdateSubscriptionModel {
  SubscriptionId: string,
  SubscriptionName: string,
  SubscriptionVerifiedName: string,
  OfferName: string,
  CurrentPlanName: string,
  PlanName: string,
  isUpdatePlan: boolean
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
  success: boolean  
}