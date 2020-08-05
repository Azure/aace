import React, { useEffect, useState } from 'react';
import {
  DatePicker,
  Dropdown,
  getTheme,
  IChoiceGroupOption,
  IDatePickerStrings,
  IDropdownOption,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Stack,
  TextField,
  ChoiceGroup,
} from 'office-ui-fabric-react';
import { Form, Formik } from 'formik';
import { ILandingModel, IParameterModel } from '../../models/IEnduserLandingModel';
import { Loading } from '../../shared/components/Loading';
import EndUserLandingService from "../../services/EndUserLandingService";
import OfferParameterService from "../../services/OfferParameterService";
import { IOfferParameterModel, ISubscriptionsModel } from '../../models';
import SubscriptionsService from '../../services/SubscriptionsService';
import { useHistory, useLocation } from 'react-router';
import { toast } from "react-toastify";
import * as qs from "query-string";
import adalContext from "../../adalConfig";
import { getInitialLandingModel } from "./formutlis/landingUtils";
import { getInitialCreateSubscriptionModel } from "../Subscriptions/formUtils/subscriptionFormUtils";
import { useGlobalContext } from '../../shared/components/GlobalProvider';
import { handleSubmissionErrorsForForm } from '../../shared/formUtils/utils';

// .ms-Layer.root-131 .ms-Callout-main
// {
//   background-color: #dedede;
//   border: 1px solid #2288d8;
// }
// .ms-Layer.root-131 .ms-Callout-main .ms-Button-flexContainer
// {
//   color: #2288d8;
// }
const LandingPage: React.FunctionComponent = (props) => {

  let body = (document.getElementsByTagName('body')[0] as HTMLElement);
  const history = useHistory();
  const [formState, setFormState] = useState<ILandingModel>(getInitialLandingModel());
  const [formError, setFormError] = useState<string | null>(null);
  const [loadingFormData, setLoadingFormData] = useState<boolean>(true);
  const location = useLocation();
  const DayPickerStrings: IDatePickerStrings = {
    months: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
    shortMonths: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
    days: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
    shortDays: ['S', 'M', 'T', 'W', 'T', 'F', 'S'],
    goToToday: 'Go to today',
    prevMonthAriaLabel: 'Go to previous month',
    nextMonthAriaLabel: 'Go to next month',
    prevYearAriaLabel: 'Go to previous year',
    nextYearAriaLabel: 'Go to next year',
    closeButtonAriaLabel: 'Close date picker'
  };
  const globalContext = useGlobalContext();

  useEffect(() => {
    body.setAttribute('class', 'landing')
    console.log('mounted');
    getinfo();

    return () => {
      console.log('will unmount');
      body.classList.remove('landing');
    }
  })


  // const handledSubmissionErrors = (result: Result<any>, setSubmitting: any): boolean => {
  //   if (!result.success) {
  //     if (result.hasErrors)
  //       // TODO - display the errors here
  //       toast.error(result.errors.join(', '));
  //     setSubmitting(false);
  //     return true;
  //   }
  //   return false;
  // }

  const getinfo = async () => {
    setLoadingFormData(true);
    var result = qs.parse(location.search);
    if (!result.token) {
      setLoadingFormData(false);
      return;
    }

    var token = decodeURI(result.token as string);

    let data = await EndUserLandingService.resolveToken(`\"${token}\"`);

    if (data.value && data.success) {

      let formData: ILandingModel = getInitialLandingModel();

      let usr = adalContext.AuthContext.getCachedUser();
      if (usr && usr.profile) {
        if (usr.profile.name)
          formData.fullName = usr.profile.name;
        if (usr.userName)
          formData.email = usr.userName;
      }

      // set resolvetoken data
      formData.planName = data.value.planId;
      formData.offerName = data.value.offerId;
      formData.beneficiaryTenantId = data.value.beneficiaryTenantId;
      formData.purchaserTenantId = data.value.purchaserTenantId;
      formData.quantity = 1;
      formData.subscriptionId = data.value.subscriptionId;
      formData.subscriptionName = data.value.subscriptionName;

      const [
        offerParametersResponse,
        subscriptionResponse,
      ] = await Promise.all([
        OfferParameterService.list(data.value.offerId),
        SubscriptionsService.list(formData.email)
      ]);

      // redirect to the subscription list because the user already has the subscription
      if ((subscriptionResponse.value && subscriptionResponse.success
        && (subscriptionResponse.value as ISubscriptionsModel[])
        && (subscriptionResponse.value as ISubscriptionsModel[]).findIndex(x => x.subscriptionId === formData.subscriptionId) >= 0)
        || !offerParametersResponse.success) {
        history.push("/Subscriptions");
        return;
      }

      if (offerParametersResponse.value
        && offerParametersResponse.success) {

        var offerParameters = offerParametersResponse.value as IOfferParameterModel[];
        let Parametersarray: IParameterModel[] = [];
        offerParameters.map((item, index) => {
          return (
          Parametersarray.push({
            parameterName: item.parameterName,
            displayName: item.displayName,
            description: item.description,
            valueType: item.valueType,
            fromList: item.fromList,
            valueList: item.valueList,
            maximum: item.maximum,
            minimum: item.minimum
          }))
        });
        formData.inputParameters = Parametersarray;
      }

      setFormState({ ...formData });
    }
    else { // resolve token call failed
      history.push("/Subscriptions");
      return;
    }
    setLoadingFormData(false);
  }

  const selectOnChange = (fieldKey: string, setFieldValue, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    console.log('changed:', fieldKey);
    if (option) {
      let key = (option.key as string);
      setFieldValue(fieldKey, key, true);
    }
  };

  const getFormErrorString = (touched, cntrlid, minvalue, maxvalue) => {
    let cntrlvalue = (document.getElementById(cntrlid) as HTMLElement) as any;
    if (cntrlvalue) {
      let value = parseInt(cntrlvalue.value)
      if (value < minvalue) {
        return 'Value can not be less than ' + minvalue;
      } else if (value > maxvalue) {
        return 'Value can not be greater than ' + maxvalue;
      } else {
        return '';
      }
    } else {
      return '';
    }

  };


  const theme = getTheme();

  const dropDownValues = (items: string): IDropdownOption[] => {
    let listitems: IDropdownOption[] = [];
    items ? items.split(';').map((value, index) => {
      return (
      listitems.push(
        {
          key: value,
          text: value
        }))
    })
      : listitems.push(
        {
          key: '',
          text: ''
        })
    return (listitems)

  };

  const _onFormatDate = (date?: Date): string => {
    if (date) {
      return (date.getMonth() + 1) + '/' + date.getDate() + '/' + (date.getFullYear() % 100);
    }
    let _date = new Date();
    return (_date.getMonth() + 1) + '/' + _date.getDate() + '/' + (_date.getFullYear() % 100);
  };

  const _onSelectDate = (date: Date | null | undefined, fieldKey: string, setFieldValue): void => {
    if (date) {
      let key = ((date.getMonth() + 1) + '/' + date.getDate() + '/' + (date.getFullYear() % 100));
      setFieldValue(fieldKey, key, true);
    }
  };

  const _GetSelectDate = (Parameter: any, fieldKey: string, setFieldValue) => {
    if (Parameter.dob) {
      return new Date(Parameter.dob)
    } else {
      let currentDate = new Date();
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      let key = ((currentDate.getMonth() + 1) + '/' + currentDate.getDate() + '/' + (currentDate.getFullYear() % 100));
      //setFieldValue(fieldKey, key, true);
    }
    return new Date();
  };

  const renderControls = (Parameter: IParameterModel, idx: number, handleChange, handleBlur, setFieldValue, touched) => {
    if (Parameter.valueType === 'string') {
      if (Parameter.valueList.length === 0) {
        return (
          <TextField
            id={`parameterValues.${idx}.${Parameter.parameterName}`}
            name={`parameterValues.${idx}.${Parameter.parameterName}`}
            onChange={handleChange}
            onBlur={handleBlur}
            // errorMessage={arrayItemErrorMessage(errors, touched, 'offerParameters', idx, 'description')}
            placeholder={Parameter.displayName} />)
      } else {
        return (
          <Dropdown options={dropDownValues(Parameter.valueList)}
            id={`parameterValues.${idx}.${Parameter.parameterName}`}
            onBlur={handleBlur} onChange={(event, option, index) => {
              selectOnChange(`parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue, event, option, index);
            }} />)

      }
    } else if (Parameter.valueType === 'number') {
      if (Parameter.valueList.length === 0) {
        if (Parameter.maximum && Parameter.maximum > 0) {
          return (
            <TextField
              id={`parameterValues.${idx}.${Parameter.parameterName}`}
              name={`parameterValues.${idx}.${Parameter.parameterName}`}
              onChange={handleChange}
              errorMessage={getFormErrorString(touched, `parameterValues.${idx}.${Parameter.parameterName}`, Parameter.minimum, Parameter.maximum)}
              onBlur={handleBlur}
              type="number"
              placeholder={Parameter.displayName} />)
        } else {
          return (
            <TextField
              name={`parameterValues.${idx}.${Parameter.parameterName}`}
              onChange={handleChange}
              onBlur={handleBlur}
              type="number"
              placeholder={Parameter.displayName} />)
        }
      } else {
        return (
          <Dropdown options={dropDownValues(Parameter.valueList)}
            id={`${Parameter.parameterName}`}
            onBlur={handleBlur} onChange={(event, option, index) => {
              selectOnChange(`parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue, event, option, index);
            }} />)
      }

    } else if (Parameter.valueType === 'datetime') {
      return (

        <React.Fragment>
          <TextField name={`parameterValues.${idx}.${Parameter.parameterName}`} type='hidden' />
          <DatePicker
            className={''}
            strings={DayPickerStrings}
            showWeekNumbers={false}
            allowTextInput={true}
            value={_GetSelectDate(Parameter, `parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue)}
            formatDate={_onFormatDate}
            firstWeekOfYear={1}
            onSelectDate={(date) => {
              _onSelectDate(date, `parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue)
            }}
            showMonthPickerAsOverlay={true}
            placeholder="Select a date..."
            ariaLabel="Select a date"
          />
        </React.Fragment>
      )
    }
    else if (Parameter.valueType === 'boolean') {
      return (
        <React.Fragment>
          <input name={`parameterValues.${idx}.${Parameter.parameterName}`} id={`parameterValues.${idx}.${Parameter.parameterName}`} type='hidden' />
          <img src='/logo.png' alt="" onLoad={() =>
            setDefaultRBTValue(`parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue, 'true')
          } style={{ display: 'none' }} />
          <ChoiceGroup
            className="defaultChoiceGroup"
            options={[
              {
                key: 'true',
                text: 'true',
                checked: true,
              },
              {
                key: 'false',
                text: 'false',
                checked: false
              }
            ]}
            onChange={(ev, option) => { _onChange(`parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue, ev as React.FormEvent<HTMLInputElement>, option as IChoiceGroupOption) }}
            onLoad={() => { alert(0) }}
            label=""
            required={true}
          />
        </React.Fragment>
      )
    }
  }

  function _onChange(fieldKey: string, setFieldValue, ev: React.FormEvent<HTMLInputElement>, option: IChoiceGroupOption): void {
    let key = (option.key as string);
    setFieldValue(fieldKey, key, true);
    let hdf = document.getElementById(fieldKey) as HTMLElement;
    hdf.setAttribute('value', key);
  }

  function setDefaultRBTValue(fieldKey: string, setFieldValue, option: any): void {
    setFieldValue(fieldKey, option, true);
    let hdf = document.getElementById(fieldKey) as HTMLElement;
    hdf.setAttribute('value', option);
  }

  return (
    <Stack
      verticalAlign="start"
      horizontal={false}
      styles={{
        root: {
          width: '100%',
          height: '100%',
          backgroundColor: theme.palette.neutralLight,
        }
      }}
    >
      <Stack
        horizontal={false}
        horizontalAlign="start"
        verticalAlign={"start"}
        verticalFill={true}
        styles={{
          root: {
            flexGrow: 1,
            width: '100%',
            maxWidth: 1234,
            backgroundColor: 'white',
            margin: '0 auto'
          }
        }}>
        {loadingFormData ?
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
          :
          !formState || !formState.planName || formState.planName.length === 0 ?
            <span>Invalid Token</span>
            :
            <Formik
              initialValues={formState}
              validateOnBlur={true}
              // validationSchema={landingInfoValidationSchema}
              onSubmit={async (values, { setSubmitting, setErrors }) => {

                globalContext.showProcessing();
                const input = { ...values };

                console.log(input);


                let subscriptionsModel = getInitialCreateSubscriptionModel();
                subscriptionsModel.SubscriptionId = input.subscriptionId;
                subscriptionsModel.Name = input.subscriptionName;
                subscriptionsModel.OfferName = input.offerName;
                subscriptionsModel.PlanName = input.planName;
                subscriptionsModel.Owner = input.email;
                subscriptionsModel.BeneficiaryTenantId = input.beneficiaryTenantId;
                subscriptionsModel.PurchaserTenantId = input.purchaserTenantId;
                subscriptionsModel.Quantity = input.quantity;
                console.log('rendering items');
                console.log('param values: ', input.parameterValues);

                input.inputParameters.map((item, index) => {  
                  if (item.valueType === 'number') {
                    return (
                      subscriptionsModel.InputParameters.push(
                        {
                          name: item.parameterName,
                          type: item.valueType,
                         value: '"'+parseInt(input.parameterValues[index][item.parameterName])+'"'
                        }))
                  }
                  else{
                    return (
                    subscriptionsModel.InputParameters.push(
                      {
                        name: item.parameterName,
                        type: item.valueType,
                        value: input.parameterValues[index][item.parameterName]
                      }))
                  }
                })

                let createSubscriptionsResult = await SubscriptionsService.create(subscriptionsModel);

                if (handleSubmissionErrorsForForm(setErrors,setSubmitting,setFormError,'landingpage',createSubscriptionsResult)) {
                  globalContext.hideProcessing();
                  return;
                }

                setSubmitting(false);
                globalContext.hideProcessing();
                toast.success("Success!");
                history.push(`Subscriptions`);
              }}
            >

              {({ isSubmitting, setFieldValue, values, handleChange, handleBlur, touched, errors }) => {
                console.log('values: ' + JSON.stringify(values));
                return (
                  <Form style={{ marginTop: 0, width: '100%' }} autoComplete={"off"}>                    
                    {formError && <MessageBar messageBarType={MessageBarType.error} style={{ marginBottom: 15 }}>
                      {{ formError }}
                    </MessageBar>}
                    <React.Fragment>
                      <div className="landingpagecontainner">
                        <div style={{ borderBottom: '1px solid #efefef', minHeight: '55px' }} className="headertitle">
                          <div style={{ textAlign: 'left' }}>
                            <span>Configure Subscription</span>
                          </div>
                          <div style={{ textAlign: 'right' }}>
                            <PrimaryButton type="submit" id="btnsubmit" style={{ width: '100px' }}
                              className="button">Submit</PrimaryButton>
                          </div>
                        </div>
                        <table className="mainlanding">
                          <tbody>
                            <tr>
                              <td>
                                <span>Email:</span>
                              </td>
                              <td>
                                <span>{values.email}</span>
                              </td>
                            </tr>
                            <tr>
                              <td>
                                <span>Subscriber full name:</span>
                              </td>
                              <td>
                                <span>{values.fullName}</span>
                              </td>
                            </tr>
                            <tr>
                              <td>
                                <span>OfferId:</span>
                              </td>
                              <td>
                                <span>{values.offerName}</span>
                              </td>
                            </tr>
                            <tr>
                              <td>
                                <span>Current Plan:</span>
                              </td>
                              <td><span>{values.planName}</span>
                              </td>
                            </tr>
                            <tr>
                              <td>
                                <span>Saas Subscription ID:</span>
                              </td>
                              <td>
                                <span>{values.subscriptionId}</span>
                              </td>
                            </tr>
                            <tr>
                              <td>
                                <span>Subscription Name:</span>
                              </td>
                              <td>
                                <span>{values.subscriptionName}</span>
                              </td>
                            </tr>
                            {values.inputParameters ?
                              values.inputParameters.map((item, index) => {
                                return (
                                  <tr key={index}>
                                    <td>
                                      <span>{item.displayName}</span>
                                    </td>
                                    <td>
                                      {
                                        renderControls(item, index, handleChange, handleBlur, setFieldValue, touched)
                                      }
                                    </td>
                                  </tr>
                                )
                              })
                              : null}
                          </tbody>
                        </table>
                      </div>
                    </React.Fragment>
                  </Form>);
              }}
            </Formik>
        }
      </Stack>
    </Stack>
  );
};

export default LandingPage;