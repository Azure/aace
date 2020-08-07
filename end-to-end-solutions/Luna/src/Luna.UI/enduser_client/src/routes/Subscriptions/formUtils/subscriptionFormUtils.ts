// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as yup from "yup";
import {ObjectSchema} from "yup";
import {
  ICreateSubscriptionModel,
  IOperationHistoryModel, ISubscriptionsV2Model, IUpdateSubscriptionModel
} from "../../../models";

export const getInitialCreateSubscriptionModel = (): ICreateSubscriptionModel => {
  return {
    SubscriptionId: "",
    Name: "",
    OfferName: "",
    PlanName: "",
    Owner: "",
    Quantity: 1,
    BeneficiaryTenantId: "",
    PurchaserTenantId: "",
    InputParameters: []
  }
};

export const getInitialUpdateSubscriptionModel = (): IUpdateSubscriptionModel => {
  return {
    SubscriptionId: "",
    SubscriptionName: "",
    SubscriptionVerifiedName: "",
    OfferName: "",
    PlanName: "",
    CurrentPlanName: "",
    isUpdatePlan: true
  }
};

export const getInitialSubscriptionV2 = (): ISubscriptionsV2Model => {
  return {
    subscriptionId: '',
    subscriptionName: '',
    userId: '',
    productName: '',
    deploymentName: '',
    status: '',
    baseUrl: '',
    primaryKey: '',
    secondaryKey: ''
  }
};


export interface OperationHistoryModel {
  data: IOperationHistoryModel[]
}
export const getInitialOperationHistoryModel: OperationHistoryModel =
{
  data:
    []
}


export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );

export const subscriptionValidator: ObjectSchema<IUpdateSubscriptionModel> = yup.object().shape(
  {
    clientId: yup.string(),
    CurrentPlanName: yup.string(),
    SubscriptionName: yup.string(),
    isUpdatePlan: yup.boolean(),
    SubscriptionId: yup.string(),
    SubscriptionVerifiedName: yup.mixed().when('isUpdatePlan', {
      is: (val) => {
        return val === false
      }, then: yup.string().test('subscriptionName', 'Subscription name does not match', function (value: string) {
        const subscriptionName: string = this.parent.SubscriptionName;
        if (!value)
          return true;

        return value.toLowerCase() === subscriptionName.toLowerCase();
      }).required("Subscription Name is required"),
      otherwise: yup.mixed().notRequired()
    }),
    OfferName: yup.string(),
    PlanName: yup.mixed().when('isUpdatePlan', {
      is: (val) => {
        return val === true;
      }, then: yup.string().test('planTest', 'That plan is already set', function (value) : boolean  {

        const planName: string = this.parent.CurrentPlanName;
        console.log('val: ' + value);

        if (!value || value === "")
          return true;

        return value.toLowerCase() !== planName.toLowerCase();
      }).required("Plan is required"),
      otherwise: yup.mixed().notRequired()
    })
  }
);
/*
export const subscriptionValidator: ObjectSchema<ISubscriptionsPostModel> = yup.object().shape(
  {
    clientId: yup.string(),
    subscriptionId: yup.string(),
    subscriptionName: yup.string()
      .test('subscriptionName', 'Subscription dose not match', function (value: string) {
        const subscriptionName: string = this.parent.name;
        if (!value)
          return true;

        return value.toLowerCase() === subscriptionName.toLowerCase();
      }).required("Subscription Name is required"),
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
    availablePlanName: yup.mixed().when('isUpdatePlan',{is:(val)=>
      {
        return val === true
      },then:yup.string().required('Available plan is required')}),
    isUpdatePlan: yup.boolean(),
    planlist: yup.array()
  }
);*/