import {ILandingModel} from "../../../models/IEnduserLandingModel";


export const getInitialLandingModel = (): ILandingModel => {
  return {
    email: '',
    fullName: '',
    subscriptionId: '',
    subscriptionName: '',
    name: '',
    offerName: '',
    planName: '',
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
    parameterValues: [],
    availablePlanName: '',
    isUpdatePlan: false,
    planlist: []
  }
};

export interface ILandingModelValues {
  values: ILandingModel;
}

// const landingValidator: ObjectSchema<ILandingModel> = yup.object().shape(
//   {
//     email: yup.string(),
//     fullName: yup.string(),
//     currentPlan: yup.string(),

//     offerId: yup.string(),
//     planId:yup.string(),
//     beneficiaryTenantId: yup.string(),
//     publisherId: yup.object(),
//     purchaserTenantId: yup.string(),
//     quantity: yup.number(),
//     state: yup.number(),    
//     subscriptionId: yup.string(),
//     subscriptionName: yup.string(),    
//     parameters:yup.array()
//   }
// );

// export const landingInfoValidationSchema: ObjectSchema<ILandingModelValues> =
//   yup.object().shape({
//     values: landingValidator
//   });
