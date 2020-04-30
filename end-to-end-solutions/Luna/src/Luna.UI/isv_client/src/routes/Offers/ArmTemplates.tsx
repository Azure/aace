import React, {useEffect, useState} from 'react';
import {useParams} from "react-router";
import {
  DefaultButton,
  FontIcon,
  Label,
  Link,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Stack,
  TextField
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import {FieldArray, Formik, useFormikContext} from "formik";
import {
  armTemplateParametersFormValidationSchema,
  armTemplatesFormValidationSchema,
  getInitialARMTemplate,
  IARMTemplateParametersForm,
  IARMTemplatesForm,
} from "./formUtils/ArmTemplatesFormUtils";
import {
  arrayItemErrorMessage,
  arrayItemErrorMessageWithoutTouch,
  handleSubmissionErrorsForArray
} from "../../shared/formUtils/utils";
import ArmTemplateService from '../../services/ArmTemplatesService';
import ArmTemplateParameterService from '../../services/ArmTemplateParameterService';
import {IARMTemplateModel} from "../../models/IARMTemplateModel";
import {IARMTemplateParameterModel} from "../../models";
import {Loading} from "../../shared/components/Loading";
import {Offers} from '../../shared/constants/infomessages';
import {useGlobalContext} from "../../shared/components/GlobalProvider";
import {toast} from "react-toastify";


const ArmTemplates: React.FunctionComponent = () => {

  let [ARMTemplates, setARMTemplates] = useState<IARMTemplatesForm>({templates: [], isDisabled: true});
  let [ARMTemplateParameters, setARMTemplateParameters] = useState<IARMTemplateParametersForm>({templateParameters: []});
  const [loadingARMTemplates, setLoadingARMTemplates] = useState<boolean>(false);
  const [formError, setFormError] = useState<string | null>(null);  
  const [loadingARMTemplateParameters, setLoadingARMTemplateParameters] = useState<boolean>(false);


  let fileReader;

  //Below code is for making design proper in Armtemplate page.  
  let body = (document.getElementsByClassName('App')[0] as HTMLElement);

  const {offerName} = useParams();

  useEffect(() => {
    getArmTemplates();
    getArmTemplateParameters();
    return () => {      
      body.style.height = '100%';
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getArmTemplates = async () => {

    setLoadingARMTemplates(true);
    const results = await ArmTemplateService.list(offerName as string);

    if (results && results.value && results.success) {

      setARMTemplates({templates: [...results.value], isDisabled: true});

      if (results.value.length > 4 && ARMTemplateParameters.templateParameters.length === 0)
        body.style.height = 'auto';
    }
    setLoadingARMTemplates(false);
  }

  const getArmTemplateParameters = async () => {

    setLoadingARMTemplateParameters(true);
    const results = await ArmTemplateParameterService.list(offerName as string);
    if (results && results.value && results.success) {

      setARMTemplateParameters({templateParameters: [...results.value]});
      if (results.value.length > 4 && ArmTemplatesForm.length === 0)
        body.style.height = 'auto';
    }
    setLoadingARMTemplateParameters(false);
  }

  const TemplateFileRead = (e, idx, setFieldValue) => {
    const content = fileReader.result;
    setFieldValue(`templates.${idx}.templateContent`, content, true)    
  }

  const uploadfile = (event, idx, setFieldValue) => {
    let file = event.target.files[0];
    if (file) {
      setFieldValue(`templates.${idx}.templateFilePath`, file.name, true)
      setFieldValue(`templates.${idx}.templateFileExtension`, file.type, true)
      if (file.type === "application/json") {
        fileReader = new FileReader();
        fileReader.onloadend = (e) => {
          TemplateFileRead(e, idx, setFieldValue)
        };
        fileReader.readAsText(file);
      }
    } else {
      setFieldValue(`templates.${idx}.templateFilePath`, '', true)
    }
  }

  const DisplayErrors = (errors, values) => {    
    return null;
  };

  const handleAdd = (arrayHelpers, values) => {
    arrayHelpers.insert(values.templates.length, getInitialARMTemplate());
    arrayHelpers.form.setFieldValue(`isDisabled`, false);
  }

  const handleRemove = (arrayHelpers, idx) => {
    arrayHelpers.form.setFieldValue(`templates.${idx}.isDeleted`, true, true);
    arrayHelpers.form.setFieldValue(`isDisabled`, false);

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

  const _arrayItemErrorMessageWithoutTouch = (primary: boolean, globalContext, errors, touched, object, idx, objectName, dirty) => {

    setTimeout(() => {
      if (primary)
        globalContext.setFormDirty(dirty);
      else
        globalContext.setSecondaryFormDirty(dirty);
    }, 1);

    return arrayItemErrorMessageWithoutTouch(errors, touched, object, idx, objectName);
  }

  const ArmTemplatesForm = () => {
    const globalContext = useGlobalContext();

    return (
      <React.Fragment>
        <Formik
          initialValues={ARMTemplates}
          validationSchema={armTemplatesFormValidationSchema}
          validateOnBlur={true}
          onSubmit={async (values, {setSubmitting, setErrors}) => {

            globalContext.showProcessing();
            
            // Now we need to save the parameters

            // First find all of the existing parameters that were deleted and attempt to delete them
            // We don't care about parameters that were created and deleted by the client but never saved
            let parametersToDelete = values.templates.filter(x => x.isDeleted && !!x.isNew === false && !!x.isSaved === false);
            for (let param of parametersToDelete) {

              var paramDeleteResult = await ArmTemplateService.delete(offerName as string, param.templateName);
              var idx = values.templates.findIndex(x => x.clientId === param.clientId);
              if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'templates', idx, paramDeleteResult)) {
                globalContext.hideProcessing();
                return;
              }

              param.isSaved = true;
            }

            // Next find all of the new parameters and attempt to create them
            let parametersToCreate = values.templates.filter(x => x.isNew && !!x.isDeleted === false && !!x.isSaved === false);
            for (let param of parametersToCreate) {
              
              try {
                JSON.parse(param.templateContent as string);
              } catch {
                // TODO - display the errors here
                toast.error('Failed to parse file for template: ' + param.templateName);
                setSubmitting(false);
                return;
              }

              var paramCreateResult = await ArmTemplateService.create(offerName as string, param);
              var _idx = values.templates.findIndex(x => x.clientId === param.clientId);
              if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'templates', _idx, paramCreateResult)) {
                globalContext.hideProcessing();
                return;
              }

              // remove the delete param from the collection
              param.isNew = false;
            }

            globalContext.hideProcessing();
            toast.success("Success!");

            setSubmitting(false);
            getArmTemplates();
            getArmTemplateParameters();

            setTimeout(() => {globalContext.setFormDirty(false);}, 500);

          }}
        >
          <ArmTemplateFormBody formError={formError}/>
        </Formik>
      </React.Fragment>
    );

  }

  type IArmTemplateFormBodyProps = {
    formError?: string | null;
  }

  const ArmTemplateFormBody: React.FunctionComponent<IArmTemplateFormBodyProps> = (props) => {
    const {setFieldValue, values, handleChange, handleBlur, touched, errors, handleSubmit,dirty} = useFormikContext<IARMTemplatesForm>(); // formikProps
    const globalContext = useGlobalContext();
    const {formError} = props;

    return (
      <form style={{width: '100%', textAlign: 'left', marginBottom: 0}} autoComplete={"off"} onSubmit={handleSubmit}>
        <h3 style={{textAlign: 'left', fontWeight: 'normal', marginTop: 0, marginBottom: 20}}>Upload Arm Templates</h3>
        <span className={"offer-details-page-info-header"}>
          {Offers.armTemplates}
        </span>
        {formError && <div style={{marginBottom: 15}}><MessageBar messageBarType={MessageBarType.error}>
            <div dangerouslySetInnerHTML={{__html: formError}} style={{textAlign: 'left'}}></div>
        </MessageBar></div>}
        <table className="noborder offer" style={{marginTop: 31}}>
          <thead>
          <tr>
            <th style={{width: 334}}>
              <FormLabel title={"ID"}/>
            </th>
            <th>
              <FormLabel title={"Arm Template File"}/>
            </th>
            <th>
            </th>
          </tr>
          </thead>
          <DisplayErrors errors={errors} values={values}/>
          <FieldArray
            name="templates"
            render={arrayHelpers => {

              return (
                <React.Fragment>
                  <tbody>
                  {errors && typeof errors.templates === 'string' ? <div>{errors.templates}</div> : null}

                  {values.templates.map((value: IARMTemplateModel, idx) => {

                    if (value.isDeleted)
                      return true;

                    return (
                      <tr key={idx} style={{border: "1px solid rgb(232, 232, 232)"}}>
                        <td className="borderright">
                          {value.isNew ?
                            <TextField
                              name={`templates.${idx}.templateName`}
                              style={{width: '100%'}}
                              value={value.templateName}
                              onChange={handleChange}
                              onBlur={handleBlur}
                              errorMessage={_arrayItemErrorMessage(true, globalContext, errors, touched, 'templates', idx, 'templateName', dirty)}
                              placeholder={'Template Name'}/>

                            :
                            <Label
                              style={{width: 200, margin: 7}}
                            >
                              {value.templateName}
                            </Label>
                          }
                        </td>
                        <td className="armfileupload">
                          <Stack horizontal={false} verticalFill={true} style={{margin: 7, height: 30}}
                                 verticalAlign={"center"}>
                            {value.isNew ?
                              <label className="armtemplatefile">
                                <span className="filetittle"
                                      title={value.templateFilePath}>{value.templateFilePath}</span>
                                <span className="browsebutton">Browse</span>
                                <input type="file" onChange={(event) => {
                                  uploadfile(event, idx, setFieldValue)
                                }} onBlur={handleBlur}
                                       accept="application/JSON" style={{width: 0}} title="Select Template File"
                                       name={`templates.${idx}.templateFilePath`}
                                       id={`templates.${idx}.templateFilePath`}
                                />
                              </label>
                              :
                              <Link href={value.templateFilePath} target={"_blank"}>Click here to view file</Link>
                            }
                            {
                              (arrayItemErrorMessage(errors, touched, 'templates', idx, 'templateFilePath')
                                || arrayItemErrorMessageWithoutTouch(errors, touched, 'templates', idx, 'templateContent'))
                                ?
                                <span className={"errormessage"}>
                                            {[_arrayItemErrorMessage(true, globalContext, errors, touched, 'templates', idx, 'templateFilePath',dirty),
                                              _arrayItemErrorMessageWithoutTouch(true, globalContext, errors, touched, 'templates', idx, 'templateContent',dirty)
                                            ].join(' ')}
                                </span>
                                :
                                null
                            }
                          </Stack>
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
                  })}
                  </tbody>
                  <tfoot>
                  <tr>
                    <td colSpan={3} style={{textAlign: 'left'}}>
                      <Stack style={{marginTop: 20}} horizontal={true} gap={15}>
                        <DefaultButton onClick={() => handleAdd(arrayHelpers, values)} className="addbutton">
                          <FontIcon iconName="Add" className="deleteicon"/> Add Template
                        </DefaultButton>
                        <PrimaryButton type="submit" disabled={values.isDisabled} id="btnupload">Update</PrimaryButton>
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

  const ArmTemplateParametersForm = () => {
    const globalContext = useGlobalContext();
    return (
      <Formik
        initialValues={ARMTemplateParameters}
        validationSchema={armTemplateParametersFormValidationSchema}
        validateOnBlur={true}
        onSubmit={async (values, {setSubmitting, setErrors}) => {
          const input = {...values};
          setSubmitting(true);
          globalContext.showProcessing();

          for (let param of input.templateParameters) {

            var idx = values.templateParameters.findIndex(x => x.clientId === param.clientId);
            let paramUpdateResult = await ArmTemplateParameterService.update(offerName as string, param);
            if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'templateParameters', idx, paramUpdateResult)) {
              globalContext.hideProcessing();
              return;
            }
          }

          setSubmitting(false);
          globalContext.hideProcessing();

          toast.success("Success!");

          getArmTemplateParameters();
          setTimeout(() => {globalContext.setSecondaryFormDirty(false);}, 500);
        }}
      >
        <ArmTemplateParametersFormBody formError={formError}/>
      </Formik>
    );

  }

  type IArmTemplateParametersFormBodyProps = {
    formError?: string | null;
  }

  const ArmTemplateParametersFormBody: React.FunctionComponent<IArmTemplateFormBodyProps> = (props) => {
    const {values, handleChange, handleBlur, touched, errors,handleSubmit, submitForm, dirty} = useFormikContext<IARMTemplateParametersForm>(); // formikProps

    const globalContext = useGlobalContext();
    const {formError} = props;

    useEffect(() => {
      globalContext.modifySaveForm(async () => {
        await submitForm();
      });
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
      <form style={{width: '100%', marginTop: 20, textAlign: 'left', marginBottom: 0}} autoComplete={"off"}
            onSubmit={handleSubmit}>
        <h3 style={{textAlign: 'left', fontWeight: 'normal'}}>Parameters</h3>
        {formError && <div style={{marginBottom: 15}}><MessageBar messageBarType={MessageBarType.error}>
            <div dangerouslySetInnerHTML={{__html: formError}} style={{textAlign: 'left'}}></div>
        </MessageBar></div>}

        <table style={{borderCollapse: 'collapse', width: '100%'}} cellPadding={10}
               className="noborder armparamtable">
          <thead>
          <tr style={{fontWeight: 'normal', borderBottom: '1px solid #e8e8e8'}}>
            <th>
              <FormLabel title={"ID"}/>
            </th>
            <th>
              <FormLabel title={"Type"}/>
            </th>
            <th>
              <FormLabel title={"Value"}/>
            </th>
          </tr>
          </thead>
          {values.templateParameters.length > 0 ?
            <React.Fragment>
              <FieldArray
                name="templateParameters"
                render={arrayHelpers => {                  
                  return (
                    <React.Fragment>
                      <tbody>
                      {errors && typeof errors.templateParameters === 'string' ?
                        <div>{errors.templateParameters}</div> : null}
                      {values.templateParameters.map((value: IARMTemplateParameterModel, idx) => {
                        return (
                          <tr key={idx}>
                            <td>
                              <span>{value.name}</span>
                            </td>
                            <td>
                              <span>{value.type}</span>
                            </td>
                            <td>
                              <TextField
                                name={`templateParameters.${idx}.value`}                                
                                value={value.value}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                errorMessage={_arrayItemErrorMessage(false, globalContext, errors, touched, 'templateParameters', idx, 'value', dirty)}
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

  if (loadingARMTemplates)
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
        <Loading/>
      </Stack>
    );

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
      {loadingARMTemplates ?
        <Loading/>
        :
        <ArmTemplatesForm/>
      }

      {loadingARMTemplateParameters ?
        <Loading/>
        :
        <ArmTemplateParametersForm/>
      }
    </Stack>
  );

}

export default ArmTemplates;