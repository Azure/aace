import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IProductModel } from "../../../models";
import { v4 as uuid } from "uuid";
import {productIdRegExp } from "./RegExp";
import { ErrorMessage } from "./ErrorMessage";
import { IDropdownOption } from "office-ui-fabric-react";

export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );

  export const ProductType: IDropdownOption[] = [
    { key: '', text: "Select" },
    { key: 'realtimeprediction', text: "Real-Time Prediction" },
    { key: 'batchinference', text: "Batch Inference" },
    { key: 'trainyourownmodel', text: "Train Your Own Model" }]

    export const HostType: IDropdownOption[] = [
      { key: '', text: "Select" },
      { key: 'saas', text: "SaaS" },
      { key: 'bringyourowncompute', text: "Bring Your Own Compute" }]

export const initialProductValues: IProductModel = {
  hostType: '',
  owner: '',
  productId: '',
  productType: '', isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
};

export const initialProductList: IProductModel[] = [{
  hostType: 'saas',
  owner: 'v-anirc@microsoft.com',
  productId: '1',
  productType: 'realtimeprediction',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
},
{
  hostType: 'bringyourowncompute',
  owner: 'zbates@affirma.com',
  productId: '2',
  productType: 'batchinference',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
},
{
  hostType: 'saas',
  owner: 'zbates@affirma.com',
  productId: '3',
  productType: 'trainyourownmodel',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
}];

export interface IProductInfoFormValues {
  product: IProductModel;
}

export const initialInfoFormValues: IProductInfoFormValues = {
  product: initialProductValues
}

const productValidator: ObjectSchema<IProductModel> = yup.object().shape(
  {
    clientId: yup.string(),
    productId: yup.string()
    .matches(productIdRegExp,
        {
          message: ErrorMessage.productID,
          excludeEmptyString: true
        }).required("Id is a required field"),

    owner: yup.string().required("Owners is a required field"),
    productType: yup.string()
      .required("Product Type is a required field"),
    hostType: yup.string().required("Host Type is a required field"),
  }
);

export const productInfoValidationSchema: ObjectSchema<IProductInfoFormValues> =
  yup.object().shape({
    product: productValidator
  });

  export const deleteProductValidator: ObjectSchema<IProductModel> = yup.object().shape(
    {
      clientId: yup.string(),
      productId: yup.string(),
      selectedProductId: yup.string()
        .test('selectedProductid', 'Product id does not match', function (value: string) {
          
          const productId: string = this.parent.productId;
          if (!value)
            return true;
  
          return value.toLowerCase() === productId.toLowerCase();
        }).matches(productIdRegExp,
          {
            message: ErrorMessage.productID,
            excludeEmptyString: true
          }).required("Product id is a required field"),
  
      owner: yup.string(),
      hostType:yup.string(),
      productType:yup.string()      
    }
  );