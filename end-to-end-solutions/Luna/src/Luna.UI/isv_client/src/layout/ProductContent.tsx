import React, { useEffect, useState } from 'react';
import { getTheme, PrimaryButton, Stack } from 'office-ui-fabric-react';
import { useHistory, useLocation } from 'react-router';
import { LayoutHelper, LayoutHelperMenuItem } from "./Layout";
import ProductService from "../services/ProductService";
import {initialProductList} from '../routes/Products/formUtils/ProductFormUtils'
import { IProductModel } from "../models";
import { useGlobalContext } from "../shared/components/GlobalProvider";

import { confirmAlert } from 'react-confirm-alert'; // Import
import 'react-confirm-alert/src/react-confirm-alert.css'; // Import css

type ProductProps = {
  productId: string | null;
};

const ProductContent: React.FunctionComponent<ProductProps> = (props) => {

  const { productId } = props;

  const history = useHistory();
  const location = useLocation();
  const globalContext = useGlobalContext();
  const [hideSave, setHideSave] = useState<boolean>(false);

  // const layoutHelper: LayoutHelper = {
  //   menuItems:
  //     [
  //       {
  //         title: "Info",
  //         paths: [`/modifyproduct/${productId}/info`],
  //         menuClick: () => {
  //           preventDataLoss('Info');
  //         }
  //       },
  //       {
  //         title: "Parameters",
  //         paths: [`/modifyproduct/${productId}/parameters`],
  //         menuClick: () => {
  //           preventDataLoss('Parameters');
  //         }
  //       },
  //       {
  //         title: "IP Addresses",
  //         paths: [`/modifyproduct/${productId}/IpConfigs`],
  //         menuClick: () => {
  //           preventDataLoss('IpConfigs');
  //         }
  //       },
  //       {
  //         title: "ARM Templates",
  //         paths: [`/modifyproduct/${productId}/ArmTemplates`],
  //         menuClick: () => {
  //           preventDataLoss('ArmTemplates');
  //         }
  //       },
  //       {
  //         title: "Webhooks",
  //         paths: [`/modifyproduct/${productId}/WebHooks`],
  //         menuClick: () => {
  //           preventDataLoss('WebHooks');
  //         }
  //       },
  //       {
  //         title: "Meters",
  //         paths: [`/modifyproduct/${productId}/Meters`],
  //         menuClick: () => {
  //           preventDataLoss('Meters');
  //         }
  //       },
  //       {
  //         title: "Plans",
  //         paths: [`/modifyproduct/${productId}/Plans`],
  //         menuClick: () => {
  //           preventDataLoss('Plans');
  //         }
  //       }
  //     ]
  //  };

  // const preventDataLoss = (pathName: string) => {

  //   if (globalContext.isDirty || globalContext.isSecondaryDirty) {

  //     confirmAlert({
  //       title: 'Data Loss Prevention',
  //       message: 'You have unsaved data that will be lost, do you wish to continue?',
  //       buttons: [
  //         {
  //           label: 'No',
  //           onClick: () => { }
  //         },
  //         {
  //           label: 'Yes',
  //           onClick: () => {
  //             globalContext.setFormDirty(false);
  //             history.push(`/modifyproduct/${productId}/${pathName}`);
  //           }
  //         }
  //       ]
  //     });
  //   }
  //   else {
  //     history.push(`/modifyproduct/${productId}/${pathName}`);
  //   }

  // }

  // const isNavItemActive = (paths: string[]): boolean => {
  //   let found = false;
  //   for (let i = 0; i < paths.length; i++) {
  //     found = location.pathname.toLowerCase() === paths[i].toLowerCase();
  //     if (found) {
  //       break;
  //     }
  //   }

  //   return found;
  // };

  const theme = getTheme();

  const getProductInfo = async (productId: string) => {

    // let response = await ProductService.get(offerName);

    let response = initialProductList.filter(p=>p.productId==productId)[0];
    setProductModel({ ...response})
    // if (!response.hasErrors && response.value) {

    //   setProductModel({ ...response.value });
    // }

  }

  const [productModel, setProductModel] = useState<IProductModel>({
    hostType: '',
    owner: '',
    productId: '',
    productType: '', isDeleted: false,
    isSaved: false,
    isModified: false,
    isNew: true,
    clientId: ""
  });

  useEffect(() => {
    if (productId)
      getProductInfo(productId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [productId]);

  useEffect(() => {

    // setHideSave(location.pathname.toLowerCase().endsWith("/plans"));
    // setHideSave(location.pathname.toLowerCase().endsWith("/meters"));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [history.location, location.pathname]);

  const handleFormSubmission = async (e) => {
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
            <span style={{ fontWeight:'bold', marginRight: 20, fontSize: 18 }}>
              Product Details
            </span>
            <span className={"offer-details-separator"}></span>
            <span style={{ fontWeight: 600 }}>
              ID:
            </span>
            <span style={{ marginLeft: 8 }}>
              {productModel.productId}
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
              gap={8}
            >
              {/* {!hideSave &&
                <PrimaryButton onClick={handleFormSubmission} text={"Save"} />
              } */}
              
              <PrimaryButton text={"Cancel"} />
                            
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
    </Stack>
  );
};

export default ProductContent;