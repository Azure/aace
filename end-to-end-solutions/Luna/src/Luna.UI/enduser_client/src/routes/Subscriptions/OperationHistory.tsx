import React, {useEffect, useState} from 'react';
import {Stack} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import {useParams} from "react-router";
import {Loading} from "../../shared/components/Loading";
import {Formik, useFormikContext} from "formik";
import {IOperationHistoryModel} from '../../models/ISubscriptionsModel';
import SubscriptionsService from '../../services/SubscriptionsService';
import {getInitialOperationHistoryModel, OperationHistoryModel} from './formUtils/subscriptionFormUtils';
import {useGlobalContext} from '../../shared/components/GlobalProvider';

function formatDate(datetime) {
  var hours = datetime.getHours();
  var minutes = datetime.getMinutes();
  var seconds = datetime.getSeconds();
  var ampm = hours >= 12 ? 'pm' : 'am';
  hours = hours % 12;
  hours = hours ? hours : 12; // the hour '0' should be '12'
  minutes = minutes < 10 ? '0' + minutes : minutes;
  seconds = seconds < 10 ? '0' + seconds : seconds;
  var strTime = hours + ':' + minutes + ':' + seconds + ' ' + ampm;

  var month = datetime.getMonth();
  month = month < 10 ? '0' + month : month;
  var date = datetime.getDate();
  date = date < 10 ? '0' + date : date;
  var fullYear = datetime.getFullYear();
  return month + 1 + "/" + date + "/" + fullYear + " " + strTime;
}

const OperationHistory: React.FunctionComponent = () => {
  const globalContext = useGlobalContext();
  const [state, setstate] = useState<OperationHistoryModel>(getInitialOperationHistoryModel);
  const [loadingOperationHistory, setLoadingOperationHistory] = useState<boolean>(true);

  const {subscriptionId} = useParams();

  useEffect(() => {
    if (subscriptionId)
      getData(subscriptionId);
  }, []);

  const getData = async (subscriptionId: string) => {
    setLoadingOperationHistory(true);
    const dataResponse = await SubscriptionsService.getOperationHistory(subscriptionId);

    // Global errors should have already been handled for get requests by this point
    if (dataResponse.value && dataResponse.success) {
      var data = dataResponse.value as IOperationHistoryModel[];

      setstate(
        {
          data: [...data]
        });
    }
    setLoadingOperationHistory(false);

  }

  return (
    <React.Fragment>
      <Stack
        verticalAlign="start"
        horizontal={false}
        styles={{
          root: {}
        }}
      >
        <Stack
          horizontalAlign="start"
          verticalAlign="center"
          styles={{
            root: {
              width: '100%'
            }
          }}
        >
          <React.Fragment>
            {loadingOperationHistory ?
              (
                <table>
                  <tbody>
                  <tr>
                    <td align={"center"}>
                      <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                        <Loading/>
                      </Stack>
                    </td>
                  </tr>
                  </tbody>
                </table>

              )
              :
              <Formik initialValues={state}
                      validateOnBlur={true}
                      enableReinitialize={true}
                      onSubmit={async (values, {setSubmitting, setErrors}) => {
                        const input = {...values};
                        console.log('submitted form:');
                        console.log(input);

                        // toast.success("Success !");
                        setSubmitting(false);

                      }}
              >
                <OperationHistoryList/>
              </Formik>
            }
          </React.Fragment>


        </Stack>
      </Stack>
    </React.Fragment>
  );
}
export type IOperationHistoryProps = {
  formError?: string | null;
}
export const OperationHistoryList: React.FunctionComponent<IOperationHistoryProps> = (props) => {
  const {isSubmitting, setFieldValue, values, handleChange, handleBlur, touched, errors, resetForm, handleSubmit, submitForm} = useFormikContext<OperationHistoryModel>(); // formikProps
  const globalContext = useGlobalContext();

  useEffect(() => {
    globalContext.modifySaveForm(async () => {
      await submitForm();
    });
  }, []);

  const OperationHistory = ({data}) => {
    if (!data || data.length == 0) {
      return <tr>
        <td colSpan={3}><span>No Outstanding Operation</span></td>
      </tr>;
    } else {
      return (
        data.map((value: IOperationHistoryModel, idx) => {
          return (
            <tr key={idx}>
              <td style={{paddingLeft: '31px', width: '30%'}}>
                <span>{formatDate(new Date(value.timeStamp))}</span>
              </td>
              <td style={{width: '20%'}}>
                {value.action === 'Unsubscribe'
                  ? <React.Fragment>
                    <span style={{color: '#0c7ed9', fontWeight: 400}}>Unsubscribe</span>
                  </React.Fragment>
                  : <span style={{color: '#0c7ed9', fontWeight: 400}}>Change Plan</span>
                }
                {/* <span style={{ color: '#248bdd', fontWeight: 500 }}>{value.action}</span> */}
              </td>
              <td style={{color: '#e8a405', fontWeight: 400}}>
                                <span>{value.status === 1
                                  ? <span>In Progress</span>
                                  : <span></span>
                                }</span>
              </td>
            </tr>
          )
        })
      )
    }
  }

  return (
    <form style={{width: '100%', marginTop: 20}} autoComplete={"off"}>
      <table className="noborder offer">
        <thead>
        <tr>
          <th style={{width: '30%'}}>
            <div style={{marginLeft: '31px'}}>
              <FormLabel title={"TimeStamp"}/>
            </div>

          </th>
          <th style={{width: '20%'}}>
            <div>
              <FormLabel title={"Action"}/>
            </div>
          </th>
          <th>
            <div>
              <FormLabel title={"Status"}/>
            </div>
          </th>
        </tr>
        </thead>
        <tbody>
        <OperationHistory data={values.data}/>
        </tbody>
      </table>
    </form>
  )
}
export default OperationHistory;