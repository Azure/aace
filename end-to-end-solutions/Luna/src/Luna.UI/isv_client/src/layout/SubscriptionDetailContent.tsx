import React from 'react';
import {getTheme, Stack} from 'office-ui-fabric-react';

const SubscriptionDetailContent: React.FunctionComponent = (props) => {

  const theme = getTheme();

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
        <div className="innercontainer">
          {props.children}
        </div>
      </Stack>
    </Stack>
  );
};

export default SubscriptionDetailContent;