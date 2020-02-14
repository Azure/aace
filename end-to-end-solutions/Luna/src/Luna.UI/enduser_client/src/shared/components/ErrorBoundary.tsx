import * as React from 'react';

export interface Props {
  generateError: () => React.ReactNode;
}

interface Info {
  componentStack: string;
}

interface State {
  error: Error | null;
  errorInfo: Info | null;
}

export class ErrorBoundary extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {error: null, errorInfo: null};
  }

  componentDidCatch(error: Error | null, errorInfo: Info) {
    this.setState({
      error,
      errorInfo
    });

    console.log('ErrorBoundary caught exception');
    console.log(error!);
  }

  render() {
    if (this.state.errorInfo) {
      return (
        this.props.generateError()
      );
    }
    // No error, just render children
    return this.props.children;
  }
}
