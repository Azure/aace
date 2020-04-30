import loadable from '@loadable/component';
import {Loading} from "../shared/components/Loading";


// Primary routes
export const Home = loadable(() => import('./Home/Home'), {
  LoadingComponent: Loading
});

export const Offers = loadable(() => import('./Offers/Offers'), {
  LoadingComponent: Loading
});

export const ModifyOfferInfo = loadable(() => import('./Offers/Info'), {
  LoadingComponent: Loading
});

export const ModifyOfferParameters = loadable(() => import('./Offers/Parameters'), {
  LoadingComponent: Loading
});

export const ModifyOfferIpConfigs = loadable(() => import('./Offers/IpConfigs'), {
  LoadingComponent: Loading
});

export const ModifyOfferArmTemplates = loadable(() => import('./Offers/ArmTemplates'), {
  LoadingComponent: Loading
});

export const ModifyOfferWebHooks = loadable(() => import('./Offers/WebHooks'), {
  LoadingComponent: Loading
});

export const ModifyOfferMeters = loadable(() => import('./Offers/Meters'), {
  LoadingComponent: Loading
});

export const ReviewOffer = loadable(() => import('./Offers/ReviewOffer'), {
  LoadingComponent: Loading
});

export const ModifyOfferPlans = loadable(() => import('./Offers/Plans'), {
  LoadingComponent: Loading
});

export const ModifyPlan = loadable(() => import('./Offers/ModifyPlan'), {
  LoadingComponent: Loading
});

export const Subscription = loadable(() => import('./Subscriptions/Subscriptions'), {
  LoadingComponent: Loading
});

export const SubscriptionDetail = loadable(() => import('./Subscriptions/SubscriptionDetail'), {
  LoadingComponent: Loading
});

export const Products = loadable(() => import('./Products/Products'), {
  LoadingComponent: Loading
});

export const ModifyProductInfo = loadable(() => import('./Products/Info'), {
  LoadingComponent: Loading
});

export const ProductDetail = loadable(() => import('./Products/ProductDetail'), {
  LoadingComponent: Loading
});

export const NoVersion = loadable(() => import('./NoVersion/NoVersion'), {
  LoadingComponent: Loading
});