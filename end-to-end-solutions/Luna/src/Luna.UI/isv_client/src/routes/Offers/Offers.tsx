import React, { useEffect, useState } from 'react';
import {
  Stack,    
  PrimaryButton,
  MessageBar,
  MessageBarType,  
  Dialog, DialogType, DialogFooter,
  FontIcon,
  TextField,
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { useHistory } from "react-router";
import { WebRoute } from "../../shared/constants/routes";
import OfferService from "../../services/OfferService";
import { IOfferModel, Result } from '../../models';
import { Loading } from "../../shared/components/Loading";
import { IOfferWarningsModel } from "../../models/IOfferWarningsModel";
import AlternateButton from "../../shared/components/AlternateButton";
import { initialInfoFormValues, IOfferInfoFormValues, offerInfoValidationSchema, deleteOfferValidator, initialOfferValues } from "./formUtils/offerFormUtils";
import { Formik } from "formik";
import { OfferForm } from "./Info";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { DialogBox } from '../../shared/components/Dialog';

const Offers: React.FunctionComponent = () => {

  const history = useHistory();  
  const globalContext = useGlobalContext();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formState, setFormState] = useState<IOfferInfoFormValues>(initialInfoFormValues);
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formError, setFormError] = useState<string | null>(null);  
  const [offers, setOffers] = useState<IOfferModel[]>([]);
  const [offerWarnings, setOfferWarnings] = useState<IOfferWarningsModel[]>([]);
  const [warningDialogVisible, setwarningDialogVisible] = useState<boolean>(false);
  const [warningDetail, setwarningDetail] = useState<string>('');
  const [loadingOffers, setLoadingOffers] = useState<boolean>(true);
  const [loadingWarnings, setLoadingWarnings] = useState<boolean>(true);
  const [offerDialogVisible, setOfferDialogVisible] = useState<boolean>(false);
  const [OfferDeleteDialog, setOfferDeleteDialog] = useState<boolean>(false);
  const [selectedOffer, setselectedOffer] = useState<IOfferModel>(initialOfferValues);

  const getOfferWarnings = async () => {

    setLoadingWarnings(true);
    const results = await OfferService.getOfferWarnings();
    if (results && results.value) {
      setOfferWarnings([...results.value]);
    }
    else {
      setOfferWarnings([]);
    }
    setLoadingWarnings(false);
  }

  const getOffers = async () => {

    setLoadingOffers(true);
    const results = await OfferService.list();
    if (results && !results.hasErrors && results.value)
      setOffers(results.value);
    else {
      setOffers([]);
      if (results.hasErrors) {
        // TODO: display errors
        alert(results.errors.join(', '));
      }

    }

    setLoadingOffers(false);
  }


  useEffect(() => {    
    getOfferWarnings();
    getOffers();
// eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const editItem = (offerName: string): void => {    
    history.push(WebRoute.ModifyOfferInfo.replace(':offerName', offerName));
  };

  const handleNewOffer = (): void => {
    showNewOfferDialog();
  };

  const deleteItem = (offerSelected: IOfferModel, idx: number): void => {
    offerSelected.selectedOfferindex = idx;
    setselectedOffer(offerSelected);
    setOfferDeleteDialog(true)
  };

  const getFormErrorString = (touched, errors, property: string) => {
    return touched && errors && touched[property] && errors[property] ? errors[property] : '';
  };

  const CloseOfferDeleteDialog = () => {
    setOfferDeleteDialog(false);
  }

  const Offers = ({ offers }) => {
    if (!offers || offers.length === 0) {
      return <tr>
        <td colSpan={4}><span>No Offers</span></td>
      </tr>;
    } else {
      return (
        offers.map((value: IOfferModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span style={{ width: 200 }}>{value.offerName}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.status}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.offerVersion}</span>
              </td>
              <td>
                <Stack
                  verticalAlign="center"
                  horizontalAlign={"space-evenly"}
                  gap={15}
                  horizontal={true}
                  styles={{
                    root: {
                      width: '40%'
                    },
                  }}
                >
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => { editItem(value.offerName) }} />
                  <FontIcon iconName="Cancel" className="deleteicon" onClick={() => { deleteItem(value, idx) }} />
                </Stack>
              </td>
            </tr>
          );
        })
      );
    }
  }

  const handleDeleteWarning = (idx: number): void => {

    offerWarnings.splice(idx, 1);
    setOfferWarnings([...offerWarnings]);
  }

  const showNewOfferDialog = (): void => {
    setOfferDialogVisible(true);
  };

  const hideNewOfferDialog = (): void => {
    setOfferDialogVisible(false);
  };

  const showWarningDialog = (detail: string): void => {
    setwarningDialogVisible(true);
    setwarningDetail(detail);
  };

  const hideWarningDialog = (): void => {
    setwarningDialogVisible(false);
  };

  const handleSubmissionErrors = (result: Result<any>, setSubmitting: any): boolean => {
    if (result.hasErrors) {
      // TODO - display the errors here
      alert(result.errors.join(', '));
      setSubmitting(false);
      return true;
    }
    return false;
  }

  const handleFormSubmission = async (e) => {
    if (globalContext.saveForm)
      await globalContext.saveForm();
  };

  const OfferWarnings = (): React.ReactElement | null => {
    if (loadingWarnings) {
      return (
        <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
          <Loading />
        </Stack>
      );
    } else {
      if (offerWarnings.length === 0)
        return null;

      return (
        <React.Fragment>
          <Stack
            verticalAlign="center"
            horizontal={false}
            gap={10}
            styles={{
              root: {
                marginBottom: 50,
              }
            }}
          >
            {offerWarnings.map((value, idx) => {
              return (
                <MessageBar key={`offerwarning_${idx}`} messageBarType={MessageBarType.severeWarning} isMultiline={true}
                  dismissButtonAriaLabel="Close"
                  onDismiss={() => {
                    handleDeleteWarning(idx)
                  }}>
                  <span dangerouslySetInnerHTML={{ __html: value.warningMessage }}>                    
                  </span> Click<span style={{cursor:'pointer'}} onClick={() => { showWarningDialog(value.details) }}>here</span> for more details.
                </MessageBar>
              )
            })}
            <Dialog
              hidden={!warningDialogVisible}
              onDismiss={hideWarningDialog}              
              dialogContentProps={{
                styles: {
                  subText: {
                    paddingTop: 0
                  },
                  title: {
                  }

                },
                type: DialogType.normal,
                title: 'Warning'
              }}
              modalProps={{
                isBlocking: false,
                isDarkOverlay:true,
                styles: {
                  main: {
                    minWidth: 440
                  }
                }
              }}
            >
              <span dangerouslySetInnerHTML={{ __html: warningDetail }}></span>
            </Dialog>
          </Stack>
        </React.Fragment>
      );
    }
  }

  return (
    <Stack
      verticalAlign="start"
      horizontal={false}
      styles={{
        root: {
          width: '100%',
          height: '100%',
          textAlign: 'left',
        }
      }}
    >
      <OfferWarnings />
      <Stack
        horizontalAlign="start"
        verticalAlign="center"
        styles={{
          root: {
            width: '100%'
          }
        }}
      >
        <PrimaryButton onClick={handleNewOffer} text={"New Offer"} />
        <table className="noborder offergrid" style={{ marginTop: 20, width: '100%' }} cellPadding={5} cellSpacing={0}>
          <thead>
            <tr style={{ fontWeight: 'normal' }}>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Offer ID"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Status"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Version"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Operations"} /></th>
            </tr>
          </thead>
          <tbody>
            {loadingOffers ?
              (
                <tr>
                  <td colSpan={4} align={"center"}>
                    <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                      <Loading />
                    </Stack>
                  </td>
                </tr>
              )
              : <Offers offers={offers} />}
          </tbody>
        </table>
      </Stack>
      <Dialog
        hidden={!offerDialogVisible}
        onDismiss={hideNewOfferDialog}

        dialogContentProps={{
          styles: {
            subText: {
              paddingTop: 0
            },
            title: {
            }

          },
          type: DialogType.normal,
          title: 'New Offer'
        }}
        modalProps={{
          isBlocking: true,
          styles: {

            main: {
              minWidth: 440
            }
          }
        }}
      >
        <Formik
          initialValues={formState}
          validationSchema={offerInfoValidationSchema}
          validateOnBlur={true}
          onSubmit={async (values, { setSubmitting, setErrors }) => {

            setFormError(null);
            setSubmitting(true);

            globalContext.showProcessing();
            var updateOfferResult = await OfferService.create(values.offer);
            if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'offer', updateOfferResult)) {
              globalContext.hideProcessing();
              return;
            }

            setSubmitting(false);
            globalContext.hideProcessing();
            toast.success("Success!");

            history.push(WebRoute.ModifyOfferInfo.replace(':offerName', values.offer.offerName));

          }}
        >
          <OfferForm isNew={true} offers={offers} />
        </Formik>
        <DialogFooter>
          <AlternateButton
            onClick={hideNewOfferDialog}
            text="Cancel" />
          <PrimaryButton
            onClick={handleFormSubmission}
            text="Save" />
        </DialogFooter>
      </Dialog>

      <DialogBox keyindex='Offermodal' dialogVisible={OfferDeleteDialog}
        title="Delete Offer" subText="" isDarkOverlay={true} className="" cancelButtonText="Cancel"
        submitButtonText="Submit" maxwidth={500}
        cancelonClick={() => {
          CloseOfferDeleteDialog();
        }}
        submitonClick={() => {
          const btnsubmit = document.getElementById('btnsubmit') as HTMLButtonElement;
          btnsubmit.click();
        }}
        children={
          <React.Fragment>
            <Formik
              initialValues={selectedOffer}
              validationSchema={deleteOfferValidator}
              enableReinitialize={true}
              validateOnBlur={true}
              onSubmit={async (values, { setSubmitting, setErrors }) => {
                var offerDeleteResponse = await OfferService.delete(values.selectedOfferName as string);
                handleSubmissionErrors(offerDeleteResponse, setSubmitting);
                if (offerDeleteResponse.success) {
                  offers.splice(values.selectedOfferindex as number, 1);
                  setOffers([...offers]);
                }
                setSubmitting(false);
                CloseOfferDeleteDialog();
              }}
            >
              {({ handleChange, values, handleBlur, touched, errors, handleSubmit }) => (
                <form autoComplete={"off"} onSubmit={handleSubmit}>
                  <table>
                    <tbody>
                      <tr>
                        <td colSpan={2}>
                          <span> Are you sure you want to delete this offer ?</span>
                        </td>
                      </tr>
                      <tr>
                        <td>
                          <b>Offer ID:</b>
                        </td>
                        <td>
                          <span>{values.offerName} </span>
                        </td>
                      </tr>
                      <tr>
                        <td>
                          <b>Owner ID: </b>
                        </td>
                        <td>
                          <span>{values.owners}</span>
                        </td>
                      </tr>
                      <tr>
                        <td colSpan={2}>
                          {
                            <React.Fragment>
                              <span>Type the offer name</span>
                              <br />
                              <TextField
                                name={'selectedOfferName'}
                                value={values.selectedOfferName}
                                onChange={handleChange}                                
                                onBlur={handleBlur}
                                errorMessage={getFormErrorString(touched, errors, 'selectedOfferName')}
                                placeholder={'Offer Name'}
                                className="txtFormField" />
                            </React.Fragment>
                          }
                        </td>
                      </tr>
                    </tbody>
                  </table>
                  <div style={{ display: 'none' }}>
                    <PrimaryButton type="submit" id="btnsubmit" text="Save" />
                  </div>
                </form>
              )}
            </Formik>
          </React.Fragment>
        } />
    </Stack>
  );
};

export default Offers;