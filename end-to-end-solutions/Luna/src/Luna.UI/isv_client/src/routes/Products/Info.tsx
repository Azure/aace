import React, { useEffect, useState } from 'react';
import { MessageBar, MessageBarType, Stack, TextField, Dropdown, IDropdownOption, } from 'office-ui-fabric-react';
import { useParams } from "react-router";
import { Formik, useFormikContext } from "formik";
import ProductService from "../../services/ProductService";
import { Loading } from '../../shared/components/Loading';
import { IProductModel } from "../../models";
import {
  initialInfoFormValues, IProductInfoFormValues,
  productInfoValidationSchema, initialProductList, ProductType
} from "./formUtils/ProductFormUtils";
import FormLabel from "../../shared/components/FormLabel";
import { Products } from '../../shared/constants/infomessages';
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { toast } from "react-toastify";
import { useGlobalContext } from '../../shared/components/GlobalProvider';

const Info: React.FunctionComponent = () => {

  const { productId } = useParams();
  const globalContext = useGlobalContext();
  const [formState, setFormState] = useState<IProductInfoFormValues>(initialInfoFormValues);
  const [loadingFormData, setLoadingFormData] = useState<boolean>(true);
  const [formError, setFormError] = useState<string | null>(null);

  const getFormData = async (productId: string) => {

    setLoadingFormData(true);
    // const productResponse = await ProductService.get(productId);

    // // Global errors should have already been handled for get requests by this point
    // if (productResponse.value && productResponse.success) {
    //   var product = productResponse.value as IProductModel;

    //   setFormState(
    //     {
    //       product: { ...product }
    //     });
    // }

    let product = initialProductList.filter(p => p.productId == productId)[0];
    setFormState(
      {
        product: { ...product }
      });
    setLoadingFormData(false);

  }

  useEffect(() => {
    if (productId) {
      getFormData(productId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);


  if (loadingFormData)
    return (
      <Stack
        horizontalAlign="center"
        verticalAlign="center"
        verticalFill
        styles={{
          root: {
            width: '100%'
          }
        }}
      >
        <Loading />
      </Stack>
    );

  return (
    <Stack
      horizontalAlign="start"
      verticalAlign="start"
      verticalFill
      styles={{
        root: {
          margin: 31
        }
      }}
      gap={15}
    >
      <Formik
        validationSchema={productInfoValidationSchema}
        initialValues={formState}
        enableReinitialize={true}
        validateOnBlur={true}
        onSubmit={async (values, { setSubmitting, setErrors, resetForm }) => {

          setFormError(null);

          setSubmitting(true);
          globalContext.showProcessing();
          var updateProductResult = await ProductService.update(values.product);
          if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'product', updateProductResult)) {
            globalContext.hideProcessing();
            return;
          }

          setSubmitting(false);
          globalContext.hideProcessing();

          toast.success("Success!");

          getFormData((productId as string))
          setTimeout(() => { globalContext.setFormDirty(false); }, 500);

        }}
      >
        <ProductForm isNew={false} formError={formError} products={[]} />
      </Formik>
    </Stack>
  );
};

export type IProductFormFormProps = {
  isNew: boolean;
  formError?: string | null;
  products: IProductModel[];
}
export const ProductForm: React.FunctionComponent<IProductFormFormProps> = (props) => {
  const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty, setFieldValue } = useFormikContext<IProductInfoFormValues>(); // formikProps
  const { formError, isNew } = props;

  const globalContext = useGlobalContext();

  useEffect(() => {
    globalContext.modifySaveForm(async () => {
      await submitForm();
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getProductFormErrorString = (touched, errors, property: string, dirty) => {
    globalContext.setFormDirty(dirty);

    return touched.product && errors.product && touched.product[property] && errors.product[property] ? errors.product[property] : '';
  };

  const DisplayErrors = (errors) => {
    return null;
  };
  const getidlist = (): string => {
    let idlist = ''
    props.products.map((values, index) => {
      idlist += values.productId + ',';
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
              <FormLabel title={"Id:"} toolTip={Products.product.ProductId} />
              <input type="hidden" name={'product.Idlist'} value={getidlist()} />
              <TextField
                name={'product.productId'}
                value={values.product.productId}
                maxLength={50}
                onChange={handleChange}
                onBlur={handleBlur}
                errorMessage={getProductFormErrorString(touched, errors, 'productId', dirty)}
                placeholder={'ID'}
                className={textboxClassName} />
            </Stack>
          </React.Fragment>
        }
        <Stack className={"form_row"}>
          <FormLabel title={"Product Type:"} toolTip={Products.product.ProductType} />
          {/* <TextField
                name={'product.productType'}
                value={values.product.productType}
                onChange={handleChange}
                onBlur={handleBlur}
                errorMessage={getProductFormErrorString(touched, errors, 'productType', dirty)}
                placeholder={'Product Type'}
                className={textboxClassName} /> */}
          <Dropdown
            options={ProductType}
            id={`product.productType`} onBlur={handleBlur}
            onChange={(event, option, index) => {
              selectOnChange(`product.productType`, event, option, index)
            }}
            errorMessage={getProductFormErrorString(touched, errors, 'productType', dirty)}
            defaultSelectedKey={values.product.productType}
          />
        </Stack>
        <Stack className={"form_row"}>
          <FormLabel title={"Host-Type:"} toolTip={Products.product.HostType} />
          {/* <TextField
            name={'product.hostType'}
            value={values.product.hostType}
            onChange={handleChange}
            onBlur={handleBlur}
            errorMessage={getProductFormErrorString(touched, errors, 'hostType', dirty)}
            placeholder={'Host-Type'}
            className={textboxClassName} /> */}

          <Dropdown
            options={ProductType}
            id={`product.hostType`} onBlur={handleBlur}
            onChange={(event, option, index) => {
              selectOnChange(`product.hostType`, event, option, index)
            }}
            errorMessage={getProductFormErrorString(touched, errors, 'hostType', dirty)}
            defaultSelectedKey={values.product.productType}
          />

        </Stack>
        <Stack className={"form_row"}>
          <FormLabel title={"Owner:"} toolTip={Products.product.Owner} />
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

export default Info;