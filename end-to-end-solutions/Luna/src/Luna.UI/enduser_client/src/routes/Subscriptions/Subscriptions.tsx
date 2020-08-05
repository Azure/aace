import React, {useEffect, useState} from 'react';
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
import {useHistory} from "react-router";
import {IError, IUpdateSubscriptionModel} from '../../models';
import {Loading} from "../../shared/components/Loading";
import AlternateButton from "../../shared/components/AlternateButton";
import {Formik} from "formik";
import {
  ISubscriptionsModel,
  ISubscriptionsV2Model,
  ISubscriptionsV2RefreshKeyModel
} from '../../models/ISubscriptionsModel';
import SubscriptionsService from '../../services/SubscriptionsService';

import {toast} from "react-toastify";
import PlansService from '../../services/PlansService';
import {
  getInitialSubscriptionV2,
  getInitialUpdateSubscriptionModel,
  subscriptionValidator
} from "./formUtils/subscriptionFormUtils";
import adalContext from "../../adalConfig";
import {useGlobalContext} from '../../shared/components/GlobalProvider';
import {handleSubmissionErrorsForForm} from '../../shared/formUtils/utils';
import FormLabel from '../../shared/components/FormLabel';
import {CopyToClipboard} from 'react-copy-to-clipboard';
import { SubscriptionV2 } from '../../shared/constants/infomessages';

interface IDetailsListDocumentsExampleState {
  columns: IColumn[];
  items: ISubscriptionsModel[];
  itemsV2: ISubscriptionsV2Model[];
}

