import React, {useEffect, useState} from 'react';
import {useParams} from "react-router";
import {
  DefaultButton,
  Dropdown,
  FontIcon,
  IDropdownOption,
  Label,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Stack,
  TextField,
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import {FieldArray, Formik, useFormikContext} from "formik";
import {arrayItemErrorMessage, handleSubmissionErrorsForArray} from "../../shared/formUtils/utils";

import {Offers} from '../../shared/constants/infomessages';
import {ICustomMeterModel, ITelemetryDataConnectorModel} from "../../models";
import {useGlobalContext} from "../../shared/components/GlobalProvider";
import {Loading} from "../../shared/components/Loading";
import {toast} from "react-toastify";
import {
  getInitialTelemetryDataConnector,
  ITelemetryDataConnectorForm,
  telemetryDataConnectorFormValidationSchema
} from "./formUtils/TelemetryDataConnectorsFormUtils";
import {
  CustomMeterFormValidationSchema,
  getInitialCustomMeter,
  ICustomMeterForm
} from "./formUtils/CustomMetersFormUtils";
import TelemetryDataConnectorService from "../../services/TelemetryDataConnectorService";
import CustomMetersService from "../../services/MetersService";

const Meters: React.FunctionComponent = () => {
  const [formError, setFormError] = useState<string | null>(null);
  const [telemetryDataConnectorState, setTelemetryDataConnectorState] = useState<ITelemetryDataConnectorForm>({telemetryDataConnectors: [], isDisabled: true});
  const [telemetryDataConnectorTypeDropdownOptions, setTelemetryDataConnectorTypeDropdownOptions] = useState<IDropdownOption[]>([]);
  const [customMeterTypeDropdownOptions, setCustomMeterTypeDropdownOptions] = useState<IDropdownOption[]>([]);
  const [customMeters, setCustomMeters] = useState<ICustomMeterForm>({customMeters: [], isDisabled: true});
  const [loadingTelemetryDataConnectors, setLoadingTelemetryDataConnectors] = useState<boolean>(true);
  const [loadingCustomMeters, setLoadingCustomMeters] = useState<boolean>(true);
  const {offerName} = useParams();

  const DisplayErrors = (errors, values) => {
    console.log('display errors:');
    console.log(errors);
    console.log(values);
    return null;
  };

  useEffect(() => {

    if (offerName) {
      getTelemetryDataConnectors();
      getCustomMeters()
    }

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getTelemetryDataConnectors = async () => {

    setLoadingTelemetryDataConnectors(true);

    const [
      dataConnectorTypesResponse,
      telemetryDataConnectorsResponse
    ] = await Promise.all([
      TelemetryDataConnectorService.getTypes(),
      TelemetryDataConnectorService.list()
    ]);

    var dataConnectorTypes: string[] = [];
    var telemetryDataConnectors: ITelemetryDataConnectorModel[] = [];

    if (dataConnectorTypesResponse.success && telemetryDataConnectorsResponse.success) {

      if (dataConnectorTypesResponse.value)
        dataConnectorTypes = dataConnectorTypesResponse.value as string[];

      let typeOptions: IDropdownOption[] = [];
      dataConnectorTypes.forEach(element => {
        typeOptions.push({ key: element, text: element });
      });

      if (telemetryDataConnectorsResponse.value)
        telemetryDataConnectors = telemetryDataConnectorsResponse.value as ITelemetryDataConnectorModel[];

      let customMeterOptions: IDropdownOption[] = [];
      telemetryDataConnectors.forEach(element => {
        customMeterOptions.push({ key: element.name, text: element.name });
      });

      setTelemetryDataConnectorTypeDropdownOptions([...typeOptions]);
      setCustomMeterTypeDropdownOptions([...customMeterOptions]);
      setTelemetryDataConnectorState({telemetryDataConnectors: [...telemetryDataConnectors], isDisabled: true});

    }

    setLoadingTelemetryDataConnectors(false);
  }

  const getCustomMeters = async () => {
    setLoadingCustomMeters(true);
    const results = await CustomMetersService.list(offerName as string);
    if (results && results.value && results.success) {
      setCustomMeters({customMeters: [...results.value], isDisabled: true});
    }
    setLoadingCustomMeters(false);
  }

  const handleAddTelemetryDataConnector = (arrayHelpers, values) => {
    //zb: set the initial value of the type to the first available type we encounter
    let initialValue = getInitialTelemetryDataConnector();
    if (telemetryDataConnectorTypeDropdownOptions.length > 0)
      initialValue.type = telemetryDataConnectorTypeDropdownOptions[0].text;

    arrayHelpers.insert(values.telemetryDataConnectors.length, initialValue);
    arrayHelpers.form.setFieldValue(`isDisabled`, false);
  }

  const handleRemoveTelemetryDataConnector = (arrayHelpers, idx) => {
    arrayHelpers.form.setFieldValue(`telemetryDataConnectors.${idx}.isDeleted`, true, true);
    arrayHelpers.form.setFieldValue(`isDisabled`, false);
  }

  const handleAddCustomMeter = (arrayHelpers, values) => {
    arrayHelpers.insert(values.customMeters.length, getInitialCustomMeter());
    arrayHelpers.form.setFieldValue(`isDisabled`, false);
  }

  const handleRemoveCustomMeter = (arrayHelpers, idx) => {
    arrayHelpers.form.setFieldValue(`customMeters.${idx}.isDeleted`, true, true);
    arrayHelpers.form.setFieldValue(`isDisabled`, false);
  }

  const TelemetryDataConnectorsForm = () => {
    const globalContext = useGlobalContext();
    return (
      <React.Fragment>
        <Formik
          initialValues={telemetryDataConnectorState}
          validationSchema={telemetryDataConnectorFormValidationSchema}
          validateOnBlur={true}
          onSubmit={async (values, {setSubmitting, setErrors}) => {

            setSubmitting(true);
            globalContext.showProcessing();

            setFormError(null);

            // Now we need to save the parameters
            // Grab all of our modified entries before we start modifying the state of items during the saving process
            let parametersToUpdate = values.telemetryDataConnectors.filter(x => !!x.isNew === false && !!x.isDeleted === false && !!x.isSaved === false);

            // First find all of the existing parameters that were deleted and attempt to delete them
            // We don't care about parameters that were created and deleted by the client but never saved
            let parametersToDelete = values.telemetryDataConnectors.filter(x => x.isDeleted && !!x.isNew === false && !!x.isSaved === false);
            for (let param of parametersToDelete) {

              var paramDeleteResult = await TelemetryDataConnectorService.delete(param.name);
              var idx = values.telemetryDataConnectors.findIndex(x => x.clientId === param.clientId);
              if (handleSubmissionErrorsForArray (setErrors, setSubmitting, setFormError, 'telemetryDataConnectors', idx, paramDeleteResult)) {
                globalContext.hideProcessing();
                return;
              }

              param.isSaved = true;
            }

            // Next find all of the new parameters and attempt to create them
            let parametersToCreate = values.telemetryDataConnectors.filter(x => x.isNew && !!x.isDeleted === false && !!x.isSaved === false);
            for (let param of parametersToCreate) {

              var paramCreateResult = await TelemetryDataConnectorService.createOrUpdate(param);
              var idx1 = values.telemetryDataConnectors.findIndex(x => x.clientId === param.clientId);
              if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'telemetryDataConnectors', idx1, paramCreateResult)) {
                globalContext.hideProcessing();
                return;
              }

              param.isNew = false;
            }

            // Finally, find all of the existing parameters that are not new or deleted and update them
            for (let param of parametersToUpdate) {

              // find the index of the parameter we are updating
              var paramUpdateResult = await TelemetryDataConnectorService.createOrUpdate(param);
              var idx2 = values.telemetryDataConnectors.findIndex(x => x.clientId === param.clientId);
              if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'telemetryDataConnectors', idx2, paramUpdateResult)) {
                globalContext.hideProcessing();
                return;
              }

              // do not mark the record as saved since the user could potentially change something about it for the next pass
              // if one of the other records had a problem
            }

            setSubmitting(false);
            globalContext.hideProcessing();
            toast.success("Success!");
            getTelemetryDataConnectors();
            getCustomMeters();

            setTimeout(() => {globalContext.setFormDirty(false);}, 500);

          }}
        >
          <TelemetryDataConnectorsFormBody formError={formError}/>
        </Formik>
      </React.Fragment>
    );

  }

  type ITelemetryDataConnectorsFormBodyProps = {
    formError?: string | null;
  }

  const _arrayItemErrorMessage = (primary: boolean, globalContext, errors, touched, object, idx, objectName, dirty) => {

    setTimeout(() => {
      if (primary)
        globalContext.setFormDirty(dirty);
      else
        globalContext.setSecondaryFormDirty(dirty);
    }, 1);

    return arrayItemErrorMessage(errors, touched, object, idx, objectName);
  }

  const selectOnChange = (fieldKey: string, fieldName: string, arrayHelpers: any, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    if (option) {
      arrayHelpers.form.setFieldValue(fieldKey + `.${fieldName}`, option.key, false);
    }
  };

  const headerStyles = {
    fontSize: 18.72,
    fontWeight:400,
    color:'#615f5d'
  }

  const TelemetryDataConnectorsFormBody: React.FunctionComponent<ITelemetryDataConnectorsFormBodyProps> = (props) => {
    const {values, handleChange, handleBlur, touched, errors, handleSubmit, dirty} = useFormikContext<ITelemetryDataConnectorForm>(); // formikProps
    const globalContext = useGlobalContext();
    const {formError} = props;

    return (
      <form style={{width: '100%', textAlign: 'left', marginBottom: 0}} autoComplete={"off"} onSubmit={handleSubmit}>
        <span className={"offer-details-page-info-header"}>
          {Offers.meters}
        </span>
        <FormLabel title={"Telemetry Data Connectors"} toolTip={"TODO: replace this text"} style={{marginTop:20, ...headerStyles}}/>
        {formError && <div style={{marginBottom: 15}}><MessageBar messageBarType={MessageBarType.error}>
            <div dangerouslySetInnerHTML={{__html: formError}} style={{textAlign: 'left'}}></div>
        </MessageBar></div>}
        <table className="noborder offer" style={{marginTop: 31}}>
          <thead>
          <tr>
            <th style={{width: 200}}>
              <FormLabel title={"Name"}/>
            </th>
            <th style={{width: 200}}>
              <FormLabel title={"Type"}/>
            </th>
            <th>
              <FormLabel title={"Configuration"}/>
            </th>
            <th style={{width: 50}}>
            </th>
          </tr>
          </thead>
          <DisplayErrors errors={errors} values={values}/>
          <FieldArray
            name="telemetryDataConnectors"
            render={arrayHelpers => {

              return (
                <React.Fragment>
                  <tbody>
                  {errors && typeof errors.telemetryDataConnectors === 'string' ? <div>{errors.telemetryDataConnectors}</div> : null}

                  {values.telemetryDataConnectors.map((value: ITelemetryDataConnectorModel, idx) => {

                    if (value.isDeleted)
                    {
                      return true;
                    }
                    else{
                      return (
                        <tr key={idx} style={{border: "1px solid rgb(232, 232, 232)"}}>
                          <td>
                            {value.isNew ? <TextField
                                name={`telemetryDataConnectors.${idx}.name`}
                                value={value.name}
                                onBlur={handleBlur}
                                onChange={handleChange}
                                errorMessage={_arrayItemErrorMessage(true, globalContext, errors, touched, 'telemetryDataConnectors', idx, 'name', dirty)}
                                placeholder={'Name'}/> :
                              <Label
                                style={{width: 200, marginLeft:10}}
                              >
                                {value.name}
                              </Label>}
                          </td>
                          <td style={{ verticalAlign: 'middle' }}>
                            <Dropdown
                              options={telemetryDataConnectorTypeDropdownOptions}
                              id={`telemetryDataConnectors.${idx}.type`} onBlur={handleBlur}
                              onChange={(event, option, index) => {
                                selectOnChange(`telemetryDataConnectors.${idx}`, 'type', arrayHelpers, event, option, index)
                              }}
                              errorMessage={_arrayItemErrorMessage(true, globalContext, errors, touched, 'telemetryDataConnectors', idx, 'type', dirty)}
                              defaultSelectedKey={value.type}
                            />
                          </td>
                          <td>
                            <TextField
                              name={`telemetryDataConnectors.${idx}.configuration`}
                              value={value.configuration}
                              onBlur={handleBlur}
                              onChange={handleChange}
                              multiline={true}
                              errorMessage={_arrayItemErrorMessage(true, globalContext, errors, touched, 'telemetryDataConnectors', idx, 'configuration',dirty)}
                              placeholder={'Configuration'}/>
                          </td>
                          <td style={{borderRight: '1px solid rgb(232, 232, 232)', verticalAlign: 'middle'}}>
                            <Stack horizontal={true} horizontalAlign={"center"} verticalFill={true}
                                  style={{margin: 7, height: 30}} verticalAlign={"center"}>
                              <FontIcon iconName="Cancel" className="deleteicon"
                                        onClick={() => handleRemoveTelemetryDataConnector(arrayHelpers, idx)}/>
                            </Stack>
                          </td>
                        </tr>
                      );
                    }
                  })}
                  </tbody>
                  <tfoot>
                  <tr>
                    <td colSpan={3} style={{textAlign: 'left'}}>
                      <Stack style={{marginTop: 20}} horizontal={true} gap={15}>
                        <DefaultButton onClick={() => handleAddTelemetryDataConnector(arrayHelpers, values)} className="addbutton">
                          <FontIcon iconName="Add" className="deleteicon"/> Add
                        </DefaultButton>
                        <PrimaryButton type="submit" id="btnSaveTelemetryDataConnectors">Save</PrimaryButton>
                      </Stack>
                    </td>
                  </tr>
                  </tfoot>
                </React.Fragment>
              );
            }}
          />
        </table>

      </form>
    );
  }

  const CustomMetersForm = () => {
    const globalContext = useGlobalContext();
    return (
      <Formik
        initialValues={customMeters}
        validationSchema={CustomMeterFormValidationSchema}
        validateOnBlur={true}
        onSubmit={async(values, {setSubmitting, setErrors}) => {

          setSubmitting(true);
          globalContext.showProcessing();

          setFormError(null);

          // Now we need to save the parameters
          // Grab all of our modified entries before we start modifying the state of items during the saving process
          let parametersToUpdate = values.customMeters.filter(x => !!x.isNew === false && !!x.isDeleted === false && !!x.isSaved === false);

          // First find all of the existing parameters that were deleted and attempt to delete them
          // We don't care about parameters that were created and deleted by the client but never saved
          let parametersToDelete = values.customMeters.filter(x => x.isDeleted && !!x.isNew === false && !!x.isSaved === false);
          for (let param of parametersToDelete) {

            var paramDeleteResult = await CustomMetersService.delete(offerName as string, param.meterName);
            var idx = values.customMeters.findIndex(x => x.clientId === param.clientId);
            if (handleSubmissionErrorsForArray (setErrors, setSubmitting, setFormError, 'customMeters', idx, paramDeleteResult)) {
              globalContext.hideProcessing();
              return;
            }

            param.isSaved = true;
          }

          // Next find all of the new parameters and attempt to create them
          let parametersToCreate = values.customMeters.filter(x => x.isNew && !!x.isDeleted === false && !!x.isSaved === false);
          for (let param of parametersToCreate) {
            param.offerName = offerName as string;
            var paramCreateResult = await CustomMetersService.createOrUpdate(offerName as string, param);
            var idx1 = values.customMeters.findIndex(x => x.clientId === param.clientId);
            if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'customMeters', idx1, paramCreateResult)) {
              globalContext.hideProcessing();
              return;
            }

            param.isNew = false;
          }

          // Finally, find all of the existing parameters that are not new or deleted and update them
          for (let param of parametersToUpdate) {
            param.offerName = offerName as string;
            // find the index of the parameter we are updating
            var paramUpdateResult = await CustomMetersService.createOrUpdate(offerName as string, param);
            var idx2 = values.customMeters.findIndex(x => x.clientId === param.clientId);
            if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'customMeters', idx2, paramUpdateResult)) {
              globalContext.hideProcessing();
              return;
            }

            // do not mark the record as saved since the user could potentially change something about it for the next pass
            // if one of the other records had a problem
          }

          setSubmitting(false);
          globalContext.hideProcessing();
          toast.success("Success!");

          getCustomMeters();
          setTimeout(() => {globalContext.setSecondaryFormDirty(false);}, 500);

        }}
      >
        <CustomMetersFormBody formError={formError}/>
      </Formik>
    );

  }

  type ICustomMetersFormBodyProps = {
    formError?: string | null;
  }

  const CustomMetersFormBody: React.FunctionComponent<ICustomMetersFormBodyProps> = (props) => {
    const {values, handleChange, handleBlur, touched, errors,handleSubmit, submitForm, dirty} = useFormikContext<ICustomMeterForm>(); // formikProps

    const globalContext = useGlobalContext();
    const {formError} = props;

    useEffect(() => {
      globalContext.modifySaveForm(async () => {
        await submitForm();
      });
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
      <form style={{width: '100%', marginTop: 20, textAlign: 'left', marginBottom: 0}} autoComplete={"off"} onSubmit={handleSubmit}>
        <FormLabel title={"Custom Meters"} toolTip={"TODO: replace this text"} style={headerStyles}/>
        {formError && <div style={{marginBottom: 15}}><MessageBar messageBarType={MessageBarType.error}>
            <div dangerouslySetInnerHTML={{__html: formError}} style={{textAlign: 'left'}}></div>
        </MessageBar></div>}

        <table style={{borderCollapse: 'collapse', width: '100%', marginTop:31}}
               className="noborder offer">
          <thead>
          <tr style={{fontWeight: 'normal', borderBottom: '1px solid #e8e8e8'}}>
            <th style={{width: 200}}>
              <FormLabel title={"Name"}/>
            </th>
            <th style={{width: 200}}>
              <FormLabel title={"Telemetry Data Connector"}/>
            </th>
            <th>
              <FormLabel title={"Query"}/>
            </th>
            <th style={{width: 50}}></th>
          </tr>
          </thead>
          <DisplayErrors errors={errors} values={values}/>
          <FieldArray
            name="customMeters"
            render={arrayHelpers => {

              return (
                <React.Fragment>
                  <tbody>
                  {errors && typeof errors.customMeters === 'string' ? <div>{errors.customMeters}</div> : null}

                  {values.customMeters.map((value: ICustomMeterModel, idx) => {

                    if (value.isDeleted)
                    {
                      return true;
                    }
                    else{
                      return (
                        <tr key={idx} style={{border: "1px solid rgb(232, 232, 232)"}}>
                          <td>
                            {value.isNew ? <TextField
                                name={`customMeters.${idx}.meterName`}
                                value={value.meterName}
                                onBlur={handleBlur}
                                onChange={handleChange}
                                errorMessage={_arrayItemErrorMessage(false, globalContext, errors, touched, 'customMeters', idx, 'meterName', dirty)}
                                placeholder={'Name'}/> :
                              <Label
                                style={{width: 200, marginLeft:10}}
                              >
                                {value.meterName}
                              </Label>}
                          </td>
                          <td style={{ verticalAlign: 'middle' }}>
                            <Dropdown
                              options={customMeterTypeDropdownOptions}
                              id={`customMeters.${idx}.telemetryDataConnectorName`} onBlur={handleBlur}
                              onChange={(event, option, index) => {
                                selectOnChange(`customMeters.${idx}`, 'telemetryDataConnectorName', arrayHelpers, event, option, index)
                              }}
                              errorMessage={_arrayItemErrorMessage(false, globalContext, errors, touched, 'customMeters', idx, 'telemetryDataConnectorName', dirty)}
                              defaultSelectedKey={value.telemetryDataConnectorName}
                            />
                          </td>
                          <td>
                            <TextField
                              name={`customMeters.${idx}.telemetryQuery`}
                              value={value.telemetryQuery}
                              multiline={true}
                              onBlur={handleBlur}
                              onChange={handleChange}
                              errorMessage={_arrayItemErrorMessage(false, globalContext, errors, touched, 'customMeters', idx, 'telemetryQuery', dirty)}
                              placeholder={'Query'}/>
                          </td>
                          <td style={{borderRight: '1px solid rgb(232, 232, 232)', verticalAlign: 'middle'}}>
                            <Stack horizontal={true} horizontalAlign={"center"} verticalFill={true}
                                   style={{margin: 7, height: 30}} verticalAlign={"center"}>
                              <FontIcon iconName="Cancel" className="deleteicon"
                                        onClick={() => handleRemoveCustomMeter(arrayHelpers, idx)}/>
                            </Stack>
                          </td>
                        </tr>
                      );
                    }
                  })}
                  </tbody>
                  <tfoot>
                  <tr>
                    <td colSpan={3} style={{textAlign: 'left'}}>
                      <Stack style={{marginTop: 20}} horizontal={true} gap={15}>
                        <DefaultButton onClick={() => handleAddCustomMeter(arrayHelpers, values)} className="addbutton">
                          <FontIcon iconName="Add" className="deleteicon"/> Add
                        </DefaultButton>
                        <PrimaryButton type="submit" id="btnSaveCustomMeters">Save</PrimaryButton>
                      </Stack>
                    </td>
                  </tr>
                  </tfoot>
                </React.Fragment>
              );
            }}
          />
        </table>
      </form>
    );
  }

  return (
    <Stack
      horizontalAlign="center"
      verticalAlign="start"
      verticalFill
      styles={{
        root: {
          width: '90%',
          margin: '0 auto',
          textAlign: 'center',
          color: '#605e5c'
        }
      }}
      gap={15}
    >
      {loadingTelemetryDataConnectors ?
        <Loading/>
        :
        <TelemetryDataConnectorsForm/>
      }

      {loadingCustomMeters ?
        <Loading/>
        :
        <CustomMetersForm/>
      }
    </Stack>
  );

}

export default Meters;