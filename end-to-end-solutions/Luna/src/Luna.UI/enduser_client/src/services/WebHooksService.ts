// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceBase } from "../services/ServiceBase";
import { IWebHookModel, Result } from "../models";
import { v4 as uuid } from "uuid";

export default class WebHooksService extends ServiceBase {

    public static async list(OfferName: string): Promise<Result<IWebHookModel[]>> {
        var result = await this.requestJson<IWebHookModel[]>({
            url: `/offers/${OfferName}/webhooks`,
            method: "GET"
        });

        if (!result.hasErrors && result.value)
            result.value.map(u => u.clientId = uuid());

        return result;
    }

    public static async createOrUpdate(OfferName: string, model: IWebHookModel): Promise<Result<IWebHookModel>> {
        var result = await this.requestJson<IWebHookModel>({
            url: `/offers/${OfferName}/webhooks/${model.webhookName}`,
            method: "PUT",
            data: model
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async get(OfferName: string, webhookName: string): Promise<Result<IWebHookModel>> {
        var result = await this.requestJson<IWebHookModel>({
            url: `/offers/${OfferName}/webhooks/${webhookName}`,
            method: "GET",
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async delete(OfferName: string, webhookName: string): Promise<Result<IWebHookModel>> {
        var result = await this.requestJson<IWebHookModel>({
            url: `/offers/${OfferName}/webhooks/${webhookName}`,
            method: "DELETE",
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }
}