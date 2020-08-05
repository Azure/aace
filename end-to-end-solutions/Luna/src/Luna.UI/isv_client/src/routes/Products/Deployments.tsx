import React, { useEffect, useState } from 'react';
import { useParams } from "react-router";
import {
  ChoiceGroup,
  DefaultButton,
  Dialog,
  DialogFooter,
  DialogType,
  Dropdown,
  FontIcon,
  IChoiceGroupOption,
  IDropdownOption,
  PrimaryButton,
  Stack,
  TextField
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { Formik, useFormikContext } from "formik";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { IDeploymentsModel, IDeploymentVersionModel, IError } from "../../models";
import { Loading } from "../../shared/components/Loading";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import {
  deploymentFormValidationSchema,
  getInitialDeployment,
  getInitialVersion,
  IDeploymentFormValues,
  IDeploymentVersionFormValues,
  initialDeploymentFormValues,
  initialDeploymentList,
  versionFormValidationSchema,
  deletedeploymentValidator,
  deleteVersionValidator,
} from './formUtils/ProductDetailsUtils';
import ProductService from '../../services/ProductService';
import AlternateButton from '../../shared/components/AlternateButton';
import { Hub } from "aws-amplify";
import { ProductMessages } from '../../shared/constants/infomessages';
import { DialogBox } from '../../shared/components/Dialog';
import * as yup from "yup";

export type IProductDeploymentsProps =
  {
    productType: string;
  }
const ProductDeployments: React.FunctionComponent<IProductDeploymentsProps> = (props) => {

  const globalContext = useGlobalContext();

  const { productType } = props;
  useEffect(() => {
  }, []);

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

          globalContext.showProcessing();

          //setFormError(null);

          globalContext.hideProcessing();
          toast.success("Success!");
          setSubmitting(false);
          setTimeout(() => {
            globalContext.setFormDirty(false);
          }, 500);

        }}
      >
        <Deployments productType={productType} />
      </Formik>
    </Stack>
  );
};

