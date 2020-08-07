// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.


export interface IError {
  [key: string] : string[]
};

export class Result<T> {
  public value: T | null;
  public success: boolean;
  public errors: IError[];
  public get hasErrors(): boolean {
    return this.errors != null && Array.isArray(this.errors) && this.errors.length > 0;
  }

  constructor(value: T | null, success: boolean, errors?: IError[] | null) {
    this.value = value;
    this.success = success;
    //errors[0] == undefined || errors[0] == null || errors[0] == "" ? [] :
    this.errors = (errors ? errors : []);
  }
}