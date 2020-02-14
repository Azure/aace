import * as React from 'react';
import {useContext, useState} from 'react';
import useFunctionAsState from "../../shared/formUtils/utils";

// tslint:disable no-any
export const SubscriptionContext: React.Context<any> = React.createContext(undefined as any);

export interface SubscriptionProps {
  saveForm: () => Promise<void>;
  modifySaveForm: (submitFunc: () => Promise<void>) => void;
}
// tslint:enable no-any

export const SubscriptionProvider = ({ children }) => {

  const subscriptionProvider = useSubscriptionProvider();
  return (
      <SubscriptionContext.Provider value={subscriptionProvider}>
        {children}
      </SubscriptionContext.Provider>
  );
};

// Hook for child components to get the auth object ...
// ... and re-render when it changes.
export const useSubscriptionContext = () => {
  return useContext<SubscriptionProps>(SubscriptionContext);
};

export const useSubscriptionProvider = (): SubscriptionProps => {

  const [saveForm, setSaveForm] = useFunctionAsState(() => {});

  const modifySaveForm = (submitFunc: () => Promise<void>) => {
    setSaveForm(submitFunc);
  };
  // Return the user object and auth methods
  return {
    saveForm,
    modifySaveForm
  };
};