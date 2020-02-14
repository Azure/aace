import React, { Children } from "react"
import { confirmAlert } from 'react-confirm-alert'; // Import
import 'react-confirm-alert/src/react-confirm-alert.css'; // Import css

export type ConfirmDialogBoxProps =  {
    cancelonClick: any,
    submitonClick: any,
    title: string,
    message: string,
    className?: string,
    cancelButtonText: string,
    submitButtonText: string,
}

// export const ConfirmDialog: React.FunctionComponent<ConfirmDialogBoxProps> = (props):void => {
//     return (        
//         confirmAlert({
//             title: props.title,
//             message: props.message,
//             buttons: [
//                 {
//                     label: props.submitButtonText,
//                     onClick: () => {
//                         props.submitonClick
//                     }
//                 },
//                 {
//                     label: props.cancelButtonText,
//                     onClick: () => {
//                         props.cancelonClick
//                     }
//                 }
//             ]
//         })

//     )
// }