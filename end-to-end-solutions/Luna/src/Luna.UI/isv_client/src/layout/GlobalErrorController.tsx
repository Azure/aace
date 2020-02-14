import * as React from 'react';
import {useEffect, useState} from 'react';
import {GlobalErrorModal} from './GlobalErrorModal';
import {IGlobalError, useGlobalContext} from "../shared/components/GlobalProvider";
import {Cache, Hub} from "aws-amplify";

export const ERROR_STATE = 'error_state';
const GlobalErrorController: React.FC = () => {

  const [modalVisible, setModalVisible] = useState<boolean>(false);  

  const globalContext = useGlobalContext();

  const handleDismiss = async () => {
    Cache.removeItem( ERROR_STATE );
    setModalVisible(false);    
  };


  useEffect(() => {
    Hub.listen('ErrorChannel', (data) => {
      let errorState: IGlobalError | null = null;
      // eslint-disable-next-line react-hooks/exhaustive-deps
      errorState = Cache.getItem( ERROR_STATE );     

      if (errorState) {
        globalContext.showGlobalError(errorState);
      }
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {    
    setModalVisible(!(globalContext.globalError === null));    
  },[globalContext.globalError]);

  return (
    <GlobalErrorModal
      errorState={globalContext.globalError}
      handleDismiss={handleDismiss}
      modalVisible={modalVisible}
    />
  );
};

export default GlobalErrorController;