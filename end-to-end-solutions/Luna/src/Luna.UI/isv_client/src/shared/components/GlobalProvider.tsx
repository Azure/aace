import * as React from 'react';
import {useContext, useState} from 'react';
import useFunctionAsState from "../formUtils/utils";

// tslint:disable no-any
export const GlobalContext: React.Context<any> = React.createContext(undefined as any);

export interface GlobalProps {
  globalError: IGlobalError | null;
  showGlobalError: (gError: IGlobalError) => void;
  showProcessing: () => void;
  saveForm: () => Promise<void>;
  modifySaveForm: (submitFunc: () => Promise<void>) => void;
  hideProcessing: () => void;
  isProcessing: boolean;
  setFormDirty:(val: boolean)=>void;
  isDirty: boolean;
  setSecondaryFormDirty:(val: boolean)=>void;
  isSecondaryDirty: boolean;
}
// tslint:enable no-any

export const GlobalProvider = ({ children }) => {

  const GlobalProvider = useGlobalProvider();
  return (
    <GlobalContext.Provider value={GlobalProvider}>
      {children}
    </GlobalContext.Provider>
  );
};

// Hook for child components to get the auth object ...
// ... and re-render when it changes.
export const useGlobalContext = () => {
  return useContext<GlobalProps>(GlobalContext);
};

export interface IGlobalError {
  title: string;
  description: string;
};

export const useGlobalProvider = (): GlobalProps => {

  const [isProcessing, setIsProcessing] = useState<boolean>(false);
  const [globalError, setGlobalError] = useState<IGlobalError | null>(null);
  const [saveForm, setSaveForm] = useFunctionAsState(() => {});
  const [isDirty, setIsDirty] = useState<boolean>(false);
  const [isSecondaryDirty, setIsSecondaryDirty] = useState<boolean>(false);


  const showProcessing = () => {
    setIsProcessing(true);
  };

  const hideProcessing = () => {
    setIsProcessing(false);
  };

  const setFormDirty = (val: boolean) => {
    setIsDirty(val);
  };
  const setSecondaryFormDirty = (val: boolean) => {
    setIsSecondaryDirty(val);
  };

  const showGlobalError = (gError: IGlobalError) => {
    setIsProcessing(false);
    setGlobalError(gError);
  };

  const modifySaveForm = (submitFunc: () => Promise<void>) => {
    setSaveForm(submitFunc);
  };

  return {
    showGlobalError,
    showProcessing,
    modifySaveForm,
    saveForm,
    isProcessing,
    globalError,
    hideProcessing,
    isDirty,
    setFormDirty,
    isSecondaryDirty,
    setSecondaryFormDirty
  };
};