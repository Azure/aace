import React from 'react';
import {Redirect} from "react-router";
import {WebRoute} from "../../shared/constants/routes";

const Home: React.FunctionComponent = () => {
  return (
    <Redirect to={WebRoute.Offers} />
  );
};

export default Home;