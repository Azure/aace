import { Dialog, DialogType, DialogFooter, PrimaryButton } from "office-ui-fabric-react"
import React from "react"
import AlternateButton from "./AlternateButton"

export type DialogBoxProps = {
    keyindex: string,
    dialogVisible?: boolean,
    onDismiss?: any,
    cancelonClick: any
    submitonClick: any
    title: string,
    subText: string,
    className: string,
    cancelButtonText: string,
    submitButtonText:string,
    isDarkOverlay?:boolean
    maxwidth:number
}

export const DialogBox: React.FunctionComponent<DialogBoxProps> = (props) => {
    return (
        <Dialog key={props.keyindex}
            hidden={!props.dialogVisible}
            onDismiss={props.onDismiss}
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
                className: props.className,
                isDarkOverlay: props.isDarkOverlay,
                styles: {

                    main: {
                        maxWidth: props.maxwidth
                    }
                }
            }}
        >

            {props.children}

            <DialogFooter key={props.keyindex}>
                <AlternateButton
                    onClick={props.cancelonClick}
                    text={props.cancelButtonText} />
                <PrimaryButton
                    text={props.submitButtonText} onClick={props.submitonClick} />
            </DialogFooter>
        </Dialog>
    )
}