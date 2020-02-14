
export interface IError {
  //[key: string] : ISubError
  code: string;
  message: string;
  target: string;
};
/*
export interface ISubError {
  code: string;
  message: string;
  target: string;
}*/

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
    this.errors = (errors ? errors : []);
  }
}