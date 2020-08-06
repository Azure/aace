// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
import * as yup from "yup";
import { ObjectSchema } from "yup";
import { ISubscriptionsPostModel, ISubscriptionsModel, ISubscriptionsWarnings } from "../../../models";

export const getInitialSubscriptionsPostModel = (): ISubscriptionsPostModel => {
  return {
    subscriptionId: '',
    subscriptionName: '',
    name: '',
    offerName: '',
    planName: '',
    availablePlanName: '',
    owner: '',
    quantity: 1,
    beneficiaryTenantId: '',
    purchaserTenantId: '',
    subscribeWebhookName: '',
    unsubscribeWebhookName: '',
    suspendWebhookName: '',
    deleteDataWebhookName: '',
    priceModel: '',
    monthlyBase: 0,
    privatePlan: false,
    inputParameters: [],
    planlist: []
  }
};

export const getInitialSubscriptionsModel = (): ISubscriptionsModel => {
  return {
    subscriptionId: '',
    name: '',
    offerName: '',
    planName: '',
    owner: '',
    quantity: 0,
    beneficiaryTenantId: '',
    purchaserTenantId: '',
    subscribeWebhookName: '',
    unsubscribeWebhookName: '',
    suspendWebhookName: '',
    deleteDataWebhookName: '',
    priceModel: '',
    monthlyBase: 0,
    privatePlan: false,
    inputParameters: [],
    provisioningStatus: '',

    publisherId: '',
    status: '',
    isTest: false,
    allowedCustomerOperationsMask: 0,
    sessionMode: '',
    sandboxType: '',
    isFreeTrial: false,
    createdTime: '',
    activatedTime: '',
    lastUpdatedTime: '',
    lastSuspendedTime: '',
    unsubscribedTime: '',
    dataDeletedTime: '',
    operationId: '',
    deploymentName: '',
    deploymentId: '',
    resourceGroup: '',
    activatedBy: '',
  }
};

export const getInitialSubscriptionsV1List =
{
  hasErrors: false,
  value: [
    {
      subscriptionId: '1',
      name: '1',
      offerName: '1',
      planName: '1',
      owner: '1',
      quantity: 0,
      beneficiaryTenantId: '1',
      purchaserTenantId: '2',
      subscribeWebhookName: '2',
      unsubscribeWebhookName: '2',
      suspendWebhookName: '2',
      deleteDataWebhookName: '2',
      priceModel: '2',
      monthlyBase: 0,
      privatePlan: false,
      inputParameters: [],
      provisioningStatus: '2',

      publisherId: '2',
      status: 'Test',
      isTest: false,
      allowedCustomerOperationsMask: 0,
      sessionMode: '2',
      sandboxType: '2',
      isFreeTrial: false,
      createdTime: '',
      activatedTime: '',
      lastUpdatedTime: '',
      lastSuspendedTime: '',
      unsubscribedTime: '',
      dataDeletedTime: '',
      operationId: '',
      deploymentName: '1',
      deploymentId: '2',
      resourceGroup: '1',
      activatedBy: '1',
    },
    {
      subscriptionId: '2',
      name: '1',
      offerName: '1',
      planName: '1',
      owner: '1',
      quantity: 0,
      beneficiaryTenantId: '1',
      purchaserTenantId: '2',
      subscribeWebhookName: '2',
      unsubscribeWebhookName: '2',
      suspendWebhookName: '2',
      deleteDataWebhookName: '2',
      priceModel: '2',
      monthlyBase: 0,
      privatePlan: false,
      inputParameters: [],
      provisioningStatus: '2',

      publisherId: '2',
      status: 'Test1',
      isTest: false,
      allowedCustomerOperationsMask: 0,
      sessionMode: '2',
      sandboxType: '2',
      isFreeTrial: false,
      createdTime: '',
      activatedTime: '',
      lastUpdatedTime: '',
      lastSuspendedTime: '',
      unsubscribedTime: '',
      dataDeletedTime: '',
      operationId: '',
      deploymentName: '1',
      deploymentId: '2',
      resourceGroup: '1',
      activatedBy: '1',
    }
  ]
};

export const getInitialSubscriptionsV2 =
{
  subscriptionId: '',
  subscriptionName: '',
  userId: '',
  productName: '',
  deploymentName: '',
  status: '',
  baseUrl: '',
  primaryKey: '',
  secondaryKey: ''
};

export const getInitialSubscriptionsV2List =
{
  hasErrors: false,
  value: [
    {
      subscriptionId: '1',
      subscriptionName: '1',
      userId: '1',
      productName: 'EDDI',
      deploymentName: 'westUs',
      status: 'Subscribed',
      baseUrl: 'https://luna.apim.net/eddi/eastus/predict',
      primaryKey: '*************',
      secondaryKey: '*************'
    },
    {
      subscriptionId: '60bc9d4a-09d7-42e1-89aa-c188e0aff9db',
      subscriptionName: 'scottgutest',
      userId: 'scottgu@microsoft.com',
      productName: 'BDDI',
      deploymentName: 'eastus',
      status: 'UnSubscribed',
      baseUrl: 'https://luna.apim.net/bddi/eastus/predict',
      primaryKey: '*************',
      secondaryKey: '*************'
    }
  ]
};

export const getInitialSubscriptionsWarningsModel = (): ISubscriptionsWarnings[] => {
  return (
    [
      {
        subscriptionId: '',
        warningMessage: '',
        details: ''
      }
    ]
  )
}
export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );

export const subscriptionValidator: ObjectSchema<ISubscriptionsPostModel> = yup.object().shape(
  {
    clientId: yup.string(),
    subscriptionId: yup.string(),
    subscriptionName: yup.string().required("Subscription Name is required"),
    availablePlanName: yup.string().required('Please select plan'),
    name: yup.string(),
    offerName: yup.string(),
    planName: yup.string(),
    owner: yup.string(),
    quantity: yup.number(),
    beneficiaryTenantId: yup.string(),
    purchaserTenantId: yup.string(),
    subscribeWebhookName: yup.string(),
    unsubscribeWebhookName: yup.string(),
    suspendWebhookName: yup.string(),
    deleteDataWebhookName: yup.string(),
    priceModel: yup.string(),
    monthlyBase: yup.number(),
    privatePlan: yup.boolean(),
    inputParameters: yup.array(),
    planlist: yup.array()
  }
);

export const UpdateplanValidator: ObjectSchema<ISubscriptionsPostModel> = yup.object().shape(
  {
    clientId: yup.string(),
    subscriptionId: yup.string(),
    subscriptionName: yup.string(),
    availablePlanName: yup.string().required('Available plan is required')
      .when('planName', {
        is: (val) => {
          return val !== ""
        },
        then: yup.string().test('existingPlan', 'That plan is already set', function (value: string) {
          const currentPlanName: string = this.parent.planName;
          if (!value)
            return true;

          return !value.toLowerCase().includes(currentPlanName.toLowerCase());
        }).required('Available plan is required'),
        otherwise: yup.mixed().notRequired()
      }),
    name: yup.string(),
    offerName: yup.string(),
    planName: yup.string(),
    owner: yup.string(),
    quantity: yup.number(),
    beneficiaryTenantId: yup.string(),
    purchaserTenantId: yup.string(),
    subscribeWebhookName: yup.string(),
    unsubscribeWebhookName: yup.string(),
    suspendWebhookName: yup.string(),
    deleteDataWebhookName: yup.string(),
    priceModel: yup.string(),
    monthlyBase: yup.number(),
    privatePlan: yup.boolean(),
    inputParameters: yup.array(),
    planlist: yup.array()
  }
);