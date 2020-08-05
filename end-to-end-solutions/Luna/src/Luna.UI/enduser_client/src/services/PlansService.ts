import { ServiceBase } from "../services/ServiceBase";
import { IPlanModel, IPlanWarningsModel, Result, IARMTemplateModel,IRestrictedUsersModel } from "../models";
import { v4 as uuid } from "uuid";

// eslint-disable-next-line @typescript-eslint/no-unused-vars
let armTemplateModellist: IARMTemplateModel[];
export default class PlansService extends ServiceBase {

    public static async list(OfferName: string): Promise<Result<IPlanModel[]>> {
        var result = await this.requestJson<IPlanModel[]>({
            url: `/offers/${OfferName}/plans`,
            method: "GET"
        });

        if (!result.hasErrors && result.value)
            result.value.map(u => u.clientId = uuid());

        return result;
    }

    public static async getPlansWarnings(): Promise<Result<IPlanWarningsModel>> {

        let warnings = [
            "Plans blah is out of sync with the Azure Marketplace. Click<a href='https://www.bing.com' target='_blank'>here</a> for more details.",
            "Oh no...something happened with your deployment!  Click<a href='https://www.bing.com' target='_blank'>here</a> for more details."
        ];
        return new Result<IPlanWarningsModel>({ warnings: warnings }, true);
        /*var result = await this.requestJson<IOfferWarningsModel>({
          url: `/offerwarnings`,
          method: "GET"
        });
        return result;*/
    }

    public static async get(offerName: string, planName: string): Promise<Result<IPlanModel>> {

        var result = await this.requestJson<IPlanModel>({
            url: `/offers/${offerName}/plans/${planName}`,
            method: "GET"
        });

        if (!result.hasErrors && result.value)
            result.value.clientId = uuid();

        return result;
    }   

    public static async update(OfferName: string, model: IPlanModel): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/offers/${OfferName}/plans/${model.planName}`,
            method: "PUT",
            data: model
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
        var result = await this.requestJson<IPlanModel>({
            url: `/offers/${OfferName}/plans`,
            method: "PUT",
            data: model
        });

        
        return result;
    }

    public static async createRestrictedUsers(offerName: string, planName: string,model: IRestrictedUsersModel): Promise<Result<IRestrictedUsersModel>> {

        var result = await this.requestJson<IRestrictedUsersModel>({
            url: `/offers/${offerName}/plans/${planName}/restrictedUsers`,
            method: "PUT",
            data: model
        });        

        return result;
    }

    public static async updateRestrictedUsers(Id: string, model: IRestrictedUsersModel): Promise<Result<IRestrictedUsersModel>> {

        var result = await this.requestJson<IRestrictedUsersModel>({
            url: `/restrictedUsers/${Id}`,
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

    public static async deleteRestrictedUsers(Id: number): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/restrictedUsers/${Id}`,
            method: "DELETE"
        });
        return result;
    }
}

