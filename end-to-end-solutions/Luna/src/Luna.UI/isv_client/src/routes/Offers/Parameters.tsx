import React, { useEffect, useState } from 'react';
import {
  Checkbox,
  FontIcon,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Stack,
  TextField,
  IDropdownOption,
  Dropdown, Label,
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { RouteComponentProps, useParams } from "react-router";
import { FieldArray, Formik, useFormikContext } from "formik";
import {
  getInitialOfferParameter,
  initialParametersFormValues,
  IOfferParametersFormValues,
  offerParametersValidationSchema
} from "./formUtils/offerFormUtils";
import { arrayItemErrorMessage, handleSubmissionErrorsForArray } from "../../shared/formUtils/utils";
import { IOfferParameterModel } from "../../models/IOfferParameterModel";
import { Loading } from '../../shared/components/Loading';
import OfferParameterService from "../../services/OfferParameterService";
import { Offers } from '../../shared/constants/infomessages';
import { toast } from "react-toastify";
import { useGlobalContext } from "../../shared/components/GlobalProvider";

type OfferProps =
  RouteComponentProps<{ offerName: string }>

const Parameters: React.FunctionComponent<OfferProps> = (props) => {

  const { offerName } = useParams();
  const globalContext = useGlobalContext();

  const getFormData = async (offerName: string) => {

    setLoadingFormData(true);
    const offerParametersResponse = await OfferParameterService.list(offerName);

    // Global errors should have already been handled for get requests by this point
    if (offerParametersResponse.value && offerParametersResponse.success) {
      var offerParameters = offerParametersResponse.value as IOfferParameterModel[];

      setFormState(
        {
          offerParameters: [...offerParameters]
        });
    }
    setLoadingFormData(false);

  }

  const [formState, setFormState] = useState<IOfferParametersFormValues>(initialParametersFormValues);
  const [formError, setFormError] = useState<string | null>(null);
  const [loadingFormData, setLoadingFormData] = useState<boolean>(false);

  useEffect(() => {
    if (offerName) {
      getFormData(offerName);
    }
// eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);


  if (loadingFormData)
    return (
      <Stack
        horizontalAlign="center"
        verticalAlign="center"
        verticalFill
        styles={{
          root: {
            width: '100%'
          }
        }}
      >
        <Loading />
      </Stack>
    );

  return (
    <Stack
      horizontalAlign="start"
      verticalAlign="start"
      verticalFill
      styles={{
        root: {
          margin: 31
        }
      }}
      gap={15}
    >
      <span className={"offer-details-page-info-header"}>
        {Offers.offer.Parameters}
      </span>
      <Formik
        initialValues={formState}        
        validationSchema={offerParametersValidationSchema}
        validateOnBlur={true}
        onSubmit={async (values, { setSubmitting, setErrors }) => {

          globalContext.showProcessing();
          
          setFormError(null);

          // Now we need to save the parameters
          // Grab all of our modified entries before we start modifying the state of items during the saving process
          let parametersToUpdate = values.offerParameters.filter(x => !!x.isNew === false && !!x.isDeleted === false && !!x.isSaved === false);

          // Next find all of the new parameters and attempt to create them
          let parametersToCreate = values.offerParameters.filter(x => x.isNew && !!x.isDeleted === false && !!x.isSaved === false);

          // First find all of the existing parameters that were deleted and attempt to delete them
          // We don't care about parameters that were created and deleted by the client but never saved
          let parametersToDelete = values.offerParameters.filter(x => x.isDeleted && !!x.isNew === false && !!x.isSaved === false);
          for (let param of parametersToDelete) {

            var paramDeleteResult = await OfferParameterService.delete(offerName as string, param.parameterName);
            var idx = values.offerParameters.findIndex(x => x.clientId === param.clientId);
            if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'offerParameters', idx, paramDeleteResult)) {
              globalContext.hideProcessing();
              return;
            }

            param.isSaved = true;
          }

          
          for (let param of parametersToCreate) {

            if (param.maximum)
              param.maximum = parseInt(param.maximum.toString());

            if (param.minimum)
              param.minimum = parseInt(param.minimum.toString());

             var paramCreateResult = await OfferParameterService.create(offerName as string, param);
             var _idx = values.offerParameters.findIndex(x => x.clientId === param.clientId);
             if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'offerParameters', _idx, paramCreateResult)) {
               globalContext.hideProcessing();
               return;
             }

            param.isNew = false;            
          }

          // Finally, find all of the existing parameters that are not new or deleted and update them
          for (let param of parametersToUpdate) {

            if (param.maximum)
              param.maximum = parseInt(param.maximum.toString());

            if (param.minimum)
              param.minimum = parseInt(param.minimum.toString());

            // // find the index of the parameter we are updating
             var paramUpdateResult = await OfferParameterService.update(offerName as string, param);
             var idx1 = values.offerParameters.findIndex(x => x.clientId === param.clientId);
             if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'offerParameters', idx1, paramUpdateResult)) {
               globalContext.hideProcessing();
               return;
             }

            // do not mark the record as saved since the user could potentially change something about it for the next pass
            // if one of the other records had a problem            
          }

          globalContext.hideProcessing();
          toast.success("Success!");
          setSubmitting(false);

          getFormData(offerName as string);
          setTimeout(() => {globalContext.setFormDirty(false);}, 500);

        }}
      >
        <ParametersForm formError={formError} />
      </Formik>
    </Stack>
  );
};

