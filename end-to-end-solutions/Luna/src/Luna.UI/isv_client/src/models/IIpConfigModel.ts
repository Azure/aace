// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import {IBaseModel} from "./IBaseModel";

export interface IIpConfigModel extends IBaseModel {
  name: string;
  ipBlocks: string[];
  enhancedIpBlocks: IIpBlockModel[];
  iPsPerSub: number;
  ipRangeDialogVisible?:boolean;
}

export interface IIpBlockModel extends IBaseModel {
  value: string;
}