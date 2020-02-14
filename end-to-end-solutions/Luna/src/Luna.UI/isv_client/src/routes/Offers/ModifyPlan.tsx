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
import { getInitialPlan,getInitialRestrictedUser, IPlanFormValues } from "./formUtils/planFormUtils";
import PlansService from "../../services/PlansService";
import ArmTemplateService from "../../services/ArmTemplatesService";
import WebHooksService from "../../services/WebHooksService";
import { Loading } from '../../shared/components/Loading';
import { IARMTemplateModel, IPlanModel, IRestrictedUsersModel, IWebHookModel } from "../../models";
import { Offers } from '../../shared/constants/infomessages';
import { useGlobalContext } from '../../shared/components/GlobalProvider';
import AlternateButton from "../../shared/components/AlternateButton";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { toast } from "react-toastify";

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
    touched, errors,handleSubmit, submitForm, setErrors, setSubmitting } = useFormikContext<IPlanFormValues>(); // formikProps
  
  const globalContext = useGlobalContext();  
  const [loadingFormData, setLoadingFormData] = useState<boolean>(true);
  const [loadingdisable, setLoadingdisable] = useState<boolean>(true);
  const [disableValue, setDisableValue] = useState<boolean>(true);
  const [formError, setFormError] = useState<string | null>(null);  
  const [selectedKey, setSelectedKey] = useState<string>('public');
  const OfferName = props.offerName as string;
  const PlanName = props.planName as string;
  const IsNew = props.isNew as boolean;
  
  const [armTemplateDropdownOptions, setArmTemplateDropdownOptions] = useState<IDropdownOption[]>([]);
  const [webHookDropdownOptions, setWebHookDropdownOptions] = useState<IDropdownOption[]>([]);
  let selectedSubscribe: string = "";  
  let planIdList = 'Medium';
  let planNameList = 'Medium plan';
  const getPlansFormErrorString = (touched, errors, property: string) => {
    let retobj = touched.plan && errors.plan && touched.plan[property] && errors.plan[property] ? errors.plan[property] : '';
    return retobj;
  };

  const DisplayErrors = (errors) => {    
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
    if (IsNew) {
      LoadFormData(OfferName);
    }
    else {
      getFormData(OfferName, PlanName);
    }    
    // eslint-disable-next-line react-hooks/exhaustive-deps    
    globalContext.modifySaveForm(async () => { await submitForm(); })    
  }, []);

  const getUserFormErrorString = (touched, errors, idx: number, property: string) => {
    return touched && touched.plan && touched.plan.restrictedUsers && touched.plan.restrictedUsers[idx] != null && errors && errors.plan && errors.plan.restrictedUsers && errors.plan.restrictedUsers[idx] != null && touched.plan.restrictedUsers[idx][property] && errors.plan.restrictedUsers[idx][property] ? errors.plan.restrictedUsers[idx][property] : '';
  };

  const getFormData = async (offerName: string, planName: string) => {

    setLoadingFormData(true);
    const [armTemplatesResponse, planResponse, webHookResponse] = await Promise.all([
      ArmTemplateService.list(offerName),
      PlansService.get(offerName, planName),
      WebHooksService.list(offerName),
    ]);

    if (armTemplatesResponse.hasErrors || planResponse.hasErrors || webHookResponse.hasErrors) {

      // TODO: display errors in a better, consistent way
      if (armTemplatesResponse.hasErrors)
        alert(armTemplatesResponse.errors.join(', '));

      if (planResponse.hasErrors)
        alert(planResponse.errors.join(', '));

      if (webHookResponse.hasErrors)
        alert(webHookResponse.errors.join(', '));

    } else {
      var armTemplates = armTemplatesResponse.value as IARMTemplateModel[];
      var webHook = webHookResponse.value as IWebHookModel[];
      var plan = planResponse.value as IPlanModel;
      plan.isNew = false;

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

      setArmTemplateDropdownOptions([...templateItems]);
      setWebHookDropdownOptions([...webHookItems]);
      const selectedOption = plan.privatePlan ? 'private' : 'public';
      setSelectedKey(selectedOption);
      if (plan.privatePlan) {
        const [restrictedUserResponse] = await Promise.all([
          PlansService.getRestrictedUsers(offerName, plan.planName),
        ]);
        if (restrictedUserResponse.hasErrors) {
          alert(restrictedUserResponse.errors.join(', '));
          plan.restrictedUsers = [];
        }
        else {
          plan.restrictedUsers = restrictedUserResponse.value ? restrictedUserResponse.value : [];
        }
      }
      else {
        plan.restrictedUsers = [];
      }

      if (plan.subscribeArmTemplateName) {
        setLoadingdisable(true);
        setDisableValue(false);
        setLoadingdisable(false);
      }
      else {
        setLoadingdisable(true);
        setDisableValue(true);
        setLoadingdisable(false);
      }

      props.setFormState({
        plan: { ...plan }
      });

    }

    setLoadingFormData(false);

  }

  const LoadFormData = async (offerName: string) => {

    setLoadingFormData(true);
    const [armTemplatesResponse, webHookResponse] = await Promise.all([
      ArmTemplateService.list(offerName),
      WebHooksService.list(offerName),
    ]);

    if (armTemplatesResponse.hasErrors || webHookResponse.hasErrors) {

      // TODO: display errors in a better, consistent way
      if (armTemplatesResponse.hasErrors)
        alert(armTemplatesResponse.errors.join(', '));

      if (webHookResponse.hasErrors)
        alert(webHookResponse.errors.join(', '));

    } else {
      var armTemplates = armTemplatesResponse.value as IARMTemplateModel[];
      var webHook = webHookResponse.value as IWebHookModel[];

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

      setArmTemplateDropdownOptions([...templateItems]);
      setWebHookDropdownOptions([...webHookItems]);
      
      setLoadingdisable(true);
      setDisableValue(true);
      setLoadingdisable(false);

      props.setFormState(getInitialPlan);

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
        setLoadingdisable(true);
        setDisableValue(false);
        setLoadingdisable(false);
      }
      else {
        setLoadingdisable(true);
        setDisableValue(true);
        props.formState.plan.unsubscribeWebhookName = '';
        props.formState.plan.suspendArmTemplateName= '';
        props.formState.plan.deleteDataArmTemplateName= '';
        props.setFormState({
          plan: { ...props.formState.plan }
        });
        setLoadingdisable(false);
        setFieldValue('plan.unsubscribeArmTemplateName', '', true);
        setFieldValue('plan.suspendArmTemplateName', '', true);
        setFieldValue('plan.deleteDataArmTemplateName', '', true);
      }

    }
  };

  const handleAdd = (arrayHelpers) => {
    arrayHelpers.insert(arrayHelpers.form.values.plan.restrictedUsers.length, getInitialRestrictedUser());
  };

  const handleRemove = (arrayHelpers, clientId) => {

    var idx = arrayHelpers.form.values.plan.restrictedUsers.findIndex(x => x.clientId === clientId);
    arrayHelpers.form.setFieldValue(`plan.restrictedUsers.${idx}.isDeleted`, true, true);
  };

  const deletePlan = async (plan: IPlanModel, setErrors: any, setSubmitting: any) => {

    // Delete any restricted users first
    let usersToDelete = values.plan.restrictedUsers.filter(x => !!x.isNew === false && !!x.isSaved === false);

    for (let user of usersToDelete) {

      var paramDeleteResult = await PlansService.deleteRestrictedUser(OfferName, plan.planName, user.tenantId);      
      //TODO: NEED TO HANDLE THE DISPLAY OF ERRORS FOR subkeys for forms      
      if (!paramDeleteResult.success) {
        globalContext.hideProcessing();
        return;
      }

      user.isSaved = true;
    }

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
        <DisplayErrors errors={errors} />
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
                {loadingdisable ?
                  null
                  :
                  <Dropdown options={armTemplateDropdownOptions}
                    id={'plan.unsubscribeArmTemplateName'} onBlur={handleBlur}
                    onChange={(event, option, index) => { selectOnChange('plan.unsubscribeArmTemplateName', setFieldValue, event, option, index) }}
                    errorMessage={getPlansFormErrorString(touched, errors, 'unsubscribeArmTemplateName')}
                    isDisabled={disableValue}
                    defaultSelectedKey = {values.plan.unsubscribeArmTemplateName}
                  />
                }

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
              {loadingdisable ?
                  null
                  :
                <Dropdown options={armTemplateDropdownOptions}
                  id={'plan.suspendArmTemplateName'} onBlur={handleBlur}
                  onChange={(event, option, index) => { selectOnChange('plan.suspendArmTemplateName', setFieldValue, event, option, index) }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'suspendArmTemplateName')}
                  isDisabled={disableValue}
                  defaultSelectedKey={values.plan.suspendArmTemplateName}
                />
              }
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
              {loadingdisable ?
                  null
                  :
                <Dropdown placeholder="Select" options={armTemplateDropdownOptions}
                  id={'plan.deleteDataArmTemplateName'} onBlur={handleBlur}
                  onChange={(event, option, index) => { selectOnChange('plan.deleteDataArmTemplateName', setFieldValue, event, option, index) }}
                  errorMessage={getPlansFormErrorString(touched, errors, 'deleteDataArmTemplateName')}
                  isDisabled={disableValue}
                  defaultSelectedKey={values.plan.deleteDataArmTemplateName}
                />
              }
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
                          return value;

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
                                <FontIcon iconName="Cancel" className="deleteicon" onClick={() => handleRemove(arrayHelpers, value.clientId)} />
                              </td>
                            </tr>
                          </React.Fragment>

                        );
                      })}
                    </tbody>
                    <tfoot>
                      <tr>
                        <td colSpan={2} style={{ textAlign: 'left' }}>
                          <PrimaryButton disabled={arrayHelpers.form.values.plan.restrictedUsers.filter(x => !!x.isDeleted === false).length === 10} onClick={() => handleAdd(arrayHelpers)}
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
          : null}
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

