// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceBase } from "../services/ServiceBase";
import {IARMTemplateModel, Result} from "../models";
import { v4 as uuid } from "uuid";

export default class ArmTemplateService extends ServiceBase {
    
    public static async list(offerName: string): Promise<Result<IARMTemplateModel[]>> {
        var result = await this.requestJson<IARMTemplateModel[]>({
            url: `/offers/${offerName}/armTemplates`,
            method: "GET"
        });

        if (!result.hasErrors && result.value)
        result.value.map(u => u.clientId = uuid());

        return result;
    }

    public static async create(offerName: string, armTemplateModel: IARMTemplateModel): Promise<Result<IARMTemplateModel>> {

        var result = await this.requestJson<IARMTemplateModel>({
            url: `/offers/${offerName}/armTemplates/${armTemplateModel.templateName}`,
            method: "PUT",
            data: JSON.parse(armTemplateModel.templateContent as string)
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async upload(offerName: string,templateName:string, data: any): Promise<Result<IARMTemplateModel>> {
        var result = await this.requestJson<IARMTemplateModel>({
            url: `/offers/${offerName}/armTemplates/${templateName}/upload`,
            method: "PUT",
            data: data
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }

    public static async delete(offerName: string, templateName: string): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/offers/${offerName}/armTemplates/${templateName}`,
            method: "DELETE"
        });
        return result;
    }
}