import * as React from 'react';
import {Route, RouteProps} from 'react-router-dom';
//import {WebRoute} from '../constants/routes';


export const AuthRoute: React.FC<RouteProps> = (props) => {
  
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const { location } = props;



  //if (auth.loading)
    //return null;

  if (false) {
    return ( <span>You must sign in to see this page</span> );
  }

  return <Route {...props}/>;
};