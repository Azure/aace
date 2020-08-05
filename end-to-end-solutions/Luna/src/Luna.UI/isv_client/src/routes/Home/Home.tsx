import React from 'react';
import {Redirect} from "react-router";
import {WebRoute} from "../../shared/constants/routes";

const Home: React.FunctionComponent = () => {
  const v1Enabled = (window.Configs.ENABLE_V1.toLowerCase() === 'true' ? true : false);
  const v2Enabled = (window.Configs.ENABLE_V2.toLowerCase() === 'true' ? true : false);

  if (v1Enabled) {
    return (
      <Redirect to={WebRoute.Offers} />
    );
  }
  else if (v2Enabled) {
    return (
      <Redirect to={WebRoute.Products} />
    );
  }
  else {
    return (
      <Redirect to={WebRoute.NoVersion}/>
    );
  }

};

export default Home;