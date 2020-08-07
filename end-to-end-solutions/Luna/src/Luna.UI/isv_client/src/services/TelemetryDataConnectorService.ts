// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceBase } from "../services/ServiceBase";
import { ITelemetryDataConnectorModel, Result } from "../models";
import { v4 as uuid } from "uuid";

export default class TelemetryDataConnectorService extends ServiceBase {

    public static async getTypes(): Promise<Result<string[]>> {
        var result = await this.requestJson<string[]>({
            url: '/telemetryDataConnectors/connectorTypes',
            method: "GET"
        });

        //let result2 = new Result<string[]>(['Log Analytics', 'Foobar'], true, null);
        //return result2;
        return result;
    }

    public static async list(): Promise<Result<ITelemetryDataConnectorModel[]>> {
        var result = await this.requestJson<ITelemetryDataConnectorModel[]>({
            url: '/telemetryDataConnectors',
            method: "GET"
        });

        if (!result.hasErrors && result.value)
            result.value.map(u => u.clientId = uuid());

        return result;
    }

    public static async createOrUpdate(model: ITelemetryDataConnectorModel): Promise<Result<ITelemetryDataConnectorModel>> {
        var result = await this.requestJson<ITelemetryDataConnectorModel>({
            url: `/telemetryDataConnectors/${model.name}`,
            method: "PUT",
            data: model
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async get(name: string): Promise<Result<ITelemetryDataConnectorModel>> {
        var result = await this.requestJson<ITelemetryDataConnectorModel>({
            url: `/telemetryDataConnectors/${name}`,
            method: "GET",
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async delete(name: string): Promise<Result<ITelemetryDataConnectorModel>> {
        var result = await this.requestJson<ITelemetryDataConnectorModel>({
            url: `/telemetryDataConnectors/${name}`,
            method: "DELETE",
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }
}