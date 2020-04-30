import React, { useEffect, useState } from 'react';
import { useParams } from "react-router";
import {
    DefaultButton,
    FontIcon,
    Label,
    Link,
    MessageBar,
    MessageBarType,
    PrimaryButton,
    Stack,
    TextField,
    DialogFooter,
    Dialog,
    DialogType
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { FieldArray, Formik, useFormikContext } from "formik";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { IDeploymentsModel, IDeploymentVersionModel } from "../../models";
import { Loading } from "../../shared/components/Loading";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import { initialDeploymentList, initialDeploymentValues, deploymentFormValidationSchema, initialDeploymentFormValues, IDeploymentFormValues, initialDeploymentVersionList, initialVersionValues, versionFormValidationSchema } from './formUtils/ProductDetailsUtils';
import { initialProductList } from './formUtils/ProductFormUtils';
import ProductService from '../../services/ProductService';
import { DialogBox } from '../../shared/components/Dialog';
import AlternateButton from '../../shared/components/AlternateButton';
import { RouteComponentProps } from 'react-router-dom';

const ProductDeployments: React.FunctionComponent = () => {

    const globalContext = useGlobalContext();
    const [formError, setFormError] = useState<string | null>(null);

    return (
        <Stack
            horizontalAlign="start"
            verticalAlign="start"
            verticalFill
            styles={{
                root: {
                    width: '100%',
                    margin: 31
                }
            }}
            gap={15}
        >
            <Formik
                initialValues={initialDeploymentList}
                onSubmit={async (values, { setSubmitting, setErrors }) => {
                    debugger
                    globalContext.showProcessing();

                    setFormError(null);

                    globalContext.hideProcessing();
                    toast.success("Success!");
                    setSubmitting(false);
                    setTimeout(() => { globalContext.setFormDirty(false); }, 500);

                }}
            >
                <Deployments />
            </Formik>
        </Stack>
    );
};

export type IDeploymentProps = {
}
export const Deployments: React.FunctionComponent<IDeploymentProps> = (props) => {
    const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty } = useFormikContext<IDeploymentProps>(); // formikProps
    const { } = props;
    let [deploymentList, setdeploymentList] = useState<IDeploymentsModel[]>();
    let [deployment, setdeployment] = useState<IDeploymentFormValues>(initialDeploymentFormValues);
    let [deploymentVersionList, setDeploymentVersionList] = useState<IDeploymentVersionModel[]>([]);
    const [loadingdeployment, setLoadingdeployment] = useState<boolean>(false);
    const [formError, setFormError] = useState<string | null>(null);
    const [deploymentDialogVisible, setDeploymentDialogVisible] = useState<boolean>(false);
    const [loadVersionForm, setloadVersionForm] = useState<boolean>(false);
    const [loadVersionData, setloadVersionData] = useState<boolean>(false);
    let [selectedVersion, setselectedVersion] = useState<IDeploymentVersionModel>(initialVersionValues);
    let [isNewVersionDisabled, setIsNewVersionDisabled] = useState<boolean>(true);
    const [isDisplayDeletedeploymentButton, setDisplayDeletedeploymentButton] = useState<boolean>(true);
    const [isEdit, setIsEdit] = useState<boolean>(true);

    const { productId } = useParams();
    const globalContext = useGlobalContext();
    //Below code is for making design proper in Armtemplate page.  
    let body = (document.getElementsByClassName('App')[0] as HTMLElement);

    useEffect(() => {
        globalContext.modifySaveForm(async () => {
            await submitForm();
        });

        getDeploymentsList();
        return () => {
            body.style.height = '100%';
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const getDeploymentsList = async () => {

        setLoadingdeployment(true);
        setloadVersionData(true);
        // const results = await ProductService.getDeploymentList();
        // if (results && results.value && results.success) {
        //   setdeploymentList(results.value);
        //   if (results.value.length > 4)
        //     body.style.height = 'auto';        
        // }
        setdeploymentList(initialDeploymentList);
        setloadVersionData(false);
        setLoadingdeployment(false);
    }

    const getFormErrorString = (touched, errors, property: string) => {
        return touched.deployment && errors.deployment && touched.deployment[property] && errors.deployment[property] ? errors.deployment[property] : '';
    };

    const editDeployment = (Id: string): void => {
        let editeddeployment = initialDeploymentList.filter(a => a.deploymentId == Id)[0];
        let editeddeploymentVersionList = initialDeploymentVersionList.filter(a => a.deploymentId == Id);
        setdeployment({ deployment: editeddeployment });
        setDisplayDeletedeploymentButton(true);
        setIsEdit(true);
        setDeploymentVersionList(editeddeploymentVersionList);
        setIsNewVersionDisabled(false);
        OpenDeploymentDialog();
        //history.push(WebRoute.ModifyProductInfo.replace(':productId', productId));
    };

    const deleteDeployment = async (deploymentSelected: IDeploymentsModel) => {
        var deploymentDeleteResult = await ProductService.deleteDeployment(deploymentSelected.productId, deploymentSelected.deploymentId);
        // if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'deployment', deploymentDeleteResult)) {
        //   globalContext.hideProcessing();
        //   return;

        getDeploymentsList();
    };

    const OpenNewDeploymentDialog = () => {
        initialDeploymentFormValues.deployment.productId = productId as string;
        setdeployment(initialDeploymentFormValues);
        setDeploymentVersionList([]);
        setDisplayDeletedeploymentButton(false);
        setIsEdit(false)
        setselectedVersion(initialVersionValues);
        setIsNewVersionDisabled(true);
        OpenDeploymentDialog();
    }
    const OpenDeploymentDialog = () => {
        setDeploymentDialogVisible(true);
    }

    const CloseNewDeploymentDialog = () => {
        setloadVersionForm(false);
    }

    const CloseDeploymentDialog = () => {
        CloseNewDeploymentDialog();
        setDeploymentDialogVisible(false);
    }

    const DeploymentList = ({ deployments, setFieldValue }) => {
        if (!deployments || deployments.length === 0) {
            return <tr>
                <td colSpan={4}><span>No Deployments</span></td>
            </tr>;
        } else {
            return (
                deployments.map((value: IDeploymentsModel, idx) => {
                    return (
                        <tr key={idx}>
                            <td>
                                <span style={{ width: 200 }}>{value.deploymentId}</span>
                            </td>
                            <td>
                                <span style={{ width: 200 }}>{value.versionId}</span>
                            </td>
                            <td>
                                <Stack
                                    verticalAlign="center"
                                    horizontalAlign={"space-evenly"}
                                    gap={15}
                                    horizontal={true}
                                    styles={{
                                        root: {
                                            width: '40%'
                                        },
                                    }}
                                >
                                    <FontIcon iconName="Edit" className="deleteicon" onClick={() => { editDeployment(value.deploymentId) }} />
                                    {/* <FontIcon iconName="Cancel" className="deleteicon" onClick={() => { deleteDeployment(value) }} /> */}
                                </Stack>
                            </td>
                        </tr>
                    );
                })
            );

        }
    }

    return (
        <React.Fragment>
            <Formik
                initialValues={initialDeploymentList}
                validateOnBlur={true}
                onSubmit={async (values, { setSubmitting, setErrors }) => {
                    debugger;
                }}
            >
                {/* <DeploymentBody formError={formError} /> */}
                {({ isSubmitting, setFieldValue, values, handleChange, handleBlur, touched, errors, submitForm, dirty }) => {
                    return (
                        <React.Fragment>
                            <h3 style={{ textAlign: 'left', fontWeight: 'normal', marginTop: 0, marginBottom: 20, width: '100%' }}>Deployments</h3>
                            <table className="noborder offer" style={{}} cellPadding={5} cellSpacing={0}>                <thead>
                                <tr>
                                    <th style={{ width: 334 }}>
                                        <FormLabel title={"Deployment ID"} />
                                    </th>
                                    <th>
                                        <FormLabel title={"Latest Version"} />
                                    </th>
                                    <th>
                                    </th>
                                </tr>
                            </thead>
                                {loadingdeployment ?
                                    (
                                        <tr>
                                            <td colSpan={4} align={"center"}>
                                                <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                                                    <Loading />
                                                </Stack>
                                            </td>
                                        </tr>
                                    )
                                    :
                                    <tbody>
                                        <DeploymentList deployments={deploymentList} setFieldValue={setFieldValue} />
                                    </tbody>}
                                <tfoot>
                                    <tr>
                                        <td colSpan={3} style={{ paddingTop: '1%' }}>
                                            <PrimaryButton text={"New Deployment"} onClick={() => { OpenNewDeploymentDialog() }} />
                                        </td>
                                    </tr>
                                </tfoot>
                            </table>

                        </React.Fragment>
                    )
                }}
            </Formik>

            <Dialog
                hidden={!deploymentDialogVisible}
                onDismiss={CloseDeploymentDialog}
                dialogContentProps={{
                    styles: {
                        subText: {
                            paddingTop: 0
                        },
                        title: {
                        }

                    },
                    type: DialogType.normal,
                    title: 'New Deployment'
                }}
                modalProps={{
                    isBlocking: true,
                    isDarkOverlay: true,
                    styles: {
                        main: {
                            minWidth: '60% !important',

                        }
                    }
                }}
            >
                <table>
                    <tbody>
                        <tr>
                            <td>
                                <Formik
                                    initialValues={deployment}
                                    validationSchema={deploymentFormValidationSchema}
                                    enableReinitialize={true}
                                    validateOnBlur={true}
                                    onSubmit={async (values, { setSubmitting, setErrors }) => {

                                        setFormError(null);
                                        setSubmitting(true);
                                        globalContext.showProcessing();

                                        var createDeploymentResult = await ProductService.createOrUpdateDeployment(values.deployment);
                                        if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'deployment', createDeploymentResult)) {
                                            globalContext.hideProcessing();
                                            return;
                                        }

                                        //setdeployment({ deployment: createDeploymentResult.value as IDeploymentsModel });
                                        setSubmitting(false);
                                        globalContext.hideProcessing();
                                        setDisplayDeletedeploymentButton(true);
                                        setIsEdit(true);
                                        setIsNewVersionDisabled(false);
                                        toast.success("Success!");
                                    }}
                                >
                                    {({ handleChange, values, handleBlur, touched, errors, handleSubmit, submitForm, setFieldValue }) => (
                                        <table>
                                            <tbody className="noborder offergrid">
                                                <tr>
                                                    <td>
                                                        <React.Fragment>
                                                            <span>Id</span>
                                                            <br />
                                                            <TextField
                                                                name={'deployment.deploymentId'}
                                                                value={values.deployment.deploymentId}
                                                                onChange={handleChange}
                                                                onBlur={handleBlur}
                                                                errorMessage={getFormErrorString(touched, errors, 'deploymentId')}
                                                                placeholder={'Id'}
                                                                className="txtFormField" maxLength={50} disabled={isEdit}/>
                                                        </React.Fragment>
                                                    </td>
                                                    <td>
                                                        <React.Fragment>
                                                            <span>Description</span>
                                                            <br />
                                                            <TextField
                                                                name={'deployment.description'}
                                                                value={values.deployment.description}
                                                                onChange={handleChange}
                                                                onBlur={handleBlur}
                                                                errorMessage={getFormErrorString(touched, errors, 'description')}
                                                                placeholder={'Description'}
                                                                className="txtFormField" maxLength={1024} />
                                                        </React.Fragment>
                                                    </td>
                                                    <td>
                                                        <PrimaryButton type="submit" id="btnsubmit" text={isEdit ? "Save" : "Create"} onClick={submitForm} />
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    )}
                                </Formik>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                {
                                    loadVersionForm ?
                                        <VersionForm selectedVersion={selectedVersion}
                                            SetDeploymentDialogVisible={setDeploymentDialogVisible}
                                            setloadVersionForm={setloadVersionForm}
                                            setDeploymentVersionList={setDeploymentVersionList}
                                            deploymentVersionList={deploymentVersionList} deploymentId={deployment.deployment.deploymentId}
                                        /> : <VersionList deploymentList={deploymentVersionList}
                                            setDeploymentVersionList={setDeploymentVersionList}
                                            setloadVersionForm={setloadVersionForm}
                                            setselectedVersion={setselectedVersion}
                                            loadingVersion={loadVersionData}
                                            setloadVersionData={setloadVersionData}
                                            isNewVersionDisabled={isNewVersionDisabled} />
                                }
                            </td>
                        </tr>
                    </tbody>
                </table>
                <DialogFooter>
                    {
                        isDisplayDeletedeploymentButton ? <PrimaryButton type="button" id="btnsubmit" text="Delete" onClick={() => {
                            deleteDeployment(deployment.deployment)
                        }} /> : null
                    }

                    <AlternateButton
                        onClick={CloseDeploymentDialog}
                        text="Cancel" />
                </DialogFooter>
            </Dialog>
        </React.Fragment>
    );
}

export type IDeploymenVersionFormProps = {
    formError?: string | null;
    selectedVersion: IDeploymentVersionModel;
    SetDeploymentDialogVisible: any;
    setloadVersionForm: any;
    setDeploymentVersionList: any;
    deploymentVersionList: any;
    deploymentId: string;
}

export type IDeploymenListProps = {
    deploymentList: IDeploymentVersionModel[];
    setloadVersionForm: any;
    loadingVersion: boolean;
    setDeploymentVersionList: any;
    setloadVersionData: any;
    setselectedVersion: any;
    isNewVersionDisabled: boolean;
}

//#region Version
export const VersionForm: React.FunctionComponent<IDeploymenVersionFormProps> = (props) => {
    const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty } = useFormikContext<IDeploymentFormValues>(); // formikProps
    const { formError, SetDeploymentDialogVisible, selectedVersion, setloadVersionForm, setDeploymentVersionList,
        deploymentVersionList, deploymentId } = props;

    let [version, setVersion] = useState<IDeploymentVersionModel>(initialVersionValues);


    useEffect(() => {
        setVersion(selectedVersion);
    }, []);

    const CloseForm = () => {
        setloadVersionForm(false);
    }

    const getVersionFormErrorString = (touched, errors, property: string) => {
        return (touched.version && errors.version && touched.version[property] && errors.version[property]) ? errors.version[property] : '';
    };

    return (
        <React.Fragment>

            <Formik
                initialValues={version}
                validationSchema={versionFormValidationSchema}
                enableReinitialize={true}
                validateOnBlur={true}
                onSubmit={async (values, { setSubmitting, setErrors }) => {
                    debugger;
                    console.log("Version Form");

                    // deploymentVersionList.push(
                    //   {
                    //     versionId: values.deployment.versionId,
                    //     apiAuthenticationKey: values.deployment.apiAuthenticationKey,
                    //     realTimePredictApi: values.deployment.realTimePredictApi
                    //   });
                    // setDeploymentVersionList(deploymentVersionList);
                    // setloadVersionForm(false);
                }}
            >
                {({ handleChange, values, handleBlur, touched, errors, handleSubmit }) => (
                    <form style={{ width: '100%' }} autoComplete={"off"} onSubmit={handleSubmit}>
                        <span>New Versions</span>
                        <table className="noborder offergrid">
                            <tbody>
                                <tr>
                                    <td>
                                        <React.Fragment>
                                            <input type="hidden" name="version.deploymentid" value={deploymentId} />
                                            <span>Id</span>
                                            <br />
                                            <TextField
                                                name={'version.versionId'}
                                                value={values.versionId}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                errorMessage={getVersionFormErrorString(touched, errors, 'versionId')}
                                                placeholder={'VersionId'}
                                                className="txtFormField" maxLength={50}/>
                                        </React.Fragment>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <React.Fragment>
                                            <span>ProductId:</span>
                                            <br />
                                            <TextField
                                                name={'version.productID'}
                                                value={values.productID}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                errorMessage={getVersionFormErrorString(touched, errors, 'productID')}
                                                placeholder={'productID'}
                                                className="txtFormField" />
                                        </React.Fragment>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <React.Fragment>
                                            <span>API Authentication Key:</span>
                                            <br />
                                            <TextField
                                                name={'version.AuthenticationType'}
                                                value={values.AuthenticationType}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                errorMessage={getVersionFormErrorString(touched, errors, 'AuthenticationType')}
                                                placeholder={'AuthenticationType'}
                                                className="txtFormField" />
                                        </React.Fragment>
                                    </td>
                                </tr>
                            </tbody>

                            <tfoot>
                                <tr>
                                    <td colSpan={4} style={{ paddingTop: '12%' }}>
                                        <PrimaryButton type="submit" id="btnsaveversion" text="Save" />
                                        <PrimaryButton type="button" id="btnsaveversion" text="Cancel" onClick={CloseForm} />
                                    </td>
                                </tr>
                            </tfoot>
                        </table>
                    </form>
                )}
            </Formik>
        </React.Fragment>
    );
}

