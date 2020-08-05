import React from 'react';
import { Stack} from 'office-ui-fabric-react';


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