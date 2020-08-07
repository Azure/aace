// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { IBaseModel } from "./IBaseModel";

export interface IProductModel extends IBaseModel {
  productName: string;
  productType: string;
  hostType: string;
  owner: string;
  createdTime?: string;
  lastUpdatedTime?: string;
  Idlist?: string;
  selectedProductId?: string
  selectedProductindex?: number  
}

export interface ILookupType {
  id:string;
  displayName:string;
}

export interface IDeploymentsModel extends IBaseModel {
  productName:string;
  deploymentName: string;
  description:string;
  versionName: string;  
  selecteddeploymentName: string;  
}

export interface IDeploymentVersionModel {  
  productName:string;
  deploymentName:string;
  versionName: string;
  realTimePredictAPI: string;
  trainModelId:string;
  batchInferenceId:string;
  deployModelId:string;
  authenticationType:string;
  authenticationKey:string;
  amlWorkspaceName:string;
  advancedSettings:string | null;
  selectedVersionName:string;
  versionSourceType:string;
  gitUrl:string;
  gitPersonalAccessToken:string;
  gitVersion:string;
  projectFileUrl:string;
  projectFileContent:string;
}

export interface IAMLWorkSpaceModel extends IBaseModel{  
  workspaceName:string;
  resourceId:string;
  aadTenantId:string;
  registeredTime:string;
  aadApplicationId:string;
  aadApplicationSecrets:string;
  selectedWorkspaceName:string;
}

export interface ISourceModel {  
  displayName:string;
  id:string;
}

export interface IPipeLineModel {  
  displayName:string;
  id:string;
  lastUpdatedTime:string;
  description:string;
}
