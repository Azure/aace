import React, { useEffect, useState } from 'react';
import {
  DefaultButton,
  Dialog,
  DialogFooter,
  DialogType,
  FontIcon,
  PrimaryButton,
  Stack,
  TextField
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { Formik } from "formik";
import { IAMLWorkSpaceModel } from "../../models";
import { Loading } from "../../shared/components/Loading";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import AlternateButton from '../../shared/components/AlternateButton';
import {
  aMLWorkSpaceFormValidationSchema,
  IAMLWorkSpaceFormValues,
  initialAMLWorkSpaceFormValues,
  initialAMLWorkSpaceValues,
  deleteAMLWorkSpaceValidator,
} from './formUtils/AMLWorkSpaceUtils';
import { Hub } from "aws-amplify";
import ProductService from "../../services/ProductService";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { ProductMessages } from '../../shared/constants/infomessages';
import { DialogBox } from '../../shared/components/Dialog';

const AMLWorkSpace: React.FunctionComponent = () => {

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const globalContext = useGlobalContext();
  //const [formError, setFormError] = useState<string | null>(null);

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
      <AMLWorkSpaceList />
    </Stack>
  );
};

export type IAMLWorkSpaceListProps = {}
export const AMLWorkSpaceList: React.FunctionComponent<IAMLWorkSpaceListProps> = (props) => {
  //const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty } = useFormikContext<IAMLWorkSpaceListProps>(); // formikProps
  //const { } = props;
  let [workSpaceList, setWorkSpaceList] = useState<IAMLWorkSpaceModel[]>();
  let [workSpace, setWorkSpace] = useState<IAMLWorkSpaceFormValues>(initialAMLWorkSpaceFormValues);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  let [workSpaceDeleteIndex, setworkSpaceDeleteIndex] = useState<number>(0);
  const [loadingWorkSpace, setLoadingWorkSpace] = useState<boolean>(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [workSpaceDialogVisible, setWorkSpaceDialogVisible] = useState<boolean>(false);
  const [isDisplayDeleteButton, setDisplayDeleteButton] = useState<boolean>(true);
  const [isEdit, setisEdit] = useState<boolean>(true);

  const [AMLDeleteDialog, setAMLDeleteDialog] = useState<boolean>(false);
  const [selectedAML, setSelectedAML] = useState<IAMLWorkSpaceModel>(initialAMLWorkSpaceValues);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const globalContext = useGlobalContext();

  const getWorkSpaceList = async () => {

    setLoadingWorkSpace(true);
    const results = await ProductService.getAmlWorkSpaceList();
    if (results && results.value && results.success) {
      setWorkSpaceList(results.value);
      //   if (results.value.length > 4)
      //     body.style.height = 'auto';
      // }
      //setworkSpaceList(initialAMLWorkSpaceList);
      setLoadingWorkSpace(false);
    } else
      toast.error('Failed to load AML Workspaces');
  }

  const getFormErrorString = (touched, errors, property: string) => {
    return touched.aMLWorkSpace && errors.aMLWorkSpace && touched.aMLWorkSpace[property] && errors.aMLWorkSpace[property] ? errors.aMLWorkSpace[property] : '';
  };

  const getDeleteAMLErrorString = (touched, errors, property: string) => {
    return (touched.selectedWorkspaceName && errors.selectedWorkspaceName && touched[property] && errors[property]) ? errors[property] : '';
  };

  const editWorkSpace = async (workspaceName: string, idx: number) => {

    //let editedWorkspace = initialAMLWorkSpaceList.filter(a => a.workspaceName === Id)[0];
    let editedWorkspace = await ProductService.getAmlWorkSpaceByName(workspaceName);
    if (editedWorkspace && editedWorkspace.value && editedWorkspace.success) {
      setWorkSpace({ aMLWorkSpace: editedWorkspace.value });
      setworkSpaceDeleteIndex(idx);
    } else
      toast.error('Failed to load AML Workspaces');

    setisEdit(true);
    setDisplayDeleteButton(true);
    OpenWorkSpaceDialog();
    //history.push(WebRoute.ModifyProductInfo.replace(':productName', productName));
  };

  const deleteWorkSpace = async (aMLWorkSpaceModelSelected: IAMLWorkSpaceModel) => {

    setSelectedAML(aMLWorkSpaceModelSelected);
    setAMLDeleteDialog(true);

  };

  const OpenNewWorkSpaceDialog = () => {

    setWorkSpace(initialAMLWorkSpaceFormValues);
    setisEdit(false);
    setDisplayDeleteButton(false);
    OpenWorkSpaceDialog();
  }

  const OpenWorkSpaceDialog = () => {
    setWorkSpaceDialogVisible(true);
  }

  const CloseWorkSpaceDialog = () => {
    setWorkSpaceDialogVisible(false);
  }

  useEffect(() => {

    getWorkSpaceList();

    Hub.listen('AMLWorkspaceNewDialog', (data) => {
      OpenNewWorkSpaceDialog();
    })

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const WorkSpaceList = ({ amlWorkSpace }) => {
    if (!amlWorkSpace || amlWorkSpace.length === 0) {
      return <tr>
        <td colSpan={4}><span>No AML Workspaces</span></td>
      </tr>;
    } else {
      return (
        amlWorkSpace.map((value: IAMLWorkSpaceModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span>{value.workspaceName}</span>
              </td>
              <td>
                <span>{value.aadTenantId}</span>
              </td>
              <td>
                <span>{value.resourceId}</span>
              </td>
              <td>
                <Stack
                  verticalAlign="center"
                  horizontalAlign={"space-evenly"}
                  gap={15}
                  horizontal={true}
                >
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => {
                    editWorkSpace(value.workspaceName, idx)
                  }} />
                  {/* <FontIcon iconName="Cancel" className="deleteicon" onClick={() => { deleteWorkSpace(value) }} /> */}
                </Stack>
              </td>
            </tr>
          );
        })
      );

    }
  }

  const CloseAMLDeleteDialog = () => {
    setAMLDeleteDialog(false);
  }

  return (
    <React.Fragment>

      <React.Fragment>
        <h3 style={{ textAlign: 'left', fontWeight: 'normal', marginTop: 0, marginBottom: 20, width: '100%' }}>AML
          Workspaces</h3>
        <table className="noborder offer" cellPadding={5} cellSpacing={0}>
          <thead>
            <tr>
              <th style={{ width: 200 }}>
                <FormLabel title={"WorkSpace Name"} />
              </th>
              <th style={{ width: 300 }}>
                <FormLabel title={"AAD Tenant Id"} />
              </th>
              <th style={{ width: 300 }}>
                <FormLabel title={"Resource Id"} />
              </th>
              <th style={{ width: 100 }}>
                <FormLabel title={"Operations"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {loadingWorkSpace ?
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
              <WorkSpaceList amlWorkSpace={workSpaceList} />
            }
          </tbody>
          <tfoot>
            <tr>
              <td colSpan={3} style={{ paddingTop: '1%' }}>
                <PrimaryButton text={"Register New WorkSpace"} onClick={() => {
                  OpenNewWorkSpaceDialog()
                }} />
              </td>
            </tr>
          </tfoot>
        </table>
      </React.Fragment>

      <Dialog
        hidden={!workSpaceDialogVisible}
        onDismiss={CloseWorkSpaceDialog}
        dialogContentProps={{
          styles: {
            subText: {
              paddingTop: 0
            },
            title: {}

          },
          type: DialogType.normal,
          title: 'Register AML WorkSpace'
        }}
        modalProps={{
          isBlocking: true,
          isDarkOverlay: true,
          styles: {
            main: {
              minWidth: '35% !important',

            }
          }
        }}
      >
        <Formik
          initialValues={workSpace}
          validationSchema={aMLWorkSpaceFormValidationSchema}
          enableReinitialize={true}
          validateOnBlur={true}
          onSubmit={async (values, { setSubmitting, setErrors }) => {

            setFormError(null);
            setSubmitting(true);
            globalContext.showProcessing();

            //TODO: PUT THIS BACK IN
            var createWorkSpaceResult = await ProductService.createOrUpdateWorkSpace(values.aMLWorkSpace);
            if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'aMLWorkSpace', createWorkSpaceResult)) {
              globalContext.hideProcessing();
              return;
            }

            setSubmitting(false);

            await getWorkSpaceList();
            globalContext.hideProcessing();
            toast.success("Success!");
            setisEdit(true);
            setDisplayDeleteButton(true);

            Hub.dispatch(
              'AMLWorkspaceCreated',
              {
                event: 'WorkspaceCreated',
                data: true,
                message: ''
              });

            CloseWorkSpaceDialog();
          }}
        >
          {({ handleChange, values, handleBlur, touched, errors, handleSubmit, submitForm, setFieldValue }) => (
            <table className="offer" style={{ width: '100%' }}>
              <tbody>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Workspace Name:"} toolTip={ProductMessages.AMLWorkSpace.WorkspaceName} />
                      <TextField
                        name={'aMLWorkSpace.workspaceName'}
                        value={values.aMLWorkSpace.workspaceName}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'workspaceName')}
                        placeholder={'Workspace Name'}
                        className="txtFormField wdth_100_per" disabled={isEdit} max={50} />
                    </React.Fragment>
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Resource Id:"} toolTip={ProductMessages.AMLWorkSpace.ResourceId} />
                      <TextField
                        name={'aMLWorkSpace.resourceId'}
                        value={values.aMLWorkSpace.resourceId}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'resourceId')}
                        placeholder={'Resource Id'}
                        className="txtFormField wdth_100_per" />
                    </React.Fragment>

                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Tenant Id:"} toolTip={ProductMessages.AMLWorkSpace.AADTenantId} />
                      <TextField
                        name={'aMLWorkSpace.aadTenantId'}
                        value={values.aMLWorkSpace.aadTenantId}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'aadTenantId')}
                        placeholder={'AAD Tenant Id'}
                        className="txtFormField wdth_100_per" />
                    </React.Fragment>
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"AAD Application Id:"} toolTip={ProductMessages.AMLWorkSpace.AADApplicationId} />
                      <TextField
                        name={'aMLWorkSpace.aadApplicationId'}
                        value={values.aMLWorkSpace.aadApplicationId}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'aadApplicationId')}
                        placeholder={'AAD Application Id'}
                        className="txtFormField wdth_100_per" />
                    </React.Fragment>
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"AADApplication Secret:"} toolTip={ProductMessages.AMLWorkSpace.AADApplicationSecret} />
                      <TextField
                        name={'aMLWorkSpace.aadApplicationSecrets'}
                        value={values.aMLWorkSpace.aadApplicationSecrets}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        type={'password'}
                        errorMessage={getFormErrorString(touched, errors, 'aadApplicationSecrets')}
                        placeholder={'AAD Application Secret'}
                        className="txtFormField wdth_100_per" />
                    </React.Fragment>
                  </td>
                </tr>
                <tr>
                  <td colSpan={2}>
                    <DialogFooter>
                      <Stack horizontal={true} gap={15} style={{ width: '100%' }}>
                        {isDisplayDeleteButton &&
                          <DefaultButton type="button" id="btnsubmit" className="addbutton"
                            onClick={() => {
                              deleteWorkSpace(workSpace.aMLWorkSpace)
                            }}>
                            <FontIcon iconName="Cancel" className="deleteicon" /> Delete
                      </DefaultButton>
                        }
                        <div style={{ flexGrow: 1 }}></div>
                        <AlternateButton
                          onClick={CloseWorkSpaceDialog}
                          text="Cancel" className="mar-right-2_Per" />
                        <PrimaryButton type="submit" id="btnsubmit" className="mar-right-2_Per"
                          text={isDisplayDeleteButton ? "Update" : "Create"} onClick={submitForm} />
                      </Stack>
                    </DialogFooter>
                  </td>
                </tr>
              </tbody>
            </table>
          )}
        </Formik>
      </Dialog>

      <DialogBox keyindex='DeploymentVersionmodal' dialogVisible={AMLDeleteDialog}
        title="Delete Deployment WorkSpace" subText="" isDarkOverlay={true} className="" cancelButtonText="Cancel"
        submitButtonText="Submit" maxwidth={500}
        cancelonClick={() => {
          CloseAMLDeleteDialog();
        }}
        submitonClick={() => {
          const btnsubmit = document.getElementById('btnAMLDelete') as HTMLButtonElement;
          btnsubmit.click();
        }}
        children={
          <React.Fragment>
            <Formik
              initialValues={selectedAML}
              validationSchema={deleteAMLWorkSpaceValidator}
              enableReinitialize={true}
              validateOnBlur={true}
              onSubmit={async (values, { setSubmitting, setErrors }) => {

                globalContext.showProcessing();
                var workspaceResult = await ProductService.deleteWorkSpace(selectedAML.workspaceName);

                if (handleSubmissionErrorsForForm((item) => {
                }, (item) => {
                }, setFormError, 'aMLWorkSpace', workspaceResult)) {
                  toast.error(formError);
                  globalContext.hideProcessing();
                  return;
                }

                await getWorkSpaceList();
                globalContext.hideProcessing();
                toast.success("AML Workspace Deleted Successfully!");

                CloseWorkSpaceDialog();
              }}
            >
              {({ handleChange, values, handleBlur, touched, errors, handleSubmit }) => (
                <form autoComplete={"off"} onSubmit={handleSubmit}>
                  <input type="hidden" name={'aMLWorkSpace.workspaceName'} value={values.workspaceName} />
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
                              <span>Type the workspace name</span>
                              <br />
                              <TextField
                                name={'selectedWorkspaceName'}
                                value={values.selectedWorkspaceName}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                errorMessage={getDeleteAMLErrorString(touched, errors, 'selectedWorkspaceName')}
                                placeholder={'WorkSpace Name'}
                                className="txtFormField" />
                            </React.Fragment>
                          }
                        </td>
                      </tr>
                    </tbody>
                  </table>
                  <div style={{ display: 'none' }}>
                    <PrimaryButton type="submit" id="btnAMLDelete" text="Save" />
                  </div>
                </form>
              )}
            </Formik>
          </React.Fragment>
        } />
    </React.Fragment>
  );
}

export default AMLWorkSpace;