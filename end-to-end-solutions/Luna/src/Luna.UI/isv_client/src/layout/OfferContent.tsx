import React, {useEffect, useState} from 'react';
import {getTheme, PrimaryButton, Stack} from 'office-ui-fabric-react';
import {useHistory, useLocation} from 'react-router';
import {LayoutHelper, LayoutHelperMenuItem} from "./Layout";
import OfferService from "../services/OfferService";
import {IOfferModel} from "../models";
import {useGlobalContext} from "../shared/components/GlobalProvider";

import { confirmAlert } from 'react-confirm-alert'; // Import
import 'react-confirm-alert/src/react-confirm-alert.css'; // Import css

type OfferProps = {
  offerName: string | null;
};

const OfferContent: React.FunctionComponent<OfferProps> = (props) => {

  const {offerName} = props;

  const history = useHistory();
  const location = useLocation();
  const globalContext = useGlobalContext();
  const [hideSave, setHideSave] = useState<boolean>(false);

  const layoutHelper: LayoutHelper = {
    menuItems:
      [
        {
          title: "Info",
          paths: [`/modifyoffer/${offerName}/info`],
          menuClick: () => {
            preventDataLoss('Info');
          }
        },
        {
          title: "Parameters",
          paths: [`/modifyoffer/${offerName}/parameters`],
          menuClick: () => {
            preventDataLoss('Parameters');
          }
        },
        {
          title: "IP Addresses",
          paths: [`/modifyoffer/${offerName}/IpConfigs`],
          menuClick: () => {
            preventDataLoss('IpConfigs');
          }
        },
        {
          title: "ARM Templates",
          paths: [`/modifyoffer/${offerName}/ArmTemplates`],
          menuClick: () => {
            preventDataLoss('ArmTemplates');
          }
        },
        {
          title: "Webhooks",
          paths: [`/modifyoffer/${offerName}/WebHooks`],
          menuClick: () => {
            preventDataLoss('WebHooks');
          }
        },
        {
          title: "Plans",
          paths: [`/modifyoffer/${offerName}/Plans`],
          menuClick: () => {
            preventDataLoss('Plans');
          }
        }
      ]
  };

  const preventDataLoss = (pathName: string) => {

    if (globalContext.isDirty || globalContext.isSecondaryDirty) {

      confirmAlert({
        title: 'Data Loss Prevention',
        message: 'You have unsaved data that will be lost, do you wish to continue?',
        buttons: [
          {
            label: 'No',
            onClick: () => {}
          },
          {
            label: 'Yes',
            onClick: () => {
              globalContext.setFormDirty(false);
              history.push(`/modifyoffer/${offerName}/${pathName}`);
            }
          }
        ]
      });
    }
    else{
      history.push(`/modifyoffer/${offerName}/${pathName}`);
    }

  }

  const isNavItemActive = (paths: string[]): boolean => {
    let found = false;
    for (let i = 0; i < paths.length; i++) {
      found = location.pathname.toLowerCase() === paths[i].toLowerCase();
      if (found) {
        break;
      }
    }

    return found;
  };

  const theme = getTheme();

  const handleReviewAndPublish = () => {
    history.push(`/reviewoffer/${offerName}`);
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

  useEffect( () => {
    if (offerName)
      getOfferInfo(offerName);
// eslint-disable-next-line react-hooks/exhaustive-deps
  },[offerName]);

  useEffect( () => {

    setHideSave(location.pathname.toLowerCase().endsWith("/plans"));
// eslint-disable-next-line react-hooks/exhaustive-deps
  },[history.location,location.pathname]);

  const handleFormSubmission = async(e) => {
    if (globalContext.saveForm)
      await globalContext.saveForm();
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
              {!hideSave &&               
              <PrimaryButton onClick={handleFormSubmission} text={"Save"}/>
              }
              <PrimaryButton onClick={handleReviewAndPublish} text={"Review & Publish"}
              />
            </Stack>
          </Stack.Item>
        </Stack>
        {/* Offer navigation */}
        <Stack
          horizontal={true}
          verticalAlign={"center"}
          verticalFill={true}
          className={"nav-header"}
          styles={{
            root: {
              paddingLeft:31,
              paddingRight:31,
              height:45,
              marginBottom:20
            }
          }}
        >
          {layoutHelper.menuItems.map((value: LayoutHelperMenuItem, idx) => {
            return (
              <Stack
                key={`menuItem_${idx}`}
                horizontal={true}
                verticalAlign={"center"}
                verticalFill={true}
                onClick={value.menuClick}
                styles={{
                  root: {
                    height:40,
                    fontWeight: (isNavItemActive(value.paths) ? 600 : 'normal'),
                    borderBottom: (isNavItemActive(value.paths) ? 'solid 2px #0078d4' : 'none'),
                    marginTop: (isNavItemActive(value.paths) ? 2 : 0),
                    paddingLeft: 20,
                    paddingRight:20,
                    minWidth:94,
                    cursor: 'pointer'
                  }
                }}
              >
                <span style={{textAlign: "center", whiteSpace:"nowrap", flexGrow:1}}>
                  {value.title}
                </span>
              </Stack>
            )
          })}
        </Stack>
        <div className="innercontainer">
          {props.children}
        </div>
        
      </Stack>
    </Stack>
  );
};

export default OfferContent;