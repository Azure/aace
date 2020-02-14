import React, {useEffect, useState} from 'react';
import {useParams} from "react-router";
import {
  DefaultButton,
  FontIcon,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Stack,
  TextField,
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import {FieldArray, Formik, useFormikContext} from "formik";
import {arrayItemErrorMessage, handleSubmissionErrorsForArray} from "../../shared/formUtils/utils";
import WebHooksService from "../../services/WebHooksService";
import WebHooksParametersService from "../../services/WebHooksParametersService";

import {Offers} from '../../shared/constants/infomessages';
import {
  getInitialWebHook,
  IWebHookForm,
  IWebHookParametersForm,
  webHookParametersFormValidationSchema,
  webHooksFormValidationSchema
} from "./formUtils/WebHooksFormUtils";
import {IWebHookModel, IWebHookParameterModel} from "../../models";
import {useGlobalContext} from "../../shared/components/GlobalProvider";
import {Loading} from "../../shared/components/Loading";
import {toast} from "react-toastify";

const WebHooks: React.FunctionComponent = () => {
  const [formError, setFormError] = useState<string | null>(null);
  const [formState, setFormState] = useState<IWebHookForm>({webhooks: [], isDisabled: true});
  const [webHookParameters, setWebHookParameters] = useState<IWebHookParametersForm>({webhookParameters: []});
  const [loadingWebHooks, setLoadingWebHooks] = useState<boolean>(true);
  const [loadingWebHookParameters, setLoadingWebHookParameters] = useState<boolean>(true);
  const {offerName} = useParams();

  const DisplayErrors = (errors) => {    
    return null;
  };

  useEffect(() => {
    getWebHooks();
    getWebHookParameters();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getWebHooks = async () => {
    setLoadingWebHooks(true);
    const results = await WebHooksService.list(offerName as string);
    if (results && results.value && results.success) {
      setFormState({isDisabled: true, webhooks: results.value});
    }

    setLoadingWebHooks(false);
  }

  const getWebHookParameters = async () => {
    setLoadingWebHookParameters(true);
    const results = await WebHooksParametersService.list(offerName as string);
    if (results && results.value && results.success) {
      setWebHookParameters({webhookParameters: results.value});
    }
    setLoadingWebHookParameters(false);
  }

  const handleAdd = (arrayHelpers, values) => {
    arrayHelpers.insert(values.webhooks.length, getInitialWebHook());
    arrayHelpers.form.setFieldValue(`isDisabled`, false);
  }

  const handleRemove = (arrayHelpers, idx) => {
    arrayHelpers.form.setFieldValue(`webhooks.${idx}.isDeleted`, true, true);
    arrayHelpers.form.setFieldValue(`isDisabled`, false);
  }

  const WebHooksForm = () => {
    const globalContext = useGlobalContext();
    return (
      <React.Fragment>
        <Formik
          initialValues={formState}
          validationSchema={webHooksFormValidationSchema}
          validateOnBlur={true}
          onSubmit={async (values, {setSubmitting, setErrors}) => {

            setSubmitting(true);
            globalContext.showProcessing();

            setFormError(null);

            // Now we need to save the parameters
            // Grab all of our modified entries before we start modifying the state of items during the saving process
            let parametersToUpdate = values.webhooks.filter(x => !!x.isNew === false && !!x.isDeleted === false && !!x.isSaved === false);

            // First find all of the existing parameters that were deleted and attempt to delete them
            // We don't care about parameters that were created and deleted by the client but never saved
            let parametersToDelete = values.webhooks.filter(x => x.isDeleted && !!x.isNew === false && !!x.isSaved === false);
            for (let param of parametersToDelete) {

              var paramDeleteResult = await WebHooksService.delete(offerName as string, param.webhookName);
              var idx = values.webhooks.findIndex(x => x.clientId === param.clientId);
              if (handleSubmissionErrorsForArray (setErrors, setSubmitting, setFormError, 'webhooks', idx, paramDeleteResult)) {
                globalContext.hideProcessing();
                return;
              }

              param.isSaved = true;
            }

            // Next find all of the new parameters and attempt to create them
            let parametersToCreate = values.webhooks.filter(x => x.isNew && !!x.isDeleted === false && !!x.isSaved === false);
            for (let param of parametersToCreate) {

              var paramCreateResult = await WebHooksService.createOrUpdate(offerName as string, param);
              var idx1 = values.webhooks.findIndex(x => x.clientId === param.clientId);
              if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'webhooks', idx1, paramCreateResult)) {
                globalContext.hideProcessing();
                return;
              }

              param.isNew = false;
            }

            // Finally, find all of the existing parameters that are not new or deleted and update them
            for (let param of parametersToUpdate) {

              // find the index of the parameter we are updating
              var paramUpdateResult = await WebHooksService.createOrUpdate(offerName as string, param);
              var idx2 = values.webhooks.findIndex(x => x.clientId === param.clientId);
              if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'webhooks', idx2, paramUpdateResult)) {
                globalContext.hideProcessing();
                return;
              }

              // do not mark the record as saved since the user could potentially change something about it for the next pass
              // if one of the other records had a problem
            }

            setSubmitting(false);
            globalContext.hideProcessing();
            toast.success("Success !");

            getWebHooks();
            getWebHookParameters();

            setTimeout(() => {globalContext.setFormDirty(false);}, 500);

          }}
        >
          <WebHooksFormBody formError={formError}/>
        </Formik>
      </React.Fragment>
    );

  }

  type IWebhooksFormBodyProps = {
    formError?: string | null;
  }


  const _arrayItemErrorMessage = (primary: boolean, globalContext, errors, touched, object, idx, objectName, dirty) => {
    if (primary)
      globalContext.setFormDirty(dirty);
    else
      globalContext.setSecondaryFormDirty(dirty);

    return arrayItemErrorMessage(errors, touched, object, idx, objectName);
  }

  const WebHooksFormBody: React.FunctionComponent<IWebhooksFormBodyProps> = (props) => {
    const {values, handleChange, handleBlur, touched, errors, handleSubmit, dirty} = useFormikContext<IWebHookForm>(); // formikProps
    const globalContext = useGlobalContext();
    const {formError} = props;

    return (
      <form style={{width: '100%', textAlign: 'left', marginBottom: 0}} autoComplete={"off"} onSubmit={handleSubmit}>
        <h3 style={{textAlign: 'left', fontWeight: 'normal', marginTop: 0, marginBottom: 20}}>Webhooks</h3>
        <span className={"offer-details-page-info-header"}>
          {Offers.webHooks}
        </span>
        {formError && <div style={{marginBottom: 15}}><MessageBar messageBarType={MessageBarType.error}>
            <div dangerouslySetInnerHTML={{__html: formError}} style={{textAlign: 'left'}}></div>
        </MessageBar></div>}
        <table className="noborder offer" style={{marginTop: 31}}>
          <thead>
          <tr>
            <th>
              <FormLabel title={"ID"}/>
            </th>
            <th>
              <FormLabel title={"URL"}/>
            </th>
            <th>
            </th>
          </tr>
          </thead>
          <DisplayErrors errors={errors} values={values}/>
          <FieldArray
            name="webhooks"
            render={arrayHelpers => {

              return (
                <React.Fragment>
                  <tbody>
                  {errors && typeof errors.webhooks === 'string' ? <div>{errors.webhooks}</div> : null}

                  {values.webhooks.map((value: IWebHookModel, idx) => {

                    if (value.isDeleted)
                    {
                      return true;
                    }
                    else{
                      return (
                        <tr key={idx} style={{border: "1px solid rgb(232, 232, 232)"}}>
                          <td>
                            <TextField
                              name={`webhooks.${idx}.webhookName`}
                              value={value.webhookName}
                              onBlur={handleBlur}
                              onChange={handleChange}
                              errorMessage={_arrayItemErrorMessage(true, globalContext, errors, touched, 'webhooks', idx, 'webhookName',dirty)}
                              placeholder={'Name'}/>
                          </td>
                          <td>
                            <TextField
                              name={`webhooks.${idx}.webhookUrl`}
                              value={value.webhookUrl}
                              onBlur={handleBlur}
                              onChange={handleChange}
                              errorMessage={_arrayItemErrorMessage(true, globalContext, errors, touched, 'webhooks', idx, 'webhookUrl',dirty)}
                              placeholder={'Url'}/>
                          </td>
                          <td style={{borderRight: '1px solid rgb(232, 232, 232)', verticalAlign: 'middle'}}>
                            <Stack horizontal={true} horizontalAlign={"center"} verticalFill={true}
                                  style={{margin: 7, height: 30}} verticalAlign={"center"}>
                              <FontIcon iconName="Cancel" className="deleteicon"
                                        onClick={() => handleRemove(arrayHelpers, idx)}/>
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
                        <DefaultButton onClick={() => handleAdd(arrayHelpers, values)} className="addbutton">
                          <FontIcon iconName="Add" className="deleteicon"/> Add Webhook
                        </DefaultButton>
                        <PrimaryButton type="submit" id="btnupload">Update</PrimaryButton>
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

  const WebHookParametersForm = () => {
    const globalContext = useGlobalContext();
    return (
      <Formik
        initialValues={webHookParameters}
        validationSchema={webHookParametersFormValidationSchema}
        validateOnBlur={true}
        onSubmit={async(values, {setSubmitting, setErrors}) => {

          setSubmitting(true);
          globalContext.showProcessing();

          for (let param of values.webhookParameters) {

            var idx = values.webhookParameters.findIndex(x => x.clientId === param.clientId);
            let paramUpdateResult = await WebHooksParametersService.update(offerName as string, param);
            if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'webhookParameters', idx, paramUpdateResult)) {
              globalContext.hideProcessing();
              return;
            }
          }

          globalContext.hideProcessing();
          setSubmitting(false);
          toast.success("Success!");

          getWebHookParameters();
          setTimeout(() => {globalContext.setFormDirty(false);}, 500);

        }}
      >
        <WebHookParametersFormBody formError={formError}/>
      </Formik>
    );

  }

  type IWebHookParametersFormBodyProps = {
    formError?: string | null;
  }

  const WebHookParametersFormBody: React.FunctionComponent<IWebHookParametersFormBodyProps> = (props) => {
    const {values, handleChange, handleBlur, touched, errors,handleSubmit, submitForm, dirty} = useFormikContext<IWebHookParametersForm>(); // formikProps

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
        <h3 style={{textAlign: 'left', fontWeight: 'normal'}}>Parameters</h3>
        {formError && <div style={{marginBottom: 15}}><MessageBar messageBarType={MessageBarType.error}>
            <div dangerouslySetInnerHTML={{__html: formError}} style={{textAlign: 'left'}}></div>
        </MessageBar></div>}

        <table style={{borderCollapse: 'collapse', width: '100%'}} cellPadding={10}
               className="noborder armparamtable">
          <thead>
          <tr style={{fontWeight: 'normal', borderBottom: '1px solid #e8e8e8'}}>
            <th>
              <FormLabel title={"Name"}/>
            </th>
            <th>
              <FormLabel title={"Value"}/>
            </th>
          </tr>
          </thead>
          {values.webhookParameters.length > 0 ?
            <React.Fragment>
              <FieldArray
                name="webhookParameters"
                render={arrayHelpers => {                  
                  return (
                    <React.Fragment>
                      <tbody>
                      {errors && typeof errors.webhookParameters === 'string' ?
                        <div>{errors.webhookParameters}</div> : null}
                      {values.webhookParameters.map((value: IWebHookParameterModel, idx) => {
                        return (
                          <tr key={idx}>
                            <td>
                              <span>{value.name}</span>
                            </td>
                            <td>
                              <TextField
                                name={`webhookParameters.${idx}.value`}                                
                                value={value.value}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                errorMessage={_arrayItemErrorMessage(false, globalContext, errors, touched, 'webhookParameters', idx, 'value', dirty)}
                                placeholder={''}/>
                            </td>
                          </tr>
                        );
                      })}
                      </tbody>
                    </React.Fragment>
                  );
                }}
              />
            </React.Fragment>
            :
            <tbody>
            <tr>
              <td colSpan={4}>
                No Data Found
              </td>
            </tr>
            </tbody>
          }
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
      {loadingWebHooks ?
        <Loading/>
        :
        <WebHooksForm/>
      }

      {loadingWebHookParameters ?
        <Loading/>
        :
        <WebHookParametersForm/>
      }
    </Stack>
  );


}

export default WebHooks
