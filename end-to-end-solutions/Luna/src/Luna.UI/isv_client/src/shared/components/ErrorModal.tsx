import * as React from 'react';
import {Icon, Modal, PrimaryButton, Stack} from 'office-ui-fabric-react';

// tslint:disable no-any
interface ParentProps {
  visible: boolean;
  dismiss: () => any;
  title: React.ReactNode;
  description: React.ReactNode;
}
// tslint:enable no-any

export const ErrorModal: React.SFC<ParentProps> = (props) => {
  const { visible, dismiss, title, description} = props;

  return (
    <Modal
      onDismiss={dismiss}
      isModeless={false}
      isBlocking={true}
      isOpen={visible}
    >
      <Stack horizontal={false} horizontalAlign={"center"} style={{margin:50}}>
        <Stack horizontal={true} verticalAlign={"center"} style={{color:'red'}} gap={10}>
          <Icon iconName={"Error"} style={{fontSize:20}}/>
          <span style={{display:'inline-block',fontSize:20}}>
            {title}
          </span>
        </Stack>
        {description}
        <PrimaryButton
          style={{minWidth: 180}}
          onClick={dismiss}
          text={"Close"}
        />
      </Stack>
    </Modal>
  );
};
