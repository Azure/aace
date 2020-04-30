import * as yup from "yup";
import {ObjectSchema} from "yup";
import {v4 as uuid} from "uuid";
import {IWebHookModel, IWebHookParameterModel} from "../../../models";
import {httpURLRegExp} from "./RegExp";
import {ErrorMessage} from "./ErrorMessage";

export const getInitialWebHook = (): IWebHookModel => {
  return {
    clientId: uuid(),
    isNew: true,
    webhookName: '',
    webhookUrl: ''
  }
}

export interface IWebHookForm {
  webhooks: IWebHookModel[];
  isDisabled: boolean;
}

export interface IWebHookParametersForm {
  webhookParameters: IWebHookParameterModel[];
}

const webhooksValidators = yup
  .array<IWebHookModel>().of(
    yup.object().uniqueProperty('webhookName', "Name must be unique").shape({
      webhookName: yup.mixed().when('isDeleted', {is: (val) => { return !!val === false}, then: yup.string().required('Name is required'),otherwise: yup.mixed().notRequired()}),
      webhookUrl: yup.mixed().when('isDeleted', {is: (val) => { return !!val === false}, then: yup.string().matches(httpURLRegExp,
          {
            message:ErrorMessage.httpUrl,
            excludeEmptyString: true
          }).required('URL is required'),otherwise: yup.mixed().notRequired()}),
      clientId: yup.string(),
      isDeleted: yup.boolean(),
      isModified: yup.boolean(),
      isNew: yup.boolean(),
      isSaved: yup.boolean()
    })
  );

const webhookParametersValidator = yup
  .array<IWebHookParameterModel>().of(
    yup.object().shape({
      name: yup.string(),
      value: yup.string(),
      clientId: yup.string(),
      isDeleted: yup.boolean(),
      isModified: yup.boolean(),
      isNew: yup.boolean(),
      isSaved: yup.boolean(),

    })); // these constraints are shown if and only if inner constraints are satisfied

export const webHooksFormValidationSchema: ObjectSchema<IWebHookForm> =
  yup.object().shape({
    webhooks: webhooksValidators,
    isDisabled: yup.boolean()
  });

export const webHookParametersFormValidationSchema: ObjectSchema<IWebHookParametersForm> =
  yup.object().shape({
    webhookParameters: webhookParametersValidator
  });




