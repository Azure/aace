import * as yup from "yup";
import { ObjectSchema } from "yup";
import { v4 as uuid } from "uuid";
import { IARMTemplateParameterModel } from "../../../models";
import { IARMTemplateModel } from "../../../models/IARMTemplateModel";

export const getInitialARMTemplate = (): IARMTemplateModel => {
  return {
    clientId: uuid(),
    isNew: true,
    templateFilePath: '',
    templateName: '',
    templateContent: ''
  }
}

export interface IARMTemplatesForm {
  templates: IARMTemplateModel[];
  isDisabled: boolean;
}

export interface IARMTemplateParametersForm {
  templateParameters: IARMTemplateParameterModel[];
}

const armTemplatesValidator = yup
  .array<IARMTemplateModel>().of(
    yup.object().uniqueProperty('templateName', "ARM template ID already exists").shape({
      templateName: yup.mixed().when('isDeleted', {is: (val) => { return !!val === false}, then: yup.string().required('ID is required'),otherwise: yup.mixed().notRequired()}),

      templateFilePath: yup.string()
        .test('fileFormat', 'Invalid ARM template file type (must be JSON)', function () {
          let fileExtension = this.parent.templateFileExtension          
          if(!fileExtension)
            return true;

          return ['application/json'].includes(fileExtension);
        }),
      templateContent: yup.mixed().when(['isNew', 'isDeleted'], {
        is: (isNew, isDeleted) => {
          return !!isNew && !!isDeleted === false;
        }, then: yup.string().required('Template file is empty'), otherwise: yup.mixed().notRequired()
      }),
      clientId: yup.string(),
      isDeleted: yup.boolean(),
      isModified: yup.boolean(),
      isNew: yup.boolean(),
      isSaved: yup.boolean()
    })
  );

const armTemplateParametersValidator = yup
  .array<IARMTemplateParameterModel>().of(
    yup.object().shape({
      name: yup.string(),
      type: yup.string(),
      value: yup.string(),
      clientId: yup.string(),
      isDeleted: yup.boolean(),
      isModified: yup.boolean(),
      isNew: yup.boolean(),
      isSaved: yup.boolean(),

    })); // these constraints are shown if and only if inner constraints are satisfied

export const armTemplatesFormValidationSchema: ObjectSchema<IARMTemplatesForm> =
  yup.object().shape({
    templates: armTemplatesValidator,
    isDisabled: yup.boolean()
  });

export const armTemplateParametersFormValidationSchema: ObjectSchema<IARMTemplateParametersForm> =
  yup.object().shape({
    templateParameters: armTemplateParametersValidator
  });