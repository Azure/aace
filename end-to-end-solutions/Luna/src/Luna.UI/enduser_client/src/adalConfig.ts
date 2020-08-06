// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
import {AdalConfig, adalGetToken, AuthenticationContext} from 'react-adal';
// Endpoint URL
export const endpoint = window.Configs.AAD_ENDPOINT as string;
// App Registration ID
const appId = window.Configs.AAD_APPID as string;
export const adalConfig: AdalConfig = {
  cacheLocation: 'localStorage',
  clientId: appId,
  endpoints: {
    api:endpoint
  },
  postLogoutRedirectUri: window.location.origin
};

class AdalContext {
  private authContext: AuthenticationContext;

  constructor() {
    this.authContext = new AuthenticationContext(adalConfig);
  }
  get AuthContext() {
    return this.authContext;
  }

  public GetToken(): Promise<string | null> {
    return adalGetToken(this.authContext, endpoint);
  }
  public LogOut() {
    this.authContext.logOut();
  }
  

  public async GetApiToken() {
    
    var adalContext = this.authContext;
    this.authContext.acquireToken(appId, function (errorDesc, token, error) {
      if (error){
        console.log(errorDesc);
        adalContext.acquireTokenRedirect(appId, null, null);
    }});
    
    var token = adalContext.getCachedToken(appId);
    console.log(token);
    return token;
  }
}

const adalContext: AdalContext = new AdalContext();


export const getToken = () => adalContext.GetApiToken();
export default adalContext;