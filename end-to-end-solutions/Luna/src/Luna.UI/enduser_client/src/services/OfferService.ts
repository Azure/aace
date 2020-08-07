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

  public static async getOfferWarnings(): Promise<Result<IOfferWarningsModel>> {

    let warnings = [
      "Offer blah is out of sync with the Azure Marketplace. Click<a href='https://www.bing.com' target='_blank'>here</a> for more details.",
      "Oh no...something happened with your deployment!  Click<a href='https://www.bing.com' target='_blank'>here</a> for more details."
    ];
    return new Result<IOfferWarningsModel>( {warnings: warnings}, true);
    /*var result = await this.requestJson<IOfferWarningsModel>({
      url: `/offerwarnings`,
      method: "GET"
    });
    return result;*/
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

  public static async delete(offerName: string): Promise<Result<{}>> {
    var result = await this.requestJson<Result<{}>>({
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
}