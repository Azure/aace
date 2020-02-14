import { IBaseModel } from "./IBaseModel";

export interface IPlanModel extends IBaseModel {
    planName: string,
    dataRetentionInDays: number,
    subscribeArmTemplateName: string,
    unsubscribeArmTemplateName: string,
    suspendArmTemplateName: string,
    deleteDataArmTemplateName: string,
    subscribeWebhookName: string,
	unsubscribeWebhookName: string,
	suspendWebhookName: string,
	deleteDataWebhookName: string,
    priceModel: string,
    monthlyBase: number,
    annualBase: number,
    privatePlan: boolean
    restrictedUsers: IRestrictedUsersModel[];
    planIdList?: string,    
    planNameList?: string,
}

export interface IRestrictedUsersModel extends IBaseModel {
    tenantId: string,
    description: string
}

export interface IPlanWarningsModel {
    warnings: string[];
}