export const VersionList: React.FunctionComponent<IDeploymenListProps> = (props) => {
    const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty } = useFormikContext<IDeploymentVersionModel[]>(); // formikProps
    const { setloadVersionData, setloadVersionForm, loadingVersion,
        deploymentList, setDeploymentVersionList, setselectedVersion, isNewVersionDisabled } = props;

    const OpenNewVersionDialog = () => {
        setselectedVersion(initialVersionValues);
        OpenVersionForm();
    }

    const OpenVersionForm = () => {
        setloadVersionForm(true);
    }

    const editVersionItem = (values, index) => {
        setselectedVersion(values);
        OpenVersionForm();
    }

    const deleteVersionItem = (values, index) => {
        setloadVersionData(true);
        deploymentList.splice(index);
        setDeploymentVersionList(...deploymentList);
        setloadVersionData(false);
    }

    return (
        <React.Fragment>
            <span>Versions</span>
            <table className="noborder offergrid">
                <thead>
                    <tr>
                        <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}>
                            VersionId
                              </th>
                        <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}>
                            APIs
                              </th>
                        <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}>
                            AuthenticationType
                              </th>
                        <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}>
                            Operations
                                </th>
                    </tr>
                </thead>
                <tbody>
                    {
                        loadingVersion
                            ?
                            (
                                <tr>
                                    <td colSpan={4} align={"center"}>
                                        <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                                            <Loading />
                                        </Stack>
                                    </td>
                                </tr>
                            )
                            :
                            deploymentList.map((value: IDeploymentVersionModel, idx) => {
                                return (
                                    <tr key={idx}>
                                        <td>
                                            <span style={{ width: 200 }}>{value.versionId}</span>
                                        </td>
                                        <td>
                                            <span style={{ width: 200 }}>{value.productID}</span>
                                        </td>
                                        <td>
                                            <span style={{ width: 200 }}>{value.AuthenticationType}</span>
                                        </td>
                                        <td>
                                            <Stack
                                                verticalAlign="center"
                                                horizontalAlign={"space-evenly"}
                                                gap={15}
                                                horizontal={true}
                                                styles={{
                                                    root: {
                                                        width: '40%'
                                                    },
                                                }}
                                            >
                                                <FontIcon iconName="Edit" className="deleteicon" onClick={() => { editVersionItem(value, idx) }} />
                                                <FontIcon iconName="Cancel" className="deleteicon" onClick={() => { deleteVersionItem(value, idx) }} />
                                            </Stack>
                                        </td>
                                    </tr>
                                );
                            })
                    }
                </tbody>
                <tfoot>
                    <tr>
                        <td colSpan={4}>
                            <PrimaryButton type="button" id="btnnewVersion" text="NewVersion" onClick={OpenNewVersionDialog} disabled={isNewVersionDisabled} />
                        </td>
                    </tr>
                </tfoot>
            </table>

        </React.Fragment>
    );
}
//#endregion 

export default ProductDeployments;