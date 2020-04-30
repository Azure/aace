import React from 'react';
import './App.css';
import {Route, Switch} from 'react-router-dom';
import {WebRoute} from "./shared/constants/routes";
import {AuthRoute} from './shared/components/AuthRoute';
import * as Routes from './routes';
import {NotFound} from "./shared/components/NotFound";
import Layout from "./layout/Layout";

import {initializeIcons} from 'office-ui-fabric-react/lib/Icons';
import {registerYupMethods} from "./routes/Offers/formUtils/registerYupMethods";
import {toast} from "react-toastify";
import 'react-toastify/dist/ReactToastify.css';

initializeIcons(/* optional base url */);
registerYupMethods();

toast.configure({autoClose: 2000});

const App: React.FC = () => {

  const BaseRoutes = () => {

    return (
    <Layout>
      <Switch>
        {/* Protected Routes */}
        <AuthRoute exact={true} path={WebRoute.Home} component={Routes.Home}/>
        <AuthRoute exact={true} path={WebRoute.Offers} component={Routes.Offers}/>
        <AuthRoute exact={true} path={WebRoute.NoVersion} component={Routes.NoVersion}/>

        <AuthRoute path={WebRoute.ModifyOfferIpConfigs} component={Routes.ModifyOfferIpConfigs} />
        <AuthRoute path={WebRoute.ModifyOfferArmTemplates} component={Routes.ModifyOfferArmTemplates} />

        <AuthRoute path={WebRoute.ModifyOfferWebHooks} component={Routes.ModifyOfferWebHooks} />
        <AuthRoute path={WebRoute.ModifyOfferMeters} component={Routes.ModifyOfferMeters} />
        <AuthRoute path={WebRoute.ModifyOfferPlans} component={Routes.ModifyOfferPlans} />
        <AuthRoute path={WebRoute.ModifyOfferParameters} component={Routes.ModifyOfferParameters} />
        <AuthRoute path={WebRoute.ModifyOfferInfo} component={Routes.ModifyOfferInfo} />
        <AuthRoute path={WebRoute.ReviewOffer} component={Routes.ReviewOffer} />

        <AuthRoute path={WebRoute.Subscriptions} component={Routes.Subscription} />
        <AuthRoute path={WebRoute.SubscriptionDetail} component={Routes.SubscriptionDetail} />

        <AuthRoute path={WebRoute.Products} component={Routes.Products} />
        <AuthRoute path={WebRoute.ModifyProductInfo} component={Routes.ModifyProductInfo} />
        <AuthRoute path={WebRoute.ProductDetail} component={Routes.ProductDetail} />

        <Route component={NotFound}/>
      </Switch>
    </Layout>
    );
  };

  return (
    <div className="App">
      <Switch>
        <Route render={() => <BaseRoutes/>}/>
      </Switch>
    </div>
  );
}

export default App;
