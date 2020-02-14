import * as React from 'react';
import {IButtonProps, PrimaryButton} from "office-ui-fabric-react";


const AlternateButton: React.FunctionComponent<IButtonProps> = (props) => {

  return (
    <PrimaryButton styles={{
      root: {
        backgroundColor: '#eaeaea',
        color: '#333333',

      },
      rootHovered: {
        backgroundColor: '#d9d9d9',
        color: '#333333'
      },
      rootPressed: {
        backgroundColor: '#b7b7b7',
        color: '#333333'
      }
    }} {...props}>{props.children}</PrimaryButton>
  );
}

export default AlternateButton;