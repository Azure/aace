import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IOfferModel } from "../../../models";
import { IOfferParameterModel } from "../../../models/IOfferParameterModel";
import { v4 as uuid } from "uuid";
import { offerIdRegExp, emailRegExp, aplicationID_AADTenantRegExp } from "./RegExp";
import { ErrorMessage } from "./ErrorMessage";

export const getInitialOfferParameter = (): IOfferParameterModel => {
  return {
    parameterName: '',
    description: '',
    displayName: '',
    fromList: false,
    valueList: '',
    valueType: 'string',
    maximum: 0,
    minimum: 0,
    isNew: true,
    clientId: uuid(),
    isdisablemaximum: true,
    isdisableminimum: true,
    isdisablevalueList: true,
    isdisablefromList: false
  }
};

export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );

export const initialOfferParameters: IOfferParameterModel[] = [];

export const initialOfferValues: IOfferModel = {

  owners: '',
  offerAlias: '',
  hostSubscription: '',
  status: '',
  offerVersion: '',
  offerName: '',
  isNew: true,
  clientId: uuid()
};

export interface IOfferInfoFormValues {
  offer: IOfferModel;
}

export interface IOfferParametersFormValues {
  offerParameters: IOfferParameterModel[];
}

export const initialInfoFormValues: IOfferInfoFormValues = {
  offer: initialOfferValues
}

export const initialParametersFormValues: IOfferParametersFormValues = {
  offerParameters: initialOfferParameters
}

const offerValidator: ObjectSchema<IOfferModel> = yup.object().shape(
  {
    clientId: yup.string(),
    offerName: yup.string().test('offerName', 'Id already exist', function (value) {
      if (!value)
        return true;

      if (!this.parent.Idlist || this.parent.Idlist.length === 0)
        return true;

      let idlist = (this.parent.Idlist as string).split(',');
      return !idlist.includes(value);
    })
      .matches(offerIdRegExp,
        {
          message: ErrorMessage.offerID,
          excludeEmptyString: true
        }).required("Id is a required field"),

    owners: yup.string()
      .matches(emailRegExp,
        {
          message: ErrorMessage.Email,
          excludeEmptyString: true
        })
      .required("Owners is a required field"),
    offerAlias: yup.string()
      .required("Alias is a required field"),
    hostSubscription: yup.string()
      .matches(aplicationID_AADTenantRegExp,
        {
          message: ErrorMessage.hostSubscription,
          excludeEmptyString: true
        })
      .required("Host Subscription is a required field"),
    offerVersion: yup.string()
      .required("Offer Version is a required field"),
    status: yup.string(),
    createdTime: yup.string(),
    lastUpdatedTime: yup.string()
  }
);

const numberTest = yup.mixed().when('isDeleted', {
  is: (val) => {
    return !!val === false
  },
  then: yup.mixed().test('validNumber', 'Not a valid integer', (val): boolean => {
    if (val === null || val === undefined || val === '') {
      return true;
    }
    else {
      return yup.number().integer().isValidSync(val);
    }
  }),
  otherwise: yup.mixed().notRequired()
});

const paramValidator = yup
  .array<IOfferParameterModel>().of(
    yup.object().uniqueProperty('parameterName', 'no duplicate names')
      .shape({
        clientId: yup.string(),
        isDeleted: yup.boolean(),
        isNew: yup.boolean(),
        isModified: yup.boolean(),
        isSaved: yup.boolean(),

        parameterName: yup.mixed().when('isDeleted', {
          is: (val) => { return !!val === false }, then: yup.string()
            .matches(offerIdRegExp,
              {
                message: ErrorMessage.parameterName,
                excludeEmptyString: true
              })
            .required('Id is required'), otherwise: yup.mixed().notRequired()
        }),

        displayName: yup.mixed().when('isDeleted', { is: (val) => { return !!val === false }, then: yup.string().required('DisplayName is required'), otherwise: yup.mixed().notRequired() }),

        description: yup.mixed().when('isDeleted', { is: (val) => { return !!val === false }, then: yup.string().required('Description is required'), otherwise: yup.mixed().notRequired() }),

        valueType: yup.mixed().when('isDeleted', { is: (val) => { return !!val === false }, then: yup.string().required('ValueType is required'), otherwise: yup.mixed().notRequired() }),

        fromList: yup.boolean(),

        valueList: yup.mixed().when('isdisablevalueList', {
          is: (val) => { return val === false },
          then: yup.mixed().when('isDeleted', {
            is: (val) => { return !!val === false }, then:
              yup.mixed().when('fromList', {
                is: (valueType) => {
                  return !!valueType === true
                }, then: yup.string().required('ValueList is required'),
                otherwise: yup.mixed().notRequired()
              }),
            otherwise: yup.mixed().notRequired()
          }),
          otherwise: yup.mixed().notRequired()
        }),

        maximum: yup.mixed()
          .when('fromList', {
            is: (val) => {
              return val === false;
            },
            then: yup.mixed().when('isdisablemaximum', {
              is: (val) => {
                return val === false;
              }, then: numberTest.test('maximum', 'Maximum must be greater than minimum', function (value) {
                const minimumValue = parseInt(this.parent.minimum)

                if (!minimumValue) {
                  return true;
                }
                return parseInt(value) > minimumValue;
              }), otherwise: yup.mixed().notRequired()
            }),
            otherwise: yup.mixed()
          }),

        minimum: yup.mixed()
          .when('fromList', {
            is: (val) => {
              return val === false;
            },
            then: yup.mixed().when('isdisableminimum', {
              is: (val) => {
                return val === false;
              }, then: numberTest.test('minimum', 'Minimum must be less than maximum', function (value) {
                const maximumValue = parseInt(this.parent.maximum)

                if (!maximumValue) {
                  return true;
                }
                return parseInt(value) < maximumValue;
              }), otherwise: yup.mixed().notRequired()
            }),
            otherwise: yup.mixed(),
          }),
        isdisablemaximum: yup.boolean(),
        isdisableminimum: yup.boolean(),
        isdisablevalueList: yup.boolean(),
        isdisablefromList: yup.boolean()
      })
  );

export const offerInfoValidationSchema: ObjectSchema<IOfferInfoFormValues> =
  yup.object().shape({
    offer: offerValidator
  });

export const offerParametersValidationSchema: ObjectSchema<IOfferParametersFormValues> =
  yup.object().shape({
    offerParameters: paramValidator
  });

export const deleteOfferValidator: ObjectSchema<IOfferModel> = yup.object().shape(
  {
    clientId: yup.string(),
    offerName: yup.string(),
    selectedOfferName: yup.string()
      .test('selectedOfferName', 'Offer name does not match', function (value: string) {
        const offerName: string = this.parent.offerName;
        if (!value)
          return true;

        return value.toLowerCase() === offerName.toLowerCase();
      }).matches(offerIdRegExp,
        {
          message: ErrorMessage.offerID,
          excludeEmptyString: true
        }).required("Offer name is a required field"),

    owners: yup.string(),
    offerAlias: yup.string(),
    hostSubscription: yup.string(),
    offerVersion: yup.string(),
    status: yup.string(),
    createdTime: yup.string(),
    lastUpdatedTime: yup.string()
  }
);