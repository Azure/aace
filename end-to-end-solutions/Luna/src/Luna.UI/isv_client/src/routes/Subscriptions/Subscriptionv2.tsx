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
import { ISubscriptionsV2Model, ISubscriptionsWarnings } from '../../models/ISubscriptionsModel';
import SubscriptionsService from '../../services/SubscriptionsService';
import { getInitialSubscriptionsWarningsModel } from './formUtils/subscriptionFormUtils';
import { NavLink } from "react-router-dom";


interface IDetailsListDocumentsExampleState {
    columns: IColumn[];
    items: ISubscriptionsV2Model[];
}
const SubscriptionsV2: React.FunctionComponent = () => {
    const history = useHistory();

    const [subscriptionV2, setsubscriptionV2] = useState<ISubscriptionsV2Model[]>([]);
    const [state, setstate] = useState<IDetailsListDocumentsExampleState>();    
    const [loadingSubscriptionV2, setLoadingSubscriptionV2] = useState<boolean>(true);
    const [loadStatus, setLoadStatus] = useState<boolean>(true);
    const [statusList, setStatusList] = useState<IDropdownOption[]>([]);    


    const _onColumnClick = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
        if (column.key !== 'operation') {
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
            key: 'subscriptionName',
            name: 'Name',
            className: '',
            iconClassName: '',
            ariaLabel: '',
            iconName: '',
            isIconOnly: false,
            fieldName: 'subscriptionName',
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
                return <span>{item.subscriptionName}</span>;
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
        {
            key: 'baseUrl',
            name: 'Base Url',
            className: '',
            iconClassName: '',
            ariaLabel: '',
            iconName: '',
            isIconOnly: false,
            fieldName: 'baseUrl',
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
                return <span>{item.baseUrl}</span>;
            }
        },
        {
            key: 'primaryKey',
            name: 'Primary Key',
            className: '',
            iconClassName: '',
            ariaLabel: '',
            iconName: '',
            isIconOnly: false,
            fieldName: 'primaryKey',
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
                return <span>{item.primaryKey}</span>;
            }
        },
        {
            key: 'secondaryKey',
            name: 'Secondary Key',
            className: '',
            iconClassName: '',
            ariaLabel: '',
            iconName: '',
            isIconOnly: false,
            fieldName: 'secondaryKey',
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
                return <span>{item.secondaryKey}</span>;
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
            onRender: (item: ISubscriptionsV2Model) => {
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
                        <FontIcon style={{ lineHeight: '20px' }} iconName="Edit" className="deleteicon" onClick={() => { editdetails(item.productName, item.subscriptionId) }} />
                    </Stack>
                )
            }
        },
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
        setLoadingSubscriptionV2(true);
        const results = await SubscriptionsService.listV2();
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
            setsubscriptionV2(results.value);
            setstate({ items: results.value, columns: columns });
        }
        else {
            setsubscriptionV2([]);
            setstate({ items: [], columns: columns });
            if (results.hasErrors) {
                // TODO: display errors
                alert(results.errors.join(', '));
            }
        }
        setLoadingSubscriptionV2(false);
        setLoadStatus(false);
    }

    useEffect(() => {
        getSubscriptions();
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

        let data = subscriptionV2;
        let filterdata = text ? data.filter(i =>
            i.productName.toLowerCase().indexOf(text.toLowerCase()) > -1 ||
            i.baseUrl.toLowerCase().indexOf(text.toLowerCase()) > -1 ||
            i.deploymentName.toString().toLowerCase().indexOf(text.toLowerCase()) > -1 ||
            i.subscriptionId.toLowerCase().indexOf(text.toLowerCase()) > -1 ||
            i.primaryKey.toLowerCase().indexOf(text.toLowerCase()) > -1 ||
            i.secondaryKey.toLowerCase().indexOf(text.toLowerCase()) > -1 ||
            i.subscriptionName.toLowerCase().indexOf(text.toLowerCase()) > -1) : data
        setstate({ items: filterdata, columns: columns });
    };

    const selectOnChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
        if (option) {
            let text = (option.key as string);

            if (text !== 'all') {
                let data = subscriptionV2;
                let filterdata = text ? data.filter(i => i.status.toLowerCase() === text.toLowerCase()) : data
                setstate({ items: filterdata, columns: columns });
            }
            else {
                setstate({ items: subscriptionV2, columns: columns });
            }

        }
    };

    return (
        <React.Fragment>
            {
                loadingSubscriptionV2 ?
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
                        </div>
                    </React.Fragment>
            }
        </React.Fragment>
    )
}

export default SubscriptionsV2;