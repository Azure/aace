// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
import { ServiceBase } from "../services/ServiceBase";
import {IARMTemplateParameterModel, Result} from "../models";
import { v4 as uuid } from "uuid";

export default class ArmTemplateParameterService extends ServiceBase {       

    public static async list(offerName: string): Promise<Result<IARMTemplateParameterModel[]>> {
        var result = await this.requestJson<IARMTemplateParameterModel[]>({
            url: `/offers/${offerName}/armTemplateParameters`,
            method: "GET"
        });

        if (!result.hasErrors && result.value)        
        result.value.map(u => u.clientId = uuid());

        return result;
    }

    public static async update(offerName: string, model: IARMTemplateParameterModel): Promise<Result<IARMTemplateParameterModel>> {

        var result = await this.requestJson<IARMTemplateParameterModel>({
            url: `/offers/${offerName}/armTemplateParameters/${model.name}`,
            method: "PUT",
            data: model
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }
}