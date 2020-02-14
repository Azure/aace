import React, {useEffect, useState} from 'react';
import {getTheme, PrimaryButton, Stack} from 'office-ui-fabric-react';
import {useHistory} from 'react-router';
import OfferService from "../services/OfferService";
import {IOfferModel} from "../models";
import {useGlobalContext} from "../shared/components/GlobalProvider";

type OfferProps = {
  offerName: string | null;
};

const ReviewOfferContent: React.FunctionComponent<OfferProps> = (props) => {

  const {offerName} = props;
  const history = useHistory();
  const theme = getTheme();

  const handlePublish = async(e) => {
    if (globalContext.saveForm)
      await globalContext.saveForm();
  }

  const getOfferInfo = async (offerName: string) => {

    let response = await OfferService.get(offerName);

    if (!response.hasErrors && response.value) {

      setOfferModel({...response.value});
    }

  }

  const [offerModel, setOfferModel] = useState<IOfferModel>({
    owners: '',
    offerAlias: '',
    hostSubscription: '',
    status: '',
    offerVersion: '',
    offerName: '',
    isNew: true,
    clientId: ""
  });

  const globalContext = useGlobalContext();

  useEffect( () => {
    if (offerName)
      getOfferInfo(offerName);
      // eslint-disable-next-line react-hooks/exhaustive-deps
  },[]);

  const EditOffer = async(e) => {
    history.push(`/modifyoffer/${offerName}/Info`);
  };
  const BackToOffers = async(e) => {
    history.push(`/offers`);
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
        <Stack
          horizontal={true}
          verticalAlign={"center"}
          verticalFill={true}
          className={"review-header"}
          styles={{
            root: {
              height: 70,
              paddingLeft:31,
              paddingRight:31,
              width:'100%'
            }
          }}
        >
          <Stack.Item styles={{
            root: {
              flexGrow: 0
            }
          }}>
            <span style={{marginRight: 20, fontSize: 18}}>
              Offer Details
            </span>
              <span className={"offer-details-separator"}></span>
              <span style={{fontWeight: 600}}>
              ID:
            </span>
              <span style={{marginLeft: 8}}>
              {offerModel.offerName}
            </span>
              <span style={{marginLeft: 100, fontWeight: 600}}>
              Alias:
            </span>
              <span style={{marginLeft: 8}}>
              {offerModel.offerAlias}
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
              <PrimaryButton onClick={BackToOffers} text="Back to Offers"/>
              <PrimaryButton onClick={EditOffer} text={"Edit"}/>
              {offerModel.status.toLowerCase()!=='active' && offerModel.status.toLowerCase()!==''
              ?
              <PrimaryButton onClick={handlePublish} text={"Publish"}/> :null}
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

export default ReviewOfferContent;