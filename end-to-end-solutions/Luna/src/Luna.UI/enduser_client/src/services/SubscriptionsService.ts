import {ServiceBase} from "../services/ServiceBase";
import {
    ICreateSubscriptionModel,
    ILandingModel,
    IOperationHistoryModel,
    ISubscriptionsModel,
    ISubscriptionWarningsModel, IUpdateSubscriptionModel,
    Result
} from "../models";

export default class SubscriptionsService extends ServiceBase {

    public static async list(email: string): Promise<Result<ISubscriptionsModel[]>> {

        var result = await this.requestJson<ISubscriptionsModel[]>({
            url: `/subscriptions?owner=${email}`,
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

    public static async getSubscriptionWarnings(): Promise<Result<ISubscriptionWarningsModel>> {

        let warnings = [
            "Offer blah is out of sync with the Azure Marketplace. Click<a href='https://www.bing.com' target='_blank'>here</a> for more details.",
            "Oh no...something happened with your deployment!  Click<a href='https://www.bing.com' target='_blank'>here</a> for more details."
        ];
        return new Result<ISubscriptionWarningsModel>({ warnings: warnings }, true);
        /*var result = await this.requestJson<IOfferWarningsModel>({
          url: `/offerwarnings`,
          method: "GET"
        });
        return result;*/
    }

    public static async create(model: ICreateSubscriptionModel): Promise<Result<ISubscriptionsModel>> {
        var result = await this.requestJson<ISubscriptionsModel>({
            url: `/subscriptions/${model.SubscriptionId}`,
            method: "PUT",
            data: model
        });

        return result;
    }

    public static async update(model: IUpdateSubscriptionModel): Promise<Result<ISubscriptionsModel>> {
        var result = await this.requestJson<ISubscriptionsModel>({
            url: `/subscriptions/${model.SubscriptionId}`,
            method: "PUT",
            data: model
        });

        return result;
    }
/*
    public static async create_update(model: ISubscriptionsPostModel): Promise<Result<ISubscriptionsModel>> {        
        var result = await this.requestJson<ISubscriptionsModel>({
            url: `/subscriptions/${model.subscriptionId}`,
            method: "PUT",
            data: model
        });

        return result;
    }*/

    public static async delete(subscriptionId: string): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/subscriptions/${subscriptionId}`,
            method: "DELETE"
        });
        return result;
    }

    public static async getOperationHistory(subscriptionId: string): Promise<Result<IOperationHistoryModel[]>> {

        var result = await this.requestJson<IOperationHistoryModel[]>({
            url: `/subscriptions/${subscriptionId}/operations`,
            // url: `http://localhost:3002/Subscriptions/${email}`,
            method: "GET"
        });
                
        // return result;        
        return result;
    }
}