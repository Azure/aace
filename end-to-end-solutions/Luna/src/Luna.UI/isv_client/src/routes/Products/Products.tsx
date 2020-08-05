import React, { useEffect, useState } from 'react';
import {
  Dialog,
  DialogFooter,
  DialogType,
  Dropdown,
  FontIcon,
  IDropdownOption,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Stack,
  TextField
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { useHistory } from "react-router";
import { WebRoute } from "../../shared/constants/routes";
import { IError, IProductModel } from '../../models';
import { Loading } from "../../shared/components/Loading";
import AlternateButton from "../../shared/components/AlternateButton";
import { initialInfoFormValues, IProductInfoFormValues, productInfoValidationSchema } from "./formUtils/ProductFormUtils";
import { Formik, useFormikContext } from "formik";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import ProductService from '../../services/ProductService';
import { ProductMessages } from '../../shared/constants/infomessages';
import {CopyToClipboard} from 'react-copy-to-clipboard';
import ReactHtmlParser from 'react-html-parser';

export type IProductFormFormProps = {
  isNew: boolean;
  formError?: string | null;
  products: IProductModel[];
  productTypes: IDropdownOption[];
  hostTypes: IDropdownOption[];
}

export const ProductForm: React.FunctionComponent<IProductFormFormProps> = (props) => {
  const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty, setFieldValue } = useFormikContext<IProductInfoFormValues>(); // formikProps
  const { formError, isNew, productTypes, hostTypes } = props;

  const globalContext = useGlobalContext();

  useEffect(() => {
    globalContext.modifySaveForm(async () => {
      await submitForm();
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getProductFormErrorString = (touched, errors, property: string, dirty) => {
    setTimeout(() => { globalContext.setFormDirty(dirty); }, 500);

    return touched.product && errors.product && touched.product[property] && errors.product[property] ? errors.product[property] : '';
  };

  const DisplayErrors = (errors) => {
    return null;
  };
  const getidlist = (): string => {
    let idlist = ''
    props.products.map((values, index) => {
      idlist += values.productName + ',';
      return idlist;
    })
    values.product.Idlist = idlist.substr(0, idlist.length - 1);
    return idlist.substr(0, idlist.length - 1);
  }

  const selectOnChange = (fieldKey: string, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    if (option) {
      setFieldValue(fieldKey, option.key, false);
    }
  };

  const textboxClassName = (props.isNew ? "form_textboxmodal" : "form_textbox");

  return (
    <form style={{ width: '100%' }} autoComplete={"off"} onSubmit={handleSubmit}>
      {formError && <div style={{ marginBottom: 15 }}><MessageBar messageBarType={MessageBarType.error}>
        <div dangerouslySetInnerHTML={{ __html: formError }} style={{ textAlign: 'left' }}></div>
      </MessageBar></div>}
      <Stack
        verticalAlign="start"
        horizontal={false}
        gap={10}
        styles={{
          root: {}
        }}
      >
        <DisplayErrors errors={errors} />
        {isNew &&
          <React.Fragment>
            <Stack className={"form_row"}>
              <FormLabel title={"Id:"} toolTip={ProductMessages.product.ProductId} />
              <input type="hidden" name={'product.Idlist'} value={getidlist()} />
              <TextField
                name={'product.productName'}
                value={values.product.productName}
                maxLength={50}
                onChange={handleChange}
                onBlur={handleBlur}
                errorMessage={getProductFormErrorString(touched, errors, 'productName', dirty)}
                placeholder={'ID'}
                className={textboxClassName} />
            </Stack>
          </React.Fragment>
        }
        <Stack className={"form_row"}>
          <FormLabel title={"Product Type:"} toolTip={ProductMessages.product.ProductType} />
          <Dropdown
            options={productTypes}
            id={`product.productType`} onBlur={handleBlur}
            onChange={(event, option, index) => {
              selectOnChange(`product.productType`, event, option, index)
            }}
            errorMessage={getProductFormErrorString(touched, errors, 'productType', dirty)}
            defaultSelectedKey={values.product.productType}
          />
        </Stack>
        <Stack className={"form_row"}>
          <FormLabel title={"Host-Type:"} toolTip={ProductMessages.product.HostType} />
          <Dropdown
            options={hostTypes}
            id={`product.hostType`} onBlur={handleBlur}
            onChange={(event, option, index) => {
              selectOnChange(`product.hostType`, event, option, index)
            }}
            errorMessage={getProductFormErrorString(touched, errors, 'hostType', dirty)}
            defaultSelectedKey={values.product.productType}
          />
        </Stack>
        <Stack className={"form_row"}>
          <FormLabel title={"Owner:"} toolTip={ProductMessages.product.Owner} />
          <TextField
            name={'product.owner'}
            value={values.product.owner}
            onChange={handleChange}
            onBlur={handleBlur}
            errorMessage={getProductFormErrorString(touched, errors, 'owner', dirty)}
            placeholder={'Owner'}
            className={textboxClassName} />
        </Stack>
      </Stack>
    </form>
  );
}

const Products: React.FunctionComponent = () => {
  const history = useHistory();
  const globalContext = useGlobalContext();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formState, setFormState] = useState<IProductInfoFormValues>(initialInfoFormValues);
  const [products, setProducts] = useState<IProductModel[]>([]);
  const [loadingProducts, setLoadingProducts] = useState<boolean>(true);
  const [productDialogVisible, setProductDialogVisible] = useState<boolean>(false);
  const [productTypeDropdownOptions, setProductTypeDropdownOptions] = useState<IDropdownOption[]>([]);
  const [hostTypeDropdownOptions, setHostTypeDropdownOptions] = useState<IDropdownOption[]>([]);
  const [LunaWebhookUrlv2DialogVisible, setLunaWebhookUrlv2DialogVisible] = useState<boolean>(false);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formError, setFormError] = useState<string | null>(null);

  const getProducts = async () => {

    globalContext.showProcessing();
    setLoadingProducts(true);
    const [
      productResponse,
      productTypeResponse,
      hostTypeResponse
    ] = await Promise.all([
      await ProductService.list(),
      ProductService.getProductTypes(),
      ProductService.getHostTypes()
    ]);
    setLoadingProducts(false);
    globalContext.hideProcessing();

    if (productResponse.success && productTypeResponse.success && hostTypeResponse.success) {

      if (productResponse.value)
        setProducts(productResponse.value);
      else
        setProducts([]);

      let productTypeOptions: IDropdownOption[] = [];
      productTypeOptions.push({ key: '', text: 'Select...' });

      if (productTypeResponse.value) {
        productTypeResponse.value.map((value, index) => {
          productTypeOptions.push(
            { key: value.id, text: value.displayName },
          )
          return productTypeResponse;
        });
      }
      setProductTypeDropdownOptions(productTypeOptions);

      let hostTypeOptions: IDropdownOption[] = [];
      hostTypeOptions.push({ key: '', text: 'Select...' });

      if (hostTypeResponse.value) {
        hostTypeResponse.value.map((value, index) => {
          hostTypeOptions.push(
            { key: value.id, text: value.displayName },
          )
          return hostTypeResponse;
        });
      }
      setHostTypeDropdownOptions(hostTypeOptions);

    } else {
      let errorMessages: IError[] = [];

      errorMessages.concat(productResponse.errors);
      errorMessages.concat(productTypeResponse.errors);
      errorMessages.concat(hostTypeResponse.errors);

      if (errorMessages.length > 0) {
        toast.error(errorMessages.join(', '));
      }
    }
  }

  const editItem = (productName: string): void => {
    history.push(WebRoute.ProductDetail.replace(':productName', productName));
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
                <span style={{ width: 200 }}>{value.productName}</span>
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
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => { editItem(value.productName) }} />
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

  const handleFormSubmission = async (e) => {
    if (globalContext.saveForm)
      await globalContext.saveForm();
  };

  // const handleSubmissionErrors = (result: Result<any>, setSubmitting: any): boolean => {
  //   if (result.hasErrors) {
  //     // TODO - display the errors here
  //     alert(result.errors.join(', '));
  //     setSubmitting(false);
  //     return true;
  //   }
  //   return false;
  // }

  // const getFormErrorString = (touched, errors, property: string) => {
  //   return touched && errors && touched[property] && errors[property] ? errors[property] : '';
  // };

  const showNewProductDialog = (): void => {
    setProductDialogVisible(true);
  };

  const handleNewProduct = (): void => {
    showNewProductDialog();
  };

  const hideLunaWebhookUrlv2Dialog = (): void => {
    setLunaWebhookUrlv2DialogVisible(false);
  };

  const showLunaWebhookUrlv2Dialog = (): void => {
    setLunaWebhookUrlv2DialogVisible(true);
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

          <PrimaryButton text={"Copy Luna webhook URL"} style={{ left: '15%', bottom: '50%' }} onClick={showLunaWebhookUrlv2Dialog}/>
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
              history.push(WebRoute.ProductDetail.replace(':productName', CreateProductResult.value.productName));
          }}
        >
          <ProductForm isNew={true} products={products} hostTypes={hostTypeDropdownOptions} productTypes={productTypeDropdownOptions} />
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
      <Dialog
        hidden={!LunaWebhookUrlv2DialogVisible}
        onDismiss={hideLunaWebhookUrlv2Dialog}

        dialogContentProps={{
          styles: {
            subText: {
              paddingTop: 0
            },
            title: {}

          },
          type: DialogType.close,
          title: 'Luna webhook URLs',
          subText:ReactHtmlParser(ProductMessages.LunaWebHookURL.HeaderTitle) 
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
              <FormLabel title={"Subscribe webhook URL:"} />
              <div style={{ width: '100%' }}>
                <div style={{ width: '93%', float: 'left' }}>
                  <TextField
                    title={ProductMessages.LunaWebHookURL.SubscribewebhookURL}
                    name={'SubscribewebhookURL'}
                    value={ProductMessages.LunaWebHookURL.SubscribewebhookURL}
                    readOnly={true}
                    className={'subv2ipinput'} />
                </div>
                <div style={{ width: '5%', float: 'left', marginLeft: '2%' }}>
                  <CopyToClipboard text={ProductMessages.LunaWebHookURL.SubscribewebhookURL}>
                    <FontIcon style={{ lineHeight: '30px', fontSize: 20 }} iconName="Tab"
                      className='deleteicon subscribewebhookURL subv2ipinputcopy' onClick={() => {
                        let class1 = document.getElementsByClassName('unSubscribewebhookURL')[0] as HTMLElement;
                        let class2 = document.getElementsByClassName('suspendwebhookURL')[0] as HTMLElement;
                        class1.className = class1.className.replace('copied', '');
                        class2.className = class2.className.replace('copied', '');

                        let copied = document.getElementsByClassName('subscribewebhookURL')[0] as HTMLElement;
                        copied.className = copied.className + " copied";
                        toast.success("Copied !");                        
                      }} />
                  </CopyToClipboard>
                </div>
              </div>
            </Stack>
         
            <Stack className={"form_row"}>
              <FormLabel title={"UnSubscribe webhook URL:"} />
              <div style={{ width: '100%' }}>
                <div style={{ width: '93%', float: 'left' }}>
                  <TextField
                    title={ProductMessages.LunaWebHookURL.UnSubscribewebhookURL}
                    name={'UnSubscribewebhookURL'}
                    value={ProductMessages.LunaWebHookURL.UnSubscribewebhookURL}
                    readOnly={true}
                    className={'subv2ipinput'} />
                </div>
                <div style={{ width: '5%', float: 'left', marginLeft: '2%' }}>
                  <CopyToClipboard text={ProductMessages.LunaWebHookURL.UnSubscribewebhookURL}>
                    <FontIcon style={{ lineHeight: '30px', fontSize: 20 }} iconName="Tab"
                      className='deleteicon unSubscribewebhookURL subv2ipinputcopy' onClick={() => {
                        let class1 = document.getElementsByClassName('subscribewebhookURL')[0] as HTMLElement;
                        let class2 = document.getElementsByClassName('suspendwebhookURL')[0] as HTMLElement;
                        class1.className = class1.className.replace('copied', '');
                        class2.className = class2.className.replace('copied', '');

                        let copied = document.getElementsByClassName('unSubscribewebhookURL')[0] as HTMLElement;
                        copied.className = copied.className + " copied";
                        toast.success("Copied !");                        
                      }} />
                  </CopyToClipboard>
                </div>
              </div>
            </Stack>
            <Stack className={"form_row"}>
              <FormLabel title={"Suspend webhook URL:"} />
              <div style={{ width: '100%' }}>
                <div style={{ width: '93%', float: 'left' }}>
                  <TextField
                    title={ProductMessages.LunaWebHookURL.SuspendwebhookURL}
                    name={'SuspendwebhookURL'}
                    value={ProductMessages.LunaWebHookURL.SuspendwebhookURL}
                    readOnly={true}
                    className={'subv2ipinput'} />
                </div>
                <div style={{ width: '5%', float: 'left', marginLeft: '2%' }}>
                  <CopyToClipboard text={ProductMessages.LunaWebHookURL.SuspendwebhookURL}>
                    <FontIcon style={{ lineHeight: '30px', fontSize: 20 }} iconName="Tab"
                      className='deleteicon suspendwebhookURL subv2ipinputcopy' onClick={() => {
                        let class1 = document.getElementsByClassName('subscribewebhookURL')[0] as HTMLElement;
                        let class2 = document.getElementsByClassName('unSubscribewebhookURL')[0] as HTMLElement;
                        class1.className = class1.className.replace('copied', '');
                        class2.className = class2.className.replace('copied', '');

                        let copied = document.getElementsByClassName('suspendwebhookURL')[0] as HTMLElement;
                        copied.className = copied.className + " copied";
                        toast.success("Copied !");                        
                      }} />
                  </CopyToClipboard>
                </div>
              </div>
            </Stack>

          </div>
        </React.Fragment>
      </Dialog>
    </Stack>
  );
}

export default Products;