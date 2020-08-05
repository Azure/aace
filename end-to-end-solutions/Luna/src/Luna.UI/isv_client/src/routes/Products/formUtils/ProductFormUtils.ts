import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IProductModel } from "../../../models";
import { v4 as uuid } from "uuid";
import { productNameRegExp } from "./RegExp";
import { ErrorMessage } from "./ErrorMessage";

export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );
/*
export const ProductType: IDropdownOption[] = [
  { key: '', text: "Select" },
  { key: 'RTP', text: "Real-Time Prediction" },
  { key: 'BI', text: "Batch Inference" },
  { key: 'TYOM', text: "Train Your Own Model" }]

export const HostType: IDropdownOption[] = [
  { key: '', text: "Select" },
  { key: 'SAAS', text: "SaaS" },
  { key: 'BYOC', text: "Bring Your Own Compute" }]
*/
export const initialProductValues: IProductModel = {
  hostType: '',
  owner: '',
  productName: '',
  productType: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
};

export const initialProductList: IProductModel[] = [{
  hostType: 'saas',
  owner: 'v-anirc@microsoft.com',
  productName: '1',
  productType: 'realtimeprediction',
  createdTime: '',
  lastUpdatedTime: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
},
{
  hostType: 'bringyourowncompute',
  owner: 'zbates@affirma.com',
  productName: '2',
  productType: 'batchinference',
  createdTime: '',
  lastUpdatedTime: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
},
{
  hostType: 'saas',
  owner: 'zbates@affirma.com',
  productName: '3',
  productType: 'trainyourownmodel',
  createdTime: '',
  lastUpdatedTime: '',
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
    productName: yup.string()
      .matches(productNameRegExp,
        {
          message: ErrorMessage.productName,
          excludeEmptyString: true
        }).required("Id is a required field"),

    owner: yup.string().required("Owners is a required field"),
    productType: yup.string()
      .required("Product Type is a required field"),
    hostType: yup.string().required("Host Type is a required field"),
    createdTime: yup.string(),
    lastUpdatedTime: yup.string()
  }
);

export const productInfoValidationSchema: ObjectSchema<IProductInfoFormValues> =
  yup.object().shape({
    product: productValidator
  });

export const deleteProductValidator: ObjectSchema<IProductModel> = yup.object().shape(
  {
    clientId: yup.string(),
    productName: yup.string(),
    selectedProductId: yup.string()
      .test('selectedProductid', 'Product name does not match', function (value: string) {
        const productName: string = this.parent.productName;
        if (!value)
          return true;

        return value.toLowerCase() === productName.toLowerCase();
      }).matches(productNameRegExp,
        {
          message: ErrorMessage.productName,
          excludeEmptyString: true
        }).required("Product id is a required field"),

    owner: yup.string(),
    hostType: yup.string(),
    productType: yup.string(),
    createdTime: yup.string(),
    lastUpdatedTime: yup.string()
  }
);