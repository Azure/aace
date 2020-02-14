import React, { useEffect, useState } from 'react';
import {
  Checkbox,    
  Stack,  
  MessageBarType,
  MessageBar,
} from 'office-ui-fabric-react';
import { useHistory, useParams } from "react-router";
import OfferService from "../../services/OfferService";
import {
  IARMTemplateModel,
  IARMTemplateParameterModel,
  IIpConfigModel,
  IOfferParameterModel, IOfferReviewModel, IPlanModel,
  IWebHookModel, IWebHookParameterModel
} from "../../models";
import { Loading } from '../../shared/components/Loading';
import { IOfferModel } from "../../models";
import OfferParameterService from "../../services/OfferParameterService";
import IpConfigService from "../../services/IpConfigService";
import ArmTemplateService from "../../services/ArmTemplatesService";
import ArmTemplateParameterService from "../../services/ArmTemplateParameterService";
import WebHooksService from "../../services/WebHooksService";
import WebHooksParametersService from "../../services/WebHooksParametersService";
import PlansService from "../../services/PlansService";
import FormLabel from "../../shared/components/FormLabel";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { Formik, useFormikContext } from 'formik';
import { useGlobalContext } from '../../shared/components/GlobalProvider';
import { toast } from 'react-toastify';
import uuid from 'uuid';

//Below code is for making design proper in Armtemplate page.  
let body = (document.getElementsByClassName('App')[0] as HTMLElement);
let innerContainer = (document.getElementsByClassName('innercontainer')[0] as HTMLElement);


const ReviewOffer: React.FunctionComponent = () => {


  const { offerName } = useParams();
  const history = useHistory();
  const globalContext = useGlobalContext();

  const [formError, setFormError] = useState<string | null>(null);

  const getReviewData = async (offerName: string) => {
    setLoading(true);
    globalContext.showProcessing();
    // TODO - add the rest of the necessary service calls here
    const [
      offerResponse,
      offerParametersResponse,
      ipConfigResponse,
      armTemplateResponse,
      armTemplateParametersResponse,
      webHooksResponse,
      webHookParametersResponse,
      plansResponse
    ] = await Promise.all([

      OfferService.get(offerName),
      OfferParameterService.list(offerName),
      IpConfigService.list(offerName),
      ArmTemplateService.list(offerName),
      ArmTemplateParameterService.list(offerName),
      WebHooksService.list(offerName),
      WebHooksParametersService.list(offerName),
      PlansService.list(offerName)
    ]);

    var offer: IOfferModel = {
      owners: '',
      offerAlias: '',
      hostSubscription: '',
      status: '',
      offerVersion: '',
      offerName: '',
      isNew: true,
      clientId: uuid()
    };
    var offerParameters: IOfferParameterModel[] = [];
    var ipconfigs: IIpConfigModel[] = [];
    var armTemplates: IARMTemplateModel[] = [];
    var armTemplateParameters: IARMTemplateParameterModel[] = [];
    var webhooks: IWebHookModel[] = [];
    var webhookParameters: IWebHookParameterModel[] = [];
    var plans: IPlanModel[] = [];

    if (offerResponse.success || offerParametersResponse.success || ipConfigResponse.success || armTemplateResponse.success
      || armTemplateParametersResponse.success || webHooksResponse.success || webHookParametersResponse.success || plansResponse.success) {

      if (offerResponse.value && offerResponse.success)
        offer = offerResponse.value as IOfferModel;

      if (offerParametersResponse.value && offerParametersResponse.success)
        offerParameters = offerParametersResponse.value as IOfferParameterModel[];

      if (ipConfigResponse.value && ipConfigResponse.success)
        ipconfigs = ipConfigResponse.value as IIpConfigModel[];

      if (armTemplateResponse.value && armTemplateResponse.success)
        armTemplates = armTemplateResponse.value as IARMTemplateModel[];

      if (armTemplateParametersResponse.value && armTemplateParametersResponse.success)
        armTemplateParameters = armTemplateParametersResponse.value as IARMTemplateParameterModel[];

      if (webHooksResponse.value && webHooksResponse.success)
        webhooks = webHooksResponse.value as IWebHookModel[];

      if (webHookParametersResponse.value && webHookParametersResponse.success)
        webhookParameters = webHookParametersResponse.value as IWebHookParameterModel[];

      if (plansResponse.value && plansResponse.success)
        plans = plansResponse.value as IPlanModel[];

      setFormState(
        {
          info: { ...offer },
          offerParameters: [...offerParameters],
          ipConfigs: [...ipconfigs],
          armTemplates: [...armTemplates],
          armTemplateParameters: [...armTemplateParameters],
          webhooks: [...webhooks],
          webhookParameters: [...webhookParameters],
          plans: [...plans]
        });
    }
    globalContext.hideProcessing();
    setLoading(false);

  }

  const [formState, setFormState] = useState<IOfferReviewModel>(
    {
      info: {
        owners: '',
        offerAlias: '',
        hostSubscription: '',
        status: '',
        offerVersion: '',
        offerName: '',
        isNew: true,
        clientId: ""
      },
      offerParameters: [],
      ipConfigs: [],
      armTemplates: [],
      armTemplateParameters: [],
      webhooks: [],
      webhookParameters: [],
      plans: []
    }
  );

  const [loading, setLoading] = useState<boolean>(false);

  useEffect(() => {
    if (offerName) {
      getReviewData(offerName);
      body.style.height = 'auto';
      innerContainer.style.width = '100%';
    }
    return () => {      
      body.style.height = '100%';
      innerContainer.style.width = '96%';
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  },[]);



  if (loading)
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
          width: '100%',
          paddingLeft: 50,
          paddingRight: 50,
          paddingTop: 20,
          height: 'calc(100% - 50px)',
          textAlign: 'center',
          color: '#605e5c'
        }
      }}
      gap={15}
    >
      <Stack
        verticalAlign="start"
        horizontal={false}
        gap={10}
        styles={{
          root: {
            width: '100%',
          }
        }}
      >
        <Formik
          initialValues={formState}
          validateOnBlur={true}
          onSubmit={async (values, { setSubmitting, setErrors }) => {

            setSubmitting(true);
            globalContext.showProcessing();

            const publishResponse = await OfferService.publish(values.info.offerName);
            globalContext.hideProcessing();

            if (publishResponse.success) {
              toast.success("Success!");
              setSubmitting(false);
              history.push('/Offers')
            }
            else {
              if (publishResponse.hasErrors) {
                handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'publish', publishResponse);
              }
            }

          }}
        >
          <ReviewPage formError={formError} />
        </Formik>
      </Stack>
    </Stack>
  );
};

