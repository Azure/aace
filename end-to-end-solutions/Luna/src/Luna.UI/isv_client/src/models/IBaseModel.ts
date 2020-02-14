export interface IBaseModel {
  isNew?: boolean;
  isDeleted?: boolean;
  isModified?: boolean;
  isSaved?: boolean;
  clientId: string;
}