import * as React from 'react';
import {ISpinnerProps, IStackProps, Spinner, SpinnerSize, Stack} from "office-ui-fabric-react";
const rowProps: IStackProps = { horizontal: true, verticalAlign: 'center' };

type LoadingProps = ISpinnerProps;

export const Loading: React.SFC<LoadingProps> = (props) => (
  <Stack {...rowProps}>
    <Spinner
      title={props.title || "Loading"}
      size={props.size || SpinnerSize.large}></Spinner>
  </Stack>
);