import * as yup from "yup";
import { ObjectSchema } from "yup";
import {IAMLWorkSpaceModel } from "../../../models";
import { v4 as uuid } from "uuid";
import { workSpaceIdRegExp } from "./RegExp";
import { ErrorMessage } from "./ErrorMessage";

export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );

export const initialAMLWorkSpaceValues: IAMLWorkSpaceModel = {
  aADApplicationId: '',
  aADApplicationSecret: '',
  resourceId: '',
  workspaceId: '',
  isSaved: false,
  isModified: false,
  clientId: uuid()
};

export let initialAMLWorkSpaceList: IAMLWorkSpaceModel[] = [{
  aADApplicationId: '1',
  aADApplicationSecret: '1',
  resourceId: '1',
  workspaceId: '1',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
},
{
  aADApplicationId: '2',
  aADApplicationSecret: '2',
  resourceId: '2',
  workspaceId: '2',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
}];

export interface IAMLWorkSpaceFormValues {
  aMLWorkSpace: IAMLWorkSpaceModel;
}

export const initialAMLWorkSpaceFormValues: IAMLWorkSpaceFormValues = {
  aMLWorkSpace: initialAMLWorkSpaceValues
}

const aMLWorkSpaceValidator: ObjectSchema<IAMLWorkSpaceModel> = yup.object().shape(
  {
    clientId: yup.string(),
    aADApplicationId: yup.string(),
    aADApplicationSecret: yup.string(),
    resourceId: yup.string(),
    workspaceId: yup.string()
    .matches(workSpaceIdRegExp,
      {
        message: ErrorMessage.workSpaceID,
        excludeEmptyString: true
      }).required("WorkspaceId is required"),
  }
);

export const aMLWorkSpaceFormValidationSchema: ObjectSchema<IAMLWorkSpaceFormValues> =
  yup.object().shape({
    aMLWorkSpace: aMLWorkSpaceValidator
  });
