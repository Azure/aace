import React, { useEffect, useState } from 'react';
import { DefaultButton, FontIcon, getTheme, PrimaryButton, Stack, TextField } from 'office-ui-fabric-react';
import { useHistory, useLocation } from 'react-router';
//import { LaoutHelper, LayoutHelperMenuItem } from "./Layout";
import ProductService from "../services/ProductService";
import { initialProductValues, deleteProductValidator } from '../routes/Products/formUtils/ProductFormUtils'
import { IProductModel } from "../models";
import { useGlobalContext } from "../shared/components/GlobalProvider";

import 'react-confirm-alert/src/react-confirm-alert.css';
import { handleSubmissionErrorsForForm } from "../shared/formUtils/utils";
import { toast } from "react-toastify";
import { DialogBox } from '../shared/components/Dialog';
import { Formik } from 'formik';

type ProductProps = {
  productName: string | null;
};

const ProductContent: React.FunctionComponent<ProductProps> = (props) => {

  const { productName } = props;

  const history = useHistory();
  const location = useLocation();
  const globalContext = useGlobalContext();
  const [productModel, setProductModel] = useState<IProductModel>(initialProductValues);
  const [formError, setFormError] = useState<string | null>(null);

  const [ProductDeleteDialog, setProductDeleteDialog] = useState<boolean>(false);
  const [selectedProductName, setSelectedProductName] = useState<string>('');

  const theme = getTheme();

  const getProductInfo = async (productName: string) => {

    let response = await ProductService.get(productName);

    //let response = initialProductList.filter(p=>p.productName==productName)[0];

    if (!response.hasErrors && response.value) {
      setProductModel({ ...response.value })
    }

  }



  useEffect(() => {
    if (productName)
      getProductInfo(productName);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [productName]);

  useEffect(() => {

    // setHideSave(location.pathname.toLowerCase().endsWith("/plans"));
    // setHideSave(location.pathname.toLowerCase().endsWith("/meters"));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [history.location, location.pathname]);

  const handleProductDeletion = async (e) => {

    setSelectedProductName(productName as string);
    setProductDeleteDialog(true);

    // globalContext.showProcessing();

    // // determine if there are any deployments or aml workspaces, if there are, prevent the deletion
    // var deploymentsResponse = await ProductService.getDeploymentListByProductName(productName as string);

    // if (deploymentsResponse.success) {
    //   if (deploymentsResponse.value && deploymentsResponse.value.length > 0) {
    //     toast.error("You must delete all deployments for the product first.");
    //     globalContext.hideProcessing();
    //     return;
    //   }
    // }

    // const deleteResult = await ProductService.delete(productName as string);

    // if (handleSubmissionErrorsForForm((item) => {},(item) => {}, setFormError, 'product', deleteResult)) {
    //   toast.error(formError);
    //   globalContext.hideProcessing();
    //   return;
    // }

    // globalContext.hideProcessing();
    // toast.success("Product Deleted Successfully!");
    // history.push(`/products/`);
  };

  const OnCancel = async (e) => {
    history.push(`/products/`);
  };

  const CloseProductDeleteDialog = () => {
    setProductDeleteDialog(false);
  }

  const getDeleteProductErrorString = (touched, errors, property: string) => {
    return (touched.selectedProductId && errors.selectedProductId && touched[property] && errors[property]) ? errors[property] : '';
  };


  return (
    <Stack
      horizontal={true}
      horizontalAlign={"space-evenly"}
      styles={{
        root: {
          height: 'calc(100% - 57px)',
          backgroundColor: theme.palette.neutralLight
        }
      }}
    >
      <Stack
        horizontal={false}
        verticalAlign={"start"}
        verticalFill={true}
        styles={{
          root: {
            flexGrow: 1,
            maxWidth: 1234,
            backgroundColor: 'white'
          }
        }}
      >
        {/* Product Details Header */}
        <Stack
          horizontal={true}
          verticalAlign={"center"}
          verticalFill={true}
          className={"offer-details-header"}
          styles={{
            root: {
              height: 70,
              paddingLeft: 31,
              paddingRight: 31,
              width: '100%'
            }
          }}
        >
          <Stack.Item styles={{
            root: {
              flexGrow: 0
            }
          }}>
            <span style={{ fontWeight: 'bold', marginRight: 20, fontSize: 18 }}>
              Product Details
            </span>
            <span className={"offer-details-separator"}></span>
            <span style={{ fontWeight: 600 }}>
              ID:
            </span>
            <span style={{ marginLeft: 8 }}>
              {productModel.productName}
            </span>
            <span style={{ marginLeft: 100, fontWeight: 600 }}>
              Product Type:
            </span>
            <span style={{ marginLeft: 8 }}>
              {productModel.productType}
            </span>
            <span style={{ marginLeft: 100, fontWeight: 600 }}>
              Host Type:
            </span>
            <span style={{ marginLeft: 8 }}>
              {productModel.hostType}
            </span>
          </Stack.Item>
          <Stack.Item styles={{
            root: {
              flexGrow: 1
            }
          }}>
            <Stack
              horizontal={true}
              verticalAlign={"center"}
              verticalFill={true}
              horizontalAlign={"end"}
              gap={15}
            >
              <DefaultButton onClick={handleProductDeletion} className="addbutton">
                <FontIcon iconName="Cancel" className="deleteicon" /> Delete
              </DefaultButton>
              <PrimaryButton text={"Go Back"} onClick={OnCancel} />

            </Stack>
          </Stack.Item>
        </Stack>
        {/* Offer navigation */}
        <Stack
          horizontal={true}
          verticalAlign={"center"}
          verticalFill={true}
          className={"nav-header Productnav-header"}
          styles={{
            root: {
              paddingLeft: 31,
              paddingRight: 31,
              height: 45,
              marginBottom: 20
            }
          }}
        >
          {/* {layoutHelper.menuItems.map((value: LayoutHelperMenuItem, idx) => {
            return (
              <Stack
                key={`menuItem_${idx}`}
                horizontal={true}
                verticalAlign={"center"}
                verticalFill={true}
                onClick={value.menuClick}
                styles={{
                  root: {
                    height: 40,
                    fontWeight: (isNavItemActive(value.paths) ? 600 : 'normal'),
                    borderBottom: (isNavItemActive(value.paths) ? 'solid 2px #0078d4' : 'none'),
                    marginTop: (isNavItemActive(value.paths) ? 2 : 0),
                    paddingLeft: 20,
                    paddingRight: 20,
                    minWidth: 94,
                    cursor: 'pointer'
                  }
                }}
              >
                <span style={{ textAlign: "center", whiteSpace: "nowrap", flexGrow: 1 }}>
                  {value.title}
                </span>
              </Stack>
            )
          })} */}
        </Stack>
        <div className="innercontainer">
          {props.children}
        </div>

      </Stack>

      <DialogBox keyindex='DeploymentVersionmodal' dialogVisible={ProductDeleteDialog}
        title="Delete Deployment WorkSpace" subText="" isDarkOverlay={true} className="" cancelButtonText="Cancel"
        submitButtonText="Submit" maxwidth={500}
        cancelonClick={() => {
          CloseProductDeleteDialog();
        }}
        submitonClick={() => {
          const btnsubmit = document.getElementById('btnProductDelete') as HTMLButtonElement;
          btnsubmit.click();
        }}
        children={
          <React.Fragment>
            <Formik
              initialValues={productModel}
              validationSchema={deleteProductValidator}
              enableReinitialize={true}
              validateOnBlur={true}
              onSubmit={async (values, { setSubmitting, setErrors }) => {

                globalContext.showProcessing();

                // determine if there are any deployments or aml workspaces, if there are, prevent the deletion
                var deploymentsResponse = await ProductService.getDeploymentListByProductName(productName as string);

                if (deploymentsResponse.success) {
                  if (deploymentsResponse.value && deploymentsResponse.value.length > 0) {
                    toast.error("You must delete all deployments for the product first.");
                    globalContext.hideProcessing();
                    return;
                  }
                }

                const deleteResult = await ProductService.delete(productName as string);

                if (handleSubmissionErrorsForForm((item) => {},(item) => {}, setFormError, 'product', deleteResult)) {
                  toast.error(formError);
                  globalContext.hideProcessing();
                  return;
                }

                globalContext.hideProcessing();
                toast.success("Product Deleted Successfully!");
                history.push(`/products/`);
              }}
            >
              {({ handleChange, values, handleBlur, touched, errors, handleSubmit }) => (
                <form autoComplete={"off"} onSubmit={handleSubmit}>
                  <input type="hidden" name={'aMLWorkSpace.workspaceName'} value={selectedProductName} />
                  <table>
                    <tbody>
                      <tr>
                        <td colSpan={2}>
                          <span> Are you sure you want to delete Product?</span>
                        </td>
                      </tr>
                      <tr>
                        <td colSpan={2}>
                          {
                            <React.Fragment>
                              <span>Type the product Id</span>
                              <br />
                              <TextField
                                name={'selectedProductId'}
                                value={values.selectedProductId}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                errorMessage={getDeleteProductErrorString(touched, errors, 'selectedProductId')}
                                placeholder={'Product Id'}
                                className="txtFormField" />
                            </React.Fragment>
                          }
                        </td>
                      </tr>
                    </tbody>
                  </table>
                  <div style={{ display: 'none' }}>
                    <PrimaryButton type="submit" id="btnProductDelete" text="Save" />
                  </div>
                </form>
              )}
            </Formik>
          </React.Fragment>
        } />
    </Stack>
  );
};

export default ProductContent;