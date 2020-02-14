import * as React from 'react';
import {IGlobalError} from "../shared/components/GlobalProvider";
import {ErrorModal} from '../shared/components/ErrorModal';


interface ParentProps {
  errorState: IGlobalError | null;
  // tslint:disable-next-line no-any
  handleDismiss: () => any;
  modalVisible: boolean;
}

type Props =
  ParentProps;

export const GlobalErrorModal: React.SFC<Props> = (props) => {
  const {
    errorState,
    handleDismiss, modalVisible
  } = props;

  let title = "";
  let description = "";
  let renderError;

  if (errorState) {
    title = errorState.title;
    description = errorState.description;
  }

  renderError = (
    <React.Fragment>
      <h3>
        {description}
      </h3>
    </React.Fragment>
  );

  return (
    <ErrorModal
      visible={modalVisible}
      dismiss={handleDismiss}
      title={title}
      description={renderError}
    />
  );
};