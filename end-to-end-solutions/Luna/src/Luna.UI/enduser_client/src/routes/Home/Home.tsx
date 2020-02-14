import React from 'react';
import {Redirect} from "react-router";
import {WebRoute} from "../../shared/constants/routes";

const Home: React.FunctionComponent = () => {

  console.log('i am here');
  return (
    <Redirect to={WebRoute.Subscriptions} />
  );
};

export default Home;