export type IOfferParametersFormProps = {
  formError?: string | null;
}
export const ParametersForm: React.FunctionComponent<IOfferParametersFormProps> = (props) => {
  const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty } = useFormikContext<IOfferParametersFormValues>(); // formikProps

  const globalContext = useGlobalContext();
  const { formError } = props;

  const ValueType: IDropdownOption[] = [
    { key: 'string', text: "String" },
    { key: 'number', text: "Number" },
    { key: 'datetime', text: "DateTime" },
    { key: 'boolean', text: "Boolean" }]

  useEffect(() => {
    globalContext.modifySaveForm(async () => {
      await submitForm();
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const selectOnChange = (fieldKey: string, arrayHelpers: any, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {    
    if (option) {
      arrayHelpers.form.setFieldValue(fieldKey + '.valueType', option.key, false);
      if (option.key === 'string') {
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablemaximum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisableminimum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablevalueList', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablefromList', false, false);

        arrayHelpers.form.setFieldValue(fieldKey + '.maximum', 0, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.minimum', 0, false);
      }
      else if (option.key === 'number') {
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablemaximum', false, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisableminimum', false, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablevalueList', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablefromList', false, false);

        arrayHelpers.form.setFieldValue(fieldKey + '.fromList', false, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.valueList', '', false);
      }
      else if (option.key === 'datetime') {
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablemaximum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisableminimum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablevalueList', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablefromList', true, false);

        arrayHelpers.form.setFieldValue(fieldKey + '.maximum', 0, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.minimum', 0, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.valueList', '', false);
        arrayHelpers.form.setFieldValue(fieldKey + '.fromList', false, false);
      }
      else if (option.key === 'boolean') {
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablemaximum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisableminimum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablevalueList', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablefromList', true, false);

        arrayHelpers.form.setFieldValue(fieldKey + '.maximum', 0, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.minimum', 0, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.valueList', '', false);
        arrayHelpers.form.setFieldValue(fieldKey + '.fromList', false, false);
      }
    }
  };

  const oncheckedEvent = (fieldKey: string, arrayHelpers: any, checked: boolean, value: IOfferParameterModel) => {
    arrayHelpers.form.setFieldValue(fieldKey + `.fromList`, checked, false);

    if (value.valueType === 'string') {
      if (checked) {
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablemaximum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisableminimum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablevalueList', false, false);
      }
      else {
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablemaximum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisableminimum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablevalueList', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.valueList', '', false);
      }

    }
    else if (value.valueType === 'number') {
      if (checked) {
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablemaximum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisableminimum', true, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablevalueList', false, false);

        arrayHelpers.form.setFieldValue(fieldKey + '.maximum', '', false);
        arrayHelpers.form.setFieldValue(fieldKey + '.minimum', '', false);
      }
      else {
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablemaximum', false, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisableminimum', false, false);
        arrayHelpers.form.setFieldValue(fieldKey + '.isdisablevalueList', true, false);

        arrayHelpers.form.setFieldValue(fieldKey + '.valueList', '', false);
      }

    }
  }

  const setDirty = (errors, touched, offerParameters, idx, parameterName ,dirty) => {
    setTimeout(() => { globalContext.setFormDirty(dirty) }, 1);
    return arrayItemErrorMessage(errors, touched, offerParameters, idx, parameterName);
  }

  let parameters: JSX.Element;

  if (values.offerParameters.filter(x => !x.isDeleted).length === 0) {
    parameters = (
      <FieldArray
        name="offerParameters"
        render={arrayHelpers => {
          return (
            <React.Fragment>
              <span className={"offer-details-page-info-header"}>No Parameters</span>
              <Stack horizontal={true} verticalAlign={"center"} horizontalAlign={"start"}>
                <PrimaryButton onClick={() => arrayHelpers.insert(values.offerParameters.length, getInitialOfferParameter())} className="addbutton"><FontIcon iconName="Add" className="deleteicon" /> Add </PrimaryButton>
              </Stack>
            </React.Fragment>
          );
        }}
      />
    );
  }
  else {
    parameters = (
      <FieldArray
        name="offerParameters"
        render={arrayHelpers => {          
          return (
            <React.Fragment>
              {errors && typeof errors.offerParameters === 'string' ? <div>{errors.offerParameters}</div> : null}
              <table className="noborder offer">
                <thead>
                  <tr>
                    <th>
                      <FormLabel title={"Parameter ID"} />
                    </th>
                    <th>
                      <FormLabel title={"Display Name"} />
                    </th>
                    <th>
                      <FormLabel title={"Description"} />
                    </th>
                    <th>
                      <FormLabel title={"Value Type"} />
                    </th>
                    <th>
                      <FormLabel title={"From List"} />
                    </th>
                    <th>
                      <FormLabel title={"Value List"} />
                    </th>
                    <th>
                      <FormLabel title={"Max."} />
                    </th>
                    <th>
                      <FormLabel title={"Min."} />
                    </th>
                    <th>

                    </th>
                  </tr>
                </thead>
                <tbody>
                  {values.offerParameters.map((value: IOfferParameterModel, idx) => {

                    if (value.isDeleted)
                      return true;

                    return (
                      <tr key={idx}>
                        <td>
                          {value.isNew ? <TextField
                              name={`offerParameters.${idx}.parameterName`}
                              value={value.parameterName}
                              onChange={handleChange}
                              maxLength={50}
                              onBlur={handleBlur}
                              errorMessage={setDirty(errors, touched, 'offerParameters', idx, 'parameterName', dirty)}
                              placeholder={'Id'} /> :
                            <Label
                              style={{width: 200, marginLeft:10}}
                            >
                              {value.parameterName}
                            </Label>}
                        </td>
                        <td>
                          <TextField
                            name={`offerParameters.${idx}.displayName`}
                            value={value.displayName}
                            onChange={handleChange}
                            onBlur={handleBlur}
                            errorMessage={setDirty(errors, touched, 'offerParameters', idx, 'displayName', dirty)}
                            placeholder={'DisplayName'} />
                        </td>
                        <td>
                          <TextField
                            name={`offerParameters.${idx}.description`}
                            value={value.description}
                            onChange={handleChange}
                            onBlur={handleBlur}
                            errorMessage={setDirty(errors, touched, 'offerParameters', idx, 'description', dirty)}
                            placeholder={'Description'} />
                        </td>
                        <td style={{ verticalAlign: 'middle' }}>
                          <Dropdown
                            options={ValueType}
                            id={`offerParameters.${idx}.valueType`} onBlur={handleBlur}
                            onChange={(event, option, index) => {
                              selectOnChange(`offerParameters.${idx}`, arrayHelpers, event, option, index)
                            }}
                            errorMessage={setDirty(errors, touched, 'offerParameters', idx, 'valueType', dirty)}
                            defaultSelectedKey={value.valueType}
                          />
                        </td>
                        <td style={{ verticalAlign: 'middle' }} className="noborder">
                          <Stack style={{ width: 92 }} verticalAlign={"center"} horizontalAlign={"center"}>
                            {value.isdisablefromList ?
                              <React.Fragment>
                                <div className="ms-Checkbox is-disabled checkbox root-163 disablecheckbox">
                                  <input type="checkbox" className="input-130" name={`offerParameters.${idx}.fromList`} id="checkbox-20" aria-disabled="true" disabled style={{ visibility: "hidden" }} />
                                  <label className="ms-Checkbox-label label-164">
                                    <div className="ms-Checkbox-checkbox">
                                      <i data-icon-name="CheckMark" role="presentation" aria-hidden="true" className="ms-Checkbox-checkmark checkmark"></i>
                                    </div>
                                  </label>
                                </div>
                              </React.Fragment>
                              :
                              <Checkbox
                                name={`offerParameters.${idx}.fromList`}
                                defaultChecked={value.fromList}
                                onChange={(ev, checked) => { oncheckedEvent(`offerParameters.${idx}`, arrayHelpers, checked as boolean, value) }}
                                onBlur={handleBlur} className="checkbox" disabled={value.isdisablefromList} />}

                          </Stack>
                        </td>
                        <td>
                          <TextField
                            name={`offerParameters.${idx}.valueList`}
                            value={value.valueList}
                            onChange={handleChange}
                            onBlur={handleBlur}
                            errorMessage={setDirty(errors, touched, 'offerParameters', idx, 'valueList', dirty)}
                            placeholder={'ValueList'}
                            disabled={value.isdisablevalueList}
                          />
                        </td>
                        <td>
                          <TextField
                            name={`offerParameters.${idx}.maximum`}
                            style={{ width: 67 }}
                            value={value.maximum ? value.maximum.toString() : undefined}
                            onChange={handleChange}
                            onBlur={handleBlur}
                            errorMessage={setDirty(errors, touched, 'offerParameters', idx, 'maximum', dirty)}
                            placeholder={'Maximum'}
                            disabled={value.isdisablemaximum} />

                        </td>
                        <td>
                          <TextField
                            name={`offerParameters.${idx}.minimum`}
                            style={{ width: 67 }}
                            value={value.minimum ? value.minimum.toString() : undefined}
                            onChange={handleChange}
                            onBlur={handleBlur}
                            errorMessage={setDirty(errors, touched, 'offerParameters', idx, 'minimum', dirty)}
                            placeholder={'Minimum'}
                            disabled={value.isdisableminimum} />
                        </td>
                        <td style={{ verticalAlign: 'middle', borderRight: '1px solid #efefef' }}>
                          <FontIcon iconName="Cancel" className="deleteicon" onClick={() => { arrayHelpers.form.setFieldValue(`offerParameters.${idx}.isDeleted`, true, true) }} />
                        </td>

                      </tr>
                    );
                  })}
                </tbody>
                <tfoot>
                  <tr>
                    <td colSpan={10}>
                      <Stack horizontal={true} verticalAlign={"center"} horizontalAlign={"start"} style={{ marginTop: '2%' }}>
                        <PrimaryButton onClick={() => arrayHelpers.insert(values.offerParameters.length, getInitialOfferParameter())} className="addbutton"><FontIcon iconName="Add" className="deleteicon" /> Add </PrimaryButton>
                      </Stack>
                    </td>
                  </tr>
                </tfoot>
              </table>
            </React.Fragment>
          );
        }}
      />
    );
  }

  const DisplayErrors = (errors) => {    
    return null;
  };

  return (
    <form style={{ width: '100%', marginTop: 20 }} autoComplete={"off"} onSubmit={handleSubmit}>
      {formError && <div style={{ marginBottom: 15 }}><MessageBar messageBarType={MessageBarType.error}>
        <div dangerouslySetInnerHTML={{ __html: formError }} style={{ textAlign: 'left' }}></div>
      </MessageBar></div>}
      <DisplayErrors errors={errors} />
      {parameters}
    </form>
  );
}

export default Parameters;