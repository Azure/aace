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
    availablePlanName:'',
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
    planlist:[]
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
    availablePlanName:yup.string().required('Please select plan'),
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
    planlist:yup.array()
  }
);

export const UpdateplanValidator: ObjectSchema<ISubscriptionsPostModel> = yup.object().shape(
  {
    clientId: yup.string(),
    subscriptionId: yup.string(),
    subscriptionName: yup.string(),    
    availablePlanName:yup.string().required('Available plan is required')
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
    planlist:yup.array()
  }
);