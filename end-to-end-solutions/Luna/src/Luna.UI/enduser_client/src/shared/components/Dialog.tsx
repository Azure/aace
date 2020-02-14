import { Dialog, DialogType, TextField, FontIcon, DialogFooter, PrimaryButton, DialogContent } from "office-ui-fabric-react"
import React, { Children } from "react"
import { values } from "@uifabric/utilities"
import { FieldArray } from "formik"
import { ChildarrayItemErrorMessage } from "../formUtils/utils"
import AlternateButton from "./AlternateButton"

export type DialogBoxProps = {
    keyindex: string,
    dialogVisible?: boolean,
    onDismiss: any,
    cancelonClick: any
    submitonClick: any
    title: string,
    subText: string,
    className: string,
}

export const DialogBox: React.FunctionComponent<DialogBoxProps> = (props) => {
    return (
        <Dialog key={props.keyindex}
            // hidden={!ipRangeDialogVisible}
            hidden={!props.dialogVisible}
            // onDismiss={hideNewIpRangeDialog}
            onDismiss={props.onDismiss}
            className={props.className}

            dialogContentProps={{
                styles: {
                    title: {
                        fontWeight: 'normal',
                        paddingRight: 0,
                    },
                    subText: {
                        paddingTop: 0,
                    }
                },
                type: DialogType.normal,
                title: props.title,
                subText: props.subText
            }}
            modalProps={{
                isBlocking: true,
                styles: {

                    main: {
                        maxWidth: 100
                    }
                }
            }}
        >

            {props.children}

            <DialogFooter key={props.keyindex}>
                <AlternateButton
                    // onClick={hideNewIpRangeDialog}
                    onClick={props.cancelonClick}
                    text="Cancel" />
                <PrimaryButton
                    text="Save" onClick={props.submitonClick} />
            </DialogFooter>
        </Dialog>
    )
}