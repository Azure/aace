import React, { useEffect, useState } from 'react';
import {
  DetailsList,
  DetailsListLayoutMode,
  Dialog,
  DialogContent,
  DialogFooter,
  DialogType,
  Dropdown,
  FontIcon,
  IColumn,
  IDropdownOption,
  PrimaryButton,
  SelectionMode,
  Stack,
  TextField
} from 'office-ui-fabric-react';
import { useHistory } from "react-router";
import { IUpdateSubscriptionModel, Result } from '../../models';
import { Loading } from "../../shared/components/Loading";
import AlternateButton from "../../shared/components/AlternateButton";
import { Formik } from "formik";
import { ISubscriptionsModel } from '../../models/ISubscriptionsModel';
import SubscriptionsService from '../../services/SubscriptionsService';

import { toast } from "react-toastify";
import PlansService from '../../services/PlansService';
import { getInitialUpdateSubscriptionModel, subscriptionValidator } from "./formUtils/subscriptionFormUtils";
import adalContext from "../../adalConfig";
import { useGlobalContext } from '../../shared/components/GlobalProvider';
import { handleSubmissionErrorsForForm } from '../../shared/formUtils/utils';

interface IDetailsListDocumentsExampleState {
  columns: IColumn[];
  items: ISubscriptionsModel[];
}

