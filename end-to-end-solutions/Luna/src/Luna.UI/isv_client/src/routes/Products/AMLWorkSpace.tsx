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
import { IDeploymentsModel, IDeploymentVersionModel, IAMLWorkSpaceModel } from "../../models";
import { Loading } from "../../shared/components/Loading";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import { initialProductList } from './formUtils/ProductFormUtils';
import ProductService from '../../services/ProductService';
import { DialogBox } from '../../shared/components/Dialog';
import AlternateButton from '../../shared/components/AlternateButton';
import { RouteComponentProps } from 'react-router-dom';
import { initialAMLWorkSpaceList, IAMLWorkSpaceFormValues, initialAMLWorkSpaceFormValues, aMLWorkSpaceFormValidationSchema } from './formUtils/AMLWorkSpaceUtils';

const AMLWorkSpace: React.FunctionComponent = () => {

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
        initialValues={initialAMLWorkSpaceList}
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
        <AMLWorkSpaceList />
      </Formik>
    </Stack>
  );
};

export type IAMLWorkSpaceListProps = {
}
export const AMLWorkSpaceList: React.FunctionComponent<IAMLWorkSpaceListProps> = (props) => {
  const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty } = useFormikContext<IAMLWorkSpaceListProps>(); // formikProps
  const { } = props;
  let [workSpaceList, setworkSpaceList] = useState<IAMLWorkSpaceModel[]>();
  let [workSpace, setworkSpace] = useState<IAMLWorkSpaceFormValues>(initialAMLWorkSpaceFormValues);
  const [loadingWorkSpace, setLoadingWorkSpace] = useState<boolean>(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [workSpaceDialogVisible, setWorkSpaceDialogVisible] = useState<boolean>(false);
  const [isDisplayDeleteButton, setDisplayDeleteButton] = useState<boolean>(true);
  const [isEdit, setisEdit] = useState<boolean>(true);

  const { productId } = useParams();
  const globalContext = useGlobalContext();
  //Below code is for making design proper in Armtemplate page.  
  let body = (document.getElementsByClassName('App')[0] as HTMLElement);

  useEffect(() => {

    getWorkSpaceList();
    return () => {
      body.style.height = '100%';
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getWorkSpaceList = async () => {

    setLoadingWorkSpace(true);
    // const results = await ProductService.getAmlWorkSpaceList();
    // if (results && results.value && results.success) {
    //   setworkSpaceList(results.value);
    //   if (results.value.length > 4)
    //     body.style.height = 'auto';
    // }
    setworkSpaceList(initialAMLWorkSpaceList);
    setLoadingWorkSpace(false);
  }

  const getFormErrorString = (touched, errors, property: string) => {
    return touched.aMLWorkSpace && errors.aMLWorkSpace && touched.aMLWorkSpace[property] && errors.aMLWorkSpace[property] ? errors.aMLWorkSpace[property] : '';
  };

  const editWorkSpace = (Id: string): void => {
    let editedWorkspace = initialAMLWorkSpaceList.filter(a => a.workspaceId == Id)[0];
    setworkSpace({ aMLWorkSpace: editedWorkspace });
    setisEdit(true);
    setDisplayDeleteButton(true);
    OpenWorkSpaceDialog();
    //history.push(WebRoute.ModifyProductInfo.replace(':productId', productId));
  };

  const deleteWorkSpace = async (aMLWorkSpaceModelSelected: IAMLWorkSpaceModel) => {
    // var deploymentDeleteResult = await ProductService.deleteWorkSpace(aMLWorkSpaceModelSelected.resourceId);
    // if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'deployment', deploymentDeleteResult)) {
    //   globalContext.hideProcessing();
    //   return;    
    getWorkSpaceList();
  };

  const OpenNewWorkSpaceDialog = () => {

    setworkSpace(initialAMLWorkSpaceFormValues);
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

  const WorkSpaceList = ({ amlWorkSpace }) => {
    if (!amlWorkSpace || amlWorkSpace.length === 0) {
      return <tr>
        <td colSpan={4}><span>No Deployments</span></td>
      </tr>;
    } else {
      return (
        amlWorkSpace.map((value: IAMLWorkSpaceModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span style={{ width: 200 }}>{value.workspaceId}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.resourceId}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.aADApplicationId}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.aADApplicationSecret}</span>
              </td>
              <td style={{width: '20%'}}>
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
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => { editWorkSpace(value.workspaceId) }} />
                  {/* <FontIcon iconName="Cancel" className="deleteicon" onClick={() => { deleteWorkSpace(value) }} /> */}
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

      <React.Fragment>
        <h3 style={{ textAlign: 'left', fontWeight: 'normal', marginTop: 0, marginBottom: 20, width: '100%' }}>Deployments</h3>
        <table className="noborder offer" style={{}} cellPadding={5} cellSpacing={0}>
          <thead>
            <tr>
              <th style={{ width: 334 }}>
                <FormLabel title={"WorkSpace Id"} />
              </th>
              <th>
                <FormLabel title={"Resource Id"} />
              </th>
              <th>
                <FormLabel title={"AAD Application Id"} />
              </th>
              <th>
                <FormLabel title={"AAD Application Secret"} />
              </th>
              <th>
              </th>
            </tr>
          </thead>
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
            <tbody>
              <WorkSpaceList amlWorkSpace={workSpaceList} />
            </tbody>}
          <tfoot>
            <tr>
              <td colSpan={3} style={{ paddingTop: '1%' }}>
                <PrimaryButton text={"Register New WorkSpace"} onClick={() => { OpenNewWorkSpaceDialog() }} />
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
            title: {
            }

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
        <>
          <Formik
            initialValues={workSpace}
            validationSchema={aMLWorkSpaceFormValidationSchema}
            enableReinitialize={true}
            validateOnBlur={true}
            onSubmit={async (values, { setSubmitting, setErrors }) => {
              debugger
              setFormError(null);
              setSubmitting(true);
              globalContext.showProcessing();

              initialAMLWorkSpaceList.push(
                {
                  aADApplicationId: values.aMLWorkSpace.aADApplicationId,
                  aADApplicationSecret: values.aMLWorkSpace.aADApplicationSecret,
                  resourceId: values.aMLWorkSpace.resourceId,
                  workspaceId: values.aMLWorkSpace.workspaceId,
                  isDeleted: false,
                  clientId: values.aMLWorkSpace.clientId
                })

              // var createWorkSpaceResult = await ProductService.createOrUpdateWorkSpace(values.aMLWorkSpace);
              // if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'deployment', createWorkSpaceResult)) {
              //   globalContext.hideProcessing();
              //   return;
              // }

              setSubmitting(false);
              globalContext.hideProcessing();
              setisEdit(true);
              setDisplayDeleteButton(true);              
              toast.success("Success!");
              getWorkSpaceList();
              CloseWorkSpaceDialog();
            }}
          >
            {({ handleChange, values, handleBlur, touched, errors, handleSubmit, submitForm, setFieldValue }) => (
              <table className="offer" style={{ width: '100%' }}>
                <tbody>
                  <tr>
                    <td>
                      <React.Fragment>
                        <span>WorkSpace Id</span>
                        <br />
                        <TextField
                          name={'aMLWorkSpace.workspaceId'}
                          value={values.aMLWorkSpace.workspaceId}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getFormErrorString(touched, errors, 'workspaceId')}
                          placeholder={'Workspace Id'}
                          className="txtFormField wdth_100_per" disabled={isEdit} max={50}/>
                      </React.Fragment>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <React.Fragment>
                        <span>Resource Id</span>
                        <br />
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
                        <span>AAD Application Id</span>
                        <br />
                        <TextField
                          name={'aMLWorkSpace.aADApplicationId'}
                          value={values.aMLWorkSpace.aADApplicationId}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getFormErrorString(touched, errors, 'aADApplicationId')}
                          placeholder={'AAD Application Id'}
                          className="txtFormField wdth_100_per" />
                      </React.Fragment>

                    </td>
                  </tr>
                  <tr>
                    <td>
                      <React.Fragment>
                        <span>AADApplication Secret</span>
                        <br />
                        <TextField
                          name={'aMLWorkSpace.aADApplicationSecret'}
                          value={values.aMLWorkSpace.aADApplicationSecret}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getFormErrorString(touched, errors, 'aADApplicationSecret')}
                          placeholder={'AAD Application Secret'}
                          className="txtFormField wdth_100_per" />
                      </React.Fragment>
                    </td>
                  </tr>
                  <tr>
                    <td colSpan={2} style={{ textAlign: 'right', paddingTop: '10%' }}>
                      <AlternateButton
                        onClick={CloseWorkSpaceDialog}
                        text="Cancel" className="mar-right-2_Per" />
                      <PrimaryButton type="submit" id="btnsubmit" className="mar-right-2_Per" text={isDisplayDeleteButton ? "Update" : "Create"} onClick={submitForm} />
                      {
                        isDisplayDeleteButton ?
                          <PrimaryButton type="button" id="btnsubmit" text="Delete" onClick={() => {
                            deleteWorkSpace(workSpace.aMLWorkSpace)
                          }} /> : null
                      }
                    </td>
                  </tr>
                </tbody>
              </table>
            )}
          </Formik>
        </>        
      </Dialog>
    </React.Fragment>
  );
}

export default AMLWorkSpace;