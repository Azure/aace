import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IDeploymentsModel, IDeploymentVersionModel } from "../../../models";
import { v4 as uuid } from "uuid";
import { deploymentIdRegExp, versionIdRegExp } from "./RegExp";
import { ErrorMessage } from "./ErrorMessage";

export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );

export const initialDeploymentValues: IDeploymentsModel = {
  productId: '',
  versionId: '',
  deploymentId: '',
  description: '',
  isSaved: false,
  isModified: false,
  clientId: uuid()
};

export const initialVersionValues: IDeploymentVersionModel = {
  deploymentId: '',
  productID: '',
  AMLWorkspaceId: '',
  AuthenticationType: '',
  BatchInferenceAPI: '',
  DeployModelAPI: '',
  trainModelApi: '',
  versionId: ''
};

export const initialDeploymentList: IDeploymentsModel[] = [{
  productId: '1',
  deploymentId: '1',
  versionId: '1.0',
  description: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
},
{
  productId: '1',
  deploymentId: '2',
  versionId: '2.0',
  description: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
}];

export const initialDeploymentVersionList: IDeploymentVersionModel[] = [
  {
    deploymentId: '1',
    versionId: '1.0',
    productID: 'realTimePredictApi',
    AMLWorkspaceId: '',
    AuthenticationType: '',
    BatchInferenceAPI: '',
    DeployModelAPI: '',
    trainModelApi: ''
  },
  {
    deploymentId: '1',
    versionId: '2.0',
    productID: 'realTimePredictApi',
    AMLWorkspaceId: '',
    AuthenticationType: '',
    BatchInferenceAPI: '',
    DeployModelAPI: '',
    trainModelApi: ''
  },
  {
    deploymentId: '1',
    versionId: '3.0',
    productID: 'realTimePredictApi',
    AMLWorkspaceId: '',
    AuthenticationType: '',
    BatchInferenceAPI: '',
    DeployModelAPI: '',
    trainModelApi: ''
  }
];

export interface IDeploymentFormValues {
  deployment: IDeploymentsModel;
}

export interface IVersionFormValues {
  version: IDeploymentVersionModel;
}

export const initialDeploymentFormValues: IDeploymentFormValues = {
  deployment: initialDeploymentValues
}

const deploymentValidator: ObjectSchema<IDeploymentsModel> = yup.object().shape(
  {
    clientId: yup.string(),
    productId: yup.string(),
    versionId: yup.string(),
    deploymentId: yup.string()
    .required("Id is a required field")
    .matches(deploymentIdRegExp,
      {        
        message: ErrorMessage.deploymentID,
        excludeEmptyString: true        
      }),    
    description: yup.string()
  }
);

const versionFormValidator: ObjectSchema<IDeploymentVersionModel> = yup.object().shape(
  {
    deploymentId: yup.mixed().notRequired(),
    deploymentVersionList: yup.array(),

    versionId: yup.string()
    .matches(versionIdRegExp,
      {
        message: ErrorMessage.versionID,
        excludeEmptyString: true
      })
    .required("VersionId is a required field"),
    productID: yup.string(),
    trainModelApi: yup.string(),
    BatchInferenceAPI: yup.string(),
    DeployModelAPI: yup.string(),
    AuthenticationType: yup.string(),
    AMLWorkspaceId: yup.string(),
  }
);

export const deploymentFormValidationSchema: ObjectSchema<IDeploymentFormValues> =
  yup.object().shape({
    deployment: deploymentValidator
  });

export const versionFormValidationSchema: ObjectSchema<IVersionFormValues> =
  yup.object().shape({
    version: versionFormValidator
  });