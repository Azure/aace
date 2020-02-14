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
import {
  arrayItemErrorMessage,
  arrayItemErrorMessageWithoutTouch,
  ChildarrayItemErrorMessage,
  handleSubmissionErrorsForArray
} from "../../shared/formUtils/utils";
import {IIpBlockModel, IIpConfigModel} from "../../models";
import {Offers} from '../../shared/constants/infomessages';
import {
  getInitialIpBlock,
  getInitialIpConfig,
  IIpConfigFormValues,
  initialIpConfigFormValues,
  ipConfigValidationSchema
} from "./formUtils/IpConfigFormUtil";
import IpConfigService from "../../services/IpConfigService";
import {toast} from "react-toastify";
import {useGlobalContext} from "../../shared/components/GlobalProvider";
import {DialogBox} from '../../shared/components/Dialog';
import {Loading} from "../../shared/components/Loading";

const IpConfigs: React.FunctionComponent = () => {

  const [formState, setFormState] = useState<IIpConfigFormValues>(initialIpConfigFormValues);
  const [formError, setFormError] = useState<string | null>(null);
  const [loadingFormData, setLoadingFormData] = useState<boolean>(false);
  const globalContext = useGlobalContext();

  const {offerName} = useParams();

  const getFormData = async (offerName: string) => {

    setLoadingFormData(true);
    const response = await IpConfigService.list(offerName);

    // Global errors should have already been handled for get requests by this point
    if (response.value && response.success) {
      var ipConfigs = response.value as IIpConfigModel[];

      setFormState(
        {
          ipConfigs: [...ipConfigs]
        });
    }
    setLoadingFormData(false);

  }


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
      horizontalAlign="center"
      verticalAlign="start"
      verticalFill
      styles={{
        root: {
          width: '90%',
          margin: '0 auto',
          color: '#605e5c',
          textAlign: 'center'
        }
      }}
      gap={15}
    >      
      <span className={"offer-details-page-info-header"} style={{width: '100%'}}>
                {Offers.ipAddress}
            </span>
      <Formik
        initialValues={formState}
        validationSchema={ipConfigValidationSchema}
        validateOnBlur={true}
        onSubmit={async (values, {setSubmitting, setErrors}) => {
          
          globalContext.showProcessing();
          setFormError(null);

          // Update all of our ipconfigs and set the ipBlocks for all of the ipConfigs that aren't 'deleted'
          let configsToUpdate = values.ipConfigs.filter(x => !!x.isDeleted === false && !!x.isSaved === false);
          for (let ip of configsToUpdate) {
            ip.ipBlocks = [];            
            for (let eb of ip.enhancedIpBlocks) {
              ip.ipBlocks.push(eb.value);
            }
          }

          // Now we need to save the parameters
          // Grab all of our modified entries before we start modifying the state of items during the saving process
          let parametersToUpdate = values.ipConfigs.filter(x => !!x.isNew === false && !!x.isDeleted === false && !!x.isSaved === false);

          // First find all of the existing parameters that were deleted and attempt to delete them
          // We don't care about parameters that were created and deleted by the client but never saved
          let parametersToDelete = values.ipConfigs.filter(x => x.isDeleted && !!x.isNew === false && !!x.isSaved === false);
          for (let param of parametersToDelete) {

            var paramDeleteResult = await IpConfigService.delete(offerName as string, param.name);
            var idx = values.ipConfigs.findIndex(x => x.clientId === param.clientId);
            if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'ipConfigs', idx, paramDeleteResult)) {
              globalContext.hideProcessing();
              return;
            }

            param.isSaved = true;
          }

          // Next find all of the new parameters and attempt to create them
          let parametersToCreate = values.ipConfigs.filter(x => x.isNew && !!x.isDeleted === false && !!x.isSaved === false);
          for (let param of parametersToCreate) {

            var paramCreateResult = await IpConfigService.createOrUpdate(offerName as string, param);
            var idx1 = values.ipConfigs.findIndex(x => x.clientId === param.clientId);
            if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'ipConfigs', idx1, paramCreateResult)) {
              globalContext.hideProcessing();
              return;
            }

            param.isNew = false;
          }

          // Finally, find all of the existing parameters that are not new or deleted and update them
          for (let param of parametersToUpdate) {

            // find the index of the parameter we are updating
            var paramUpdateResult = await IpConfigService.createOrUpdate(offerName as string, param);
            var idx2 = values.ipConfigs.findIndex(x => x.clientId === param.clientId);
            if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'ipConfigs', idx2, paramUpdateResult)) {
              globalContext.hideProcessing();
              return;
            }

            // do not mark the record as saved since the user could potentially change something about it for the next pass
            // if one of the other records had a problem
          }

          setSubmitting(false);
          globalContext.hideProcessing();

          toast.success("Success!");

          getFormData(offerName as string);
          setTimeout(() => {globalContext.setFormDirty(false);}, 500);

        }}
      >
        <IpConfigsForm formError={formError}/>
      </Formik>
    </Stack>
  );
};

