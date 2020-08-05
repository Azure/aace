import React from 'react';
import { ErrorBoundary } from "../shared/components/ErrorBoundary";
import { NotFound } from "../shared/components/NotFound";
import Header from "./Header";
import OfferContent from "./OfferContent";
import ProductContent from './ProductContent'
import { useLocation } from "react-router";
import Content from './Content';
import GlobalErrorController from './GlobalErrorController';
import ReviewOfferContent from './ReviewOfferContent';
import SubscriptionDetailContent from "./SubscriptionDetailContent";
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
  let genericContentWrapper = true;

  const v1Enabled = (window.Configs.ENABLE_V1.toLowerCase() === 'true' ? true : false);
  const v2Enabled = (window.Configs.ENABLE_V2.toLowerCase() === 'true' ? true : false);

  let modifyOfferActive = (location.pathname.toLowerCase().startsWith('/modifyoffer'));
  let noVersionActive = (location.pathname.toLowerCase().startsWith('/noversion'));
  let reviewOfferActive = (location.pathname.toLowerCase().startsWith('/reviewoffer'));
  let subscriptionDetailActive = (location.pathname.toLowerCase().startsWith('/subscriptiondetail'));
  let listViewActive = (location.pathname.toLowerCase().startsWith('/subscriptions')
    || location.pathname.toLowerCase().startsWith('/offers')
    || location.pathname.toLowerCase().startsWith('/products'));
  let modifyProductActive = (location.pathname.toLowerCase().startsWith('/modifyproduct'));

  if (modifyOfferActive || reviewOfferActive || listViewActive || subscriptionDetailActive || modifyProductActive || noVersionActive)
    genericContentWrapper = false;

  let offerName: string | null = null;
  let productName: string | null = null;
  if (modifyOfferActive || reviewOfferActive || subscriptionDetailActive) {
    // get offerName from the path
    var idx = location.pathname.indexOf('/', 1);
    var idx2 = 0;
    if (idx > 0) {
      idx2 = location.pathname.indexOf('/', idx + 1);
      if (idx2 > 0) {

        offerName = location.pathname.toLowerCase().substr(idx + 1, idx2 - (idx + 1));
      }
      else
        offerName = location.pathname.toLowerCase().substr(idx + 1);
    }
  }

  if (modifyProductActive) {
    idx = location.pathname.indexOf('/', 1);

    if (idx > 0) {
      idx2 = location.pathname.indexOf('/', idx + 1);
      if (idx2 > 0) {

        productName = location.pathname.toLowerCase().substr(idx + 1, idx2 - (idx + 1));
      }
      else
        productName = location.pathname.toLowerCase().substr(idx + 1);
    }
  }


  return (
    <React.Fragment>
      <Header />
      <ErrorBoundary generateError={() => <NotFound title={"Error"} message={"An unknown error has occurred"} statusCode={500} />}>
        {modifyOfferActive && v1Enabled && (
          <OfferContent offerName={offerName}>
            {children}
          </OfferContent>
        )}

        {subscriptionDetailActive && (v1Enabled || v2Enabled) && (
          <SubscriptionDetailContent>
            {children}
          </SubscriptionDetailContent>
        )}

        {modifyProductActive && v2Enabled && (
          <ProductContent productName={productName}>
            {children}
          </ProductContent>
        )}

        {(noVersionActive) && <div>{children}</div>}

        {(listViewActive) && (
          <Content>
            {children}
          </Content>
        )}
        {/* This must be the last content wrapper */}
        {(reviewOfferActive || genericContentWrapper) && (
          <ReviewOfferContent offerName={offerName}>
            {children}
          </ReviewOfferContent>
        )}
      </ErrorBoundary>
      {/* Handle global errors */}
      <GlobalErrorController />
      <GlobalProcessingController />
    </React.Fragment>
  );
};

export default Layout;