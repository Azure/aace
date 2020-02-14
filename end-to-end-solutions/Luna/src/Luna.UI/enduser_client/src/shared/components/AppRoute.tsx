import * as React from 'react';
import {Route, RouteProps} from 'react-router-dom';
import {RouterProps} from "react-router";

export const AppRoute: React.FC<RouteProps & RouterProps> = (props) => {

  return <Route {...props}/>;

};