export type IOfferParametersFormProps = {
  formError?: string | null;
}
export const ReviewPage: React.FunctionComponent<IOfferParametersFormProps> = (props) => {
  const { values,handleSubmit, submitForm } = useFormikContext<IOfferReviewModel>(); // formikProps
  const { formError } = props;
  const globalContext = useGlobalContext();

  useEffect(() => {
    globalContext.modifySaveForm(async () => {
      await submitForm();
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getFileName = (FilePath: string) => {
    let filename = FilePath.substring(FilePath.lastIndexOf("/") + 1, FilePath.length);
    return filename.split("?")[0];
  }
  return (
    <form style={{ width: '100%' }} autoComplete={"off"} onSubmit={handleSubmit}>
      {formError && <div style={{ marginBottom: 15 }}>
        <MessageBar messageBarType={MessageBarType.error}>
          <div dangerouslySetInnerHTML={{ __html: formError }} style={{ textAlign: 'left' }}></div>
        </MessageBar></div>}
      <Stack verticalAlign="center" horizontalAlign={"start"} horizontal={false} className="reviewoffer" style={{ marginBottom: '20px' }}>
        <span className={"review-section-label"}>Info:</span>
        <Stack className={"form_row"} horizontal={true} gap={20}>
          <FormLabel title={"Version:"} />
          <span>{values.info.offerVersion}</span>
        </Stack>
        <Stack className={"form_row"} horizontal={true} gap={20}>
          <FormLabel title={"Owners:"} />
          <span>{values.info.owners}</span>
        </Stack>
        <Stack className={"form_row"} horizontal={true} gap={20}>
          <FormLabel title={"Host Subscription:"} />
          <span>{values.info.hostSubscription}</span>
        </Stack>
        <span className={"review-section-label"}>Offer Parameters:</span>
        <table className="noborder offer">
          <thead>
            <tr>
              <th>
                <FormLabel title={"Parameter ID"} />
              </th>
              <th>
                <FormLabel title={"Display Name"} />
              </th>
              <th>
                <FormLabel title={"Description"} />
              </th>
              <th>
                <FormLabel title={"Value Type"} />
              </th>
              <th>
                <FormLabel title={"From List"} />
              </th>
              <th>
                <FormLabel title={"Value List"} />
              </th>
              <th>
                <FormLabel title={"Max."} />
              </th>
              <th>
                <FormLabel title={"Min."} />
              </th>
            </tr>
          </thead>
          <tbody>
            {values.offerParameters.map((value: IOfferParameterModel, idx) => {

              return (
                <tr key={idx}>
                  <td>
                    <span>{value.parameterName}</span>
                  </td>
                  <td>
                    <span>{value.displayName}</span>
                  </td>
                  <td>
                    <span>{value.description}</span>
                  </td>
                  <td>
                    <span>{value.valueType}</span>
                  </td>
                  <td style={{ verticalAlign: 'middle', border: 'none', backgroundColor: '#fff' }} className="noborder">
                    <Stack style={{ width: 92 }} verticalAlign={"center"} horizontalAlign={"center"}>
                      <Checkbox
                        name={`offerParameters.${idx}.fromList`}
                        defaultChecked={value.fromList}
                        className="checkbox"
                        disabled={true} />
                    </Stack>
                  </td>
                  <td>
                    <span>{value.valueList}</span>
                  </td>
                  <td>
                    <span>{value.maximum}</span>
                  </td>
                  <td>
                    <span>{value.minimum}</span>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
        <br />
        <span className={"review-section-label"}>Ip Address:</span>
        <table className="noborder offer">
          <thead>
            <tr>
              <th>
                <FormLabel title={"ID"} />
              </th>
              <th>
                <FormLabel title={"Ip Blocks"} />
              </th>
              <th>
                <FormLabel title={"IpsPerSub"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {
              values.ipConfigs.map((value, idx) => {
                return (
                  <tr key={idx}>
                    <td>
                      <span>{value.name}</span>
                    </td>
                    <td>
                      <span>{value.ipBlocks.toString()}</span>
                    </td>
                    <td>
                      <span>{value.iPsPerSub}</span>
                    </td>
                  </tr>
                )
              })
            }
          </tbody>
        </table>
        <br />
        <span className={"review-section-label"}>Arm Template:</span>
        <table className="noborder offer">
          <thead>
            <tr>
              <th>
                <FormLabel title={"ID"} />
              </th>
              <th>
                <FormLabel title={"Arm Template File"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {values.armTemplates.map((value, idx) => {
              return (
                <tr key={idx}>
                  <td>
                    <span>{value.templateName}</span>
                  </td>
                  <td>
                    <span>{getFileName(value.templateFilePath as string)}</span>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
        <br />
        <span className={"review-section-label"}>Arm Template Parameters:</span>
        <table className="noborder offer">
          <thead>
            <tr>
              <th>
                <FormLabel title={"ID"} />
              </th>
              <th>
                <FormLabel title={"Type"} />
              </th>
              <th>
                <FormLabel title={"Value"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {values.armTemplateParameters.map((value, idx) => {
              return (
                <tr key={idx}>
                  <td>
                    <span>{value.name}</span>
                  </td>
                  <td>
                    <span>{value.type}</span>
                  </td>
                  <td>
                    <span>{value.value}</span>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
        <br />
        <br />
        <span className={"review-section-label"}>Web Hooks:</span>
        <table className="noborder offer">
          <thead>
            <tr>
              <th>
                <FormLabel title={"ID"} />
              </th>
              <th>
                <FormLabel title={"Url"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {values.webhooks.map((value, idx) => {
              return (
                <tr key={idx}>
                  <td>
                    <span>{value.webhookName}</span>
                  </td>
                  <td>
                    <span>{value.webhookUrl}</span>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
        <br />
        <span className={"review-section-label"}>Web Hooks Parameters:</span>
        <table className="noborder offer">
          <thead>
            <tr>
              <th>
                <FormLabel title={"Name"} />
              </th>
              <th>
                <FormLabel title={"Value"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {values.webhookParameters.map((value, idx) => {
              return (
                <tr key={idx}>
                  <td>
                    <span>{value.name}</span>
                  </td>
                  <td>
                    <span>{value.value}</span>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
        <br />
        <span className={"review-section-label"}>Plans:</span>
        <table className="noborder offer">
          <thead>
            <tr>
              <th>
                <FormLabel title={"Plan Name"} />
              </th>
              <th>
                <FormLabel title={"Availability"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {values.plans.map((value, idx) => {
              return (
                <tr key={idx}>
                  <td>
                    <span>{value.planName}</span>
                  </td>
                  <td>
                    <span>{value.privatePlan ? 'private' : 'public'}</span>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </Stack>
    </form>

  )
}

export default ReviewOffer;