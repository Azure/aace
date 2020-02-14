import React from 'react';
import {ErrorBoundary} from "../shared/components/ErrorBoundary";
import {NotFound} from "../shared/components/NotFound";
import Header from "./Header";
import SubscriptionContent from "./SubscriptionContent";
import {useLocation} from "react-router";
import Content from './Content';
import GlobalErrorController from './GlobalErrorController';
import GlobalProcessingController from './GlobalProcessingController';

export interface LayoutHelperMenuItem {
  title: string;
  paths: string[];
  menuClick: () => void;
}
export interface LayoutHelper {
  menuItems: LayoutHelperMenuItem[];
}

const Layout: React.FunctionComponent = (props) => {
  const { children } = props;

  const location = useLocation();  
  let modifySubscriptionActive = (location.pathname.toLowerCase().startsWith('/operationhistory'));

  let subscriptionId: string | null = null;
  if (modifySubscriptionActive) {
    // get offerName from the path
    var idx = location.pathname.indexOf('/',1);

    if (idx > 0) {
      var idx2 = location.pathname.indexOf('/',idx + 1);
      if (idx2 > 0) {

        subscriptionId = location.pathname.toLowerCase().substr(idx + 1, idx2 - (idx + 1));
      }
      else
      subscriptionId = location.pathname.toLowerCase().substr(idx + 1);
    }
  }
  return (
    <React.Fragment>
      <Header/>
      <ErrorBoundary generateError={() => <NotFound title={"Error"} message={"An unknown error has occurred"} statusCode={500}/>}>                
      {modifySubscriptionActive && (
          <SubscriptionContent subscriptionId={subscriptionId}>
            {children}
          </SubscriptionContent>
        )}
          <Content>
            {children}
          </Content>                
      </ErrorBoundary>
      {/* Handle global errors */}
      <GlobalErrorController/>
      <GlobalProcessingController/>
    </React.Fragment>
  );
};

export default Layout;