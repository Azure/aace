import React, { useEffect, useState } from 'react';
import {
    Stack,
    MessageBar,
    MessageBarType,
    Dialog, DialogType,
    FontIcon,
    DetailsList, DetailsListLayoutMode, SelectionMode, IColumn,
    TextField,
    IDropdownOption,
    Dropdown,    
} from 'office-ui-fabric-react';
import { useHistory } from "react-router";
import { Loading } from "../../shared/components/Loading";
import { ISubscriptionsModel, ISubscriptionsWarnings, ISubscriptionsV2Model } from '../../models/ISubscriptionsModel';
import SubscriptionsService from '../../services/SubscriptionsService';
import { getInitialSubscriptionsWarningsModel } from './formUtils/subscriptionFormUtils';
import { NavLink } from "react-router-dom";
//import { Formik } from 'formik';
// import AlternateButton from '../../shared/components/AlternateButton';
// import FormLabel from '../../shared/components/FormLabel';

interface IDetailsListDocumentsExampleState {
    columns: IColumn[];
    items: ISubscriptionsModel[];
    itemsV2: ISubscriptionsV2Model[];
}

const Subscriptions: React.FunctionComponent = () => {
    const history = useHistory();

    const v1Enabled = (window.Configs.ENABLE_V1.toLowerCase() === 'true' ? true : false);

    const [subscription, setsubscription] = useState<ISubscriptionsModel[]>([]);    
    const [state, setstate] = useState<IDetailsListDocumentsExampleState>();
    const [subscriptionWarnings, setsubscriptionWarnings] = useState<ISubscriptionsWarnings[]>(getInitialSubscriptionsWarningsModel);
    const [loadingSubscription, setLoadingSubscription] = useState<boolean>(true);
    const [loadStatus, setLoadStatus] = useState<boolean>(true);
    const [statusList, setStatusList] = useState<IDropdownOption[]>([]);
    const [loadingWarnings, setLoadingWarnings] = useState<boolean>(true);
    const [warningDetail, setwarningDetail] = useState<string>('');
    const [warningDialogVisible, setwarningDialogVisible] = useState<boolean>(false);
    const [subscriptionV2, setsubscriptionV2] = useState<ISubscriptionsV2Model[]>([]);

    const _onColumnClick = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
        if (column.key !== 'operation') {
            const { columns, items, itemsV2 } = state as IDetailsListDocumentsExampleState;
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
            const newItemsv2 = _copyAndSort(itemsV2, currColumn.fieldName!, currColumn.isSortedDescending);
            v1Enabled ? setstate({ itemsV2: [], items: newItemsv1, columns: newColumns })
                : setstate({ itemsV2: newItemsv2, items: [], columns: newColumns });
        }
    };

    function _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
        const key = columnKey as keyof T;
        return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
    }

    const columns: IColumn[] = [
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
            onRender: (item: ISubscriptionsModel) => {
                return <span>{item.subscriptionId}</span>;
            }
        },
        {
            key: 'name',
            name: 'Name',
            className: '',
            iconClassName: '',
            ariaLabel: '',
            iconName: '',
            isIconOnly: false,
            fieldName: 'name',
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
            onRender: (item: ISubscriptionsModel) => {
                return <span>{item.name}</span>;
            }
        },
        {
            key: 'offerid',
            name: 'Offer Id',
            className: '',
            iconClassName: '',
            ariaLabel: '',
            iconName: '',
            isIconOnly: false,
            fieldName: 'offerId',
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
            onRender: (item: ISubscriptionsModel) => {
                return <span>{item.offerName}</span>;
            }
        },
        {
            key: 'planid',
            name: 'Plan Id',
            className: '',
            iconClassName: '',
            ariaLabel: '',
            iconName: '',
            isIconOnly: false,
            fieldName: 'planId',
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
            onRender: (item: ISubscriptionsModel) => {
                return <span>{item.planName}</span>;
            }
        },
        {
            key: 'quantity',
            name: 'Quantity',
            className: '',
            iconClassName: '',
            ariaLabel: '',
            iconName: '',
            isIconOnly: false,
            fieldName: 'quantity',
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
            onRender: (item: ISubscriptionsModel) => {
                return <span>{item.quantity}</span>;
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
            onRender: (item: ISubscriptionsModel) => {
                return <span>{item.status}</span>;
            }
        },
        {
            key: 'operation',
            name: 'Operation',
            className: '',
            iconClassName: '',
            ariaLabel: '',
            iconName: '',
            isIconOnly: false,
            isRowHeader: true,
            isResizable: true,
            isSorted: false,
            fieldName: '',
            minWidth: 210,
            maxWidth: 350,
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
                        <FontIcon style={{ lineHeight: '20px' }} iconName="Edit" className="deleteicon" onClick={() => { editdetails(item.offerName, item.subscriptionId) }} />
                    </Stack>
                )
            }
        },
    ];

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
                // return <a style={{cursor:'pointer',color:'rgb(0, 120, 212)'}} onClick={() => { editdetailsV2(item.productName, item.subscriptionId) }}>{item.name}</a>;
                return <span>{item.subscriptionName}</span>;
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
        }
    ];

    const getStatusList = async (statusarray: string[]) => {
        let statusDropDown: IDropdownOption[] = [];
        statusDropDown.push(
            { key: 'all', text: 'All' },
        )
        statusarray.map((value, index) => {
            statusDropDown.push(
                { key: value.toLowerCase(), text: value },
            )
            return statusDropDown;
        })
        setStatusList(statusDropDown);
    }

    const getSubscriptions = async () => {
        setLoadingSubscription(true);

        let results: any;
        v1Enabled ? results = await SubscriptionsService.list() : results = await SubscriptionsService.listV2();

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
            v1Enabled ? setsubscription(results.value) : setsubscriptionV2(results.value);
            v1Enabled ? setstate({ items: results.value, columns: columns, itemsV2: [] }) : setstate({ items: [], itemsV2: results.value, columns: columnsV2 });
        }
        else {
            setsubscription([]);
            setsubscriptionV2([]);
            v1Enabled ? setstate({ itemsV2: [], items: [], columns: columns }) : setstate({ items: [], itemsV2: [], columns: columnsV2 });
            if (results.hasErrors) {
                // TODO: display errors
                alert(results.errors.join(', '));
            }
        }
        setLoadingSubscription(false);
        setLoadStatus(false);
    }

    const getSubscriptionWarnings = async () => {

        setLoadingWarnings(true);
        const results = await SubscriptionsService.getAllSubscriptionWarnings();
        if (results && results.value) {
            setsubscriptionWarnings([...results.value]);
        }
        else {
            setsubscriptionWarnings([]);
        }
        setLoadingWarnings(false);
    }

    useEffect(() => {
        getSubscriptions();
        getSubscriptionWarnings();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const editdetails = (offerName: string, subscriptionId: string): void => {
        history.push('SubscriptionDetail/' + offerName + '/' + subscriptionId);
    };

    const _getKey = (item: any, index?: number) => {
        return item.key;
    }

    const _onItemInvoked = (item: any) => {
        //alert(`Item invoked: ${item.name}`);
    }

    const _onChangeText = (ev: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, text?: string): void => {
        if (v1Enabled) {
            let data = subscription;
            let filterdata = text ? data.filter(i =>
              (i.offerName && i.offerName.toLowerCase().indexOf(text.toLowerCase()) > -1) ||
              (i.planName && i.planName.toLowerCase().indexOf(text.toLowerCase()) > -1) ||
              (i.quantity && i.quantity.toString().toLowerCase().indexOf(text.toLowerCase()) > -1) ||
              (i.subscriptionId && i.subscriptionId.toLowerCase().indexOf(text.toLowerCase()) > -1) ||
              (i.name && i.name.toLowerCase().indexOf(text.toLowerCase()) > -1)) : data
            setstate({ itemsV2: [], items: filterdata, columns: columns });
        }
        else {
            let data = subscriptionV2;
            let filterdata = text ? data.filter(i =>
              (i.productName && i.productName.toLowerCase().indexOf(text.toLowerCase()) > -1) ||
              (i.baseUrl && i.baseUrl.toLowerCase().indexOf(text.toLowerCase()) > -1) ||
              (i.deploymentName && i.deploymentName.toString().toLowerCase().indexOf(text.toLowerCase()) > -1) ||
              (i.subscriptionId && i.subscriptionId.toLowerCase().indexOf(text.toLowerCase()) > -1) ||
              (i.primaryKey && i.primaryKey.toLowerCase().indexOf(text.toLowerCase()) > -1) ||
              (i.secondaryKey && i.secondaryKey.toLowerCase().indexOf(text.toLowerCase()) > -1) ||
              (i.subscriptionName && i.subscriptionName.toLowerCase().indexOf(text.toLowerCase()) > -1)) : data
            setstate({ items: [], itemsV2: filterdata, columns: columnsV2 });
        }
    };

    const selectOnChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
        if (option) {
            let text = (option.key as string);

            if (text !== 'all') {
                let data: any = [];
                if (v1Enabled) {
                    data = subscription;
                }
                else {
                    data = subscriptionV2;
                }
                let filterdata = text ? data.filter(i => i.status.toLowerCase() === text.toLowerCase()) : data
                v1Enabled ? setstate({ itemsV2: [], items: filterdata, columns: columns })
                    : setstate({ itemsV2: filterdata, items: [], columns: columnsV2 });
            }
            else {
                v1Enabled ? setstate({ itemsV2: [], items: subscription, columns: columns })
                    : setstate({ itemsV2: subscriptionV2, items: [], columns: columnsV2 });
            }
        }
    };

    const handleDeleteWarning = (idx: number): void => {

        subscriptionWarnings.splice(idx, 1);
        setsubscriptionWarnings([...subscriptionWarnings]);
    }

    const showWarningDialog = (detail: string): void => {
        setwarningDialogVisible(true);
        setwarningDetail(detail);
    };

    const hideWarningDialog = (): void => {
        setwarningDialogVisible(false);
    };

    // const hideSubscriptionv2Dialog = (): void => {
    //     setSubscriptionv2DialogVisible(false);
    // };

    const SubscriptionWarnings = (): React.ReactElement | null => {
        if (loadingWarnings) {
            return (
                <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                    <Loading />
                </Stack>
            );
        } else {
            if (subscriptionWarnings.length === 0)
                return null;

            return (
                <React.Fragment>
                    <Stack
                        verticalAlign="center"
                        horizontal={false}
                        gap={10}
                        styles={{
                            root: {
                                marginBottom: 50,
                            }
                        }}
                    >
                        {subscriptionWarnings.map((value, idx) => {
                            return (
                                <MessageBar key={`subscriptionwarning_${idx}`} messageBarType={MessageBarType.severeWarning} isMultiline={true}
                                    dismissButtonAriaLabel="Close"
                                    onDismiss={() => {
                                        handleDeleteWarning(idx)
                                    }}>
                                    <span>Subscription Id:</span>{value.subscriptionId} - <span dangerouslySetInnerHTML={{ __html: value.warningMessage }}>
                                    </span> Click <NavLink to="#" onClick={e => { e.preventDefault(); showWarningDialog(value.details) }}>here</NavLink> for more details.
                                </MessageBar>
                            )
                        })}
                        <Dialog
                            hidden={!warningDialogVisible}
                            onDismiss={hideWarningDialog}
                            dialogContentProps={{
                                styles: {
                                    subText: {
                                        paddingTop: 0
                                    },
                                    title: {
                                    }

                                },
                                type: DialogType.normal,
                                title: 'Warning'
                            }}
                            modalProps={{
                                isBlocking: false,
                                isDarkOverlay: true,
                                styles: {
                                    main: {
                                        minWidth: 440
                                    }
                                }
                            }}
                        >
                            <span dangerouslySetInnerHTML={{ __html: warningDetail }}></span>
                        </Dialog>
                    </Stack>
                </React.Fragment>
            );
        }
    }

    return (
        <React.Fragment>
            <Stack
                verticalAlign="start"
                horizontal={false}
                styles={{
                    root: {
                    }
                }}
            >
                <SubscriptionWarnings />
                <Stack
                    horizontalAlign="start"
                    verticalAlign="center"
                    styles={{
                        root: {
                            width: '100%'
                        }
                    }}
                >
                    {loadingSubscription ?
                        <Loading />
                        :
                        <React.Fragment>
                            <table className="filterheader">
                                <tbody>
                                    <tr>
                                        <td>
                                            <TextField onChange={_onChangeText} placeholder="Search..." style={{ width: '300px' }} />
                                        </td>
                                        <td>
                                            {loadStatus ? <Loading /> :
                                                <Dropdown options={statusList} id={`statuslist`} onChange={(event, option) => {
                                                    selectOnChange(event, option);
                                                }} defaultSelectedKey={'all'} className="statusdrp" />}
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            <div className="subscriptionlist">
                                {
                                    v1Enabled ?
                                    //Subscriptionv1
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
                                            onColumnHeaderClick={(event, column) => { _onColumnClick(event as React.MouseEvent<HTMLElement>, column as IColumn) }}
                                        />                                         
                                        :
                                        //Subscriptionv2
                                        <DetailsList
                                            items={state != null ? state.itemsV2 : []}
                                            compact={false}
                                            columns={state != null ? state.columns : []}
                                            selectionMode={SelectionMode.none}
                                            getKey={_getKey}
                                            setKey="none"
                                            layoutMode={DetailsListLayoutMode.justified}
                                            isHeaderVisible={true}
                                            onItemInvoked={_onItemInvoked}
                                            onColumnHeaderClick={(event, columnsV2) => { _onColumnClick(event as React.MouseEvent<HTMLElement>, columnsV2 as IColumn) }}
                                        />
                                }
                            </div>
                        </React.Fragment>
                    }

                </Stack>
            </Stack>
            {/* <Dialog
                hidden={!Subscriptionv2DialogVisible}
                onDismiss={hideSubscriptionv2Dialog}

                dialogContentProps={{
                    styles: {
                        subText: {
                            paddingTop: 0
                        },
                        title: {
                        }

                    },
                    type: DialogType.normal,
                    title: 'Subscription'
                }}
                modalProps={{
                    isBlocking: true,
                    styles: {
                        main: {
                            minWidth: '40% !important',
                        }
                    }
                }}
            >
                <React.Fragment>
                    <Stack className={"form_row"}>
                        <FormLabel title={"ID:"} />
                        <TextField
                            name={'subscriptionId'}
                            value={subscriptionV2Selected.subscriptionId}
                            maxLength={50}
                            readOnly={true}
                            className={'form_textbox'} />
                    </Stack>
                    <Stack className={"form_row"}>
                        <FormLabel title={"End Point:"} />
                        <TextField
                            name={'baseUrl'}
                            value={subscriptionV2Selected.baseUrl}
                            maxLength={50}
                            readOnly={true}
                            className={'form_textbox'} />
                    </Stack>
                    <Stack className={"form_row"}>
                        <FormLabel title={"Keys:"} />
                        <table style={{lineHeight:4}}>
                            <tr>
                                <td>
                                    <span>Primary : </span>
                                </td>
                                <td>
                                    <TextField                                    
                                        name={'primaryKey'}
                                        value={subscriptionV2Selected.primaryKey}
                                        maxLength={50}
                                        readOnly={true}
                                        className={'form_textbox'} />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <span>Secondary : </span>
                                </td>
                                <td>
                                    <TextField                                    
                                        name={'secondaryKey'}
                                        value={subscriptionV2Selected.secondaryKey}
                                        maxLength={50}
                                        readOnly={true}
                                        className={'form_textbox'} />
                                </td>
                            </tr>
                        </table>
                    </Stack>
                </React.Fragment>
                <DialogFooter>
                    <AlternateButton
                        onClick={hideSubscriptionv2Dialog}
                        text="Cancel" />
                </DialogFooter>
            </Dialog> */}
        </React.Fragment>
    );
}
export default Subscriptions;