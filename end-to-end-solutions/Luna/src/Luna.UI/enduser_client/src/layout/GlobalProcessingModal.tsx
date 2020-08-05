import * as React from 'react';
import { Spinner, SpinnerSize, IStackProps } from 'office-ui-fabric-react';

interface ParentProps {
  processingVisible: boolean;
}

type Props =
  ParentProps;

export const GlobalProcessingModal: React.SFC<Props> = (props) => {
  const {
    processingVisible
  } = props;
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const rowProps: IStackProps = { horizontal: true, verticalAlign: 'center' };
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const tokens = {
    sectionStack: {
      childrenGap: 10
    },
    spinnerStack: {
      childrenGap: 20
    }
  };

  return (
    processingVisible ?
      <React.Fragment>
          <div className="overlay show"></div>
          <div className="spanner show">
            {/* <div className="loader"></div> */}            
            <Spinner size={SpinnerSize.large} className="loadingspinner"/>            
          </div>
        
      </React.Fragment>
      : null
  );
};