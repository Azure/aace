import React, { useEffect, useState } from 'react';
import {
  Stack,
  PrimaryButton,
  MessageBar,
  MessageBarType,
  Dialog, DialogType, DialogFooter,
  FontIcon,
  TextField,
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { useHistory } from "react-router";
import { WebRoute } from "../../shared/constants/routes";
import { Result, IProductModel } from '../../models';
import { Loading } from "../../shared/components/Loading";
import AlternateButton from "../../shared/components/AlternateButton";
import {
  initialInfoFormValues, IProductInfoFormValues,
  productInfoValidationSchema, initialProductValues, deleteProductValidator, initialProductList
} from "./formUtils/ProductFormUtils";
import { Formik } from "formik";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { DialogBox } from '../../shared/components/Dialog';
import { ProductForm } from './Info';
import ProductService from '../../services/ProductService';

const Products: React.FunctionComponent = () => {
  const history = useHistory();
  const globalContext = useGlobalContext();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formState, setFormState] = useState<IProductInfoFormValues>(initialInfoFormValues);
  const [products, setProducts] = useState<IProductModel[]>([]);
  const [loadingProducts, setLoadingProducts] = useState<boolean>(true);
  const [productDialogVisible, setProductDialogVisible] = useState<boolean>(false);
  const [productDeleteDialog, setProductDeleteDialog] = useState<boolean>(false);
  const [selectedProduct, setselectedProduct] = useState<IProductModel>(initialProductValues);
  const [formError, setFormError] = useState<string | null>(null);

  const getProducts = async () => {

    setLoadingProducts(true);
    // const results = await ProductService.list();
    // if (results && !results.hasErrors && results.value)
    //   setProducts(results.value);
    // else {
    //   setProducts([]);
    //   if (results.hasErrors) {
    //     // TODO: display errors
    //     alert(results.errors.join(', '));
    //   }
    // }

    setProducts(initialProductList);

    setLoadingProducts(false);
  }
  const editItem = (productId: string): void => {
    // history.push(WebRoute.ModifyProductInfo.replace(':productId', productId));

    history.push(WebRoute.ProductDetail.replace(':productId', productId));
    // let product = initialProductList.filter(p => p.productId == productId)[0];
    // setFormState(
    //   {
    //     product: { ...product }
    //   });
    // setProductDialogVisible(true);
  };

  const deleteItem = (productSelected: IProductModel, idx: number): void => {
    productSelected.selectedProductindex = idx;
    setselectedProduct(productSelected);
    setProductDeleteDialog(true);
  };

  const Products = ({ products }) => {
    if (!products || products.length === 0) {
      return <tr>
        <td colSpan={4}><span>No Products</span></td>
      </tr>;
    } else {
      return (
        products.map((value: IProductModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span style={{ width: 200 }}>{value.productId}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.productType}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.hostType}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.owner}</span>
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
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => { editItem(value.productId) }} />
                  <FontIcon iconName="Cancel" className="deleteicon" onClick={() => { deleteItem(value, idx) }} />
                </Stack>
              </td>
            </tr>
          );
        })
      );
    }
  }

  useEffect(() => {
    getProducts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const hideNewProductDialog = (): void => {
    setProductDialogVisible(false);
  };

  const CloseProductDeleteDialog = () => {
    setProductDeleteDialog(false);
  }

  const handleFormSubmission = async (e) => {
    if (globalContext.saveForm)
      await globalContext.saveForm();
  };

  const handleSubmissionErrors = (result: Result<any>, setSubmitting: any): boolean => {
    if (result.hasErrors) {
      // TODO - display the errors here
      alert(result.errors.join(', '));
      setSubmitting(false);
      return true;
    }
    return false;
  }

  const getFormErrorString = (touched, errors, property: string) => {
    return touched && errors && touched[property] && errors[property] ? errors[property] : '';
  };

  const showNewProductDialog = (): void => {
    setProductDialogVisible(true);
  };

  const handleNewProduct = (): void => {
    showNewProductDialog();
  };

  return (
    <Stack
      verticalAlign="start"
      horizontal={false}
      styles={{
        root: {
          width: '100%',
          height: '100%',
          textAlign: 'left',
        }
      }}
    >
      <Stack
        horizontalAlign="start"
        verticalAlign="center"
        styles={{
          root: {
            width: '100%'
          }
        }}
      >
        <Stack
          horizontalAlign="start"
          verticalAlign="center"
          styles={{
            root: {
              width: '100%',
              //position:'absolute'
            }
          }}
        >
          <PrimaryButton text={"New Product"} onClick={handleNewProduct} />

          <PrimaryButton text={"Copy Luna webhook URL"} className="OrangeButton" style={{ left: '15%', bottom: '50%' }} />
        </Stack>
        <table className="noborder offergrid" style={{ marginTop: 20, width: '100%' }} cellPadding={5} cellSpacing={0}>
          <thead>
            <tr style={{ fontWeight: 'normal' }}>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Product ID"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Product Type"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Host Type"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Owner"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Operations"} /></th>
            </tr>
          </thead>
          <tbody>
            {loadingProducts ?
              (
                <tr>
                  <td colSpan={4} align={"center"}>
                    <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                      <Loading />
                    </Stack>
                  </td>
                </tr>
              )
              : <Products products={products} />}
          </tbody>
        </table>
      </Stack>
      <Dialog
        hidden={!productDialogVisible}
        onDismiss={hideNewProductDialog}

        dialogContentProps={{
          styles: {
            subText: {
              paddingTop: 0
            },
            title: {
            }

          },
          type: DialogType.normal,
          title: 'New Product'
        }}
        modalProps={{
          isBlocking: true,
          styles: {

            main: {
              minWidth: 440
            }
          }
        }}
      >
        <Formik
          initialValues={formState}
          validationSchema={productInfoValidationSchema}
          validateOnBlur={true}
          onSubmit={async (values, { setSubmitting, setErrors }) => {

            setFormError(null);
            setSubmitting(true);

            globalContext.showProcessing();
            var CreateProductResult = await ProductService.create(values.product);
            if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'product', CreateProductResult)) {
              globalContext.hideProcessing();
              return;
            }

            setSubmitting(false);
            globalContext.hideProcessing();
            toast.success("Success!");
            if (CreateProductResult.value != null)
              history.push(WebRoute.ProductDetail.replace(':productId', CreateProductResult.value.productId));
          }}
        >
          <ProductForm isNew={true} products={products} />
        </Formik>
        <DialogFooter>
          <AlternateButton
            onClick={hideNewProductDialog}
            text="Cancel" />
          <PrimaryButton
            onClick={handleFormSubmission}
            text="Save" />
        </DialogFooter>
      </Dialog>

      <DialogBox keyindex='Productmodal' dialogVisible={productDeleteDialog}
        title="Delete Product" subText="" isDarkOverlay={true} className="" cancelButtonText="Cancel"
        submitButtonText="Submit" maxwidth={500}
        cancelonClick={() => {
          CloseProductDeleteDialog();
        }}
        submitonClick={() => {
          const btnsubmit = document.getElementById('btnsubmit') as HTMLButtonElement;
          btnsubmit.click();
        }}
        children={
          <React.Fragment>
            <Formik
              initialValues={selectedProduct}
              validationSchema={deleteProductValidator}
              enableReinitialize={true}
              validateOnBlur={true}
              onSubmit={async (values, { setSubmitting, setErrors }) => {
                // var productDeleteResponse = await ProductService.delete(values.selectedProductId as string);
                // handleSubmissionErrors(productDeleteResponse, setSubmitting);
                // if (productDeleteResponse.success) {
                //     products.splice(values.selectedProductindex as number, 1);
                //   setProducts([...products]);
                // }

                products.splice(values.selectedProductindex as number, 1);
                setProducts([...products]);
                setSubmitting(false);
                CloseProductDeleteDialog();
              }}
            >
              {({ handleChange, values, handleBlur, touched, errors, handleSubmit }) => (
                <form autoComplete={"off"} onSubmit={handleSubmit}>
                  <table>
                    <tbody>
                      <tr>
                        <td colSpan={2}>
                          <span> Are you sure you want to delete this product ?</span>
                        </td>
                      </tr>
                      <tr>
                        <td>
                          <b>Product ID:</b>
                        </td>
                        <td>
                          <span>{values.productId} </span>
                        </td>
                      </tr>
                      <tr>
                        <td>
                          <b>Owner: </b>
                        </td>
                        <td>
                          <span>{values.owner}</span>
                        </td>
                      </tr>
                      <tr>
                        <td colSpan={2}>
                          {
                            <React.Fragment>
                              <span>Type the product id</span>
                              <br />
                              <TextField
                                name={'selectedProductId'}
                                value={values.selectedProductId}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                errorMessage={getFormErrorString(touched, errors, 'selectedProductId')}
                                placeholder={'Product Id'}
                                className="txtFormField" />
                            </React.Fragment>
                          }
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
          </React.Fragment>
        } />
    </Stack>
  );
}

export default Products;