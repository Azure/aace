import * as yup from "yup";
import {ObjectSchema} from "yup";
import {v4 as uuid} from "uuid";
import {ITelemetryDataConnectorModel} from "../../../models";
import {planIdRegExp} from "./RegExp";
import {ErrorMessage} from "./ErrorMessage";

export const getInitialTelemetryDataConnector = (): ITelemetryDataConnectorModel => {
  return {
    clientId: uuid(),
    isNew: true,
    name: '',
    configuration: '',
    type: ''
  }
}

export interface ITelemetryDataConnectorForm {
  telemetryDataConnectors: ITelemetryDataConnectorModel[];
  isDisabled: boolean;
}

const telemetryDataConnectorValidators = yup
  .array<ITelemetryDataConnectorModel>().of(
    yup.object().uniqueProperty('name', "Name must be unique").shape({
      name: yup.mixed().when('isDeleted', {is: (val) => { return !!val === false}, then: yup.string()
          .matches(planIdRegExp,
            {
              message: ErrorMessage.telemetryDataConnectorName,
              excludeEmptyString: true
            }).required('Name is required'),otherwise: yup.mixed().notRequired()}),
      type: yup.mixed().when('isDeleted', {is: (val) => { return !!val === false}, then: yup.string().required('Type is required'),otherwise: yup.mixed().notRequired()}),
      configuration: yup.mixed().when('isDeleted', {is: (val) => { return !!val === false}, then: yup.string().required('Configuration is required'),otherwise: yup.mixed().notRequired()}),
      clientId: yup.string(),
      isDeleted: yup.boolean(),
      isModified: yup.boolean(),
      isNew: yup.boolean(),
      isSaved: yup.boolean()
    })
  );

export const telemetryDataConnectorFormValidationSchema: ObjectSchema<ITelemetryDataConnectorForm> =
  yup.object().shape({
    telemetryDataConnectors: telemetryDataConnectorValidators,
    isDisabled: yup.boolean()
  });