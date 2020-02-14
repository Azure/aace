import {IBaseModel} from "./IBaseModel";

export interface IIpConfigModel extends IBaseModel {
  name: string;
  ipBlocks: string[];
  iPsPerSub: number;
  ipRangeDialogVisible?:boolean;
}