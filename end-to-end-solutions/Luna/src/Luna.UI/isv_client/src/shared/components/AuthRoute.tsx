import * as React from 'react';
import {Route, RouteProps} from 'react-router-dom';

export const AuthRoute: React.FC<RouteProps> = (props) => {
  return <Route {...props}/>;
};