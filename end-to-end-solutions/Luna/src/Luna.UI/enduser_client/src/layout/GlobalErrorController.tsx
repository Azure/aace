import * as React from 'react';
import {useEffect, useState} from 'react';
import {GlobalErrorModal} from './GlobalErrorModal';
import {IGlobalError, useGlobalContext} from "../shared/components/GlobalProvider";
import {Cache, Hub} from "aws-amplify";

export const ERROR_STATE = 'error_state';
const GlobalErrorController: React.FC = () => {

  const [modalVisible, setModalVisible] = useState<boolean>(false);

  //let errorState: IGlobalError | null = null;

  const globalContext = useGlobalContext();

  const handleDismiss = async () => {
    Cache.removeItem( ERROR_STATE );
    setModalVisible(false);
  };


  useEffect(() => {
    let errorState: IGlobalError | null = null;
    Hub.listen('ErrorChannel', (data) => {

      errorState = Cache.getItem( ERROR_STATE );

      if (errorState) {
        globalContext.showGlobalError(errorState);
      }
    })
  });

  useEffect(() => {

    //console.log('global error: ', globalContext.showGlobalError)
    setModalVisible(!(globalContext.globalError === null));
  },[globalContext.globalError]);

console.log('modal visible: ', modalVisible);

  return (
    <GlobalErrorModal
      errorState={globalContext.globalError}
      handleDismiss={handleDismiss}
      modalVisible={modalVisible}
    />
  );
};

export default GlobalErrorController;