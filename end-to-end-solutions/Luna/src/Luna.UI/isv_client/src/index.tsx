import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import * as serviceWorker from './serviceWorker';
import {ErrorBoundary} from "./shared/components/ErrorBoundary";
import {BrowserRouter as Router} from 'react-router-dom';
import {FluentCustomizations} from '@uifabric/fluent-theme';
import {Customizer, mergeStyles} from 'office-ui-fabric-react';
import {GlobalProvider} from "./shared/components/GlobalProvider";
import { runWithAdal } from 'react-adal';
import adalContext from './adalConfig';
const DO_NOT_LOGIN = false;

declare global {
  interface Window { Configs: any; }
}
window.Configs = window.Configs || {};

// Inject some global styles
mergeStyles({
  selectors: {
    ':global(body), :global(html), :global(#root)': {
      margin: 0,
      padding: 0,
      height: '100vh'
    }
  }
});

const AppIndex: React.SFC<{}> = () => {
  const generateError = () =>
    <span>Not Found</span>;

  return (
    <ErrorBoundary generateError={generateError}>
      <Router>
        <Customizer {...FluentCustomizations}>
          <GlobalProvider>
            <App/>
          </GlobalProvider>
        </Customizer>
      </Router>
    </ErrorBoundary>
  );
};

runWithAdal(adalContext.AuthContext, () => {
  ReactDOM.render(
    <AppIndex />,
    document.getElementById('root') as HTMLElement
  );
  serviceWorker.unregister();
},DO_NOT_LOGIN);

