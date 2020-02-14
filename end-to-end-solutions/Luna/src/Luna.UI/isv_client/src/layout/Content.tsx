import React from 'react';
import {Stack, Nav} from 'office-ui-fabric-react';
import { useHistory, useLocation } from 'react-router';
import { WebRoute } from "../shared/constants/routes";


const Content: React.FunctionComponent = (props) => {

  const { children } = props;
  
  const history = useHistory();
  const location = useLocation();
  let offersTabActive = (location.pathname.toLowerCase().startsWith('/offers') || location.pathname.toLowerCase().startsWith('/modifyoffer'));
  let selectedMenuItemKey = 'Offers';
  if (!offersTabActive) {
    selectedMenuItemKey = 'Subscriptions';
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
            links: [
              {
                url:'',
                onClick: (ev, item) => { history.push(WebRoute.Offers) },
                name: 'Offers',
                key:'Offers',
              },
              {
                url:'',
                onClick: (ev, item) => { history.push(WebRoute.Subscriptions) },
                name: 'Subscriptions',
                key: 'Subscriptions',
              }
            ]
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