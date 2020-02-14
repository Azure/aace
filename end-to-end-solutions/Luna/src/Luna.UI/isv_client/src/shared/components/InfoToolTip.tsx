import * as React from 'react';
import {Icon, TooltipHost} from "office-ui-fabric-react";
import {v4 as uuid} from "uuid";

type InfoToolTipProps = {
  toolTip: string
};

const InfoToolTip: React.FunctionComponent<InfoToolTipProps> = (props) => {
  const {toolTip} = props;
  return (
    <TooltipHost
      content={<span dangerouslySetInnerHTML={{__html: toolTip}}></span>}
      id={uuid()}
      calloutProps={{gapSpace: 0}}
      styles={{root: {
        display: 'inline',
        marginLeft:5,
        cursor:'pointer',
        height:12
      }}}
    ><Icon iconName={"info"} style={{fontSize:12}}></Icon>
    </TooltipHost>
  );
}

export default InfoToolTip;