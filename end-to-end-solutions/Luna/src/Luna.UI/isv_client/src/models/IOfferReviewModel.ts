// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
import {IOfferModel} from "./IOfferModel";
import {IOfferParameterModel} from "./IOfferParameterModel";
import {IIpConfigModel} from "./IIpConfigModel";
import {IARMTemplateModel} from "./IARMTemplateModel";
import {IARMTemplateParameterModel} from "./IARMTemplateParameterModel";
import {IWebHookModel} from "./IWebHookModel";
import {IWebHookParameterModel} from "./IWebHookParameterModel";
import {IPlanModel} from "./IPlanModel";

export interface IOfferReviewModel {
  info: IOfferModel;
  offerParameters: IOfferParameterModel[];
  ipConfigs: IIpConfigModel[];
  armTemplates: IARMTemplateModel[];
  armTemplateParameters: IARMTemplateParameterModel[];
  webhooks: IWebHookModel[];
  webhookParameters: IWebHookParameterModel[];
  plans: IPlanModel[];
}