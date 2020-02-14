import React, {useEffect, useState} from 'react';
import {MessageBar, MessageBarType, Stack, TextField,} from 'office-ui-fabric-react';
import {useParams} from "react-router";
import {Formik, useFormikContext} from "formik";
import OfferService from "../../services/OfferService";
import {Loading} from '../../shared/components/Loading';
import {IOfferModel} from "../../models";
import {initialInfoFormValues, IOfferInfoFormValues, offerInfoValidationSchema} from "./formUtils/offerFormUtils";
import FormLabel from "../../shared/components/FormLabel";
import {Offers} from '../../shared/constants/infomessages';
import {handleSubmissionErrorsForForm} from "../../shared/formUtils/utils";
import {toast} from "react-toastify";
import {useGlobalContext} from '../../shared/components/GlobalProvider';

const Info: React.FunctionComponent = () => {

  const {offerName} = useParams();
  const globalContext = useGlobalContext();
  const [formState, setFormState] = useState<IOfferInfoFormValues>(initialInfoFormValues);
  const [loadingFormData, setLoadingFormData] = useState<boolean>(true);
  const [formError, setFormError] = useState<string | null>(null);

  const getFormData = async (offerName: string) => {

    setLoadingFormData(true);
    const offerResponse = await OfferService.get(offerName);

    // Global errors should have already been handled for get requests by this point
    if (offerResponse.value && offerResponse.success) {
      var offer = offerResponse.value as IOfferModel;

      setFormState(
        {
          offer: {...offer}
        });
    }
    setLoadingFormData(false);

  }

  useEffect(() => {
    if (offerName) {
      getFormData(offerName);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  },[]);


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
        <Loading/>
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
        validationSchema={offerInfoValidationSchema}
        initialValues={formState}
        enableReinitialize={true}
        validateOnBlur={true}
        onSubmit={async (values, {setSubmitting, setErrors, resetForm}) => {

          setFormError(null);

          setSubmitting(true);
          globalContext.showProcessing();
          var updateOfferResult = await OfferService.update(values.offer);
          if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'offer', updateOfferResult)) {
            globalContext.hideProcessing();
            return;
          }

          setSubmitting(false);
          globalContext.hideProcessing();

          toast.success("Success!");

          getFormData((offerName as string))
          setTimeout(() => {globalContext.setFormDirty(false);}, 500);

        }}
      >
        <OfferForm isNew={false} formError={formError} offers={[]}/>
      </Formik>
    </Stack>
  );
};

export type IOfferFormProps = {
  isNew: boolean;
  formError?: string | null;
  offers: IOfferModel[];
}
export const OfferForm: React.FunctionComponent<IOfferFormProps> = (props) => {
  const {values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty} = useFormikContext<IOfferInfoFormValues>(); // formikProps
  const {formError, isNew} = props;

  const globalContext = useGlobalContext();

  useEffect(() => {
    globalContext.modifySaveForm(async () => {
      await submitForm();
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  },[]);

  const getOfferFormErrorString = (touched, errors, property: string, dirty) => {

    globalContext.setFormDirty(dirty);

    return touched.offer && errors.offer && touched.offer[property] && errors.offer[property] ? errors.offer[property] : '';
  };

  const DisplayErrors = (errors) => {    
    return null;
  };
  const getidlist = (): string => {
    let idlist = ''
    props.offers.map((values, index) => {
      idlist += values.offerName + ',';   
      return idlist;   
    })
    values.offer.Idlist = idlist.substr(0, idlist.length - 1);
    return idlist.substr(0, idlist.length - 1);
  }

  const textboxClassName = (props.isNew ? "form_textboxmodal" : "form_textbox");

  return (
    <form style={{width: '100%'}} autoComplete={"off"} onSubmit={handleSubmit}>
      {formError && <div style={{marginBottom: 15}}><MessageBar messageBarType={MessageBarType.error}>
          <div dangerouslySetInnerHTML={{__html: formError}} style={{textAlign: 'left'}}></div>
      </MessageBar></div>}
      <Stack
        verticalAlign="start"
        horizontal={false}
        gap={10}
        styles={{
          root: {}
        }}
      >
        <DisplayErrors errors={errors}/>
        {isNew &&
        <React.Fragment>
            <Stack className={"form_row"}>
                <FormLabel title={"ID:"} toolTip={Offers.offer.ID}/>
                <input type="hidden" name={'offer.Idlist'} value={getidlist()}/>
                <TextField
                    name={'offer.offerName'}
                    value={values.offer.offerName}
                    maxLength={50}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    errorMessage={getOfferFormErrorString(touched, errors, 'offerName', dirty)}
                    placeholder={'Offer ID'}
                    className={textboxClassName}/>
            </Stack>
            <Stack className={"form_row"}>
                <FormLabel title={"Alias:"} toolTip={Offers.offer.Alias}/>
                <TextField
                    name={'offer.offerAlias'}
                    value={values.offer.offerAlias}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    errorMessage={getOfferFormErrorString(touched, errors, 'offerAlias', dirty)}
                    placeholder={'Alias'}
                    className={textboxClassName}/>
            </Stack>
        </React.Fragment>
        }
        <Stack className={"form_row"}>
          <FormLabel title={"Version:"} toolTip={Offers.offer.Version}/>
          <TextField
            name={'offer.offerVersion'}
            value={values.offer.offerVersion}
            onChange={handleChange}
            onBlur={handleBlur}
            errorMessage={getOfferFormErrorString(touched, errors, 'offerVersion', dirty)}
            placeholder={'Version'}
            className={textboxClassName}/>
        </Stack>
        <Stack className={"form_row"}>
          <FormLabel title={"Owners:"} toolTip={Offers.offer.Owners}/>
          <TextField
            name={'offer.owners'}
            value={values.offer.owners}
            onChange={handleChange}
            onBlur={handleBlur}
            errorMessage={getOfferFormErrorString(touched, errors, 'owners', dirty)}
            placeholder={'Owners'}
            className={textboxClassName}/>
        </Stack>
        <Stack className={"form_row"}>
          <FormLabel title={"Host Subscription:"} toolTip={Offers.offer.HostSubscription}/>
          <TextField
            name={'offer.hostSubscription'}
            value={values.offer.hostSubscription}
            onChange={handleChange}
            onBlur={handleBlur}
            errorMessage={getOfferFormErrorString(touched, errors, 'hostSubscription', dirty)}
            placeholder={'Host Subscription'}
            className={textboxClassName}/>
        </Stack>
      </Stack>
    </form>
  );
}

export default Info;