import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IDeploymentsModel, IDeploymentVersionModel } from "../../../models";
import { v4 as uuid } from "uuid";
import { deploymentNameRegExp, versionNameRegExp } from "./RegExp";
import { ErrorMessage } from "./ErrorMessage";

export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );

export const getInitialDeployment = (): IDeploymentsModel => {
  return {
    productName: '',
    selecteddeploymentName:'',
    versionName: '',
    deploymentName: '',
    description: '',
    isSaved: false,
    isModified: false,
    clientId: uuid()
  }
};

export const getInitialVersion = (): IDeploymentVersionModel => {
  return {
    productName: '',
    deploymentName: '',
    versionName: '',
    realTimePredictAPI: '',
    trainModelId: '',
    batchInferenceId: '',
    deployModelId: '',
    authenticationType: '',
    authenticationKey: '',
    amlWorkspaceName: '',
    advancedSettings: '',
    selectedVersionName:'',
    versionSourceType:'',
    gitUrl:'',
    gitPersonalAccessToken  :'',
    gitVersion:'',
    projectFileUrl:'',
    projectFileContent:''
  }
};

export const initialDeploymentList: IDeploymentsModel[] = [{
  productName: 'a1',
  selecteddeploymentName:'',
  deploymentName: 'd1',
  versionName: '1.0',
  description: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
},
{
  productName: 'b1',
  selecteddeploymentName:'',
  deploymentName: 'd2',
  versionName: '2.0',
  description: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
}];

export interface IDeploymentFormValues {
  deployment: IDeploymentsModel;
}

export interface IDeploymentVersionFormValues {
  version: IDeploymentVersionModel;
}

export const initialDeploymentFormValues: IDeploymentFormValues = {
  deployment: getInitialDeployment()
}

const deploymentValidator: ObjectSchema<IDeploymentsModel> = yup.object().shape(
  {
    clientId: yup.string(),
    productName: yup.string(),
    selecteddeploymentName:yup.string(),
    versionName: yup.string(),
    deploymentName: yup.string()
      .required("Id is a required field")
      .matches(deploymentNameRegExp,
        {
          message: ErrorMessage.deploymentName,
          excludeEmptyString: true
        }),
    description: yup.string(),
  }
);

export const deletedeploymentValidator: ObjectSchema<IDeploymentsModel> = yup.object().shape(
  {

    clientId: yup.string(),
    productName: yup.string(),
    selecteddeploymentName:yup.string()
      .test('selecteddeploymentName', 'Deployment name does not match', function (value: string) {        
        const name: string = this.parent.deployment.deploymentName;
        if (!value)
          return true;

        return value.toLowerCase() === name.toLowerCase();
      }).required("Deployment Name is a required field"),
    versionName: yup.string(),
    deploymentName: yup.string(),
    description: yup.string(),

  }
);

const versionFormValidator: ObjectSchema<IDeploymentVersionModel> = yup.object().shape(
  {
    deploymentName: yup.mixed().notRequired(),
    deploymentVersionList: yup.array(),

    versionName: yup.string()
      .matches(versionNameRegExp,
        {
          message: ErrorMessage.versionName,
          excludeEmptyString: true
        })
      .required("versionName is a required field"),
    productName: yup.string(),
    trainModelApi: yup.string(),
    batchInferenceId: yup.string(),
    deployModelId: yup.string(),
    authenticationType: yup.string(),
    authenticationKey: yup.string(),
    //amlWorkspaceName: yup.mixed().notRequired(),
    amlWorkspaceName: yup.mixed()
    .when('source', {is: (val) => { return val === 'aml_pipelines'}, 
                then: yup.string().required('AMLWorkspace is Required'),
                otherwise: yup.mixed().notRequired()}),
    realTimePredictAPI: yup.string(),
    trainModelId: yup.string(),
    advancedSettings: yup.string().nullable(true),
    selectedVersionName:yup.string(),
    versionSourceType:yup.string(),
    gitUrl:yup.string(),
    gitPersonalAccessToken:yup.string(),
    gitVersion:yup.string(),
    projectFileUrl:yup.string(),
    projectFileContent:yup.string(),
  }
);

export const deleteVersionValidator: ObjectSchema<IDeploymentVersionModel> = yup.object().shape(
  {
    deploymentName: yup.mixed().notRequired(),
    deploymentVersionList: yup.array(),
    versionName: yup.string(),
    productName: yup.string(),
    trainModelApi: yup.string(),
    batchInferenceId: yup.string(),
    deployModelId: yup.string(),
    authenticationType: yup.string(),
    authenticationKey: yup.string(),
    amlWorkspaceName: yup.mixed().notRequired(),
    realTimePredictAPI: yup.string(),
    trainModelId: yup.string(),
    advancedSettings: yup.string().nullable(true),
    versionSourceType:yup.string(),
    gitUrl:yup.string(),
    gitPersonalAccessToken:yup.string(),
    gitVersion:yup.string(),
    projectFileUrl:yup.string(),
    projectFileContent:yup.string(),
    selectedVersionName:yup.string()
    .test('selectedVersionName', 'Version name does not match', function (value: string) {         
      const name: string = this.parent.versionName;
      if (!value)
        return true;

      return value.toLowerCase() === name.toLowerCase();
    }).required("versionName is a required field")
  }
);

export const deploymentFormValidationSchema: ObjectSchema<IDeploymentFormValues> =
  yup.object().shape({
    deployment: deploymentValidator
  });

export const versionFormValidationSchema: ObjectSchema<IDeploymentVersionFormValues> =
  yup.object().shape({
    version: versionFormValidator
  });

  // export const deleteVersionValidationSchema: ObjectSchema<IDeploymentVersionFormValues> =
  // yup.object().shape({
  //   version: deleteVersionValidator
  // });