const Subscriptions: React.FunctionComponent = () => {
  const history = useHistory();
  const appdiv = document.getElementsByClassName('App')[0] as HTMLElement;
  const [subscription, setsubscription] = useState<ISubscriptionsModel[]>([]);
  const [state, setstate] = useState<IDetailsListDocumentsExampleState>();
  const [formError, setFormError] = useState<string | null>(null);
  const [loadingSubscription, setLoadingSubscription] = useState<boolean>(true);
  const [loadStatus, setLoadStatus] = useState<boolean>(true);
  const [statusList, setStatusList] = useState<IDropdownOption[]>([]);
  const [planList, setPlanList] = useState<IDropdownOption[]>([]);
  const [dialogVisible, setDialogVisible] = useState<boolean>(false);
  const [loadingSubcriptionPost, setloadingSubcriptionPost] = useState<boolean>(true);
  const [subscriptionPost, setSubscriptionPost] = useState<IUpdateSubscriptionModel>(getInitialUpdateSubscriptionModel());
  const globalContext = useGlobalContext();

  let usr = adalContext.AuthContext.getCachedUser();
  let ownerEmail = "";
  if (usr && usr.profile) {
    if (usr.userName)
      ownerEmail = usr.userName;
  }

  const _onColumnClick = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
    if (column.key != 'operation') {
      const { columns, items } = state as IDetailsListDocumentsExampleState;
      const newColumns: IColumn[] = columns.slice();
      const currColumn: IColumn = newColumns.filter(currCol => column.key === currCol.key)[0];
      newColumns.forEach((newCol: IColumn) => {
        if (newCol === currColumn) {
          currColumn.isSortedDescending = !currColumn.isSortedDescending;
          currColumn.isSorted = true;
        } else {
          newCol.isSorted = false;
          newCol.isSortedDescending = true;
        }
      });
      const newItems = _copyAndSort(items, currColumn.fieldName!, currColumn.isSortedDescending);
      setstate({ items: newItems, columns: newColumns });
    }
  };

  function _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
  }

  const columns: IColumn[] = [
    {
      key: 'name',
      name: 'Name',
      className: '',
      iconClassName: 'iconright',
      ariaLabel: '',
      isIconOnly: false,
      fieldName: 'name',
      minWidth: 175,
      maxWidth: 175,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: false,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsModel) => {
        if (item.status === 'Subscribed' && item.entryPointUrl != "") {
          return <span><a target="_blank" href={item.entryPointUrl}>{item.name}</a></span>;
        }
        else
        {
          return <span>{item.name}</span>;
        }
      }
    },
    {
      key: 'subscribeid',
      name: 'Subscription ID',
      className: '',
      iconClassName: 'iconright',
      ariaLabel: '',
      isIconOnly: false,
      fieldName: 'subscriptionId',
      minWidth: 270,
      maxWidth: 270,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: false,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsModel) => {
        return <span>{item.subscriptionId}</span>;
      }
    },
    {
      key: 'offerid',
      name: 'Offer Id',
      className: '',
      iconClassName: 'iconright',
      ariaLabel: '',
      isIconOnly: false,
      fieldName: 'offerId',
      minWidth: 210,
      maxWidth: 350,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: false,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsModel) => {
        return <span>{item.offerName}</span>;
      }
    },
    {
      key: 'planid',
      name: 'Plan Id',
      className: '',
      iconClassName: 'iconright',
      ariaLabel: '',
      isIconOnly: false,
      fieldName: 'planId',
      minWidth: 100,
      maxWidth: 100,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: false,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsModel) => {
        return <span>{item.planName}</span>;
      }
    },
    {
      key: 'quantity',
      name: 'Quantity',
      className: '',
      iconClassName: 'iconright',
      ariaLabel: '',
      isIconOnly: false,
      fieldName: 'quantity',
      minWidth: 50,
      maxWidth: 50,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: false,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsModel) => {
        return <span>{item.quantity}</span>;
      }
    },
    {
      key: 'status',
      name: 'Status',
      className: '',
      iconClassName: 'iconright',
      ariaLabel: '',
      isIconOnly: false,
      fieldName: 'status',
      minWidth: 100,
      maxWidth: 100,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: false,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsModel) => {
        return <span>{item.status}</span>;
      }
    },
    {
      key: 'operation',
      name: 'Action',
      className: '',
      iconClassName: 'iconright',
      ariaLabel: '',
      isIconOnly: false,
      isRowHeader: true,
      isResizable: true,
      isSorted: false,
      fieldName: '',
      minWidth: 100,
      maxWidth: 100,
      onRender: (item: ISubscriptionsModel) => {
        return (
          <Stack
            verticalAlign="center"
            horizontalAlign={"space-evenly"}
            gap={15}
            horizontal={true}
            style={{ float: 'left' }}
            styles={{
              root: {}
            }}
          >

            {item.status == "Subscribed" && item.provisioningStatus == "Succeeded" ?
              <FontIcon style={{ lineHeight: '20px' }} iconName="Edit" className="deleteicon" onClick={() => {
                updatePlan(item.subscriptionId, item.name, item)
              }} /> : null}

            <FontIcon style={{ lineHeight: '20px' }} iconName="History" className="deleteicon" onClick={() => {
              showHistory(item.subscriptionId)
            }} />

            {item.status == "Subscribed" && item.provisioningStatus == "Succeeded" ?
              <FontIcon style={{ lineHeight: '20px' }} iconName="Cancel" className="deleteicon" onClick={() => {
                deleteSubscription(item.subscriptionId, item.name)
              }} /> : null}
          </Stack>
        )
      }
    },
  ];

  const handleSubmissionErrors = (result: Result<any>): boolean => {
    if (!result.success) {
      if (result.hasErrors)
        toast.error(result.errors.join(', '));

      return true;
    }
    return false;
  }

  const getFormErrorString = (touched, errors, property: string) => {
    return touched && errors && touched[property] && errors[property] ? errors[property] : '';
  };

  const getStatusList = async (statusarray: string[]) => {
    let statusDropDown: IDropdownOption[] = [];
    statusDropDown.push(
      { key: 'all', text: 'All' },
    )
    statusarray.map((value, index) => {
      statusDropDown.push(
        { key: value.toLowerCase(), text: value },
      )
    })
    setStatusList(statusDropDown);
  }

  const getSubscriptions = async () => {
    setLoadingSubscription(true);
    const results = await SubscriptionsService.list(ownerEmail as string);
    if (results && !results.hasErrors && results.value) {

      setLoadStatus(true);
      const map = new Map();
      let stringArray: string[] = [];
      for (const item of results.value.map(s => s.status)) {
        if (!map.has(item)) {
          map.set(item, true);    // set any value to Map
          stringArray.push(item);
        }
      }
      getStatusList(stringArray);
      setsubscription(results.value);
      setstate({ items: results.value, columns: columns });
    } else {
      setsubscription([]);
      setstate({ items: [], columns: columns });
      if (results.hasErrors) {
        // TODO: display errors
        alert(results.errors.join(', '));
      }
    }
    setLoadingSubscription(false);
    setLoadStatus(false);
  }

  const getPlans = async (offername) => {

    let planList: IDropdownOption[] = [{
      key: '',
      text: ''
    }];
    let PlanResponse = await PlansService.list(offername);
    if (PlanResponse.value && PlanResponse.success) {
      var Plans = PlanResponse.value;
      Plans.map((item, index) => {
        planList.push(
          {
            key: item.planName,
            text: item.planName
          })
      })
    }
    setPlanList([...planList]);
  }

  useEffect(() => {
    //appdiv.classList.add('mainsubscriptionlist');
    getSubscriptions();

    return () => {
      console.log('will unmount');
      //appdiv.classList.remove('mainsubscriptionlist');
    }
  }, []);

  const updatePlan = async (subscriptionId: string, subscriptionName: string, selectedSubscription: ISubscriptionsModel) => {
    await getPlans(selectedSubscription.offerName)
    setloadingSubcriptionPost(true);
    console.log('openconfirmCancellationPopUp: ' + subscriptionId);
    setSubscriptionPost(
      {
        SubscriptionId: subscriptionId,
        CurrentPlanName: selectedSubscription.planName,
        PlanName: "",
        OfferName: selectedSubscription.offerName,
        SubscriptionName: subscriptionName,
        SubscriptionVerifiedName: "",
        isUpdatePlan: true
      })
    setloadingSubcriptionPost(false);
    showDialog();
  };

  const showHistory = (subscriptionId: string) => {
    history.push('operationhistory/' + subscriptionId)
  };

  const deleteSubscription = async (subscriptionId: string, subscriptionName: string) => {
    setloadingSubcriptionPost(true);
    subscriptionPost.SubscriptionId = subscriptionId;
    subscriptionPost.SubscriptionName = subscriptionName;
    subscriptionPost.isUpdatePlan = false;
    showDialog();
    setSubscriptionPost(subscriptionPost);
    setloadingSubcriptionPost(false);
  };

  const _getKey = (item: any, index?: number) => {
    return item.key;
  }

  const _onItemInvoked = (item: any) => {
    alert(`Item invoked: ${item.name}`);
  }


  const selectOnChange = (fieldKey: string, setFieldValue, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    if (option) {
      let key = (option.key as string);
      setFieldValue(fieldKey, key, true);
      subscriptionPost.PlanName = key;

      setSubscriptionPost(subscriptionPost);
    }
  };

  const showDialog = (): void => {
    setDialogVisible(true);
  };

  const hideDialog = (): void => {
    setDialogVisible(false);
  };

  const triggerSubmitButton = () => {
    const btnsubmit = document.getElementById('btnsubmit') as HTMLButtonElement;
    btnsubmit.click();
  }

  return (
    <React.Fragment>
      <Stack
        verticalAlign="start"
        horizontal={false}
        styles={{
          root: {
            margin: '0 auto',
            backgroundColor: 'white',
            width: '87%'
          }
        }}
      >
        <Stack
          horizontalAlign="start"
          verticalAlign="center"
          styles={{
            root: {
              width: '100%',
              padding: ' 50px'
            }
          }}
        >
          {loadingSubscription ?
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
            <React.Fragment>
              <div className="subscriptiontitle">
                <span style={{ fontSize: 18, color: '#333333', fontFamily: 'SegoeUI' }}>All Offer Subscriptions</span>
              </div>
              <div className="subscriptionlist">
                <DetailsList
                  items={state != null ? state.items : []}
                  compact={false}
                  columns={state != null ? state.columns : []}
                  selectionMode={SelectionMode.none}
                  getKey={_getKey}
                  setKey="none"
                  layoutMode={DetailsListLayoutMode.justified}
                  isHeaderVisible={true}
                  onItemInvoked={_onItemInvoked}
                  onColumnHeaderClick={(event, column) => {
                    _onColumnClick(event as React.MouseEvent<HTMLElement>, column as IColumn)
                  }}
                />
                <Dialog
                  className="updateplanmodal"
                  hidden={!dialogVisible}
                  onDismiss={hideDialog}                  
                  dialogContentProps={{
                    styles: {
                      subText: {
                        paddingTop: 0
                      },
                      title: {
                        paddingBottom: 0,
                        fontWeight: 'normal'
                      }

                    },
                    type: DialogType.normal,
                    title: subscriptionPost.isUpdatePlan ? 'Update Plan' : 'Confirm Cancellation'
                  }}
                  modalProps={{
                    isBlocking: true,
                    isDarkOverlay:true,
                    styles: {

                      main: {
                        minWidth: 440
                      }
                    }
                  }}
                >
                  <DialogContent>
                    {loadingSubscription ?
                      <Loading />
                      :
                      <Formik
                        initialValues={subscriptionPost}
                        validationSchema={subscriptionValidator}
                        enableReinitialize={true}
                        validateOnBlur={true}
                        onSubmit={async (values, { setSubmitting, setErrors }) => {
                          globalContext.showProcessing();
                          const input = { ...values };
                          console.log('submitted form:');
                          console.log(input);

                          // //Update plan
                          if (values.isUpdatePlan) {

                            let createSubscriptionsResult = await SubscriptionsService.update(values);
                            if (handleSubmissionErrorsForForm(setErrors,setSubmitting,setFormError,'subscription',createSubscriptionsResult)) {
                              globalContext.hideProcessing();
                              return;
                            }


                          } else {
                            var paramDeleteResult = await SubscriptionsService.delete(values.SubscriptionId);
                            if (handleSubmissionErrorsForForm(setErrors,setSubmitting,setFormError,'subscription',paramDeleteResult)) {
                              globalContext.hideProcessing();
                              return;
                            }
                          }

                          getSubscriptions();
                          hideDialog();
                          globalContext.hideProcessing();
                          toast.success("Success !");
                          setSubmitting(false);
                        }}
                      >
                        {({ isSubmitting, setFieldValue, handleChange, values, handleBlur, touched, errors, resetForm, handleSubmit }) => (
                          <form autoComplete={"off"} onSubmit={handleSubmit}>
                            <table>
                              <tbody>
                                <tr>
                                  <td>
                                    <b>ID:</b>
                                  </td>
                                  <td>
                                    <span>{values.SubscriptionId} </span>
                                  </td>
                                </tr>
                                <tr>
                                  <td>
                                    <b>Subscription Name: </b>
                                  </td>
                                  <td>
                                    <span>{values.SubscriptionName}</span>
                                  </td>
                                </tr>

                                {
                                  values.isUpdatePlan ?
                                    <React.Fragment>
                                      <tr>
                                        <td>
                                          <b>Current Plan: </b>
                                        </td>
                                        <td>
                                          <span>{values.CurrentPlanName}</span>
                                        </td>
                                      </tr>
                                      <tr>
                                        <td colSpan={2}>
                                          <span>Available Plans</span>
                                          <br />
                                          <Dropdown options={planList} id={`PlanName`}
                                            onBlur={handleBlur}
                                            placeHolder="Choose a new plan"
                                            errorMessage={getFormErrorString(touched, errors, 'PlanName')}
                                            onChange={(event, option, index) => {
                                              selectOnChange(`PlanName`, setFieldValue, event, option, index);
                                            }} defaultSelectedKey="" />
                                        </td>
                                      </tr>
                                    </React.Fragment>
                                    :
                                    <React.Fragment>
                                      <tr>
                                        <td colSpan={2}>
                                          <span>Type the subscription name</span>
                                          <br />
                                          <TextField
                                            name={'SubscriptionVerifiedName'}
                                            onChange={handleChange}
                                            onBlur={handleBlur}
                                            errorMessage={getFormErrorString(touched, errors, 'SubscriptionVerifiedName')}
                                            placeholder={'Subscription Name'}
                                            className="txtFormField" />
                                        </td>
                                      </tr>
                                    </React.Fragment>
                                }
                              </tbody>
                            </table>
                            <div style={{ display: 'none' }}>
                              <PrimaryButton type="submit" id="btnsubmit" text="Save" />
                            </div>
                          </form>
                        )}
                      </Formik>
                    }
                  </DialogContent>
                  <DialogFooter>
                    <AlternateButton onClick={hideDialog} text="Not Now" />
                    <PrimaryButton text="Confirm" onClick={triggerSubmitButton} />
                  </DialogFooter>
                </Dialog>
              </div>
            </React.Fragment>
          }

        </Stack>
      </Stack>
    </React.Fragment>
  );
}
export default Subscriptions;