export type IIpConfigsFormProps = {
  formError?: string | null;
}
export const IpConfigsForm: React.FunctionComponent<IIpConfigsFormProps> = (props) => {
  const {values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty} = useFormikContext<IIpConfigFormValues>(); // formikProps

  const globalContext = useGlobalContext();
  const {formError} = props;

  const handleAdd = (arrayHelpers) => {
    arrayHelpers.insert(arrayHelpers.form.values.ipConfigs ? arrayHelpers.form.values.ipConfigs.length : 0, getInitialIpConfig());
  };

  const handleIpAdd = (IparrayHelpers, ipblocks) => {
    IparrayHelpers.insert(ipblocks ? ipblocks.length : 0, getInitialIpBlock());
  };

  let iPsPerSubItems: IDropdownOption[] = [
    {key: 1, text: "1"},
    {key: 2, text: "2"},
    {key: 4, text: "4"},
    {key: 8, text: "8"},
    {key: 16, text: "16"},
    {key: 32, text: "32"},
    {key: 64, text: "64"},
    {key: 128, text: "128"},
    {key: 256, text: "256"}
  ];

  const selectOnChange = (fieldKey: string, arrayHelpers: any, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {

    if (option)
      arrayHelpers.form.setFieldValue(fieldKey, option.key, true);
  };

  const hideIpModal = async(arrayHelpers, idx) => {
    // detect if there are any errors in the ip addresses
    await arrayHelpers.form.validateForm();

    let errors = arrayHelpers.form.errors;
    if (errors
      && errors.ipConfigs
      && errors.ipConfigs[idx]
      && errors.ipConfigs[idx].enhancedIpBlocks
      && typeof errors.ipConfigs[idx].enhancedIpBlocks !== 'string') {
      toast.error('Please fix the ip block errors.')
    }
    else
      arrayHelpers.form.setFieldValue(`ipConfigs.${idx}.ipRangeDialogVisible`, false, false);

  }

  const openIpRangeDialog = (arrayHelpers, idx) => {

    arrayHelpers.form.setFieldValue(`ipConfigs.${idx}.ipRangeDialogVisible`, true, false);

  }

  const getEnhancedIpBlockValues = (enhancedIpBlocks: IIpBlockModel[]) => {
    let arr: string[] = [];

    // eslint-disable-next-line array-callback-return
    enhancedIpBlocks.map(val => {
      
      if (!val.isDeleted)                  
        arr.push(val.value);        
    });
    return arr;
  }

  useEffect(() => {
    globalContext.modifySaveForm(async () => {
      await submitForm();
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const _arrayItemErrorMessageWithoutTouch = (errors, touched, ipConfigs, idx, iPsPerSub, dirty) => {
    globalContext.setFormDirty(dirty);
    return arrayItemErrorMessageWithoutTouch(errors,touched,ipConfigs,idx,iPsPerSub)
  }

  const _ChildarrayItemErrorMessage = (errors, touched, ipConfigs, idx, enhancedIpBlocks, ipx, value, dirty) => {

    globalContext.setFormDirty(dirty);
    return ChildarrayItemErrorMessage(errors,touched,ipConfigs,idx,enhancedIpBlocks,ipx,value)
  }

  const _arrayItemErrorMessage = (errors, touched, ipconfigs, idx, name, dirty) => {
      globalContext.setFormDirty(dirty);
    return arrayItemErrorMessage(errors, touched, ipconfigs, idx, name);
  }

  let parameters: JSX.Element;

  if (values.ipConfigs.filter(x => !x.isDeleted).length === 0) {
    parameters = (
      <FieldArray
        name="ipConfigs"
        render={arrayHelpers => {
          return (
            <React.Fragment>
              <span className={"offer-details-page-info-header"}>No IP Addresses</span>
              <Stack horizontal={true} verticalAlign={"center"} horizontalAlign={"start"}>
                <PrimaryButton onClick={() => arrayHelpers.insert(values.ipConfigs.length, getInitialIpConfig())}
                               className="addbutton"><FontIcon iconName="Add" className="deleteicon"/> Add
                </PrimaryButton>
              </Stack>
            </React.Fragment>
          );
        }}
      />
    );
  } else {
    parameters = (
      <FieldArray
        name="ipConfigs"
        render={arrayHelpers => {
          
          return (
            <React.Fragment>
              {errors && typeof errors.ipConfigs === 'string' ? <div>{errors.ipConfigs}</div> : null}
              <table className="noborder offer" style={{borderCollapse: 'collapse'}} cellPadding={10}>
                <thead>
                <tr style={{fontWeight: 'normal', borderBottom: '1px solid #e8e8e8'}}>
                  <th>
                    <FormLabel title={"ID"}/>
                  </th>
                  <th>
                    <FormLabel title={"Ip Blocks"}/>
                  </th>
                  <th>
                    <FormLabel title={"# of ip per subscription"}/>
                  </th>
                  <th>

                  </th>
                </tr>
                </thead>
                <tbody>
                {values.ipConfigs.map((value: IIpConfigModel, idx) => {

                  if (value.isDeleted)
                    return value;

                  return (
                    <tr key={idx}>
                      <td>
                        {value.isNew ? <TextField
                            name={`ipConfigs.${idx}.name`}
                            value={value.name}
                            onChange={handleChange}
                            onBlur={handleBlur}
                            errorMessage={_arrayItemErrorMessage(errors, touched, 'ipConfigs', idx, 'name', dirty)}
                            placeholder={'Name'}/> :
                          <Label
                            style={{width: 200}}
                          >
                            {value.name}
                          </Label>}

                      </td>
                      <td style={{width:392}}>
                        <React.Fragment>
                          <Stack horizontal={true} verticalAlign={"center"}>
                            <span style={{flexGrow:1}}>
                              {
                                (arrayItemErrorMessageWithoutTouch(errors, touched, 'ipConfigs', idx, 'enhancedIpBlocks')
                                  && typeof arrayItemErrorMessageWithoutTouch(errors, touched, 'ipConfigs', idx, 'enhancedIpBlocks') === 'string'
                                  ?
                                  <span className={"errormessage"}>
                                    {arrayItemErrorMessageWithoutTouch(errors, touched, 'ipConfigs', idx, 'enhancedIpBlocks')}
                                  </span>
                                  :
                                  value.enhancedIpBlocks.length > 0 ? getEnhancedIpBlockValues(value.enhancedIpBlocks).join(', ') : ""
                                )
                              }
                            </span>
                            <div onClick={() => {
                              openIpRangeDialog(arrayHelpers, idx);
                            }} style={{width:175, cursor: 'pointer', color: 'rgb(0, 120, 212)', textAlign: 'right'}}>
                              <FontIcon iconName="Add" className="deleteicon" style={{fontSize: 13, marginRight: '4%'}}/>
                              <span>New IP range</span>
                            </div>
                          </Stack>
                          {
                            value.ipRangeDialogVisible ?
                              <DialogBox keyindex={idx + 'modal'} dialogVisible={true}
                                         title="Add Ip Range" subText="Provide the ip range in IPv4 CIDR format"
                                         className="ipaddressmodal" cancelButtonText="Cancel" submitButtonText="Add"
                                         isDarkOverlay= {false} maxwidth = {100}
                                         cancelonClick={() => {
                                           hideIpModal(arrayHelpers, idx)
                                         }}
                                         submitonClick={() => {
                                           hideIpModal(arrayHelpers, idx)
                                         }}
                                         children={
                                           <FieldArray key={idx} name={`ipConfigs.${idx}.enhancedIpBlocks`} render=
                                             {IparrayHelpers => {

                                               return (
                                                 <React.Fragment key={idx}>
                                                   {
                                                     value.enhancedIpBlocks.map((ipvalue: IIpBlockModel, ipx: number) => {

                                                       if (ipvalue.isDeleted)
                                                         return ipvalue;

                                                       if (!ipvalue.isNew) {
                                                        return (
                                                          <div key={ipx + 'read-only-span'}>{ipvalue.value}</div>
                                                        );
                                                       }
                                                       else {

                                                         return (
                                                           <React.Fragment key={ipx + 'ipbox'}>
                                                             <TextField key={ipx + 'ipcontrol'}
                                                                        className="ipinput"
                                                                        name={`ipConfigs.${idx}.enhancedIpBlocks.${ipx}.value`}
                                                                        style={{width: 150,}}
                                                                        value={ipvalue.value}
                                                                        onChange={handleChange}
                                                                        onBlur={handleBlur}
                                                                        errorMessage={_ChildarrayItemErrorMessage(errors, touched, `ipConfigs`, idx, 'enhancedIpBlocks', ipx, 'value', dirty)}
                                                                        placeholder={'Ip'}/>
                                                             <FontIcon key={ipx + 'delete-ip'} iconName="Cancel"
                                                                       className="deleteicon ipdelete"
                                                                       onClick={() =>
                                                                         IparrayHelpers.form.setFieldValue(`ipConfigs.${idx}.enhancedIpBlocks.${ipx}.isDeleted`, true, true)
                                                                       }/>
                                                           </React.Fragment>
                                                         )
                                                       }
                                                     })
                                                   }
                                                   <div onClick={() => handleIpAdd(IparrayHelpers, value.enhancedIpBlocks)}
                                                        style={{
                                                          cursor: 'pointer',
                                                          color: 'rgb(0, 120, 212)',
                                                          textAlign: 'left',
                                                          marginTop: '20%',
                                                          width: '25%'
                                                        }}>
                                                     <FontIcon iconName="Add" className="deleteicon"
                                                               style={{fontSize: 13, marginRight: '4%'}} key={idx}/>
                                                     <span>Add</span>
                                                   </div>
                                                 </React.Fragment>
                                               )
                                             }}/>
                                         }/>
                              : null
                          }
                        </React.Fragment>
                      </td>
                      <td>
                        {value.isNew ?
                          <Dropdown
                            options={iPsPerSubItems}
                            id={`ipConfigs.${idx}.iPsPerSub`} onBlur={handleBlur}
                            onChange={(event, option, index) => {
                              selectOnChange(`ipConfigs.${idx}.iPsPerSub`, arrayHelpers, event, option, index)
                            }}
                            errorMessage={_arrayItemErrorMessageWithoutTouch(errors, touched, 'ipConfigs', idx, 'iPsPerSub', dirty)}
                            defaultSelectedKey={value.iPsPerSub}
                          />
                          :
                          <Label
                            style={{width: 200}}
                          >
                            {value.iPsPerSub}
                          </Label>
                        }

                      </td>
                      <td>                        
                        <FontIcon iconName="Cancel" className="deleteicon"
                                  onClick={() => arrayHelpers.form.setFieldValue(`ipConfigs.${idx}.isDeleted`, true, true)}/>
                      </td>
                    </tr>
                  );
                })}
                </tbody>
                <tfoot>
                <tr>
                  <td colSpan={4} style={{textAlign: 'left'}}>
                    <DefaultButton onClick={() => handleAdd(arrayHelpers)} className="addbutton"><FontIcon
                      iconName="Add" className="deleteicon"/> Add </DefaultButton>
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

  const DisplayErrors = (props) => {    
    return null;
  };

  return (
    <form style={{width: '100%', marginTop: 20}} autoComplete={"off"} onSubmit={handleSubmit}>
      {formError && <div style={{marginBottom: 15}}><MessageBar messageBarType={MessageBarType.error}>
          <div dangerouslySetInnerHTML={{__html: formError}} style={{textAlign: 'left'}}></div>
      </MessageBar></div>}
      <DisplayErrors errors={errors} values={values}/>
      {parameters}
    </form>
  );
}


export default IpConfigs;