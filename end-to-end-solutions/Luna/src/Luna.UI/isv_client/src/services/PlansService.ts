// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceBase } from "../services/ServiceBase";
import {IPlanModel, Result, IRestrictedUsersModel, ICustomMeterDimensionsModel} from "../models";
import { v4 as uuid } from "uuid";

export default class PlansService extends ServiceBase {

    // since the controls work easier with strings we will scrub the incoming and outgoing model
    private static scrubIncomingModel(model: IPlanModel) : void {
        if (model.subscribeArmTemplateName === null)
            model.subscribeArmTemplateName = "";

        if (model.unsubscribeArmTemplateName === null)
            model.unsubscribeArmTemplateName = "";

        if (model.suspendArmTemplateName === null)
            model.suspendArmTemplateName = "";

        if (model.deleteDataArmTemplateName === null)
            model.deleteDataArmTemplateName = "";

        if (model.subscribeWebhookName === null)
            model.subscribeWebhookName = "";

        if (model.unsubscribeWebhookName === null)
            model.unsubscribeWebhookName = "";

        if (model.suspendWebhookName === null)
            model.suspendWebhookName = "";

        if (model.deleteDataWebhookName === null)
            model.deleteDataWebhookName = "";
    }

    private static scrubOutgoingModel(model: IPlanModel) : IPlanModel {
        let result : IPlanModel = {...model};
        
        if (result.subscribeArmTemplateName === "")
            result.subscribeArmTemplateName = null;

        if (result.unsubscribeArmTemplateName === "")
            result.unsubscribeArmTemplateName = null;

        if (result.suspendArmTemplateName === "")
            result.suspendArmTemplateName = null;

        if (result.deleteDataArmTemplateName === "")
            result.deleteDataArmTemplateName = null;

        if (result.subscribeWebhookName === "")
            result.subscribeWebhookName = null;

        if (result.unsubscribeWebhookName === "")
            result.unsubscribeWebhookName = null;

        if (result.suspendWebhookName === "")
            result.suspendWebhookName = null;

        if (result.deleteDataWebhookName === "")
            result.deleteDataWebhookName = null;

        return result;
    }

    public static async list(OfferName: string): Promise<Result<IPlanModel[]>> {
        var result = await this.requestJson<IPlanModel[]>({
            url: `/offers/${OfferName}/plans`,
            method: "GET"
        });

        if (!result.hasErrors && result.value)
            result.value.map(u => {
                u.clientId = uuid();                
                return PlansService.scrubIncomingModel(u);                 
            });

        return result;
    }

    public static async get(offerName: string, planName: string): Promise<Result<IPlanModel>> {

        var result = await this.requestJson<IPlanModel>({
            url: `/offers/${offerName}/plans/${planName}`,
            method: "GET"
        });

        if (!result.hasErrors && result.value) {
            result.value.clientId = uuid();
            PlansService.scrubIncomingModel(result.value);
        }

        return result;
    }   

    public static async update(OfferName: string, model: IPlanModel): Promise<Result<{}>> {

        const scrubbedModel = PlansService.scrubOutgoingModel(model);

        var result = await this.requestJson<Result<{}>>({
            url: `/offers/${OfferName}/plans/${scrubbedModel.planName}`,
            method: "PUT",
            data: scrubbedModel
        });
        return result;
    }

    public static async delete(offerName: string, planName: string): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/offers/${offerName}/plans/${planName}`,
            method: "DELETE"
        });
        return result;
    }

    public static async create(OfferName: string, model: IPlanModel): Promise<Result<IPlanModel>> {

        model = PlansService.scrubOutgoingModel(model);

        var result = await this.requestJson<IPlanModel>({
            url: `/offers/${OfferName}/plans/${model.planName}`,
            method: "PUT",
            data: model
        });

        
        return result;
    }

    public static async createRestrictedUser(offerName: string, planName: string, model: IRestrictedUsersModel): Promise<Result<IRestrictedUsersModel>> {

        var result = await this.requestJson<IRestrictedUsersModel>({
            url: `/offers/${offerName}/plans/${planName}/restrictedUsers/${model.tenantId}`,
            method: "PUT",
            data: model
        });        

        return result;
    }

    public static async getRestrictedUsers(offerName: string, planName: string): Promise<Result<IRestrictedUsersModel[]>> {

        var result = await this.requestJson<IRestrictedUsersModel[]>({
            url: `/offers/${offerName}/plans/${planName}/restrictedUsers`,
            method: "GET"
        });        

        return result;
    }

    public static async deleteRestrictedUser(offerName: string, planName: string, tenantId: string): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/offers/${offerName}/plans/${planName}/restrictedUsers/${tenantId}`,
            method: "DELETE"
        });
        return result;
    }

    public static async createOrUpdateCustomMeterDimension(offerName: string, model: ICustomMeterDimensionsModel): Promise<Result<ICustomMeterDimensionsModel>> {

        var result = await this.requestJson<ICustomMeterDimensionsModel>({
            url: `/offers/${offerName}/plans/${model.planName}/customMeterDimensions/${model.meterName}`,
            method: "PUT",
            data: model
        });

        return result;
    }

    public static async getCustomMeterDimensions(offerName: string, planName: string): Promise<Result<ICustomMeterDimensionsModel[]>> {

        var result = await this.requestJson<ICustomMeterDimensionsModel[]>({
            url: `/offers/${offerName}/plans/${planName}/customMeterDimensions`,
            method: "GET"
        });

        return result;
    }

    public static async deleteCustomMeterDimension(offerName: string, planName: string, meterName: string): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/offers/${offerName}/plans/${planName}/customMeterDimensions/${meterName}`,
            method: "DELETE"
        });
        return result;
    }
}

