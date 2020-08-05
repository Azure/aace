import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IAMLWorkSpaceModel } from "../../../models";
import { v4 as uuid } from "uuid";
import { workSpaceNameRegExp } from "./RegExp";
import { ErrorMessage } from "./ErrorMessage";
import { guidRegExp } from "../../Offers/formUtils/RegExp";

export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );

export const initialAMLWorkSpaceValues: IAMLWorkSpaceModel = {
  aadApplicationId: '',
  aadTenantId: '',
  aadApplicationSecrets: '',
  resourceId: '',
  registeredTime: new Date().toLocaleString(),
  workspaceName: '',
  isSaved: false,
  isModified: false,
  clientId: uuid(),
  selectedWorkspaceName: '',
};

export interface IAMLWorkSpaceFormValues {
  aMLWorkSpace: IAMLWorkSpaceModel;
}

export const initialAMLWorkSpaceFormValues: IAMLWorkSpaceFormValues = {
  aMLWorkSpace: initialAMLWorkSpaceValues
}

const aMLWorkSpaceValidator: ObjectSchema<IAMLWorkSpaceModel> = yup.object().shape(
  {
    clientId: yup.string(),
    aadApplicationId: yup.string().matches(guidRegExp,
      {
        message: ErrorMessage.aadApplicationId,
        excludeEmptyString: true
      })
      .required('AAD Application Id is required'),
    aadTenantId: yup.string().matches(guidRegExp,
      {
        message: ErrorMessage.tenantId,
        excludeEmptyString: true
      })
      .required('Tenant Id is required'),
    aadApplicationSecrets: yup.string(),
    resourceId: yup.string(),
    registeredTime: yup.string(),
    workspaceName: yup.string()
      .matches(workSpaceNameRegExp,
        {
          message: ErrorMessage.workSpaceName,
          excludeEmptyString: true
        }).required("Workspace Name is required"),
    selectedWorkspaceName: yup.string()
  }
);

export const deleteAMLWorkSpaceValidator: ObjectSchema<IAMLWorkSpaceModel> = yup.object().shape(
  {
    clientId: yup.string(),
    aadApplicationId: yup.string(),
    aadTenantId: yup.string(),
    aadApplicationSecrets: yup.string(),
    resourceId: yup.string(),
    registeredTime: yup.string(),
    workspaceName: yup.string(),
    selectedWorkspaceName: yup.string()
      .matches(workSpaceNameRegExp,
        {
          message: ErrorMessage.workSpaceName,
          excludeEmptyString: true
        })
      .test('selectedWorkspaceName', 'WorkSpace name does not match', function (value: string) {
        const name: string = this.parent.workspaceName;
        if (!value)
          return true;

        return value.toLowerCase() === name.toLowerCase();
      })
      .required("Workspace Name is required"),
  }
);

export const aMLWorkSpaceFormValidationSchema: ObjectSchema<IAMLWorkSpaceFormValues> =
  yup.object().shape({
    aMLWorkSpace: aMLWorkSpaceValidator
  });
