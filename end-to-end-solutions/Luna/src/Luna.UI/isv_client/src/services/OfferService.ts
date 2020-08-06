// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
import { ServiceBase } from "../services/ServiceBase";
import {IOfferModel, Result} from "../models";
import {IOfferWarningsModel} from "../models/IOfferWarningsModel";
import {v4 as uuid} from "uuid";

export default class OfferService extends ServiceBase {

  public static async list(): Promise<Result<IOfferModel[]>> {

    var result = await this.requestJson<IOfferModel[]>({
      url: `/offers`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async getOfferWarnings(): Promise<Result<IOfferWarningsModel[]>> {        
    var result = await this.requestJson<IOfferWarningsModel[]>({
      url: `/offers/warnings`,
      method: "GET"
    });
    return result;
  }

  public static async get(offerName: string): Promise<Result<IOfferModel>> {

    var result = await this.requestJson<IOfferModel>({
      url: `/offers/${offerName}`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async update(model: IOfferModel): Promise<Result<IOfferModel>> {
    var result = await this.requestJson<IOfferModel>({
      url: `/offers/${model.offerName}`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async delete(offerName: string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/offers/${offerName}`,
      method: "DELETE"
    });
    return result;
  }

  public static async create(model: IOfferModel): Promise<Result<IOfferModel>> {
    var result = await this.requestJson<IOfferModel>({
      url: `/offers/${model.offerName}`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async publish(offerName:string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/offers/${offerName}/publish`,
      method: "POST",      
    });
    return result;
  }

  public static async getError(): Promise<Result<any>> {
    var result = await this.requestJson<any>({
      url: `http://localhost:3002/errors`,
      method: "GET"
    });

    result.errors = result.value;
    result.success = false;
    result.value = null;
    return result;
  }
}