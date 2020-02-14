import * as React from 'react';
import {Icon, Stack, TooltipHost} from "office-ui-fabric-react";
import {v4 as uuid} from "uuid";
import InfoToolTip from "./InfoToolTip";

type FormLabelProps = {
  toolTip?: string;
  title: string;
};

const FormLabel: React.FunctionComponent<FormLabelProps> = (props) => {
  const {toolTip, title} = props;
  
  if (toolTip) {
    return (
      <Stack horizontal={true} verticalAlign={"baseline"} gap={5} style={{marginBottom:5}}>
          <span className="form_label" style={{marginBottom:0}}>{title}</span>
          <InfoToolTip toolTip={toolTip}/>
      </Stack>
    );
  }  
  else {
    return (
      <span className="form_label">{title}</span>
    );
  }

}

export default FormLabel;