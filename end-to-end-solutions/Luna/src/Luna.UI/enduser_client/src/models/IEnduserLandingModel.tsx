import {IDropdownOption} from "office-ui-fabric-react";

export interface IParameterModel {
    parameterName: string,
    displayName: string,
    description: string,
    valueType: string,
    fromList: boolean,
    valueList: string;
    maximum: number | null;
    minimum: number | null;
}
export interface ILandingModel {
    email: string,
    fullName : string,
    subscriptionId: string,
    subscriptionName: string,
    name: string,
    offerName: string,
    planName: string,
    owner: string,
    quantity: number,
    beneficiaryTenantId: string,
    purchaserTenantId: string,
    subscribeWebhookName: string,
    unsubscribeWebhookName: string,
    suspendWebhookName: string,
    deleteDataWebhookName: string,
    priceModel: string,
    monthlyBase: number,
    privatePlan: boolean,
    inputParameters: IParameterModel[],
    parameterValues: [],
    isUpdatePlan:boolean,
    availablePlanName:string,
    planlist: IDropdownOption[]
}

export interface IResolveTokenModel {

    offerId: string,
    planId: string,
    publisherId: string | null,
    beneficiaryTenantId: string,
    purchaserTenantId: string,
    quantity: number,
    state: number,
    subscriptionId: string,
    subscriptionName: string
}
