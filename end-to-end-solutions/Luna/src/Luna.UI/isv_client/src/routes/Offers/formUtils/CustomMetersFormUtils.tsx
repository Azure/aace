import * as yup from "yup";
import {ObjectSchema} from "yup";
import {v4 as uuid} from "uuid";
import {ICustomMeterModel} from "../../../models";
import {planIdRegExp} from "./RegExp";
import {ErrorMessage} from "./ErrorMessage";

export const getInitialCustomMeter = (): ICustomMeterModel => {
  return {
    clientId: uuid(),
    isNew: true,
    meterName: '',
    offerName: '',
    telemetryDataConnectorName: '',
    telemetryQuery: ''
  }
}

export interface ICustomMeterForm {
  customMeters: ICustomMeterModel[];
  isDisabled: boolean;
}

const CustomMeterValidators = yup
  .array<ICustomMeterModel>().of(
    yup.object().uniqueProperty('meterName', "Name must be unique").shape({
      meterName: yup.mixed().when('isDeleted', {is: (val) => { return !!val === false}, then: yup.string()
          .matches(planIdRegExp,
            {
              message: ErrorMessage.customMeterName,
              excludeEmptyString: true
            }).required('Name is required'),otherwise: yup.mixed().notRequired()}),
      offerName: yup.string(),
      telemetryDataConnectorName: yup.mixed().when('isDeleted', {is: (val) => { return !!val === false}, then: yup.string().required('Telemetry Data Connector is required'),otherwise: yup.mixed().notRequired()}),
      telemetryQuery: yup.mixed().when('isDeleted', {is: (val) => { return !!val === false}, then: yup.string().required('Query is required'),otherwise: yup.mixed().notRequired()}),
      clientId: yup.string(),
      isDeleted: yup.boolean(),
      isModified: yup.boolean(),
      isNew: yup.boolean(),
      isSaved: yup.boolean()
    })
  );

export const CustomMeterFormValidationSchema: ObjectSchema<ICustomMeterForm> =
  yup.object().shape({
    customMeters: CustomMeterValidators,
    isDisabled: yup.boolean()
  });