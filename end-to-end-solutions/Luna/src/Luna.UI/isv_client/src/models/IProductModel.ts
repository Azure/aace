import { IBaseModel } from "./IBaseModel";

export interface IProductModel extends IBaseModel {
  productId: string;
  productType: string;
  hostType: string;
  owner: string;
  Idlist?: string;
  selectedProductId?: string
  selectedProductindex?: number
}

// export interface IDeploymentsModel extends IBaseModel {
//   id: string;
//   versionId: string;
//   realTimePredictApi: string;
//   apiAuthenticationKey: string;
// }

export interface IDeploymentsModel extends IBaseModel {
  productId:string;
  deploymentId: string;
  description:string;
  versionId: string;
  // realTimePredictApi: string;
  // apiAuthenticationKey: string;
  //deploymentVersionList:IDeploymentVersionModel[];    
}

export interface IDeploymentVersionModel {  
  productID:string;
  deploymentId:string;
  versionId: string;
  trainModelApi:string;
  BatchInferenceAPI:string;
  DeployModelAPI:string;
  AuthenticationType:string;
  AMLWorkspaceId:string; 
}

export interface IAMLWorkSpaceModel extends IBaseModel{  
  workspaceId:string;
  resourceId:string;
  aADApplicationId:string;
  aADApplicationSecret:string;
}
