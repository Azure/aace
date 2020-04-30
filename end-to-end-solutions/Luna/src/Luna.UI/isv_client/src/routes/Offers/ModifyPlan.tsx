import React, { useEffect, useState } from 'react';
import {
  ChoiceGroup, DefaultButton, DialogFooter,
  Dropdown,
  FontIcon,  
  IChoiceGroupOption,
  IDropdownOption,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Stack,
  TextField,
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { FieldArray, useFormikContext } from "formik";
import {
  getInitialCustomMeterDimension,
  getInitialPlan,
  getInitialRestrictedUser,
  IPlanFormValues
} from "./formUtils/planFormUtils";
import PlansService from "../../services/PlansService";
import ArmTemplateService from "../../services/ArmTemplatesService";
import WebHooksService from "../../services/WebHooksService";
import { Loading } from '../../shared/components/Loading';
import {
  IARMTemplateModel,
  ICustomMeterDimensionsModel, ICustomMeterModel, IError,
  IPlanModel,
  IRestrictedUsersModel,
  IWebHookModel, Result
} from "../../models";
import { Offers } from '../../shared/constants/infomessages';
import { useGlobalContext } from '../../shared/components/GlobalProvider';
import AlternateButton from "../../shared/components/AlternateButton";
import {
  arrayItemErrorMessage, arrayItemErrorMessageFromForm,
  arrayItemErrorMessageWithoutTouch,
  handleSubmissionErrorsForForm
} from "../../shared/formUtils/utils";
import { toast } from "react-toastify";
import CustomMetersService from "../../services/MetersService";

export type IPlanFormProps = {
  isNew: boolean;
  planName: string;
  offerName: string;
  formState: IPlanFormValues;
  setFormState: any;
  setFormKey: any;
  planlist: IPlanModel[];
  hidePlanDialog: () => void;
  refreshPlanList: () => void;
}

const ModifyPlan: React.FunctionComponent<IPlanFormProps> = (props) => {
  const { setFieldValue, values, handleChange, handleBlur,
    touched, errors,handleSubmit, submitForm, setErrors, setSubmitting, dirty } = useFormikContext<IPlanFormValues>(); // formikProps
  
  const globalContext = useGlobalContext();  
  const [loadingFormData, setLoadingFormData] = useState<boolean>(true);
  const [disableValue, setDisableValue] = useState<boolean>(true);
  const [formError, setFormError] = useState<string | null>(null);  
  const [selectedKey, setSelectedKey] = useState<string>('public');
  const OfferName = props.offerName as string;
  const PlanName = props.planName as string;
  const IsNew = props.isNew as boolean;
  
  const [armTemplateDropdownOptions, setArmTemplateDropdownOptions] = useState<IDropdownOption[]>([]);
  const [webHookDropdownOptions, setWebHookDropdownOptions] = useState<IDropdownOption[]>([]);
  const [customMeterDropdownOptions, setCustomMeterDropdownOptions] = useState<IDropdownOption[]>([]);

  let selectedSubscribe: string = "";  
  let planIdList = 'Medium';
  let planNameList = 'Medium plan';
  const getPlansFormErrorString = (touched, errors, property: string) => {
    let retobj = touched.plan && errors.plan && touched.plan[property] && errors.plan[property] ? errors.plan[property] : '';
    return retobj;
  };

  const DisplayErrors = (errors, values) => {
    console.log('display errors:');
    console.log(errors);
    console.log(values);
    return null;
  };

  useEffect(() => {
    // eslint-disable-next-line react-hooks/exhaustive-deps        
    //Get all plan name and list  
    //eslint-disable-next-line array-callback-return  
    props.planlist.map((plan, index) => {
      // eslint-disable-next-line react-hooks/exhaustive-deps
      planIdList += plan.planName + ',';
      // eslint-disable-next-line react-hooks/exhaustive-deps
      planNameList += plan.planName + ',';  
      //eslint-disable-next-line array-callback-return
    });
    values.plan.planIdList = planIdList
    values.plan.planNameList = planNameList;

    //Get all plan name and list
    getFormData(OfferName, PlanName, IsNew);

    // eslint-disable-next-line react-hooks/exhaustive-deps    
    globalContext.modifySaveForm(async () => { await submitForm(); })    
  }, []);

  const getUserFormErrorString = (touched, errors, idx: number, property: string) => {
    return touched && touched.plan && touched.plan.restrictedUsers && touched.plan.restrictedUsers[idx] != null && errors && errors.plan && errors.plan.restrictedUsers && errors.plan.restrictedUsers[idx] != null && touched.plan.restrictedUsers[idx][property] && errors.plan.restrictedUsers[idx][property] ? errors.plan.restrictedUsers[idx][property] : '';
  };

  const getFormData = async (offerName: string, planName: string, isNew: boolean) => {

    setLoadingFormData(true);
    let serviceCallList: Promise<Result<any>>[] = [];

    serviceCallList.push(ArmTemplateService.list(offerName));
    serviceCallList.push(WebHooksService.list(offerName));
    serviceCallList.push(CustomMetersService.list(offerName));

    // we will be loading additional calls if this is an existing plan
    if (!isNew) {
      serviceCallList.push(PlansService.get(offerName, planName));
      serviceCallList.push(PlansService.getCustomMeterDimensions(offerName, planName));
      //zb: commenting this out for now per xiaochen
      //serviceCallList.push(PlansService.getRestrictedUsers(offerName, planName));
    }

    let promiseResult = await Promise.all(serviceCallList);

    let errorMessages: IError[] = [];

    errorMessages.concat(promiseResult[0].errors);
    errorMessages.concat(promiseResult[1].errors);
    errorMessages.concat(promiseResult[2].errors);

    if (!isNew) {
      errorMessages.concat(promiseResult[3].errors);
      errorMessages.concat(promiseResult[4].errors);
      //zb: commenting this out for now per xiaochen
      //errorMessages.concat(promiseResult[5].errors);
    }

    if (errorMessages.length > 0) {
      toast.error(errorMessages.join(', '));
    }
    else {

      let armTemplates = promiseResult[0].value as IARMTemplateModel[];
      let webHook = promiseResult[1].value as IWebHookModel[];
      let customMeters = promiseResult[2].value as ICustomMeterModel[];

      let plan: IPlanModel;

      if (!isNew) {
        plan = promiseResult[3].value as IPlanModel;
        plan.isNew = false;

        plan.customMeterDimensions = promiseResult[4].value as ICustomMeterDimensionsModel[];
        //zb: commenting this out for now per xiaochen
        /*
        const selectedOption = plan.privatePlan ? 'private' : 'public';
        setSelectedKey(selectedOption);
        if (plan.privatePlan) {
          plan.restrictedUsers = promiseResult[5].value as IRestrictedUsersModel[];
        }
        else {
          plan.restrictedUsers = [];
        }*/

        if (plan.subscribeArmTemplateName) {
          setDisableValue(false);
        }
        else {
          setDisableValue(true);
        }

        props.setFormState({
          plan: { ...plan }
        });
      }
      else {
        props.setFormState(getInitialPlan);
        setDisableValue(true);
      }

      let templateItems: IDropdownOption[] = [];
      templateItems.push({ key: '', text: 'None' });

      if (armTemplates) {
        armTemplates.forEach(element => {
          templateItems.push({ key: element.templateName, text: element.templateName });
        });
      }

      let webHookItems: IDropdownOption[] = [];
      webHookItems.push({ key: '', text: 'None' });
      if (webHook) {
        webHook.forEach(element => {
          webHookItems.push({ key: element.webhookName, text: element.webhookName });
        });
      }

      let customMeterItems: IDropdownOption[] = [];

      if (customMeters) {
        customMeters.forEach(element => {
          customMeterItems.push({ key: element.meterName, text: element.meterName });
        });
      }

      setArmTemplateDropdownOptions([...templateItems]);
      setWebHookDropdownOptions([...webHookItems]);
      setCustomMeterDropdownOptions([...customMeterItems]);
    }

    setLoadingFormData(false);

  }

 const privateOnChange = (fieldKey: string, setFieldValue, ev?: React.SyntheticEvent<HTMLElement>, option?: IChoiceGroupOption) => {

    if (option) {
      setSelectedKey(option.key);

      if (option.key === 'private') {
        setFieldValue(fieldKey, true, true);
      }
      else {
        setFieldValue(fieldKey, false, true);
      }
    }
  };

  const selectOnChange = (fieldKey: string, setFieldValue, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {

    if (option) {
      selectedSubscribe = (option.key as string);
      setFieldValue(fieldKey, selectedSubscribe, true);
    }
  };

  const armTemplateSelectOnChange = (fieldKey: string, setFieldValue, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    if (option) {
      selectedSubscribe = (option.key as string);
      setFieldValue(fieldKey, selectedSubscribe, true);


      if (option.key) {
        setDisableValue(false);
      }
      else {
        setDisableValue(true);
        props.formState.plan.unsubscribeWebhookName = '';
        props.formState.plan.suspendArmTemplateName= '';
        props.formState.plan.deleteDataArmTemplateName= '';
        props.setFormState({
          plan: { ...props.formState.plan }
        });
        setFieldValue('plan.unsubscribeArmTemplateName', '', true);
        setFieldValue('plan.suspendArmTemplateName', '', true);
        setFieldValue('plan.deleteDataArmTemplateName', '', true);
      }

    }
  };

  const handleAddUser = (arrayHelpers) => {
    arrayHelpers.insert(arrayHelpers.form.values.plan.restrictedUsers.length, getInitialRestrictedUser());
  };

  const handleRemoveUser = (arrayHelpers, clientId) => {

    var idx = arrayHelpers.form.values.plan.restrictedUsers.findIndex(x => x.clientId === clientId);
    arrayHelpers.form.setFieldValue(`plan.restrictedUsers.${idx}.isDeleted`, true, true);
  };

  const handleAddCustomMeterDimension = (arrayHelpers) => {
    arrayHelpers.insert(arrayHelpers.form.values.plan.customMeterDimensions.length, getInitialCustomMeterDimension());
  };

  const handleRemoveCustomMeterDimension = (arrayHelpers, clientId) => {

    var idx = arrayHelpers.form.values.plan.customMeterDimensions.findIndex(x => x.clientId === clientId);
    arrayHelpers.form.setFieldValue(`plan.customMeterDimensions.${idx}.isDeleted`, true, true);
  };

  const deletePlan = async (plan: IPlanModel, setErrors: any, setSubmitting: any) => {

    // Delete any restricted users first
    //zb: commenting this out for now per xiaochen
    /*let usersToDelete = values.plan.restrictedUsers.filter(x => !!x.isNew === false && !!x.isSaved === false);

    for (let user of usersToDelete) {

      var paramDeleteResult = await PlansService.deleteRestrictedUser(OfferName, plan.planName, user.tenantId);      
      //TODO: NEED TO HANDLE THE DISPLAY OF ERRORS FOR subkeys for forms      
      if (!paramDeleteResult.success) {
        globalContext.hideProcessing();
        return;
      }

      user.isSaved = true;
    }*/

    const deleteResult = await PlansService.delete(OfferName, plan.planName);
    if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'plan', deleteResult)) {
      globalContext.hideProcessing();
      return;
    }

    toast.success("Plan Deleted Successfully!");
    props.refreshPlanList();
    props.hidePlanDialog();

  };


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

  const headerStyles = {
    fontSize: 18.72,
    fontWeight:400,
    color:'#615f5d'
  };

  const textboxClassName = "form_textboxmodal";  
  return (
    <form style={{ width: '100%', marginTop: 20 }} autoComplete={"off"} onSubmit={handleSubmit}>
      {formError && <MessageBar messageBarType={MessageBarType.error} style={{ marginBottom: 15 }}>
        {{ formError }}
      </MessageBar>
      }
      <Stack
        verticalAlign="start"
        horizontal={false}
        gap={10}
        styles={{
          root: {}
        }}
      >
        <DisplayErrors errors={errors} values={values} />
        <table>
          <tbody>
            <tr>
              <td>
                <FormLabel title={"Name:"} toolTip={Offers.plans.planName} />
                <TextField
                  name={'plan.planName'}
                  value={values.plan.planName}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  maxLength={50}
                  style={{ marginLeft: 11 }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'planName')}
                  placeholder={'Name'}
                  className={textboxClassName} disabled={values.plan.isNew === false} />
              </td>
              <td>
                <FormLabel title={"Data Retention(days) :"} toolTip={"Data Retention(days)"} />
                <TextField
                  name={'plan.dataRetentionInDays'}
                  type="number"
                  value={values.plan.dataRetentionInDays.toString()}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  style={{ marginLeft: 11 }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'dataRetentionInDays')}
                  placeholder={'Data Retention(days)'}
                  className={textboxClassName} />
              </td>

            </tr>
          </tbody>
        </table>

        <table style={{ textAlign: 'left', marginTop: 45 }}>
          <colgroup>
            <col style={{ width: 155 }} />
            <col style={{ width: 320 }} />
            <col style={{ width: 320 }} />
          </colgroup>

          <thead>
            <tr>
              <td>

              </td>
              <td style={{ textAlign: 'justify' }}>
                ARM Templates
              </td>
              <td style={{ textAlign: 'justify' }}>
                Webhooks
              </td>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>
                <FormLabel title={"Subscribe:"} />
              </td>
              <td>
                <Dropdown options={armTemplateDropdownOptions}
                  id={'plan.subscribeArmTemplateName'} onBlur={handleBlur}
                  onChange={(event, option, index) => { armTemplateSelectOnChange('plan.subscribeArmTemplateName', setFieldValue, event, option, index) }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'subscribeArmTemplateName')}
                  defaultSelectedKey={values.plan.subscribeArmTemplateName}
                />
              </td>
              <td>
                <Dropdown options={webHookDropdownOptions}
                  id={'plan.subscribeWebhookName'} onBlur={handleBlur}
                  onChange={(event, option, index) => { selectOnChange('plan.subscribeWebhookName', setFieldValue, event, option, index) }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'subscribeWebhookName')}
                  defaultSelectedKey={values.plan.subscribeWebhookName}
                />
              </td>
            </tr>
            <tr>
              <td>
                <FormLabel title={"Unsubscribe:"} />
              </td>
              <td>
                <Dropdown options={armTemplateDropdownOptions}
                  id={'plan.unsubscribeArmTemplateName'} onBlur={handleBlur}
                  onChange={(event, option, index) => { selectOnChange('plan.unsubscribeArmTemplateName', setFieldValue, event, option, index) }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'unsubscribeArmTemplateName')}
                  isDisabled={disableValue}
                  defaultSelectedKey = {values.plan.unsubscribeArmTemplateName}
                />
              </td>
              <td>
                <Dropdown options={webHookDropdownOptions}
                  id={'plan.unsubscribeWebhookName'} onBlur={handleBlur}
                  onChange={(event, option, index) => { selectOnChange('plan.unsubscribeWebhookName', setFieldValue, event, option, index) }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'unsubscribeWebhookName')}
                  defaultSelectedKey={values.plan.unsubscribeWebhookName}                  
                />
              </td>
            </tr>
            <tr>
              <td>
                <FormLabel title={"Suspend:"} />
              </td>
              <td>
                <Dropdown options={armTemplateDropdownOptions}
                  id={'plan.suspendArmTemplateName'} onBlur={handleBlur}
                  onChange={(event, option, index) => { selectOnChange('plan.suspendArmTemplateName', setFieldValue, event, option, index) }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'suspendArmTemplateName')}
                  isDisabled={disableValue}
                  defaultSelectedKey={values.plan.suspendArmTemplateName}
                />
              </td>
              <td>
                <Dropdown placeholder="Select" options={webHookDropdownOptions}
                  id={'plan.suspendWebhookName'} onBlur={handleBlur}
                  onChange={(event, option, index) => { selectOnChange('plan.suspendWebhookName', setFieldValue, event, option, index) }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'suspendWebhookName')}
                  defaultSelectedKey={values.plan.suspendWebhookName}
                />
              </td>
            </tr>
            <tr>
              <td>
                <FormLabel title={"DeleteData:"} />
              </td>
              <td>
                <Dropdown placeholder="Select" options={armTemplateDropdownOptions}
                  id={'plan.deleteDataArmTemplateName'} onBlur={handleBlur}
                  onChange={(event, option, index) => { selectOnChange('plan.deleteDataArmTemplateName', setFieldValue, event, option, index) }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'deleteDataArmTemplateName')}
                  isDisabled={disableValue}
                  defaultSelectedKey={values.plan.deleteDataArmTemplateName}
                />
              </td>
              <td>
                <Dropdown placeholder="Select" options={webHookDropdownOptions}
                  id={'plan.deleteDataWebhookName'} onBlur={handleBlur}
                  onChange={(event, option, index) => { selectOnChange('plan.deleteDataWebhookName', setFieldValue, event, option, index) }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'deleteDataWebhookName')}
                  defaultSelectedKey={values.plan.deleteDataWebhookName}
                />
              </td>
            </tr>
          </tbody>
        </table>
        {/*
        <table style={{ width: '75%' }}>
          <tbody>
            <tr>
              <td style={{ width: '15%' }}>
                <FormLabel title={"Availability:"} />
              </td>
              <td style={{ width: '60%', textAlign: 'left' }}>
                <ChoiceGroup
                  className="defaultChoiceGroup" name="plan.privatePlan"
                  defaultSelectedKey={selectedKey}
                  options={[
                    {
                      key: 'public',
                      text: 'Public'
                    },
                    {
                      key: 'private',
                      text: 'Private'
                    },
                  ]}
                  onChange={(event, option) => { privateOnChange('plan.privatePlan', setFieldValue, event, option) }}
                  label=""
                />
              </td>
            </tr>
          </tbody>
        </table>
        {selectedKey === 'private' ?
          <table>
            <thead>
              <tr>
                <td>
                  <FormLabel title={"TenantId"} toolTip={Offers.plans.restrictedAudience} />
                </td>
                <td>
                  <FormLabel title={"Description"} toolTip={"Description"} />
                </td>
              </tr>
            </thead>
            <FieldArray
              name="plan.restrictedUsers"
              render={arrayHelpers => {
                return (
                  <React.Fragment>
                    <tbody>
                      {values.plan.restrictedUsers.map((value: IRestrictedUsersModel, idx) => {
                        if (value.isDeleted)
                          return true;

                        return (
                          <React.Fragment key={idx}>
                            <tr>
                              <td>
                                {value.isNew ?
                                  <TextField
                                    name={`plan.restrictedUsers.${idx}.tenantId`}
                                    value={value.tenantId}
                                    onChange={handleChange}
                                    onBlur={handleBlur}
                                    errorMessage={getUserFormErrorString(touched, errors, idx, 'tenantId')}
                                    placeholder={'Id'} />
                                  :
                                  <span>{value.tenantId}</span>
                                }
                              </td>
                              <td>
                                {value.isNew ?
                                  <TextField
                                    name={`plan.restrictedUsers.${idx}.description`}
                                    value={value.description}
                                    onChange={handleChange}
                                    onBlur={handleBlur}
                                    errorMessage={getUserFormErrorString(touched, errors, idx, 'description')}
                                    placeholder={'Description'} />
                                  :
                                  <span>{value.description}</span>
                                }
                              </td>
                              <td>
                                <FontIcon iconName="Cancel" className="deleteicon" onClick={() => handleRemoveUser(arrayHelpers, value.clientId)} />
                              </td>
                            </tr>
                          </React.Fragment>

                        );
                      })}
                    </tbody>
                    <tfoot>
                      <tr>
                        <td colSpan={2} style={{ textAlign: 'left' }}>
                          <PrimaryButton disabled={arrayHelpers.form.values.plan.restrictedUsers.filter(x => !!x.isDeleted === false).length === 10} onClick={() => handleAddUser(arrayHelpers)}
                            className={arrayHelpers.form.values.plan.restrictedUsers.filter(x => !!x.isDeleted === false).length === 10 ? 'disabled' : 'addbutton'}>
                            <FontIcon iconName="Add" className={arrayHelpers.form.values.plan.restrictedUsers.filter(x => !!x.isDeleted === false).length === 10 ? 'disabled' : 'deleteicon'} /> Add</PrimaryButton>
                          <span style={{ padding: '5px 0 0 5px', position: 'absolute' }}> (Max 10)</span>
                        </td>
                      </tr>
                    </tfoot>
                  </React.Fragment>
                );
              }}
            />
          </table>
          : null*/}
        <FormLabel title={"Custom Meters"} toolTip={"TODO: replace this text"} style={headerStyles}/>
        <table className={"fixed_header"}>
          <thead>
          <tr>
            <td style={{width:200}}>
              <FormLabel title={"Meter Name"} toolTip={"TODO: replace this text"} />
            </td>
            <td style={{width:200}}>
              <FormLabel title={"Unlimited"} toolTip={"TODO: replace this text"} />
            </td>
            <td style={{width:200}}>
              <FormLabel title={"Included In Base"} toolTip={"TODO: replace this text"} />
            </td>
          </tr>
          </thead>
          <DisplayErrors errors={errors} values={values}/>
          <FieldArray
            name="plan.customMeterDimensions"
            render={arrayHelpers => {
              return (
                <React.Fragment>
                  <tbody>
                  {values.plan.customMeterDimensions.map((value: ICustomMeterDimensionsModel, idx) => {
                    if (value.isDeleted)
                      return true;

                    return (
                      <React.Fragment key={idx}>
                        <tr>
                          <td style={{ verticalAlign: 'middle',width:200 }}>
                            {value.isNew ?
                              <Dropdown
                                options={customMeterDropdownOptions}
                                id={`plan.customMeterDimensions.${idx}.meterName`} onBlur={handleBlur}
                                onChange={(event, option, index) => { selectOnChange(`plan.customMeterDimensions.${idx}.meterName`, setFieldValue, event, option, index) }}
                                errorMessage={arrayItemErrorMessageFromForm(errors, touched, 'plan', 'customMeterDimensions', idx, 'meterName')}
                                defaultSelectedKey={value.meterName}
                              />
                              :
                              <span>{value.meterName}</span>
                            }
                          </td>
                          <td style={{width:200}}>
                            <Dropdown
                              options={[{key: 'false', text: 'false'}, {key: 'true', text: 'true'}]}
                              id={`plan.customMeterDimensions.${idx}.monthlyUnlimited`} onBlur={handleBlur}
                              onChange={(event, option, index) => { selectOnChange(`plan.customMeterDimensions.${idx}.monthlyUnlimited`, setFieldValue, event, option, index) }}
                              errorMessage={arrayItemErrorMessage(errors, touched, 'plan.customMeterDimensions', idx, 'monthlyUnlimited')}
                              defaultSelectedKey={value.monthlyUnlimited ? 'true' : 'false'}
                              disabled={true}
                            />
                          </td>
                          <td style={{width:200}}>
                            <TextField
                              name={`plan.customMeterDimensions.${idx}.monthlyQuantityIncludedInBase`}
                              value={value.monthlyQuantityIncludedInBase.toString()}
                              onChange={handleChange}
                              onBlur={handleBlur}
                              errorMessage={getUserFormErrorString(touched, errors, idx, 'monthlyQuantityIncludedInBase')}
                              placeholder={'Quantity Included In Base'}
                              disabled={true}
                            />
                          </td>
                          <td>
                            <FontIcon iconName="Cancel" className="deleteicon" onClick={() => handleRemoveCustomMeterDimension(arrayHelpers, value.clientId)} />
                          </td>
                        </tr>
                      </React.Fragment>

                    );
                  })}
                  </tbody>
                  <tfoot>
                  <tr>
                    <td colSpan={2} style={{ textAlign: 'left' }}>
                      <PrimaryButton disabled={arrayHelpers.form.values.plan.customMeterDimensions.filter(x => !!x.isDeleted === false).length === 10} onClick={() => handleAddCustomMeterDimension(arrayHelpers)}
                                     className={arrayHelpers.form.values.plan.customMeterDimensions.filter(x => !!x.isDeleted === false).length === 10 ? 'disabled' : 'addbutton'}>
                        <FontIcon iconName="Add" className={arrayHelpers.form.values.plan.customMeterDimensions.filter(x => !!x.isDeleted === false).length === 10 ? 'disabled' : 'deleteicon'} /> Add</PrimaryButton>
                      <span style={{ padding: '5px 0 0 5px', position: 'absolute' }}></span>
                    </td>
                  </tr>
                  </tfoot>
                </React.Fragment>
              );
            }}
          />
        </table>
      </Stack >
      <DialogFooter>
        <Stack horizontal={true} gap={15}>
          {!props.isNew &&
            <DefaultButton onClick={() => { deletePlan(values.plan, setErrors, setSubmitting) }} className="addbutton">
              <FontIcon iconName="Cancel" className="deleteicon" /> Delete
          </DefaultButton>
          }
          <div style={{ flexGrow: 1 }}></div>
          <AlternateButton
            onClick={props.hidePlanDialog}
            text="Cancel" />
          <PrimaryButton
            type="submit"
            text="Save"/>
        </Stack>
      </DialogFooter>
    </form >
  );
}
export default ModifyPlan;

