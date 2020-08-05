import {Result} from "../models";
import * as jsonToUrl from "json-to-url";
import Axios, {AxiosRequestConfig, AxiosResponse} from "axios";
import {ERROR_STATE} from "../layout/GlobalErrorController";
import {Cache, Hub} from "aws-amplify";
import {getToken} from "../adalConfig";

export interface IRequestOptions {
  url: string;
  data?: any;
  method: "GET" | "POST" | "PUT" | "DELETE";
}

export interface ISendFormDataOptions {
  url: string;
  data: FormData;
  method: "POST" | "PUT" | "PATCH";
}

/**
 * Represents base class of the isomorphic service.
 */
export abstract class ServiceBase {

  private static sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  private static async getTokenWithRetry(){
    var token = await getToken();
    for (var i=0; token === null&& i<10;i++){
      await this.sleep(500);
      token = await getToken();
    }

    return token;
  }

  /**
   * Make request with JSON data.
   * @param opts
   */
  public static async requestJson<T>(opts: IRequestOptions) : Promise<Result<T>> {

    let axiosResponse: AxiosResponse | null = null;

    var result: Result<T> | null = null;

    var processQuery = (url: string, data: any): string => {
      if (data) {
        return `${url}?${jsonToUrl(data)}`;
      }
      return url;
    };

    var token = await ServiceBase.getTokenWithRetry();
    
    const headers = { Authorization: `Bearer ${token}` };

    var axiosRequestConfig: AxiosRequestConfig;
    axiosRequestConfig = {
      baseURL: window.Configs.API_ENDPOINT,
      headers: headers
    };

    try {
      switch (opts.method) {
        case "GET":

          axiosResponse = await Axios.get(processQuery(opts.url, opts.data), axiosRequestConfig);
          break;
        case "POST":
          axiosResponse = await Axios.post(opts.url, opts.data, axiosRequestConfig);
          break;
        case "PUT":
          axiosResponse = await Axios.put(opts.url, opts.data, axiosRequestConfig);
          break;
        case "DELETE":
          axiosResponse = await Axios.delete(processQuery(opts.url, opts.data), axiosRequestConfig);
          break;
      }

      if (!axiosResponse)
        throw new Error('No Result');

      result = new Result<T>(axiosResponse.data as T, true, null);

    } catch (error) {

      console.log(error);

      // parse the server's error if one was provided
      if (error.response) {

        // validation error
        if (error.response.status === 400) {
          if (!Array.isArray(error.response.data.error.details))
            result = new Result<T>(null, false,[{ code: '', target: 'method_error', message: 'One or more validation errors have occurred but we were unable to parse them. Please inspect the console for more information.'}]);
          else
            result = new Result<T>(null, false, error.response.data.error.details);
        }
        else if (error.response.status === 404) {
          ServiceBase.dispatchGlobalError("Method not found!");
          result = new Result<T>(null, false,null);
        }
        else {
          let message = '';
          if (error.response.data.error.code) {
            message = (error.response.data.error.code ? error.response.data.error.code : '');
            if (error.response.data.error.message) {
              message += ' - ' + error.response.data.error.message;
            }
          }

          ServiceBase.dispatchGlobalError(message);
          result = new Result<T>(null, false,null);
        }
      }
      else {
        ServiceBase.dispatchGlobalError(error.message);
        result = new Result<T>(null,false, null);
      }
    }

    return result;
  }

  private static dispatchGlobalError(message: string) {
    Cache.setItem(ERROR_STATE, { title: 'Error', description: message});
    Hub.dispatch(
      'ErrorChannel',
      {
        event: 'errorOccurred',
        data: {hasError:true},
        message:''
      });
  }

  /**
   * Allows you to send files to the server.
   * @param opts
   */
  public static async sendFormData<T>(opts: ISendFormDataOptions): Promise<Result<T>> {
    let axiosResponse: AxiosResponse | null = null;

    var result: Result<T> | null = null;
    
    var token = await ServiceBase.getTokenWithRetry();

    const headers = { Authorization: `Bearer ${token}`, 'Content-Type': 'multipart/form-data' };

    var axiosOpts = {
      headers: headers
    };

    try {
      switch (opts.method) {
        case "POST":
          axiosResponse = await Axios.post(opts.url, opts.data, axiosOpts);
          break;
        case "PUT":
          axiosResponse = await Axios.put(opts.url, opts.data, axiosOpts);
          break;
      }

      if (!axiosResponse)
        throw new Error('No Result');

      result = new Result<T>(axiosResponse.data as T, true, null);
    } catch (error) {

      console.log(error);

      // parse the server's error if one was provided
      if (error.response) {

        // validation error
        if (error.response.status === 400) {
          if (!Array.isArray(error.response.data.error.details))
            result = new Result<T>(null, false,[{ code: '', target: 'method_error', message: 'One or more validation errors have occurred but we were unable to parse them. Please inspect the console for more information.'}]);
          else
            result = new Result<T>(null, false, error.response.data.error.details);
        }
        else if (error.response.status === 404) {
          ServiceBase.dispatchGlobalError("Method not found!");
          result = new Result<T>(null, false,null);
        }
        else {
          let message = (error.title ? error.title : error.mesage);
          ServiceBase.dispatchGlobalError(message);
          result = new Result<T>(null, false,null);
        }
      }
      else {
        ServiceBase.dispatchGlobalError(error.message);
        result = new Result<T>(null,false, null);
      }
    }

    return result;
  }
}