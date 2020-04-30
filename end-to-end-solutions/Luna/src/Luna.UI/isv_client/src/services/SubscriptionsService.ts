import {ServiceBase} from "../services/ServiceBase";
import {
    ISubscriptionsModel,
    ISubscriptionsPostModel,
    ISubscriptionsV2Model,
    ISubscriptionsWarnings,
    Result
} from "../models";

export default class SubscriptionsService extends ServiceBase {

    public static async listV2(): Promise<Result<ISubscriptionsV2Model[]>> {

        var result = await this.requestJson<ISubscriptionsV2Model[]>({
            url: `/subscriptions`,
            method: "GET"
        });

        return result;
    }

    public static async list(): Promise<Result<ISubscriptionsModel[]>> {

        var result = await this.requestJson<ISubscriptionsModel[]>({
            url: `/subscriptions`,
            method: "GET"
        });

        return result;
    }

    public static async get(subscriptionId: string): Promise<Result<ISubscriptionsModel>> {

        var result = await this.requestJson<ISubscriptionsModel>({
            url: `/subscriptions/${subscriptionId}`,
            method: "GET"
        });

        return result;
    }

    public static async getSubscriptionWarnings(subscriptionId: string): Promise<Result<ISubscriptionsWarnings[]>> {

        var result = await this.requestJson<ISubscriptionsWarnings[]>({
          url: `/subscriptions/${subscriptionId}/warnings`,
          method: "GET"
        });
        return result;
    }

    public static async getAllSubscriptionWarnings(): Promise<Result<ISubscriptionsWarnings[]>> {

        var result = await this.requestJson<ISubscriptionsWarnings[]>({
          url: `/subscriptions/warnings`,
          method: "GET"
        });
        return result;
    }

    public static async create_update(model: ISubscriptionsPostModel): Promise<Result<ISubscriptionsModel>> {
        var result = await this.requestJson<ISubscriptionsModel>({
            url: `/subscriptions/${model.subscriptionId}`,
            method: "PUT",
            data: model
        });

        return result;
    }

    public static async delete(subscriptionId: string): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/subscriptions/${subscriptionId}`,
            method: "DELETE"
        });
        return result;
    }

    public static async activateSubscription(subscriptionId: string,subscriptionName:string): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/subscriptions/${subscriptionId}/activate`,
            method: "POST"
        });
        return result;
    }

    public static async completeOperation(subscriptionId: string,subscriptionName:string): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/subscriptions/${subscriptionId}/completeOperation`,
            method: "POST"
        });
        return result;
    }

}