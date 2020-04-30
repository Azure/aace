/*eslint no-useless-escape:*/
export const httpURLRegExp  = /^((?:https?\:\/\/)(?:[-a-z0-9]+\.)*[-a-z0-9]+.*)$/;  
export const iPAddressRegExp  = /^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\/([0-9]|1[0-9]|2[0-9]|3[0-2])$/;  
export const emailRegExp = /((([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))[;]*)+/;  
export const aplicationID_AADTenantRegExp = /^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$/;  
export const productIdRegExp = /^[a-z_-][a-z0-9_-]*$/; 
export const deploymentIdRegExp = /^[a-z_-][a-z0-9_-]*$/; 
export const versionIdRegExp = /^[a-z_-][a-z0-9_-]*$/; 
export const workSpaceIdRegExp = /^[a-z_-][a-z0-9_-]*$/; 