export type IDeploymentProps = {
  productType: string
}
export const Deployments: React.FunctionComponent<IDeploymentProps> = (props) => {
  //const {values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty} = useFormikContext<IDeploymentProps>(); // formikProps
  const { productType } = props;
  let [deploymentList, setDeploymentList] = useState<IDeploymentsModel[]>();
  let [deployment, setDeployment] = useState<IDeploymentFormValues>(initialDeploymentFormValues);
  let [deploymentVersionList, setDeploymentVersionList] = useState<IDeploymentVersionModel[]>([]);
  const [loadingDeployment, setLoadingDeployment] = useState<boolean>(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [deploymentDialogVisible, setDeploymentDialogVisible] = useState<boolean>(false);
  const [versionDialogVisible, setVersionDialogVisible] = useState<boolean>(false);
  let [selectedVersion, setSelectedVersion] = useState<IDeploymentVersionFormValues>({ version: getInitialVersion() });
  let [isNewVersionDisabled, setIsNewVersionDisabled] = useState<boolean>(true);
  const [displayDeleteDeploymentButton, setDisplayDeleteDeploymentButton] = useState<boolean>(true);
  const [isEdit, setIsEdit] = useState<boolean>(true);

  const [deploytmentDeleteDialog, setDeploytmentDeleteDialog] = useState<boolean>(false);

  const [selecteddeployment, setSelectedDeployment] = useState<IDeploymentsModel>(getInitialDeployment);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [isVersionEdit, setIsVersionEdit] = useState<boolean>(true);

  const { productName } = useParams();
  //const history = useHistory();
  const globalContext = useGlobalContext();
  //Below code is for making design proper in Armtemplate page.
  let body = (document.getElementsByClassName('App')[0] as HTMLElement);

  useEffect(() => {
    // globalContext.modifySaveForm(async () => {
    //     await submitForm();
    // });
    getDeploymentsList();
    return () => {
      body.style.height = '100%';
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getDeploymentsList = async () => {

    setLoadingDeployment(true);
    //setloadVersionData(true);
    const results = await ProductService.getDeploymentListByProductName(productName as string);
    if (results && results.value && results.success) {
      setDeploymentList(results.value);
      /*if (results.value.length > 4)
        body.style.height = 'auto';*/
    }
    //setdeploymentList(initialDeploymentList);
    //setloadVersionData(false);
    setLoadingDeployment(false);
  }

  const getFormErrorString = (touched, errors, property: string) => {
    return touched.deployment && errors.deployment && touched.deployment[property] && errors.deployment[property] ? errors.deployment[property] : '';
  };

  const getDeleteErrorString = (touched, errors, property: string) => {
    return touched.selecteddeploymentName && errors && touched.selecteddeploymentName ? errors[property] : '';
  };

  const editDeployment = async (Id: string) => {
    // fetch our deployment and versions

    globalContext.showProcessing();
    const [
      deploymentResponse,
      deploymentVersionsResponse
    ] = await Promise.all([
      ProductService.getDeploymentByProductName(productName as string, Id),
      ProductService.getDeploymentVersionListByDeploymentName(productName as string, Id)
    ]);
    globalContext.hideProcessing();

    // var dataConnectorTypes: string[] = [];
    // var telemetryDataConnectors: ITelemetryDataConnectorModel[] = [];

    if (deploymentResponse.success && deploymentVersionsResponse.success) {

      if (deploymentResponse.value) {
        setDeployment({ deployment: deploymentResponse.value });
      }


      if (deploymentVersionsResponse.value)
        setDeploymentVersionList(deploymentVersionsResponse.value);

      setIsEdit(true);
      setIsNewVersionDisabled(false);
      setDisplayDeleteDeploymentButton(true);
      OpenDeploymentDialog();
    } else {
      let errorMessages: IError[] = [];

      errorMessages.concat(deploymentResponse.errors);
      errorMessages.concat(deploymentVersionsResponse.errors);

      if (errorMessages.length > 0) {
        toast.error(errorMessages.join(', '));
      }
    }
  };

  const getDeploymentVersionsList = async (deploymentName: string) => {
    globalContext.showProcessing();
    let deploymentVersionsResponse = await ProductService.getDeploymentVersionListByDeploymentName(productName as string, deploymentName);
    globalContext.hideProcessing();

    if (deploymentVersionsResponse.success) {

      if (deploymentVersionsResponse.value)
        setDeploymentVersionList(deploymentVersionsResponse.value);

    } else {
      let errorMessages: IError[] = [];

      errorMessages.concat(deploymentVersionsResponse.errors);

      if (errorMessages.length > 0) {
        toast.error(errorMessages.join(', '));
      }
    }
  }

  const deleteDeployment = async (deploymentSelected: IDeploymentsModel) => {
    // globalContext.showProcessing();

    // // // determine if there are any versions for this deployment, if there are, prevent the deletion
    // // var deploymentVersionsResponse = await ProductService.getDeploymentVersionListByDeploymentName(productName as string, deploymentSelected.deploymentName);

    // // if (deploymentVersionsResponse.success) {
    // //   if (deploymentVersionsResponse.value && deploymentVersionsResponse.value.length > 0) {
    // //     toast.error("You must delete all versions for the deployment first.");
    // //     globalContext.hideProcessing();
    // //     return;
    // //   }
    // // }

    // // var deleteResult = await ProductService.deleteDeployment(deploymentSelected.productName, deploymentSelected.deploymentName);

    // // if (handleSubmissionErrorsForForm((item) => {
    // // }, (item) => {
    // // }, setFormError, 'deployment', deleteResult)) {
    // //   toast.error(formError);
    // //   globalContext.hideProcessing();
    // //   return;
    // // }

    //  await getDeploymentsList();    
    //  globalContext.hideProcessing();
    // toast.success("Deployment Deleted Successfully!");
    // CloseDeploymentDialog();
    setSelectedDeployment(deploymentSelected);
    setDeploytmentDeleteDialog(true);
  };

  const OpenNewDeploymentDialog = () => {
    let newDeployment = getInitialDeployment();
    newDeployment.productName = productName as string;
    setDeployment({ deployment: newDeployment });
    setDeploymentVersionList([]);
    setDisplayDeleteDeploymentButton(false);
    setIsEdit(false)
    setIsNewVersionDisabled(true);
    OpenDeploymentDialog();
  }

  const OpenDeploymentDialog = () => {
    setDeploymentDialogVisible(true);
  }

  const OpenVersionDialog = () => {
    setVersionDialogVisible(true);
  }

  const CloseVersionDialog = () => {
    setVersionDialogVisible(false);
  }

  const CloseNewDeploymentDialog = () => {
    //setloadVersionForm(false);
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
                <span style={{ width: 200 }}>{value.deploymentName}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.versionName}</span>
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
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => {
                    editDeployment(value.deploymentName)
                  }} />
                </Stack>
              </td>
            </tr>
          );
        })
      );

    }
  }

  const CloseDeploymentDeleteDialog = () => {
    setDeploytmentDeleteDialog(false);
  }

  return (
    <React.Fragment>
      <Formik
        initialValues={initialDeploymentList}
        validateOnBlur={true}
        onSubmit={async (values, { setSubmitting, setErrors }) => {

        }}
      >{/* <DeploymentBody formError={formError} /> */}
        {({ isSubmitting, setFieldValue, values, handleChange, handleBlur, touched, errors, submitForm, dirty }) => {
          return (
            <React.Fragment>
              <h3 style={{
                textAlign: 'left',
                fontWeight: 'normal',
                marginTop: 0,
                marginBottom: 20,
                width: '100%'
              }}>Deployments</h3>
              <table className="noborder offer" style={{ width: 'auto' }} cellPadding={5} cellSpacing={0}>
                <thead>
                  <tr>
                    <th style={{ width: 334 }}>
                      <FormLabel title={"Deployment Name"} />
                    </th>
                    <th>
                      <FormLabel title={"Latest Version"} />
                    </th>
                    <th style={{ width: 50 }}>
                      <FormLabel title={"Operations"} />
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {loadingDeployment ?
                    (
                      <tr>
                        <td colSpan={4} align={"center"}>
                          <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                            <Loading />
                          </Stack>
                        </td>
                      </tr>
                    ) :
                    <DeploymentList deployments={deploymentList} setFieldValue={setFieldValue} />
                  }
                </tbody>
                <tfoot>
                  <tr>
                    <td colSpan={3} style={{ paddingTop: '1%' }}>
                      <PrimaryButton text={"New Deployment"} onClick={() => {
                        OpenNewDeploymentDialog()
                      }} />
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
            title: {}

          },
          type: DialogType.normal,
          title: (isEdit ? 'Deployment' : 'New Deployment')
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

            setDeployment({ deployment: createDeploymentResult.value as IDeploymentsModel });
            await getDeploymentsList();
            setSubmitting(false);
            globalContext.hideProcessing();
            setDisplayDeleteDeploymentButton(true);
            setIsEdit(true);
            setIsNewVersionDisabled(false);
            toast.success("Success!");
          }}
        >
          {({ handleChange, values, handleBlur, touched, errors, handleSubmit, submitForm, setFieldValue }) => (
            <React.Fragment>
              <table className="offergrid">
                <thead>
                  <tr>
                    <th>
                      <FormLabel title={"Deployment Name"} toolTip={ProductMessages.deployment.DeploymentName} />
                    </th>
                    <th>
                      <FormLabel title={"Description"} toolTip={ProductMessages.deployment.Description} />
                    </th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>
                      <TextField
                        name={'deployment.deploymentName'}
                        value={values.deployment.deploymentName}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'deploymentName')}
                        placeholder={'Id'}
                        className="txtFormField" maxLength={50} disabled={isEdit} />
                    </td>
                    <td>
                      <TextField
                        name={'deployment.description'}
                        value={values.deployment.description}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'description')}
                        placeholder={'Description'}
                        className="txtFormField" maxLength={1024} />
                    </td>
                    <td>
                      <Stack gap={15}>
                        <PrimaryButton type="submit" id="btnsubmit" text={isEdit ? "Update Description" : "Create"}
                          onClick={submitForm} />
                      </Stack>
                    </td>
                  </tr>
                </tbody>
              </table>
              <VersionList
                setIsVersionEdit={setIsVersionEdit}
                openVersionDialog={OpenVersionDialog}
                productType={productType}
                productName={productName as string}
                selectedDeploymentName={values.deployment.deploymentName}
                deploymentVersionList={deploymentVersionList}
                setDeploymentVersionList={setDeploymentVersionList}
                setSelectedVersion={setSelectedVersion}
                isNewVersionDisabled={isNewVersionDisabled} />
            </React.Fragment>
          )}
        </Formik>

        <DialogFooter>
          <Stack horizontal={true} gap={15}>
            {
              displayDeleteDeploymentButton ? <DefaultButton type="button" id="btnsubmit" className="addbutton"
                onClick={() => {
                  deleteDeployment(deployment.deployment)
                }}>
                <FontIcon iconName="Cancel" className="deleteicon" /> Delete
              </DefaultButton> : null
            }
            <div style={{ flexGrow: 1 }}></div>
            <AlternateButton
              onClick={CloseDeploymentDialog}
              text="Close" />
          </Stack>
        </DialogFooter>
        <Dialog
          hidden={!versionDialogVisible}
          onDismiss={CloseVersionDialog}
          dialogContentProps={{
            styles: {
              subText: {
                paddingTop: 0
              },
              title: {}

            },
            type: DialogType.normal,
            title: (isVersionEdit ? 'Version' : 'New Version')
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

          <Formik
            initialValues={selectedVersion}
            validationSchema={versionFormValidationSchema}
            enableReinitialize={true}
            validateOnBlur={true}
            onSubmit={async (values, { setSubmitting, setErrors }) => {

              console.log("Version Form");

              setFormError(null);
              setSubmitting(true);
              globalContext.showProcessing();

              switch (values.version.authenticationType) {
                case 'None':
                  values.version.amlWorkspaceName = '';
                  values.version.authenticationKey = '';
                  break;
                case 'Key':
                  values.version.amlWorkspaceName = '';
                  break;
                case 'Token':
                  values.version.authenticationKey = '';
                  break;
              }

              switch (productType) {
                case 'RTP':
                  if (!values.version.realTimePredictAPI || values.version.realTimePredictAPI.length === 0) {
                    toast.error('Real Time Predict API is required');
                    globalContext.hideProcessing();
                    return;
                  }
                  break;
                case 'BI':
                  if (values.version.versionSourceType == "") {
                    toast.error('Source is required');
                    globalContext.hideProcessing();
                    return;
                  }
                  if (values.version.versionSourceType == 'amlPipeline' && (!values.version.batchInferenceId || values.version.batchInferenceId.length === 0)) {
                    toast.error('Batch Inference API is required');
                    globalContext.hideProcessing();
                    return;
                  }
                  break;
                case 'TYOM':
                  if (values.version.versionSourceType == "") {
                    toast.error('Source is required');
                    globalContext.hideProcessing();
                    return;
                  }

                  if (values.version.versionSourceType == 'amlPipeline') {
                    if (!values.version.trainModelId || values.version.trainModelId.length === 0) {
                      toast.error('Training API is required');
                      globalContext.hideProcessing();
                      return;
                    }
                    if (!((values.version.deployModelId && values.version.deployModelId.length > 0)
                      || (values.version.batchInferenceId && values.version.batchInferenceId.length > 0))) {
                      toast.error('Deploy Model API or Batch Inference API is required');
                      globalContext.hideProcessing();
                      return;
                    }
                  }
                  break;
                default:
                  toast.error('Invalid product type detected');
                  globalContext.hideProcessing();
                  return;
              }

              var deploymentVersionResult = await ProductService.createOrUpdateDeploymentVersion(values.version);
              if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'version', deploymentVersionResult)) {
                globalContext.hideProcessing();
                return;
              }

              setSubmitting(false);
              globalContext.hideProcessing();
              toast.success("Success!");
              await getDeploymentVersionsList(selectedVersion.version.deploymentName);
              CloseVersionDialog();
            }}
          >
            <VersionForm selectedVersion={selectedVersion}
              isNewVersion={!isVersionEdit}
              refreshVersionList={() => {
                getDeploymentVersionsList(selectedVersion.version.deploymentName);
              }}
              hideVersionDialog={CloseVersionDialog}
              productType={productType}
            />
          </Formik>
        </Dialog>
      </Dialog>

      <DialogBox keyindex='Deploymentmodal' dialogVisible={deploytmentDeleteDialog}
        title="Delete Deployment" subText="" isDarkOverlay={true} className="" cancelButtonText="Cancel"
        submitButtonText="Submit" maxwidth={500}
        cancelonClick={() => {
          CloseDeploymentDeleteDialog();
        }}
        submitonClick={() => {
          const btnsubmit = document.getElementById('btndeploymentdeletesubmit') as HTMLButtonElement;
          btnsubmit.click();
        }}
        children={
          <React.Fragment>
            <Formik
              initialValues={deployment}
              validationSchema={deletedeploymentValidator}
              enableReinitialize={true}
              validateOnBlur={true}
              onSubmit={async (values, { setSubmitting, setErrors }) => {

                globalContext.showProcessing();

                // determine if there are any versions for this deployment, if there are, prevent the deletion
                var deploymentVersionsResponse = await ProductService.getDeploymentVersionListByDeploymentName(productName as string, selecteddeployment.deploymentName);

                if (deploymentVersionsResponse.success) {
                  if (deploymentVersionsResponse.value && deploymentVersionsResponse.value.length > 0) {
                    toast.error("You must delete all versions for the deployment first.");
                    globalContext.hideProcessing();
                    return;
                  }
                }

                var deleteResult = await ProductService.deleteDeployment(selecteddeployment.productName, selecteddeployment.deploymentName);

                if (handleSubmissionErrorsForForm((item) => {
                }, (item) => {
                }, setFormError, 'deployment', deleteResult)) {
                  toast.error(formError);
                  globalContext.hideProcessing();
                  return;
                }

                await getDeploymentsList();
                globalContext.hideProcessing();
                toast.success("Deployment Deleted Successfully!");

                setSubmitting(false);
                CloseDeploymentDeleteDialog();
                CloseDeploymentDialog();
              }}
            >
              {({ handleChange, values, handleBlur, touched, errors, handleSubmit }) => (
                <form autoComplete={"off"} onSubmit={handleSubmit}>
                  <table>
                    <tbody>
                      <tr>
                        <td colSpan={2}>
                          <span> Are you sure you want to delete this Deployment ?</span>
                        </td>
                      </tr>
                      <tr>
                        <td colSpan={2}>
                          {
                            <React.Fragment>
                              <span>Type the deploymentname</span>
                              <br />
                              <TextField
                                name={'selecteddeploymentName'}
                                value={values.deployment.selecteddeploymentName}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                errorMessage={getDeleteErrorString(touched, errors, 'selecteddeploymentName')}
                                placeholder={'Deployment Name'}
                                className="txtFormField" />
                            </React.Fragment>
                          }
                        </td>
                      </tr>
                    </tbody>
                  </table>
                  <div style={{ display: 'none' }}>
                    <PrimaryButton type="submit" id="btndeploymentdeletesubmit" text="Save" />
                  </div>
                </form>
              )}
            </Formik>
          </React.Fragment>
        } />
    </React.Fragment>
  );
}

