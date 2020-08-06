// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
export interface IBaseModel {
  isNew?: boolean;
  isDeleted?: boolean;
  isModified?: boolean;
  isSaved?: boolean;
  clientId: string;
}