const Subscriptions: React.FunctionComponent = () => {
  const history = useHistory();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [subscription, setsubscription] = useState<ISubscriptionsModel[]>([]);
  const [state, setstate] = useState<IDetailsListDocumentsExampleState>();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formError, setFormError] = useState<string | null>(null);
  const [loadingSubscription, setLoadingSubscription] = useState<boolean>(true);
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [loadStatus, setLoadStatus] = useState<boolean>(true);
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [statusList, setStatusList] = useState<IDropdownOption[]>([]);
  const [planList, setPlanList] = useState<IDropdownOption[]>([]);
  const [dialogVisible, setDialogVisible] = useState<boolean>(false);
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [loadingSubcriptionPost, setloadingSubcriptionPost] = useState<boolean>(true);
  const [subscriptionPost, setSubscriptionPost] = useState<IUpdateSubscriptionModel>(getInitialUpdateSubscriptionModel());
  const globalContext = useGlobalContext();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [subscriptionV2, setsubscriptionV2] = useState<ISubscriptionsV2Model[]>([]);
  const [subscriptionV2Selected, setsubscriptionV2Selected] = useState<ISubscriptionsV2Model>(getInitialSubscriptionV2());
  const [Subscriptionv2DialogVisible, setSubscriptionv2DialogVisible] = useState<boolean>(false);
  const [subscriptionv2PrimaryKey, setSubscriptionv2PrimaryKey] = useState<string>('');
  const [subscriptionv2SecondaryKey, setSubscriptionv2SecondaryKey] = useState<string>('');
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [displayPrimaryKey, setDisplayPrimaryKey] = useState<boolean>(false);
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [displaySecondaryKey, setDisplaySecondaryKey] = useState<boolean>(false);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const v1Enabled = (window.Configs.ENABLE_V1.toLowerCase() === 'true' ? true : false);
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const v2Enabled = (window.Configs.ENABLE_V2.toLowerCase() === 'true' ? true : false);

  let usr = adalContext.AuthContext.getCachedUser();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  let ownerEmail = "";
  if (usr && usr.profile) {
    if (usr.userName)
      ownerEmail = usr.userName;
  }

  const _onColumnClick = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
    if (column.key !== 'operation') {
      const {columns, items, itemsV2} = state as IDetailsListDocumentsExampleState;
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
      const newItemsv1 = _copyAndSort(items, currColumn.fieldName!, currColumn.isSortedDescending)

      setstate({itemsV2: [], items: newItemsv1, columns: newColumns});
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
        /*if (item.status === 'Subscribed' && item.entryPointUrl !== "") {
          return <span><a rel="noopener noreferrer" target="_blank" href={item.entryPointUrl}>{item.name}</a></span>;
        } else {
          return <span>{item.name}</span>;
        }*/
        if (item.status.toLowerCase() === "subscribed") {
          return <span style={{cursor: 'pointer', color: 'rgb(0, 120, 212)'}}
                       onClick={() => {
                         editdetailsV2(item.subscriptionId)
                       }}>{item.name}</span>;
        } else {
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
            style={{float: 'left'}}
            styles={{
              root: {}
            }}
          >

            {item.status === "Subscribed" && item.provisioningStatus === "Succeeded" ?
              <FontIcon style={{lineHeight: '20px'}} iconName="Edit" className="deleteicon" onClick={() => {
                updatePlan(item.subscriptionId, item.name, item)
              }}/> : null}

            <FontIcon style={{lineHeight: '20px'}} iconName="History" className="deleteicon" onClick={() => {
              showHistory(item.subscriptionId)
            }}/>

            {item.status === "Subscribed" && item.provisioningStatus === "Succeeded" ?
              <FontIcon style={{lineHeight: '20px'}} iconName="Cancel" className="deleteicon" onClick={() => {
                deleteSubscription(item.subscriptionId, item.name)
              }}/> : null}
          </Stack>
        )
      }
    },
  ];

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const columnsV2: IColumn[] = [
    {
      key: 'subscriptionName',
      name: 'Name',
      className: '',
      iconClassName: '',
      ariaLabel: '',
      iconName: '',
      isIconOnly: false,
      fieldName: 'subscriptionName',
      minWidth: 100,
      maxWidth: 100,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: true,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsV2Model) => {
        if (item.status.toLowerCase() === "subscribed") {
          return <span style={{cursor: 'pointer', color: 'rgb(0, 120, 212)'}}
                       onClick={() => {
                         editdetailsV2(item.subscriptionId)
                       }}>{item.subscriptionName}</span>;
        } else {
          return <span>{item.subscriptionName}</span>;
        }
      }
    },
    {
      key: 'subscribeid',
      name: 'Subscription ID',
      className: '',
      iconClassName: '',
      ariaLabel: '',
      iconName: '',
      isIconOnly: false,
      fieldName: 'subscriptionId',
      minWidth: 210,
      maxWidth: 350,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: true,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsV2Model) => {
        return <span>{item.subscriptionId}</span>;
      }
    },
    {
      key: 'productName',
      name: 'Product Name',
      className: '',
      iconClassName: '',
      ariaLabel: '',
      iconName: '',
      isIconOnly: false,
      fieldName: 'productName',
      minWidth: 100,
      maxWidth: 100,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: true,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsV2Model) => {
        return <span>{item.productName}</span>;
      }
    },
    {
      key: 'deploymentName',
      name: 'Deployment Name',
      className: '',
      iconClassName: '',
      ariaLabel: '',
      iconName: '',
      isIconOnly: false,
      fieldName: 'deploymentNamed',
      minWidth: 100,
      maxWidth: 100,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: true,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsV2Model) => {
        return <span>{item.deploymentName}</span>;
      }
    },
    {
      key: 'status',
      name: 'Status',
      className: '',
      iconClassName: '',
      ariaLabel: '',
      iconName: '',
      isIconOnly: false,
      fieldName: 'status',
      minWidth: 100,
      maxWidth: 100,
      data: 'string',
      isRowHeader: true,
      isResizable: true,
      isSorted: true,
      isSortedDescending: false,
      sortAscendingAriaLabel: 'Sorted A to Z',
      sortDescendingAriaLabel: 'Sorted Z to A',
      isPadded: true,
      onRender: (item: ISubscriptionsV2Model) => {
        return <span>{item.status}</span>;
      }
    },
    // {
    //     key: 'baseUrl',
    //     name: 'Base Url',
    //     className: '',
    //     iconClassName: '',
    //     ariaLabel: '',
    //     iconName: '',
    //     isIconOnly: false,
    //     fieldName: 'baseUrl',
    //     minWidth: 210,
    //     maxWidth: 350,
    //     data: 'string',
    //     isRowHeader: true,
    //     isResizable: true,
    //     isSorted: true,
    //     isSortedDescending: false,
    //     sortAscendingAriaLabel: 'Sorted A to Z',
    //     sortDescendingAriaLabel: 'Sorted Z to A',
    //     isPadded: true,
    //     onRender: (item: ISubscriptionsV2Model) => {
    //         return <span>{item.baseUrl}</span>;
    //     }
    // },
    // {
    //     key: 'primaryKey',
    //     name: 'Primary Key',
    //     className: '',
    //     iconClassName: '',
    //     ariaLabel: '',
    //     iconName: '',
    //     isIconOnly: false,
    //     fieldName: 'primaryKey',
    //     minWidth: 100,
    //     maxWidth: 100,
    //     data: 'string',
    //     isRowHeader: true,
    //     isResizable: true,
    //     isSorted: true,
    //     isSortedDescending: false,
    //     sortAscendingAriaLabel: 'Sorted A to Z',
    //     sortDescendingAriaLabel: 'Sorted Z to A',
    //     isPadded: true,
    //     onRender: (item: ISubscriptionsV2Model) => {
    //         return <span>{item.primaryKey}</span>;
    //     }
    // },
    // {
    //     key: 'secondaryKey',
    //     name: 'Secondary Key',
    //     className: '',
    //     iconClassName: '',
    //     ariaLabel: '',
    //     iconName: '',
    //     isIconOnly: false,
    //     fieldName: 'secondaryKey',
    //     minWidth: 100,
    //     maxWidth: 100,
    //     data: 'string',
    //     isRowHeader: true,
    //     isResizable: true,
    //     isSorted: true,
    //     isSortedDescending: false,
    //     sortAscendingAriaLabel: 'Sorted A to Z',
    //     sortDescendingAriaLabel: 'Sorted Z to A',
    //     isPadded: true,
    //     onRender: (item: ISubscriptionsV2Model) => {
    //         return <span>{item.secondaryKey}</span>;
    //     }
    // },
    // {
    //   key: 'operation',
    //   name: 'Operation',
    //   className: '',
    //   iconClassName: '',
    //   ariaLabel: '',
    //   iconName: '',
    //   isIconOnly: false,
    //   isRowHeader: true,
    //   isResizable: true,
    //   isSorted: false,
    //   fieldName: '',
    //   minWidth: 100,
    //   maxWidth: 100,
    //   onRender: (item: ISubscriptionsV2Model) => {
    //     return (
    //       <Stack
    //         verticalAlign="center"
    //         horizontalAlign={"space-evenly"}
    //         gap={15}
    //         horizontal={true}
    //         style={{ float: 'left' }}
    //         styles={{
    //           root: {}
    //         }}
    //       >
    //         {/* {item.status == "Subscribed" ?
    //           <FontIcon style={{ lineHeight: '20px' }} iconName="Edit" className="deleteicon" onClick={() => {
    //             updatePlan(item.subscriptionId, item.name, item)
    //           }} /> : null}

    //         <FontIcon style={{ lineHeight: '20px' }} iconName="History" className="deleteicon" onClick={() => {
    //           showHistory(item.subscriptionId)
    //         }} />

    //         {item.status == "Subscribed" ?
    //           <FontIcon style={{ lineHeight: '20px' }} iconName="Cancel" className="deleteicon" onClick={() => {
    //             deleteSubscription(item.subscriptionId, item.name)
    //           }} /> : null} */}
    //         <FontIcon style={{ lineHeight: '20px' }} iconName="Edit" className="deleteicon" />
    //       </Stack>
    //     )
    //   }
    // },
  ];

  // const handleSubmissionErrors = (result: Result<any>): boolean => {
  //   if (!result.success) {
  //     if (result.hasErrors)
  //       toast.error(result.errors.join(', '));

  //     return true;
  //   }
  //   return false;
  // }

  const getFormErrorString = (touched, errors, property: string) => {
    return touched && errors && touched[property] && errors[property] ? errors[property] : '';
  };

  const getStatusList = async (statusarray: string[]) => {
    let statusDropDown: IDropdownOption[] = [];
    statusDropDown.push(
      {key: 'all', text: 'All'},
    )
    statusarray.map((value, index) => {
      return (
        statusDropDown.push(
          {key: value.toLowerCase(), text: value},
        ))
    })
    setStatusList(statusDropDown);
  }

  const getSubscriptions = async () => {
    setLoadingSubscription(true);
    let results: any;
    results = await SubscriptionsService.list(ownerEmail as string);

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
      setstate({
        items: results.value,
        columns: columns,
        itemsV2: []
      });
    } else {
      setsubscription([]);
      setsubscriptionV2([]);
      setstate({itemsV2: [], items: [], columns: columns});

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
        return (
          planList.push(
            {
              key: item.planName,
              text: item.planName
            })
        )
      })
    }
    setPlanList([...planList]);
  }

  useEffect(() => {
    getSubscriptions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
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
    //alert(`Item invoked: ${item.name}`);
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

  const hideSubscriptionv2Dialog = (): void => {
    setSubscriptionv2DialogVisible(false);
  };

  const convertToAsterisk = (value: string): string => {
    let returnvalue = '';
    for (let index = 0; index < value.length; index++) {
      returnvalue = returnvalue + '*';
    }
    return returnvalue;
  }

  const editdetailsV2 = async (subscriptionId: string) => {

    const dataResponse = await SubscriptionsService.getV2(subscriptionId);
    // // Global errors should have already been handled for get requests by this point
    if (dataResponse.value && dataResponse.success) {
      var data = dataResponse.value as ISubscriptionsV2Model;
      setSubscriptionv2PrimaryKey(convertToAsterisk(data.primaryKey));
      setSubscriptionv2SecondaryKey(convertToAsterisk(data.secondaryKey));
      setsubscriptionV2Selected(data);
      setSubscriptionv2DialogVisible(true);
    } else {
      let errorMessages: IError[] = [];

      errorMessages.concat(dataResponse.errors);

      if (errorMessages.length > 0) {
        toast.error(errorMessages.join(', '));
      }
    }
  };

  const showKey = (key: string, subscriptionV2Selected: ISubscriptionsV2Model) => {
    let convertedstring = '';
    if (key === "primaryKey") {
      convertedstring = convertToAsterisk(subscriptionV2Selected.primaryKey);
      if (subscriptionv2PrimaryKey === convertedstring) {
        setDisplayPrimaryKey(true);
        setSubscriptionv2PrimaryKey(subscriptionV2Selected.primaryKey);
      } else {
        setDisplayPrimaryKey(false);
        setSubscriptionv2PrimaryKey(convertedstring);
      }
    } else {
      convertedstring = convertToAsterisk(subscriptionV2Selected.secondaryKey);
      if (subscriptionv2SecondaryKey === convertedstring) {
        setDisplaySecondaryKey(true);
        setSubscriptionv2SecondaryKey(subscriptionV2Selected.secondaryKey);
      } else {
        setDisplaySecondaryKey(false);
        setSubscriptionv2SecondaryKey(convertedstring);
      }
    }
  }

  const RegenerateKey = async (subscriptionId: string, key: string) => {

    globalContext.showProcessing();

    let subscriptionsV2RefreshKeyModel: ISubscriptionsV2RefreshKeyModel = {keyName: '', subscriptionId: ''};
    subscriptionsV2RefreshKeyModel.subscriptionId = subscriptionId;
    subscriptionsV2RefreshKeyModel.keyName = key;
    let convertedstring = '';
    let results = await SubscriptionsService.RefreshKey(subscriptionsV2RefreshKeyModel);
    if (results && !results.hasErrors && results.value) {
      setsubscriptionV2Selected(results.value);
      if (key === "primaryKey") {
        convertedstring = convertToAsterisk(results.value.primaryKey);
        displayPrimaryKey ? setSubscriptionv2PrimaryKey(results.value.primaryKey) : setSubscriptionv2PrimaryKey(convertedstring);
      } else {
        convertedstring = convertToAsterisk(results.value.secondaryKey);
        displayPrimaryKey ? setSubscriptionv2SecondaryKey(results.value.secondaryKey) : setSubscriptionv2SecondaryKey(convertedstring);
      }
    }
    globalContext.hideProcessing();
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
              <Loading/>
            </Stack>
            :
            <React.Fragment>
              <div className="subscriptiontitle">
                <span style={{fontSize: 18, color: '#333333', fontFamily: 'SegoeUI'}}>All Offer Subscriptions</span>
              </div>
              <div className="subscriptionlist">
                {
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
                }
                <Dialog
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
                    className: "updateplanmodal",
                    isBlocking: true,
                    isDarkOverlay: true,
                    styles: {

                      main: {
                        minWidth: 440
                      }
                    }
                  }}
                >
                  <DialogContent>
                    {loadingSubscription ?
                      <Loading/>
                      :
                      <Formik
                        initialValues={subscriptionPost}
                        validationSchema={subscriptionValidator}
                        enableReinitialize={true}
                        validateOnBlur={true}
                        onSubmit={async (values, {setSubmitting, setErrors}) => {
                          globalContext.showProcessing();
                          const input = {...values};
                          console.log('submitted form:');
                          console.log(input);

                          // //Update plan
                          if (values.isUpdatePlan) {

                            let createSubscriptionsResult = await SubscriptionsService.update(values);
                            if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'subscription', createSubscriptionsResult)) {
                              globalContext.hideProcessing();
                              return;
                            }


                          } else {
                            var paramDeleteResult = await SubscriptionsService.delete(values.SubscriptionId);
                            if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'subscription', paramDeleteResult)) {
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
                        {({isSubmitting, setFieldValue, handleChange, values, handleBlur, touched, errors, resetForm, handleSubmit}) => (
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
                                        <br/>
                                        <Dropdown options={planList} id={`PlanName`}
                                                  onBlur={handleBlur}
                                                  placeHolder="Choose a new plan"
                                                  errorMessage={getFormErrorString(touched, errors, 'PlanName')}
                                                  onChange={(event, option, index) => {
                                                    selectOnChange(`PlanName`, setFieldValue, event, option, index);
                                                  }} defaultSelectedKey=""/>
                                      </td>
                                    </tr>
                                  </React.Fragment>
                                  :
                                  <React.Fragment>
                                    <tr>
                                      <td colSpan={2}>
                                        <span>Type the subscription name</span>
                                        <br/>
                                        <TextField
                                          name={'SubscriptionVerifiedName'}
                                          onChange={handleChange}
                                          onBlur={handleBlur}
                                          errorMessage={getFormErrorString(touched, errors, 'SubscriptionVerifiedName')}
                                          placeholder={'Subscription Name'}
                                          className="txtFormField"/>
                                      </td>
                                    </tr>
                                  </React.Fragment>
                              }
                              </tbody>
                            </table>
                            <div style={{display: 'none'}}>
                              <PrimaryButton type="submit" id="btnsubmit" text="Save"/>
                            </div>
                          </form>
                        )}
                      </Formik>
                    }
                  </DialogContent>
                  <DialogFooter>
                    <AlternateButton onClick={hideDialog} text="Not Now"/>
                    <PrimaryButton text="Confirm" onClick={triggerSubmitButton}/>
                  </DialogFooter>
                </Dialog>
                <Dialog
                  hidden={!Subscriptionv2DialogVisible}
                  onDismiss={hideSubscriptionv2Dialog}

                  dialogContentProps={{
                    styles: {
                      subText: {
                        paddingTop: 0
                      },
                      title: {}

                    },
                    type: DialogType.close,
                    title: 'Subscription'
                  }}
                  modalProps={{
                    isDarkOverlay: true,
                    isBlocking: true,
                    styles: {
                      main: {
                        minWidth: '40% !important',
                      }
                    }
                  }}
                >
                  <React.Fragment>
                    <div id="subscriptionv2">
                      <Stack className={"form_row"}>
                        <FormLabel title={"Name:"} toolTip={SubscriptionV2.Subscription.subscriptionName}/>
                        <TextField
                          name={'subscriptionName'}
                          value={subscriptionV2Selected.subscriptionName}
                          readOnly={true}/>
                      </Stack>
                      <Stack className={"form_row"}>
                        <FormLabel title={"ID:"}  toolTip={SubscriptionV2.Subscription.ID}/>
                        <TextField
                          name={'subscriptionId'}
                          value={subscriptionV2Selected.subscriptionId}
                          readOnly={true}/>
                      </Stack>
                      <Stack className={"form_row"}>
                        <FormLabel title={"End Point:"}  toolTip={SubscriptionV2.Subscription.EndPoint}/>
                        <div style={{width: '100%'}}>
                          <div style={{width: '93%', float: 'left'}}>
                            <TextField
                              title={subscriptionV2Selected.baseUrl}
                              name={'baseUrl'}
                              value={subscriptionV2Selected.baseUrl ? subscriptionV2Selected.baseUrl : ''}
                              readOnly={true}
                              className={'subv2ipinput'}/>
                          </div>
                          <div style={{width: '5%', float: 'left', marginLeft: '2%'}}>
                            <CopyToClipboard text={subscriptionV2Selected.baseUrl}>
                              <FontIcon style={{lineHeight: '30px', fontSize: 20}} iconName="Tab"
                                        className='deleteicon baseurl subv2ipinputcopy' onClick={() => {
                                let secondaryKeyclass = document.getElementsByClassName('secondaryKey')[0] as HTMLElement;
                                let primaryKeyclass = document.getElementsByClassName('primaryKey')[0] as HTMLElement;
                                secondaryKeyclass.className = secondaryKeyclass.className.replace('copied', '');
                                primaryKeyclass.className = primaryKeyclass.className.replace('copied', '');

                                let copied = document.getElementsByClassName('baseurl')[0] as HTMLElement;
                                copied.className = copied.className + " copied";
                                toast.success("Copied !");

                                /*setTimeout(() => {
                                  let copied = document.getElementsByClassName('baseurl')[0] as HTMLElement;
                                  copied.className = copied.className.replace('copied', '');
                                }, 3000);*/
                              }}/>
                            </CopyToClipboard>
                          </div>
                        </div>
                      </Stack>
                      <Stack className={"form_row"}>
                        <FormLabel title={"Keys:"}  toolTip={SubscriptionV2.Subscription.Keys}/>
                        <table style={{lineHeight: 3}} id="keys">
                          <tbody>
                          <tr>
                            <td style={{width: '18%'}}>
                            <FormLabel title={"Primary:"}  toolTip={SubscriptionV2.Subscription.PrimaryKey}/>                              
                            </td>
                            <td>
                              <div style={{
                                width: '100%',
                                height: '40px'
                              }}>
                                <div style={{width: 'auto', maxWidth: '80%'}}>
                                  <TextField
                                    name={'primaryKey'}
                                    value={subscriptionv2PrimaryKey}
                                    readOnly={true}
                                    className={'subv2ipinput subv2ipinputw_90 keyborder'}/>
                                </div>
                                <Stack
                                  verticalAlign="center"
                                  horizontalAlign={"space-evenly"}
                                  gap={15}
                                  horizontal={true}
                                  styles={{
                                    root: {},
                                  }}
                                >
                                  <FontIcon style={{lineHeight: '40px'}} iconName="RedEye" className='deleteicon'
                                            onClick={() => {
                                              showKey('primaryKey', subscriptionV2Selected)
                                            }}/>
                                  <CopyToClipboard text={subscriptionV2Selected.primaryKey}>
                                    <FontIcon style={{lineHeight: '30px'}} iconName="Tab"
                                              className='deleteicon primaryKey' onClick={() => {
                                      let baseurlclass = document.getElementsByClassName('baseurl')[0] as HTMLElement;
                                      let secondarkeyclass = document.getElementsByClassName('secondaryKey')[0] as HTMLElement;
                                      baseurlclass.className = baseurlclass.className.replace('copied', '');
                                      secondarkeyclass.className = secondarkeyclass.className.replace('copied', '');

                                      let copied = document.getElementsByClassName('primaryKey')[0] as HTMLElement;
                                      copied.className = copied.className + " copied";
                                      toast.success("Copied !");
                                      /*setTimeout(() => {
                                        let copied = document.getElementsByClassName('primaryKey')[0] as HTMLElement;
                                        copied.className = copied.className.replace('copied', '');
                                      }, 3000);*/
                                    }}/>
                                  </CopyToClipboard>
                                  <FontIcon style={{lineHeight: '30px'}} iconName="Sync" className='deleteicon'
                                            onClick={() => {
                                              RegenerateKey(subscriptionV2Selected.subscriptionId, 'primaryKey')
                                            }}/>
                                </Stack>
                              </div>
                            </td>
                          </tr>
                          <tr>
                            <td>
                            <FormLabel title={"Secondary:"}  toolTip={SubscriptionV2.Subscription.SecondaryKey}/>
                            </td>
                            <td>
                              <div style={{
                                width: '100%',
                                height: '40px'
                              }}>
                                <div style={{width: 'auto', maxWidth: '80%'}}>
                                  <TextField
                                    name={'secondaryKey'}
                                    value={subscriptionv2SecondaryKey}
                                    readOnly={true}
                                    className={'subv2ipinput subv2ipinputw_90 keyborder'}/>
                                </div>
                                <Stack
                                  verticalAlign="center"
                                  horizontalAlign={"space-evenly"}
                                  gap={15}
                                  horizontal={true}
                                  styles={{
                                    root: {},
                                  }}
                                >
                                  <FontIcon style={{lineHeight: '40px'}} iconName="RedEye" className='deleteicon'
                                            onClick={() => {
                                              showKey('secondaryKey', subscriptionV2Selected)
                                            }}/>
                                  <CopyToClipboard text={subscriptionV2Selected.secondaryKey}>
                                    <FontIcon style={{lineHeight: '30px'}} iconName="Tab"
                                              className='deleteicon secondaryKey' onClick={() => {
                                      let baseurlclass = document.getElementsByClassName('baseurl')[0] as HTMLElement;
                                      let primaryKeyclass = document.getElementsByClassName('primaryKey')[0] as HTMLElement;
                                      baseurlclass.className = baseurlclass.className.replace('copied', '');
                                      primaryKeyclass.className = primaryKeyclass.className.replace('copied', '');

                                      let copied = document.getElementsByClassName('secondaryKey')[0] as HTMLElement;
                                      copied.className = copied.className + " copied";
                                      toast.success("Copied !");

                                      /*setTimeout(() => {
                                        let copied = document.getElementsByClassName('secondaryKey')[0] as HTMLElement;
                                        copied.className = copied.className.replace('copied', '');
                                      }, 3000);*/
                                    }}/>
                                  </CopyToClipboard>
                                  <FontIcon style={{lineHeight: '30px'}} iconName="Sync" className='deleteicon'
                                            onClick={() => {
                                              RegenerateKey(subscriptionV2Selected.subscriptionId, 'secondaryKey')
                                            }}/>
                                </Stack>
                              </div>
                            </td>
                          </tr>
                          </tbody>
                        </table>
                      </Stack>
                    </div>
                  </React.Fragment>
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