export type IDeploymenVersionFormProps = {
  formError?: string | null;
  selectedVersion: IDeploymentVersionFormValues;
  hideVersionDialog: () => void;
  refreshVersionList: () => void;
  productType: string;
  isNewVersion: boolean;
}

export type IDeploymentVersionListProps = {
  deploymentVersionList: IDeploymentVersionModel[];
  productName: string;
  productType: string;
  selectedDeploymentName: string;
  setDeploymentVersionList: any;
  openVersionDialog: any;
  setIsVersionEdit: any;
  setSelectedVersion: any;
  isNewVersionDisabled: boolean;
}

//#region Version
export const VersionForm: React.FunctionComponent<IDeploymenVersionFormProps> = (props) => {
  const { values, handleChange, handleBlur, touched, errors, handleSubmit, setFieldValue } = useFormikContext<IDeploymentVersionFormValues>(); // formikProps
  const {
    hideVersionDialog, selectedVersion, isNewVersion, productType, refreshVersionList
  } = props;
  const [formError, setFormError] = useState<string | null>(null);
  const [authenticationTypes, setAuthenticationTypes] = useState<IChoiceGroupOption[]>([]);
  const [amlWorkspaceDropdownOptions, setAMLWorkspaceDropdownOptions] = useState<IDropdownOption[]>([]);

  const [sourceDropdownOptions, setSourceDropdownOptions] = useState<IDropdownOption[]>([]);
  const [publishedpipelineDropdownOptions, setPublishedpipelineDropdownOptions] = useState<IDropdownOption[]>([]);
  const [deployModelIdDropdownOptions, setDeployModelAPIDropdownOptions] = useState<IDropdownOption[]>([]);
  const [batchInferenceIdDropdownOptions, setBatchInferenceAPIDropdownOptions] = useState<IDropdownOption[]>([]);
  const [trainModelIdDropdownOptions, settrainModelIdDropdownOptions] = useState<IDropdownOption[]>([]);


  const [versionDeleteDialog, setVersionDeleteDialog] = useState<boolean>(false);

  const [selectedversion, setSelectedversion] = useState<IDeploymentVersionModel>(getInitialVersion);
  const [isAmlPipeline, setIsAmlPipeline] = useState<boolean>(false);
  const [isGitRepo, setIsGitRepo] = useState<boolean>(false);
  const [isUpload, setIsUpload] = useState<boolean>(false);
  const [isDisbalePipeLine, setIsDisbalePipeLine] = useState<boolean>(true);

  //let [version, setVersion] = useState<IDeploymentVersionModel>(initialVersionValues);
  const globalContext = useGlobalContext();
  let fileReader;

  const getSourceDropdownOptions = async () => {
    // let workspaceOptions: IDropdownOption[] = [];
    // workspaceOptions.push(
    //   { key: '', text: 'Select' },
    //   { key: 'aml_pipelines', text: 'AML PipeLines' },
    //   { key: 'git', text: 'GIT repo' },
    //   { key: 'upload', text: 'Upload Project' },
    // );
    const results = await ProductService.getSourceModelList();
    if (results && results.value && results.success) {
      let workspaceOptions: IDropdownOption[] = [];

      workspaceOptions.push(
        { key: '', text: 'select' }
      );

      results.value.map((value, index) => {
        workspaceOptions.push(
          { key: value.id, text: value.displayName },
        )
        return workspaceOptions;
      });
      setSourceDropdownOptions(workspaceOptions);
    }
    else {
      toast.error('Failed to load the Source options');

    }
  }

  const getAMLWorkspaceDropdownOptions = async () => {
    // load the aml workspace dropdown results
    const results = await ProductService.getAmlWorkSpaceList();
    if (results && results.value && results.success) {
      let workspaceOptions: IDropdownOption[] = [];

      workspaceOptions.push({ key: '', text: 'select' });
      results.value.map((value, index) => {
        workspaceOptions.push(
          { key: value.workspaceName, text: value.workspaceName },
        )
        return workspaceOptions;
      });
      setAMLWorkspaceDropdownOptions(workspaceOptions);
    } else
      toast.error('Failed to load the AML Workspace options');
  }

  const getPublishedPipeLineDropdownOptions = async (workspaceName: string) => {

    const results = await ProductService.getPublishedPipeLineByAmlWorkSpaceList(workspaceName);
    if (results && results.value && results.success) {
      let workspaceOptions: IDropdownOption[] = [];
      workspaceOptions.push({ key: '', text: 'select' });
      results.value.map((value, index) => {
        workspaceOptions.push(
          { key: value.id, text: value.displayName, title: value.description },
        )
        return workspaceOptions;
      });
      setPublishedpipelineDropdownOptions(workspaceOptions);
      setIsDisbalePipeLine(false);
    } else
      toast.error('Failed to load the AML Workspace options');
  }

  useEffect(() => {


    console.log(values.version);

    let authTypes: IChoiceGroupOption[] = [];
    if (productType === "RTP") {
      authTypes.push({ key: 'Token', text: 'Token' });
      authTypes.push({ key: 'Key', text: 'Key' });
      authTypes.push({ key: 'None', text: 'None' });

    } else if (productType === "BI") {

    } else { // train your own model

    }

    setAuthenticationTypes([...authTypes]);

    getAMLWorkspaceDropdownOptions();
    getSourceDropdownOptions();

    if (values.version && values.version.amlWorkspaceName) {
      getPublishedPipeLineDropdownOptions(values.version.amlWorkspaceName as string);
      if (values.version.versionSourceType === 'amlPipeline') {
        setIsAmlPipeline(true);
        setIsGitRepo(false);
        setIsUpload(false);
      }
      else if (values.version.versionSourceType === 'git') {
        setIsGitRepo(true);
        setIsAmlPipeline(false);
        setIsUpload(false);
      }
      else if (values.version.versionSourceType === 'upload') {
        setIsAmlPipeline(false);
        setIsGitRepo(false);
        setIsUpload(true);
      }
    }


    Hub.listen('AMLWorkspaceCreated', (data) => {
      console.log('captured workspace created in version form');
      getAMLWorkspaceDropdownOptions();
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getVersionFormErrorString = (touched, errors, property: string) => {
    return (touched.version && errors.version && touched.version[property] && errors.version[property]) ? errors.version[property] : '';
  };

  const getDeleteVersionFormErrorString = (touched, errors, property: string) => {
    return (touched.selectedVersionName && errors.selectedVersionName && touched[property] && errors[property]) ? errors[property] : '';
  };

  const DisplayErrors = (errors, values) => {
    console.log('display errors:');
    console.log(errors);
    console.log(values);
    return null;
  };

  const deleteDeploymentVersion = async (versionSelected: IDeploymentVersionModel) => {
    setSelectedversion(versionSelected);
    setVersionDeleteDialog(true);

    // globalContext.showProcessing();
    // var deploymentDeleteVersionResult = await ProductService.deleteDeploymentVersion(versionSelected.productName, versionSelected.deploymentName, versionSelected.versionName);

    // if (handleSubmissionErrorsForForm((item) => {
    // }, (item) => {
    // }, setFormError, 'version', deploymentDeleteVersionResult)) {
    //   toast.error(formError);
    //   globalContext.hideProcessing();
    //   return;
    // }

    // await refreshVersionList();
    // globalContext.hideProcessing();
    // toast.success("Deployment Version Deleted Successfully!");
    // hideVersionDialog();
  };

  const authenticationOnChange = (fieldKey: string, setFieldValue, ev?: React.SyntheticEvent<HTMLElement>, option?: IChoiceGroupOption) => {

    if (option) {

      setFieldValue(fieldKey, option.key, true);
    }
  };

  const selectOnChange = (fieldKey: string, setFieldValue, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    if (option) {
      let key = (option.key as string);
      setFieldValue(fieldKey, key, true);

      if (key === 'amlPipeline') {
        setIsAmlPipeline(true);
        setIsGitRepo(false);
        setIsUpload(false);
      }
      else if (key === 'git') {
        setIsGitRepo(true);
        setIsAmlPipeline(false);
        setIsUpload(false);
      }/* zb: not handling upload for now
      else if (key === 'upload') {
        setIsAmlPipeline(false);
        setIsGitRepo(false);
        setIsUpload(true);
      }*/
    }
  };

  const amlWorkspaceselectOnChange = (fieldKey: string, setFieldValue, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    if (option) {
      let key = (option.key as string);
      setFieldValue(fieldKey, key, true);

      if (key == "") {
        setFieldValue("version.batchInferenceId", "", true);
        setFieldValue("version.trainModelId", "", true);
        setFieldValue("version.deployModelId", "", true);
        setIsDisbalePipeLine(true);
      }
      else {
        getPublishedPipeLineDropdownOptions(option.key as string);
      }
    }
  };

  const CloseDeploymentVersionDeleteDialog = () => {
    setVersionDeleteDialog(false);
  }

  const TemplateFileRead = (setFieldValue) => {
    const content = fileReader.result;
    setFieldValue(`values.version.projectFileContent`, content, true)
  }

  const uploadfile = (event, idx, setFieldValue) => {
    let file = event.target.files[0];
    if (file) {
      setFieldValue('values.version.projectFileUrl', file.name, true)
      if (file.type === "application/json") {
        fileReader = new FileReader();
        fileReader.onloadend = (e) => {
          TemplateFileRead(setFieldValue)
        };
        fileReader.readAsText(file);
      }
    } else {
      setFieldValue(`templates.${idx}.templateFilePath`, '', true)
    }
  }


  return (
    <React.Fragment>
      <form style={{ width: '100%' }} autoComplete={"off"} onSubmit={handleSubmit}>
        <DisplayErrors errors={errors} values={values} />
        <React.Fragment>
          <input type="hidden" name="version.deploymentName" value={values.version.deploymentName} />
          <Stack className={"form_row"}>
            <FormLabel title={"Deployment Name:"} toolTip={ProductMessages.Version.DeploymentName} />
            <TextField
              name={'version.deploymentName'}
              value={values.version.deploymentName}
              disabled={true}
              className="txtFormField" />
          </Stack>
          <Stack className={"form_row"}>
            <FormLabel title={"Version Name:"} toolTip={ProductMessages.Version.VersionName} />
            <TextField
              name={'version.versionName'}
              value={values.version.versionName}
              onChange={handleChange}
              onBlur={handleBlur}
              disabled={!isNewVersion}
              errorMessage={getVersionFormErrorString(touched, errors, 'versionName')}
              placeholder={'Version Name'}
              className="txtFormField" maxLength={50} />
          </Stack>
          {
            productType === 'RTP' ?
              <React.Fragment>
                <Stack className={"form_row"}>
                  <FormLabel title={"Real-time Predict API:"} toolTip={ProductMessages.Version.RealtimePredictAPI} />
                  <TextField
                    name={'version.realTimePredictAPI'}
                    value={values.version.realTimePredictAPI}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    errorMessage={getVersionFormErrorString(touched, errors, 'realTimePredictAPI')}
                    placeholder={'Real-time Predict API'}
                    className="txtFormField" />
                </Stack>

                <input type="hidden" name="version.deployModelId" value={values.version.deployModelId} />

                <Stack className={"form_row"}>
                  <FormLabel title={"Authentication:"} toolTip={ProductMessages.Version.Authentication} />
                  <ChoiceGroup
                    styles={{
                      flexContainer: { display: "flex", justifyContent: 'space-between', width: 300 }
                    }}
                    onChange={(event, option) => {
                      authenticationOnChange('version.authenticationType', setFieldValue, event, option)
                    }}
                    className="defaultChoiceGroup" name="version.authenticationType"
                    selectedKey={values.version.authenticationType}
                    options={authenticationTypes}
                  />
                </Stack>
                {(values.version.authenticationType === 'Token' ?
                  <React.Fragment>
                    <FormLabel title={"AML Workspace:"} toolTip={ProductMessages.Version.AMLWorkspace} />
                    <Stack className={"form_row"} horizontal={true} gap={15}>
                      <Dropdown
                        style={{ width: 150 }}
                        options={amlWorkspaceDropdownOptions}
                        id={`version.amlWorkspaceName`} onBlur={handleBlur}
                        onChange={(event, option, index) => {
                          selectOnChange(`version.amlWorkspaceName`, setFieldValue, event, option, index)
                        }}
                        errorMessage={getVersionFormErrorString(touched, errors, 'amlWorkspaceName')}
                        defaultSelectedKey={values.version.amlWorkspaceName}
                      />
                      <DefaultButton type="button" id="btnCreateNewAMLWorkspace" className="addbutton"
                        onClick={() => {

                          Hub.dispatch(
                            'AMLWorkspaceNewDialog',
                            {
                              event: 'NewDialog',
                              data: true,
                              message: ''
                            });

                        }}>Create New
                </DefaultButton>
                    </Stack>
                  </React.Fragment>
                  : (values.version.authenticationType === 'Key' ?
                    <Stack className={"form_row"}>
                      <FormLabel title={"Key:"} toolTip={ProductMessages.Version.Key} />
                      <TextField
                        name={'version.authenticationKey'}
                        value={values.version.authenticationKey}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        type={'password'}
                        errorMessage={getVersionFormErrorString(touched, errors, 'authenticationKey')}
                        placeholder={'Authentication Key'}
                        className="txtFormField" />
                    </Stack>
                    : null)
                )}
                <Stack className={"form_row"}>
                  <FormLabel title={"Advanced Settings:"} toolTip={ProductMessages.Version.AdvancedSettings} />
                  <TextField
                    name={'version.advancedSettings'}
                    value={(values.version.advancedSettings ? values.version.advancedSettings : '')}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    errorMessage={getVersionFormErrorString(touched, errors, 'advancedSettings')}
                    placeholder={'key1=value1;key2=value2'}
                    className="txtFormField" />
                </Stack>
              </React.Fragment>
              :
              <React.Fragment>

                <Stack className={"form_row source"}>
                  <FormLabel title={"Source:"} toolTip={ProductMessages.Version.Source} />
                  <Dropdown
                    style={{ width: 155 }}
                    options={sourceDropdownOptions}
                    id={`version.versionSourceType`} onBlur={handleBlur}
                    onChange={(event, option, index) => {
                      selectOnChange(`version.versionSourceType`, setFieldValue, event, option, index)
                    }}
                    errorMessage={getVersionFormErrorString(touched, errors, 'versionSourceType')}
                    defaultSelectedKey={values.version.versionSourceType}
                  />
                </Stack>
                {
                  isGitRepo ?
                    <React.Fragment>
                      <Stack className={"form_row"}>
                        <FormLabel title={"Git Url:"} toolTip={ProductMessages.Version.BatchInferenceAPI} />
                        <TextField
                          name={'version.gitUrl'}
                          value={values.version.gitUrl}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getVersionFormErrorString(touched, errors, 'gitUrl')}
                          placeholder={'Git Url'}
                          className="txtFormField" />
                      </Stack>
                      <Stack className={"form_row"}>
                        <FormLabel title={"Version:"} toolTip={ProductMessages.Version.BatchInferenceAPI} />
                        <TextField
                          name={'version.gitVersion'}
                          value={values.version.gitVersion}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getVersionFormErrorString(touched, errors, 'gitVersion')}
                          placeholder={'Version'}
                          className="txtFormField" />
                      </Stack>
                      <Stack className={"form_row"}>
                        <FormLabel title={"Personal access tokens:"} toolTip={ProductMessages.Version.BatchInferenceAPI} />

                        <TextField
                          type={'password'}
                          name={'version.gitPersonalAccessToken'}
                          value={values.version.gitPersonalAccessToken}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getVersionFormErrorString(touched, errors, 'gitPersonalAccessToken')}
                          placeholder={'Personal access tokens'}
                          className="txtFormField" />
                      </Stack>
                          <Stack className={"form_row"} horizontal={true} gap={15}>
                            
                      <FormLabel title={"AML Workspace:"} toolTip={ProductMessages.Version.AMLWorkspace} />
                            <Dropdown
                              style={{ width: 150 }}
                              options={amlWorkspaceDropdownOptions}
                              id={`version.amlWorkspaceName`} onBlur={handleBlur}
                              onChange={(event, option, index) => {
                                amlWorkspaceselectOnChange(`version.amlWorkspaceName`, setFieldValue, event, option, index)
                              }}
                              errorMessage={getVersionFormErrorString(touched, errors, 'amlWorkspaceName')}
                              defaultSelectedKey={values.version.amlWorkspaceName}
                            />
                            <DefaultButton type="button" id="btnCreateNewAMLWorkspace" className="addbutton"
                              onClick={() => {

                                Hub.dispatch(
                                  'AMLWorkspaceNewDialog',
                                  {
                                    event: 'NewDialog',
                                    data: true,
                                    message: ''
                                  });

                              }}>Create New
                </DefaultButton>
                          </Stack>
                    </React.Fragment>
                    : null
                }
                {
                  isAmlPipeline ?
                    <React.Fragment>
                      {(values.version.authenticationType === 'Token' ?
                        <React.Fragment>
                          <FormLabel title={"AML Workspace:"} toolTip={ProductMessages.Version.AMLWorkspace} />
                          <Stack className={"form_row"} horizontal={true} gap={15}>
                            <Dropdown
                              style={{ width: 150 }}
                              options={amlWorkspaceDropdownOptions}
                              id={`version.amlWorkspaceName`} onBlur={handleBlur}
                              onChange={(event, option, index) => {
                                amlWorkspaceselectOnChange(`version.amlWorkspaceName`, setFieldValue, event, option, index)
                              }}
                              errorMessage={getVersionFormErrorString(touched, errors, 'amlWorkspaceName')}
                              defaultSelectedKey={values.version.amlWorkspaceName}
                            />
                            <DefaultButton type="button" id="btnCreateNewAMLWorkspace" className="addbutton"
                              onClick={() => {

                                Hub.dispatch(
                                  'AMLWorkspaceNewDialog',
                                  {
                                    event: 'NewDialog',
                                    data: true,
                                    message: ''
                                  });

                              }}>Create New
                </DefaultButton>
                          </Stack>
                        </React.Fragment>
                        : (values.version.authenticationType === 'Key' ?
                          <Stack className={"form_row"}>
                            <FormLabel title={"Key:"} toolTip={ProductMessages.Version.Key} />
                            <TextField
                              name={'version.authenticationKey'}
                              value={values.version.authenticationKey}
                              onChange={handleChange}
                              onBlur={handleBlur}
                              type={'password'}
                              errorMessage={getVersionFormErrorString(touched, errors, 'authenticationKey')}
                              placeholder={'Authentication Key'}
                              className="txtFormField" />
                          </Stack>
                          : null)
                      )}
                      <React.Fragment>
                        {productType === 'RTP' ?
                          <Stack className={"form_row"}>
                            <FormLabel title={"Real-time Predict API:"} toolTip={ProductMessages.Version.RealtimePredictAPI} />
                            <TextField
                              name={'version.realTimePredictAPI'}
                              value={values.version.realTimePredictAPI}
                              onChange={handleChange}
                              onBlur={handleBlur}
                              errorMessage={getVersionFormErrorString(touched, errors, 'realTimePredictAPI')}
                              placeholder={'Real-time Predict API'}
                              className="txtFormField" />
                          </Stack> : <input type="hidden" name="version.realTimePredictAPI" value={values.version.realTimePredictAPI} />}
                        {productType === 'TYOM' ?
                          isAmlPipeline ?
                            <Stack className={"form_row"}>
                              <FormLabel title={"Training API:"} toolTip={ProductMessages.Version.TrainingAPI} />
                              <Dropdown
                                style={{ width: 350 }}
                                options={publishedpipelineDropdownOptions}
                                id={`version.trainModelId`} onBlur={handleBlur}
                                onChange={(event, option, index) => {
                                  selectOnChange(`version.trainModelId`, setFieldValue, event, option, index)
                                }}
                                errorMessage={getVersionFormErrorString(touched, errors, 'trainModelId')}
                                defaultSelectedKey={values.version.trainModelId} disabled={isDisbalePipeLine}
                              />
                            </Stack>
                            : null
                          : <input type="hidden" name="version.trainModelId" value={values.version.trainModelId} />}
                        {productType === 'BI' || productType === 'TYOM' ?
                          isAmlPipeline ?
                            <Stack className={"form_row"}>
                              <FormLabel title={"(Async) Batch Inference API:"} toolTip={ProductMessages.Version.BatchInferenceAPI} />
                              <Dropdown
                                style={{ width: 350 }}
                                options={publishedpipelineDropdownOptions}
                                id={`version.batchInferenceId`} onBlur={handleBlur}
                                onChange={(event, option, index) => {
                                  selectOnChange(`version.batchInferenceId`, setFieldValue, event, option, index)
                                }}
                                errorMessage={getVersionFormErrorString(touched, errors, 'batchInferenceId')}
                                defaultSelectedKey={values.version.batchInferenceId} disabled={isDisbalePipeLine}
                              />
                            </Stack>
                            : null
                          : <input type="hidden" name="version.batchInferenceId" value={values.version.batchInferenceId} />}
                        {productType === 'TYOM' ?

                          isAmlPipeline ?
                            <Stack className={"form_row"}>
                              <FormLabel title={"Deploy Endpoint API:"} toolTip={ProductMessages.Version.DeployEndpointAPI} />
                              <Dropdown
                                style={{ width: 350 }}
                                options={publishedpipelineDropdownOptions}
                                id={`version.deployModelId`} onBlur={handleBlur}
                                onChange={(event, option, index) => {
                                  selectOnChange(`version.deployModelId`, setFieldValue, event, option, index)
                                }}
                                errorMessage={getVersionFormErrorString(touched, errors, 'deployModelId')}
                                defaultSelectedKey={values.version.deployModelId} disabled={isDisbalePipeLine}
                              />
                            </Stack>
                            : null
                          : <input type="hidden" name="version.deployModelId" value={values.version.deployModelId} />}
                        {productType === 'RTP' ?
                          <Stack className={"form_row"}>
                            <FormLabel title={"Authentication:"} toolTip={ProductMessages.Version.Authentication} />
                            <ChoiceGroup
                              styles={{
                                flexContainer: { display: "flex", justifyContent: 'space-between', width: 300 }
                              }}
                              onChange={(event, option) => {
                                authenticationOnChange('version.authenticationType', setFieldValue, event, option)
                              }}
                              className="defaultChoiceGroup" name="version.authenticationType"
                              selectedKey={values.version.authenticationType}
                              options={authenticationTypes}
                            />
                          </Stack> : <input type="hidden" name="version.authenticationType" value={values.version.authenticationType} />}
                      </React.Fragment>
                    </React.Fragment>
                    : null

                }
                {
                  isUpload
                    ?
                    <Stack className={"form_row source"}>
                      <FormLabel title={"Project File:"} toolTip={ProductMessages.Version.ProjectFile} />

                      <label className="versionProjectupload">
                        <span className="filetittle" title={values.version.projectFileUrl}>{values.version.projectFileUrl}</span>
                        <span className="versionProjectuploadbrowsebutton">Browse</span>
                        <input type="file" onChange={(event) => {
                          uploadfile(event, 'values.version.projectFileUrl', setFieldValue)
                        }} onBlur={handleBlur}
                          accept="application/JSON" style={{ width: 0 }} title="Select Template File"
                          name={'values.version.projectFileUrl'}
                          id={'values.version.projectFileUrl'}
                        />
                      </label>
                    </Stack>
                    : null
                }
                <Stack className={"form_row"}>
                  <FormLabel title={"Advanced Settings:"} toolTip={ProductMessages.Version.AdvancedSettings} />
                  <TextField
                    name={'version.advancedSettings'}
                    value={(values.version.advancedSettings ? values.version.advancedSettings : '')}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    errorMessage={getVersionFormErrorString(touched, errors, 'advancedSettings')}
                    placeholder={'key1=value1;key2=value2'}
                    className="txtFormField" />
                </Stack>
              </React.Fragment>
          }
        </React.Fragment>

        <DialogFooter>
          <Stack horizontal={true} gap={15} style={{ width: '100%' }}>
            {!isNewVersion &&
              <DefaultButton type="button" id="btnsubmit" className="addbutton"
                onClick={() => {
                  deleteDeploymentVersion(selectedVersion.version)
                }}>
                <FontIcon iconName="Cancel" className="deleteicon" /> Delete
          </DefaultButton>
            }
            <div style={{ flexGrow: 1 }}></div>
            <AlternateButton
              onClick={hideVersionDialog}
              text="Cancel" />
            <PrimaryButton
              type="submit"
              text="Save" />
          </Stack>
        </DialogFooter>
      </form>

      <DialogBox keyindex='DeploymentVersionmodal' dialogVisible={versionDeleteDialog}
        title="Delete Deployment Version" subText="" isDarkOverlay={true} className="" cancelButtonText="Cancel"
        submitButtonText="Submit" maxwidth={500}
        cancelonClick={() => {
          CloseDeploymentVersionDeleteDialog();
        }}
        submitonClick={() => {
          const btnsubmit = document.getElementById('btnVersionDeleteubmit') as HTMLButtonElement;
          btnsubmit.click();
        }}
        children={
          <React.Fragment>
            <Formik
              initialValues={values.version}
              validationSchema={deleteVersionValidator}
              enableReinitialize={true}
              validateOnBlur={true}
              onSubmit={async (values, { setSubmitting, setErrors }) => {

                globalContext.showProcessing();
                var deploymentDeleteVersionResult = await ProductService.deleteDeploymentVersion(selectedversion.productName, selectedversion.deploymentName, selectedversion.versionName);

                if (handleSubmissionErrorsForForm((item) => {
                }, (item) => {
                }, setFormError, 'version', deploymentDeleteVersionResult)) {
                  toast.error(formError);
                  globalContext.hideProcessing();
                  return;
                }

                await refreshVersionList();
                globalContext.hideProcessing();
                toast.success("Deployment Version Deleted Successfully!");
                CloseDeploymentVersionDeleteDialog();
                hideVersionDialog();
              }}
            >
              {({ handleChange, values, handleBlur, touched, errors, handleSubmit }) => (
                <form autoComplete={"off"} onSubmit={handleSubmit}>
                  <input type="hidden" name={'version.versionName'} value={values.versionName} />
                  <table>
                    <tbody>
                      <tr>
                        <td colSpan={2}>
                          <span> Are you sure you want to delete version?</span>
                        </td>
                      </tr>
                      <tr>
                        <td colSpan={2}>
                          {
                            <React.Fragment>
                              <span>Type the version name</span>
                              <br />
                              <TextField
                                name={'selectedVersionName'}
                                value={values.selectedVersionName}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                errorMessage={getDeleteVersionFormErrorString(touched, errors, 'selectedVersionName')}
                                placeholder={'Version Name'}
                                className="txtFormField" />
                            </React.Fragment>
                          }
                        </td>
                      </tr>
                    </tbody>
                  </table>
                  <div style={{ display: 'none' }}>
                    <PrimaryButton type="submit" id="btnVersionDeleteubmit" text="Save" />
                  </div>
                </form>
              )}
            </Formik>
          </React.Fragment>
        } />
    </React.Fragment>
  );
}

export const VersionList: React.FunctionComponent<IDeploymentVersionListProps> = (props) => {
  //const {values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty} = useFormikContext<IDeploymentVersionModel[]>(); // formikProps

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const { openVersionDialog, selectedDeploymentName, productName, productType, setIsVersionEdit,
    deploymentVersionList, setSelectedVersion, isNewVersionDisabled } = props;

  //const globalContext = useGlobalContext();

  const OpenNewVersionDialog = () => {
    let v = getInitialVersion();
    v.deploymentName = selectedDeploymentName;
    v.productName = productName;
    setIsVersionEdit(false);
    v.authenticationType = "Token";
    //TODO: confirm what the default authenticationtypes should be for the other product types
    /*if (productType == "RTP") {
      v.authenticationType = "Token";
    } else if (productType == "BI") {
      v.authenticationType = "None";
    } else { // train your own model
      v.authenticationType = "None";
    }*/

    setSelectedVersion({ version: v });
    openVersionDialog();
  }

  const editVersionItem = (values, index) => {
    setIsVersionEdit(true);
    setSelectedVersion({ version: values });
    openVersionDialog();
  }

  return (
    <React.Fragment>
      <h3>Versions</h3>
      <table className="noborder offergrid">
        <thead>
          <tr>
            <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}>
              Version Name
          </th>
            <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}>
              APIs
          </th>
            <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}>
              Authentication Type
          </th>
            <th style={{ width: 50, borderBottom: '1px solid #e8e8e8' }}>
              Operations
          </th>
          </tr>
        </thead>
        <tbody>
          {
            deploymentVersionList.length === 0
              ? <tr>
                <td colSpan={4} style={{ textAlign: "center", paddingTop: '5%' }}>
                  <span>No Data Exists</span>
                </td>
              </tr>
              :
              deploymentVersionList.map((value: IDeploymentVersionModel, idx) => {
                return (
                  <tr key={idx}>
                    <td>
                      <span style={{ width: 200 }}>{value.versionName}</span>
                    </td>
                    <td>
                      <span style={{ width: 200 }}>{value.productName}</span>
                    </td>
                    <td>
                      <span style={{ width: 200 }}>{value.authenticationType}</span>
                    </td>
                    <td>
                      <Stack
                        verticalAlign="center"
                        horizontalAlign={"space-evenly"}
                        gap={15}
                        horizontal={true}
                        styles={{
                          root: {},
                        }}
                      >
                        <FontIcon iconName="Edit" className="deleteicon" onClick={() => {
                          editVersionItem(value, idx)
                        }} />
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
              <PrimaryButton type="button" id="btnnewVersion" text="New Version" onClick={OpenNewVersionDialog}
                disabled={isNewVersionDisabled} />
            </td>
          </tr>
        </tfoot>
      </table>
    </React.Fragment>
  );
}
//#endregion 

export default ProductDeployments;