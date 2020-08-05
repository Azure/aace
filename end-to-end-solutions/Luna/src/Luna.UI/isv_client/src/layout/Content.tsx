import React from 'react';
import {Stack, Nav, INavLink} from 'office-ui-fabric-react';
import { useHistory, useLocation } from 'react-router';
import { WebRoute } from "../shared/constants/routes";


const Content: React.FunctionComponent = (props) => {

  const { children } = props;
  
  const history = useHistory();
  const location = useLocation();
  const v1Enabled = (window.Configs.ENABLE_V1.toLowerCase() === 'true' ? true : false);
  const v2Enabled = (window.Configs.ENABLE_V2.toLowerCase() === 'true' ? true : false);

  let offersTabActive = (location.pathname.toLowerCase().startsWith('/offers') 
  || location.pathname.toLowerCase().startsWith('/modifyoffer'));
  let productsTabActive = (location.pathname.toLowerCase().startsWith('/products'));
  let subscriptionTabActive = (location.pathname.toLowerCase().startsWith('/subscriptions'));
  let selectedMenuItemKey = '';
  if (offersTabActive) {
    selectedMenuItemKey = 'Offers';
  }
  if(productsTabActive)
  {
    selectedMenuItemKey='Products';
  }
  if(subscriptionTabActive){
    selectedMenuItemKey= 'Subscriptions';
  }

  let navLinks: INavLink[] = [];
  if (v1Enabled) {
    navLinks.push({
      url:'',
      onClick: (ev, item) => { history.push(WebRoute.Offers) },
      name: 'Offers',
      key:'Offers',
    });
  }
  if (v1Enabled || v2Enabled) {
    navLinks.push({
      url:'',
      onClick: (ev, item) => { history.push(WebRoute.Subscriptions) },
      name: 'Subscriptions',
      key: 'Subscriptions',
    });
  }
  if (v2Enabled) {
    navLinks.push({
      url:'',
      onClick: (ev, item) => { history.push(WebRoute.Products) },
      name: 'Products',
      key:'Products',
    });
  }

  return (
    <Stack
      horizontal={true}
      styles={{
        root: {
          height: 'calc(100% - 57px)'
        }
      }}
    >
      <Nav
        selectedKey={selectedMenuItemKey}
        selectedAriaLabel="Selected"

        styles={{
          navItems: {
            margin:0
          },
          root: {
            width: 207,
            height:'100%',
            boxSizing: 'border-box',
            border: '1px solid #eee',
            overflowY: 'auto',
          }
        }}
        groups={[
          {
            links: navLinks
          }
        ]}
      />
      <Stack
        horizontal={true}
        styles={{
          root: {
            flexGrow: 1,
            height: '100%',
            padding:32
          }
        }}
      >
        {children}
      </Stack>
    </Stack>
  );
};

export default Content;