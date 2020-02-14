import * as React from 'react';
import {Stack} from "office-ui-fabric-react";

interface ParentProps {
  title?: string;
  message?: React.ReactNode;
  statusCode?: number;
}

type Props = ParentProps;

export const NotFound: React.FC <Props> = (props) => {
  const { title, message, statusCode } = props;
  const notFoundMessage = message || "Not Found";
  return (
    <Stack verticalAlign={"center"} horizontalAlign={"center"}>
      <React.Fragment>
        {notFoundMessage}
      </React.Fragment>
    </Stack>
  );
};