import * as yup from "yup";
import {ObjectSchema} from "yup";
import {iPAddressRegExp} from "./RegExp";
import {ErrorMessage} from "./ErrorMessage";
import {IIpBlockModel, IIpConfigModel} from "../../../models";
import {v4 as uuid} from "uuid";

export const getInitialIpConfig = (): IIpConfigModel => {
  return {
    name: '',
    ipBlocks: [],
    enhancedIpBlocks: [],
    iPsPerSub: 1,
    isNew: true,
    clientId: uuid(),
    ipRangeDialogVisible:false    
  }
};

export const getInitialIpBlock = (): IIpBlockModel => {
  return {
    value: '',
    isNew: true,
    clientId: uuid()
  }
};

export const initialIpConfigs: IIpConfigModel[] = [];

export interface IIpConfigFormValues {
  ipConfigs: IIpConfigModel[];
}

export const initialIpConfigFormValues: IIpConfigFormValues = {
  ipConfigs: initialIpConfigs
}

const _ipConfigValidationSchema = yup
  .array<IIpConfigModel>().of(
    yup.object().uniqueProperty('name', 'no duplicate names').shape({
      clientId: yup.string(), // these constraints take precedence      ,
      isDeleted: yup.boolean(),
      isModified: yup.boolean(), // these constraints take precedence      ,
      isNew: yup.boolean(), // these constraints take precedence      ,
      isSaved: yup.boolean(), // these constraints take precedence      ,

      name: yup.string()
        .required('Name is required'), // these constraints take precedence

        ipBlocks: yup.array<string>(),

        enhancedIpBlocks:yup.array<IIpBlockModel>().of(
          yup.object().shape({
            clientId: yup.string(),
            isDeleted: yup.boolean(),
            isModified: yup.boolean(),
            isNew: yup.boolean(),
            isSaved: yup.boolean(),
            value: yup.mixed().when('isDeleted', {is: (val) => { return !!val === false}, then: yup.string()
                .matches(iPAddressRegExp,
                  {
                    message: ErrorMessage.IpAddress,
                    excludeEmptyString: true
                  })
                .required('Ip is Required'),otherwise: yup.mixed().notRequired()})
          })
        ).min(1, 'At least one IpBlock is required'),
        iPsPerSub: yup.number().test('is-valid-ipsPerSub',
        "Invalid Ips per Sub",
        value => (value === 1 || value === 2 || value === 4 || value === 8
        || value === 16 || value === 32 || value === 64 || value === 128 || value === 256)
      )
    })
  );

export const ipConfigValidationSchema: ObjectSchema<IIpConfigFormValues> =
  yup.object().shape({
    ipConfigs: _ipConfigValidationSchema
  });