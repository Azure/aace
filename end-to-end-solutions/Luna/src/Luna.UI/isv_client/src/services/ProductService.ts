import { ServiceBase } from "./ServiceBase";
import { Result, IProductModel, IDeploymentsModel, IDeploymentVersionModel, IAMLWorkSpaceModel } from "../models";
import { v4 as uuid } from "uuid";

export default class ProductService extends ServiceBase {

  public static async list(): Promise<Result<IProductModel[]>> {

    var result = await this.requestJson<IProductModel[]>({
      url: `/products`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async get(productId: string): Promise<Result<IProductModel>> {

    var result = await this.requestJson<IProductModel>({
      url: `/products/${productId}`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async update(model: IProductModel): Promise<Result<IProductModel>> {
    var result = await this.requestJson<IProductModel>({
      url: `/products/${model.productId}`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async delete(productId: string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/products/${productId}`,
      method: "DELETE"
    });
    return result;
  }

  public static async create(model: IProductModel): Promise<Result<IProductModel>> {
    var result = await this.requestJson<IProductModel>({
      url: `/products/${model.productId}`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }


  //#region Deployments 

  public static async getDeploymentListByProductId(productId: string): Promise<Result<IDeploymentsModel[]>> {

    var result = await this.requestJson<IDeploymentsModel[]>({
      url: `/products/${productId}/deployments`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async getDeploymentByProductId(productId: string,deploymentId:string): Promise<Result<IDeploymentsModel>> {

    var result = await this.requestJson<IDeploymentsModel>({
      url: `/products/${productId}/deployments/${deploymentId}`,
      method: "GET"
    });

    return result;
  }

  public static async createOrUpdateDeployment(model: IDeploymentsModel): Promise<Result<IDeploymentsModel>> {
    var result = await this.requestJson<IDeploymentsModel>({
      url: `/products/${model.productId}/deployments/${model.deploymentId}`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async deleteDeployment(productId: string,deploymentId:string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/products/${productId}/deployments/${deploymentId}`,
      method: "DELETE"
    });
    return result;
  }
  //#endregion

  //#region Version
  public static async getDeploymentVersionListByDeploymentId(productId: string,deploymentId:string): Promise<Result<IDeploymentVersionModel[]>> {

    var result = await this.requestJson<IDeploymentVersionModel[]>({
      url: `/products/${productId}/deployments/${deploymentId}/versions`,
      method: "GET"
    });

    return result;
  }

  public static async getDeploymentVersionById(productId: string,deploymentId:string,versionId:string): Promise<Result<IDeploymentVersionModel>> {

    var result = await this.requestJson<IDeploymentVersionModel>({
      url: `/products/${productId}/deployments/${deploymentId}/versions/${versionId}`,
      method: "GET"
    });

    return result;
  }

  public static async createOrUpdateDeploymentVersion(model: IDeploymentVersionModel): Promise<Result<IDeploymentVersionModel>> {
    var result = await this.requestJson<IDeploymentVersionModel>({
      url: `/products/${model.productID}/deployments/${model.deploymentId}/versions/${model.versionId}`,
      method: "PUT",
      data: model
    });

    return result;
  }

  public static async deleteDeploymentVersion(productId: string,deploymentId:string,versionId:string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/products/${productId}/deployments/${deploymentId}/Versions/${versionId}`,
      method: "DELETE"
    });
    return result;
  }
  //#endregion

  //#region AMLWorkSpace

  public static async getAmlWorkSpaceList(): Promise<Result<IAMLWorkSpaceModel[]>> {

    var result = await this.requestJson<IAMLWorkSpaceModel[]>({
      url: `/amlworkspaces/`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async getAmlWorkSpaceById(workspaceId:string): Promise<Result<IAMLWorkSpaceModel>> {

    var result = await this.requestJson<IAMLWorkSpaceModel>({
      url: `/amlworkspaces/${workspaceId}`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async createOrUpdateWorkSpace(model: IAMLWorkSpaceModel): Promise<Result<IAMLWorkSpaceModel>> {
    var result = await this.requestJson<IAMLWorkSpaceModel>({
      url: `/amlworkspaces/${model.workspaceId}`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async deleteWorkSpace(workspaceId: string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/amlworkspaces/${workspaceId}/`,
      method: "DELETE"
    });
    return result;
  }
  //#endregion
}