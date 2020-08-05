import React, {useEffect, useState} from 'react';
import {getTheme, PrimaryButton, Stack} from 'office-ui-fabric-react';
import {useHistory, useLocation} from 'react-router';

import SubscriptionsService from "../services/SubscriptionsService";
import {ISubscriptionsModel} from "../models";
import {useGlobalContext} from "../shared/components/GlobalProvider";

type SubscriptionProps = {
  subscriptionId: string | null;
};

const SubscriptionContent: React.FunctionComponent<SubscriptionProps> = (props) => {

  const {subscriptionId} = props;

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  let title = 'New';
  if (subscriptionId) {
    title = subscriptionId;
  }
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const logo = "https://mondrian.mashable.com/uploads%252Fcard%252Fimage%252F918220%252F316bce31-4c38-4f3b-b743-a17406175286.png%252F950x534__filters%253Aquality%252880%2529.png?signature=ASyPwdNVsAIo5E7uzfpoydo-rmc=&source=https%3A%2F%2Fblueprint-api-production.s3.amazonaws.com";
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const isvName = "Slack";

  const history = useHistory();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const location = useLocation();

  const theme = getTheme();

  const getSubscriptionInfo = async (subscriptionId: string) => {

    let response = await SubscriptionsService.get(subscriptionId);

    if (!response.hasErrors && response.value) {

      setSubscriptionModel({...response.value});
    }

  }

  const [subscriptionModel, setSubscriptionModel] = useState<ISubscriptionsModel>({
    subscriptionId: '',
  name: '',
  offerName: '',
  planName: '',
  owner: '',
  quantity: 0,
  beneficiaryTenantId: '',
  purchaserTenantId: '',
  subscribeWebhookName: '',
  unsubscribeWebhookName: '',
  suspendWebhookName: '',
  deleteDataWebhookName: '',
  priceModel: '',
  monthlyBase: 0,
  privatePlan: false,  
  inputParameters: [],
  provisioningStatus: '',
  entryPointUrl: '',

  publisherId: '',
  status: '',
  isTest: false,
  allowedCustomerOperationsMask: 0,
  sessionMode: '',
  sandboxType: '',
  isFreeTrial: false,
  createdTime: '',
  activatedTime: '',
  lastUpdatedTime: '',
  lastSuspendedTime: '',
  unsubscribedTime: '',
  dataDeletedTime: '',
  operationId: '',
  deploymentName: '',
  deploymentId: '',
  resourceGroup: '',
  activatedBy: '',
  });

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const globalContext = useGlobalContext();

  useEffect( () => {
    if (subscriptionId)
    getSubscriptionInfo(subscriptionId);

  }, []);

  // const handleFormSubmission = async(e) => {
  //   if (globalContext.saveForm)
  //     await globalContext.saveForm();
  // };

  const handleBackButton = () => {
    history.push(`/Subscriptions`);
  }

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
        {/* Offer Details Header */}
        <Stack
          horizontal={true}
          verticalAlign={"center"}
          verticalFill={true}
          className={"offer-details-header"}
          styles={{
            root: {
              height: 70,
              paddingLeft:31,
              paddingRight:31,
              width:'100%',
              borderBottom:'1px solid rgb(217, 217, 217)',              
            }
          }}
        >
          <Stack.Item styles={{
            root: {
              flexGrow: 0
            }
          }}>
            <span style={{marginRight: 20, fontSize: 18}}>
              Operation History
            </span>
              <span className={"offer-details-separator"}></span>
              <span style={{fontWeight: 600}}>
              Subscription Name:
            </span>
              <span style={{marginLeft: 8}}>
              {subscriptionModel.name}
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
              gap={8}
            >
              <PrimaryButton onClick={handleBackButton} text={"Back to list"}/>
            </Stack>
          </Stack.Item>
        </Stack>                
        <div className="innercontainer">
          {props.children}
        </div>
        
      </Stack>
    </Stack>
  );
};

export default SubscriptionContent;