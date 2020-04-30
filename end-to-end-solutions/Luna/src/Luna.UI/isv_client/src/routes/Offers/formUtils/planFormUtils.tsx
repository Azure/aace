import * as yup from "yup";
import {ObjectSchema} from "yup";
import {ICustomMeterDimensionsModel, IPlanModel, IRestrictedUsersModel} from "../../../models";
import {v4 as uuid} from "uuid";
import {aplicationID_AADTenantRegExp, planIdRegExp} from "./RegExp";
import {ErrorMessage} from "./ErrorMessage";

export const getInitialPlan = (): IPlanFormValues => {
  return {
    plan: {
      planName: '',
      dataRetentionInDays: 0,
      subscribeArmTemplateName: '',
      unsubscribeArmTemplateName: '',
      suspendArmTemplateName: '',
      deleteDataArmTemplateName: '',
      subscribeWebhookName: '',
      unsubscribeWebhookName: '',
      suspendWebhookName: '',
      deleteDataWebhookName: '',
      priceModel: 'flatRate',
      monthlyBase: 100,
      annualBase: 0,
      privatePlan: false,
      isNew: true,
      clientId: uuid(),
      restrictedUsers: [],
      customMeterDimensions: []
    }
  }
};

export const getInitialRestrictedUser = (): IRestrictedUsersModel => {
  return {
    tenantId: '',
    description: '',
    isNew: true,
    clientId: uuid()
  }
};

export const getInitialCustomMeterDimension = (): ICustomMeterDimensionsModel => {
  return {
    annualQuantityIncludedInBase: 0,
    monthlyQuantityIncludedInBase: 0,
    annualUnlimited: false,
    monthlyUnlimited: false,
    meterName: '',
    planName: '',
    isNew: true,
    clientId: uuid()
  }
};

export interface IPlanFormValues {
  plan: IPlanModel;
}

const planValidator: ObjectSchema<IPlanModel> = yup.object().shape(
  {
    clientId: yup.string(),

    planName: yup.string()
    .matches(planIdRegExp,
      {
        message: ErrorMessage.planID,
        excludeEmptyString: true
      })
      .required("planName is a required field"),
    dataRetentionInDays: yup.number().test('validNumber', 'Not a valid integer', (val): boolean => {
      if (val === null || val === undefined || val === '') {
        return true;
      } else {
        return yup.number().integer().isValidSync(val);
      }
    }).min(0, "Value must be an int greater or equal to 0")
      .required("DataRetentionInDays is a required field"),
    subscribeArmTemplateName: yup.string(),
    unsubscribeArmTemplateName: yup.string(),
    suspendArmTemplateName: yup.string(),
    deleteDataArmTemplateName: yup.string(),
    subscribeWebhookName: yup.string(),
    unsubscribeWebhookName: yup.string(),
    suspendWebhookName: yup.string(),
    deleteDataWebhookName: yup.string(),

    priceModel: yup.string(),
    monthlyBase: yup.number(),
    annualBase: yup.number(),

    restrictedUsers: yup.mixed()
      .when('privatePlan', {
        is: (val) => {
          return val === true
        },
        then: yup.array<IRestrictedUsersModel>().of(
          yup.object().uniqueProperty('tenantId', 'No duplicate Tenant IDs')
          .shape({
            tenantId: yup.mixed().when('isDeleted', {
              is: (val) => {
                return !!val === false
              },
              then: yup.string().matches(aplicationID_AADTenantRegExp,
                {
                  message: ErrorMessage.userId,
                  excludeEmptyString: true
                })
                .required('User is required'),
              otherwise: yup.mixed().notRequired()
            }),
            description: yup.string(),
            clientId: yup.string(),
          })).max(10, 'Only 10 users may be used.'),
        otherwise: yup.array().notRequired()
      }),
    customMeterDimensions: yup.array<ICustomMeterDimensionsModel>().of(
      yup.object().uniqueProperty('meterName', 'No duplicate Meters')
        .shape({
          meterName: yup.mixed().when('isDeleted', {
            is: (val) => {
              return !!val === false
            },
            then: yup.string().required('Meter is required'),
            otherwise: yup.mixed().notRequired()
          }),
          planName: yup.string(),
          monthlyUnlimited: yup.boolean(),
          annualUnlimited: yup.boolean(),
          monthlyQuantityIncludedInBase: yup.number().test('validNumber', 'Not a valid integer', (val): boolean => {
            if (val === null || val === undefined || val === '') {
              return true;
            } else {
              return yup.number().integer().isValidSync(val);
            }
          }).min(0, "Value must be an int greater or equal to 0")
            .required("Included in Base is a required field"),
          annualQuantityIncludedInBase: yup.number(),
          clientId: yup.string(),
        })),
    privatePlan: yup.boolean(),
    isNew: yup.boolean(),
    status: yup.string(),
    createdTime: yup.string(),
    lastUpdatedTime: yup.string()
  }
);

export const planValidationSchema: ObjectSchema<IPlanFormValues> =
  yup.object().shape({
    plan: planValidator,
  });