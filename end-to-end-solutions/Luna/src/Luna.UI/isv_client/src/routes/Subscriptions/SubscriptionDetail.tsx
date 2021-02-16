import React, { useEffect, useState } from 'react';
import {
    Dialog,
    DialogContent,
    DialogFooter,
    DialogType,
    Dropdown,
    IDropdownOption,
    PrimaryButton,
    Stack
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { useParams } from "react-router";
import { Loading } from "../../shared/components/Loading";
import AlternateButton from "../../shared/components/AlternateButton";
import { Formik } from "formik";
import { ISubscriptionsModel, ISubscriptionsPostModel, ISubscriptionsWarnings } from '../../models/ISubscriptionsModel';
import SubscriptionsService from '../../services/SubscriptionsService';
import PlansService from "../../services/PlansService";
import {
    getInitialSubscriptionsModel,
    getInitialSubscriptionsPostModel,
    getInitialSubscriptionsWarningsModel,
    UpdateplanValidator
} from './formUtils/subscriptionFormUtils';
import { toast } from 'react-toastify';
import adalContext from '../../adalConfig';
import Moment from 'react-moment';
import { useGlobalContext } from '../../shared/components/GlobalProvider';
import { handleSubmissionErrorsForForm } from '../../shared/formUtils/utils';

const SubscriptionDetail: React.FunctionComponent = () => {

    const [state, setstate] = useState<ISubscriptionsModel>(getInitialSubscriptionsModel);
    const [setFormError] = useState<string | null>(null);
    const [warnings, setwarnings] = useState<ISubscriptionsWarnings[]>(getInitialSubscriptionsWarningsModel);
    const [loadingSubscription, setLoadingSubscription] = useState<boolean>(true);
    const [loadingWarnings, setLoadingWarnings] = useState<boolean>(true);
    const [subscriptionPost, setSubscriptionPost] = useState<ISubscriptionsPostModel>(getInitialSubscriptionsPostModel);
    const [dialogVisible, setDialogVisible] = useState<boolean>(false);
    const [loadingSubcriptionPost, setloadingSubcriptionPost] = useState<boolean>(true);    
    let userName = "";
    const { subscriptionId } = useParams();
    const globalContext = useGlobalContext();

    var response = adalContext.AuthContext.getCachedUser();
    if (response && response.profile && response.profile.name)
        userName = response.profile.name;

    useEffect(() => {
        if (subscriptionId) {
            getData(subscriptionId);
            getWarnings(subscriptionId);
        }
// eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const getFormErrorString = (touched, errors, property: string) => {
        let retobj = touched && errors && errors[property] ? errors[property] : '';
        return retobj;
    };

    const getData = async (subscriptionId: string) => {
        globalContext.showProcessing();
        setLoadingSubscription(true);
        const dataResponse = await SubscriptionsService.get(subscriptionId);

        // Global errors should have already been handled for get requests by this point
        if (dataResponse.value && dataResponse.success) {
            var data = dataResponse.value as ISubscriptionsModel;
            let planlist = await getPlans(data.offerName);
            setstate(data);
            setloadingSubcriptionPost(true)
            setSubscriptionPost(
                {
                    subscriptionId: data.subscriptionId,
                    subscriptionName: data.name,
                    name: data.name,
                    availablePlanName: '',
                    offerName: data.offerName,
                    planName: data.planName,
                    owner: data.owner,
                    quantity: data.quantity,
                    beneficiaryTenantId: data.beneficiaryTenantId,
                    purchaserTenantId: data.purchaserTenantId,
                    subscribeWebhookName: data.subscribeWebhookName,
                    unsubscribeWebhookName: data.unsubscribeWebhookName,
                    suspendWebhookName: data.suspendWebhookName,
                    deleteDataWebhookName: data.deleteDataWebhookName,
                    priceModel: data.priceModel,
                    monthlyBase: data.monthlyBase,
                    privatePlan: data.privatePlan,
                    inputParameters: data.inputParameters == null ? [] : data.inputParameters,
                    planlist: planlist
                })
            setloadingSubcriptionPost(false)
        }
        setLoadingSubscription(false);
        globalContext.hideProcessing();

    }

    const getPlans = async (offername) => {
        let planList: IDropdownOption[] = [];
        planList.push({
            key: '',
            text: 'Choose a new plan'
        })
        let PlanResponse = await PlansService.list(offername);
        if (PlanResponse.value && PlanResponse.success) {
            var Plans = PlanResponse.value;
            Plans.map((item, index) => {
                planList.push(
                    {
                        key: item.planName,
                        text: item.planName
                    })
                    return planList;
            })
        }
        return planList;
    }

    const getWarnings = async (subscriptionId: string) => {
        setLoadingWarnings(true);
        const dataResponse = await SubscriptionsService.getSubscriptionWarnings(subscriptionId);

        // Global errors should have already been handled for get requests by this point
        if (dataResponse.value && dataResponse.success) {
            var data = dataResponse.value as ISubscriptionsWarnings[];

            setwarnings(data);
        }
        setLoadingWarnings(false);
    }

    const updatePlan = (subscriptionId: string): void => {
        showDialog();
    };

    const activateSubscription = async (subscriptionId: string,subscriptionName:string): Promise<void> => {
        globalContext.showProcessing();
        let PlanResponse = await SubscriptionsService.activateSubscription(subscriptionId,subscriptionName);
        if (PlanResponse.value && PlanResponse.success) {
            globalContext.hideProcessing();
            toast.success("Subscription successfully activated!");
        }
        else{
            globalContext.hideProcessing();
            toast.warn("Fail to activate subscription");
        }
    };
    
    const completeOperation = async (subscriptionId: string,subscriptionName:string): Promise<void> => {
        globalContext.showProcessing();
        let PlanResponse = await SubscriptionsService.completeOperation(subscriptionId,subscriptionName);
        if (PlanResponse.value && PlanResponse.success) {
            globalContext.hideProcessing();
            toast.success("Subscription operation successfully completed!");
        }
        else{
            globalContext.hideProcessing();
            toast.warn("Fail to complete subscription operation");
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

    const selectOnChange = (fieldKey: string, setFieldValue, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
        if (option) {
            let key = (option.key as string);
            setFieldValue(fieldKey, key, true);
            subscriptionPost.availablePlanName = key;

            setSubscriptionPost(subscriptionPost);
        }
    };

    return (
        <React.Fragment>
            <Stack
                horizontal={true}
                verticalAlign={"center"}
                verticalFill={true}
                className={"offer-details-header"}
                styles={{
                    root: {
                        paddingLeft: 31,
                        paddingRight: 31,
                        width: '100%',
                        borderBottom: '1px solid #eaeaea',
                        height: 'auto'
                    }
                }}
            >
                <Stack.Item styles={{
                    root: {
                        flexGrow: 0,
                        width: 211,
                        height: '100%'
                    }
                }}>
                    <div style={{float: 'left' }}>
                        <span style={{ marginRight: 20, fontSize: 18, float: 'left', lineHeight: '60px' }}>
                            Subscription Details </span>                        
                    </div>
                </Stack.Item>
                <Stack.Item styles={{
                    root: {
                        flexGrow: 1,
                        height: '100%'
                    }
                }}>
                    <div className="subscription-details-separator">
                        <table style={{ lineHeight: '30px' }}>
                            <tbody>
                                <tr>
                                    <td>
                                        <span style={{ fontWeight: 600 }}>ID:</span>
                                        <span style={{ marginLeft: 8 }}>{state.subscriptionId}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span style={{ fontWeight: 600 }}> Offer ID: </span>
                                        <span style={{ marginLeft: 8 }}>{state.offerName}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span style={{ fontWeight: 600 }}>Plan ID:</span>
                                        <span style={{ marginLeft: 8 }}>{state.planName}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span style={{ fontWeight: 600 }}>Owner's Email:</span>
                                        <span style={{ marginLeft: 8 }}>{state.owner}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span style={{ fontWeight: 600 }}>Created Time:</span>
                                        <span style={{ marginLeft: 8 }}>
                                            <Moment format="YYYY/MM/DD HH:mm:ss">{state.createdTime}</Moment>
                                        </span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span style={{ fontWeight: 600 }}>Activated Time:</span>
                                        <span style={{ marginLeft: 8 }}>
                                            <Moment format="YYYY/MM/DD HH:mm:ss">{state.activatedTime}</Moment></span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span style={{ fontWeight: 600 }}>Last Update Time:</span>
                                        <span style={{ marginLeft: 8 }}>
                                            <Moment format="YYYY/MM/DD HH:mm:ss">{state.lastUpdatedTime}</Moment>
                                        </span>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                </Stack.Item>
                <Stack.Item styles={{
                    root: {
                        flexGrow: 0
                    }
                }}>
                    <Stack
                        horizontal={true}
                        verticalAlign={"center"}
                        verticalFill={true}
                        horizontalAlign={"end"}
                        gap={8}
                    >
                        <PrimaryButton disabled={state.provisioningStatus.toLowerCase() !== 'succeeded' || state.status.toLowerCase() !== 'subscribed'} text={"Update Plan"} onClick={() => { updatePlan(state.subscriptionId) }} style={{width:'100%'}}/>
                        {loadingSubscription ?
                            null : 
                            state.status.toLowerCase() === 'pendingfulfillmentstart'? 
                            <PrimaryButton disabled={state.provisioningStatus.toLowerCase() === 'succeeded'} text={"Activate"} onClick={() => { activateSubscription(state.subscriptionId,state.name) }}/> :
                            <PrimaryButton disabled={state.provisioningStatus.toLowerCase() === 'succeeded'} text={"Complete Operation"} onClick={() => { completeOperation(state.subscriptionId,state.name) }}/>
                        }

                    </Stack>
                </Stack.Item>
            </Stack>
            <Stack
                horizontal={true}
                verticalAlign={"center"}
                verticalFill={true}
                className={"offer-details-header"}
                styles={{
                    root: {

                        height: 'auto',
                        margin: '0 auto'
                    }
                }}
            >
                <React.Fragment>
                    <div style={{ width: '100%' }}>
                        <div style={{ borderBottom: '1px solid #eaeaea', width: '100%' }}>
                            <span style={{ fontSize: 18, color: '#333333' }}>
                                Outstanding Issues. </span>
                        </div>
                        {loadingWarnings ?
                            <Loading />
                            :
                            warnings.length === 0 ?
                                <span style={{ color: '#6f6f6f', fontFamily: 'SegoeUI-Italic' }}>No outstanding issues.</span>
                                :
                                <table className="noborder offer">
                                    <thead>
                                        <tr>
                                            <th style={{width:'23%',padding:0}}>
                                                <FormLabel title={"Subscription Id"} toolTip='Subscription Id' />
                                            </th>
                                            <th style={{padding:0}}>
                                                <FormLabel title={"Warning Message"} toolTip='Warning Message' />
                                            </th>
                                            <th style={{padding:0}}>
                                                <FormLabel title={"Details"} toolTip='Details' />
                                            </th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {
                                            warnings.map((values, idx) => {
                                                return (
                                                    <tr key={idx} style={{lineHeight:'30px'}}>
                                                        <td>
                                                            {values.subscriptionId}
                                                        </td>
                                                        <td>
                                                            {values.details}
                                                        </td>
                                                        <td>
                                                            {values.warningMessage}
                                                        </td>
                                                    </tr>
                                                )
                                            })
                                        }
                                    </tbody>
                                </table>
                        }
                    </div>
                </React.Fragment>
            </Stack>
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
                        },

                    },
                    type: DialogType.normal,
                    title: 'Update Plan',
                    subText: ''
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
                    {loadingSubcriptionPost ?
                        <Loading />
                        :
                        <Formik
                            initialValues={subscriptionPost}
                            validationSchema={UpdateplanValidator}
                            enableReinitialize={true}
                            validateOnBlur={true}
                            onSubmit={async (values, { setSubmitting, setErrors }) => {

                                globalContext.showProcessing();                                
                                values.planName = values.availablePlanName;

                                let createSubscriptionsResult = await SubscriptionsService.create_update(values);

                                if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'subscription', createSubscriptionsResult)) {
                                    globalContext.hideProcessing();
                                    return;
                                }

                                globalContext.hideProcessing();
                                //TODO create a nice toast message for the user for success                                    
                                toast.success("Success !");
                                setSubmitting(false);
                                // await getData(values.subscriptionId)
                                hideDialog();
                            }}
                        >
                            {({ setFieldValue,values, handleBlur, touched, errors, handleSubmit }) => (
                                <form style={{ width: '100%', marginTop: 20 }} autoComplete={"off"} onSubmit={handleSubmit}>
                                    <table>
                                        <tbody>
                                            <tr>
                                                <td style={{ width: '42%' }}>
                                                    <b>Saas Subscription ID:</b>
                                                </td>
                                                <td>
                                                    <span>{values.subscriptionId} </span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <b>Subscription Name: </b>
                                                </td>
                                                <td>
                                                    <span>{values.name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <b>Current Plan: </b>
                                                </td>
                                                <td>
                                                    <span>{values.planName}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colSpan={2}>
                                                    <span>Available Plans</span>
                                                    <br />
                                                    <React.Fragment>
                                                        <Dropdown options={values.planlist} id={`availablePlanName`}
                                                            placeHolder="Choose a new plan"
                                                            onBlur={handleBlur}
                                                            errorMessage={getFormErrorString(touched, errors, 'availablePlanName')}
                                                            onChange={(event, option, index) => {
                                                                selectOnChange(`availablePlanName`, setFieldValue, event, option, index);
                                                            }} defaultSelectedKey={values.availablePlanName} />
                                                    </React.Fragment>
                                                </td>
                                            </tr>
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
                    <AlternateButton onClick={hideDialog} text="Cancel" />
                    <PrimaryButton text="Submit" onClick={triggerSubmitButton} />
                </DialogFooter>
            </Dialog>
        </React.Fragment>
    )
}
export default SubscriptionDetail;