import * as React from 'react';
import {useState} from 'react';
import {useGlobalContext} from "../shared/components/GlobalProvider";
import {GlobalProcessingModal} from './GlobalProcessingModal';

export const PROCESSING_STATE = 'processing_state';
const GlobalProcessingController: React.FC = () => {

  const [processingVisible, setProcessingVisible] = useState<boolean>(false);  

  const globalContext = useGlobalContext();

  return (
    <GlobalProcessingModal processingVisible={globalContext.isProcessing} />
  );
};

export default GlobalProcessingController;