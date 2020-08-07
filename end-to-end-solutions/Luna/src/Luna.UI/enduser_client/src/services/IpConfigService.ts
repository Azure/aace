// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceBase } from "../services/ServiceBase";
import { IIpConfigModel, Result } from "../models";
import { v4 as uuid } from "uuid";

export default class IpConfigService extends ServiceBase {

    public static async list(offerName: string): Promise<Result<IIpConfigModel[]>> {
        var result = await this.requestJson<IIpConfigModel[]>({
            url: `/offers/${offerName}/ipConfigs`,
            method: "GET"
        });

        if (!result.hasErrors && result.value) {
            result.value.map(u => u.clientId = uuid());
            result.value.map(u => u.isNew = false);
        }

        return result;
    }

    public static async createOrUpdate(offerName: string, model: IIpConfigModel): Promise<Result<IIpConfigModel>> {

        var result = await this.requestJson<IIpConfigModel>({
            url: `/offers/${offerName}/ipConfigs/${model.name}`,
            method: "PUT",
            data: model
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async delete(offerName: string, ipConfigName: string): Promise<Result<{}>> {

        var result = await this.requestJson<Result<{}>>({
            url: `/offers/${offerName}/ipConfigs/${ipConfigName}`,
            method: "DELETE"
        });
        return result;
    }
}