// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceBase } from "../services/ServiceBase";
import {ICustomMeterModel, Result} from "../models";
import { v4 as uuid } from "uuid";

export default class CustomMetersService extends ServiceBase {

    public static async list(OfferName: string): Promise<Result<ICustomMeterModel[]>> {
        var result = await this.requestJson<ICustomMeterModel[]>({
            url: `/offers/${OfferName}/customMeters`,
            method: "GET"
        });

        if (!result.hasErrors && result.value)
            result.value.map(u => u.clientId = uuid());

        return result;
    }

    public static async createOrUpdate(OfferName: string, model: ICustomMeterModel): Promise<Result<ICustomMeterModel>> {
        var result = await this.requestJson<ICustomMeterModel>({
            url: `/offers/${OfferName}/customMeters/${model.meterName}`,
            method: "PUT",
            data: model
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async get(OfferName: string, meterName: string): Promise<Result<ICustomMeterModel>> {
        var result = await this.requestJson<ICustomMeterModel>({
            url: `/offers/${OfferName}/customMeters/${meterName}`,
            method: "GET",
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async delete(OfferName: string, meterName: string): Promise<Result<ICustomMeterModel>> {
        var result = await this.requestJson<ICustomMeterModel>({
            url: `/offers/${OfferName}/customMeters/${meterName}`,
            method: "DELETE",
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }
}