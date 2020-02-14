import React from 'react';
import {Image, Stack, Text, Link, PrimaryButton, DefaultButton, getTheme, Nav, INavLink} from 'office-ui-fabric-react';
import { useHistory, useLocation } from 'react-router';
import { WebRoute } from "../shared/constants/routes";
import { LayoutHelper, LayoutHelperMenuItem } from "./Layout";
import { IOfferModel } from "../models";

const Content: React.FunctionComponent = (props) => {

  const { children } = props;  

  return (
    <Stack
      horizontal={true}
      styles={{
        root: {
          height: 'calc(100% - 57px)'
        }
      }}
    >      
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