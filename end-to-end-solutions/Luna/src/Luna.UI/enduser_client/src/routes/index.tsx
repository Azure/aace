import loadable from '@loadable/component';
import {Loading} from "../shared/components/Loading";


// Primary routes
export const Home = loadable(() => import('./Home/Home'), {
  LoadingComponent: Loading
});

export const Subscriptions = loadable(() => import('./Subscriptions/Subscriptions'), {
  LoadingComponent: Loading
});

export const OperationHistory = loadable(() => import('./Subscriptions/OperationHistory'), {
  LoadingComponent: Loading
});

export const LandingPage = loadable(() => import('./EndUser/landingPage'), {
  LoadingComponent: Loading
});