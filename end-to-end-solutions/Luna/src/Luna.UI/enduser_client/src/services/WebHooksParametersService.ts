// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceBase } from "../services/ServiceBase";
import { IWebHookParameterModel, Result } from "../models";
import { v4 as uuid } from "uuid";

export default class WebHooksService extends ServiceBase {

    public static async list(OfferName: string): Promise<Result<IWebHookParameterModel[]>> {
        var result = await this.requestJson<IWebHookParameterModel[]>({
            url: `/offers/${OfferName}/webhookParameters`,
            method: "GET"
        });

        if (!result.hasErrors && result.value)
            result.value.map(u => u.clientId = uuid());

        return result;
    }

    public static async create(OfferName: string, model: IWebHookParameterModel): Promise<Result<IWebHookParameterModel>> {
        var result = await this.requestJson<IWebHookParameterModel>({
            url: `/offers/${OfferName}/webhookParameters`,
            method: "PUT",
            data: model
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async update(OfferName: string, model: IWebHookParameterModel): Promise<Result<IWebHookParameterModel>> {
        var result = await this.requestJson<IWebHookParameterModel>({
            url: `/offers/${OfferName}/webhookParameters/${model.name}`,
            method: "PUT",
            data: model
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async get(OfferName: string, name: string): Promise<Result<IWebHookParameterModel>> {
        var result = await this.requestJson<IWebHookParameterModel>({
            url: `/offers/${OfferName}/webhookParameters/${name}`,
            method: "GET",
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async delete(OfferName: string, name: string): Promise<Result<IWebHookParameterModel>> {
        var result = await this.requestJson<IWebHookParameterModel>({
            url: `/offers/${OfferName}/webhookParameters/${name}`,
            method: "DELETE",
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }
}