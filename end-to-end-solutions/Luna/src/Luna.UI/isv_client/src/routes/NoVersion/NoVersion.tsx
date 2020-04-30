import React from 'react';
import {Stack,} from 'office-ui-fabric-react';

const NoVersion: React.FunctionComponent = () => {

  return (
    <Stack
      verticalAlign="start"
      horizontal={false}
      styles={{
        root: {
          width: '100%',
          height: '100%',
          textAlign: 'center',
        }
      }}
    >
      <h1>Luna v1 and v2 have both been disabled on this site.<br/>The site must be configured to enable one or both versions.</h1>
    </Stack>
  );
};

export default NoVersion;