import * as React from 'react';
import { Spinner, SpinnerSize } from 'office-ui-fabric-react';


interface ParentProps {
  processingVisible: boolean;
}

type Props =
  ParentProps;

export const GlobalProcessingModal: React.SFC<Props> = (props) => {
  const {
    processingVisible
  } = props;
  
  return (
    processingVisible ?
      <React.Fragment>
          <div className="overlay show"></div>
          <div className="spanner show">            
            <Spinner size={SpinnerSize.large} className="loadingspinner"/>            
          </div>
        
      </React.Fragment>
      : null
  );
};