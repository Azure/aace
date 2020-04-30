import React, { } from 'react';
import {
  Stack
} from 'office-ui-fabric-react';
import { RouteComponentProps } from 'react-router-dom';

import ProductDeployments from '../Products/Deployments';
import AMLWorkSpace from '../Products/AMLWorkSpace' 


const ProductDetail: React.FunctionComponent = () => {

  //#region AMLWork/space 
  // const AmlWorkSpace = () => {
  //   const globalContext = useGlobalContext();
  //   return (
  //     <Formik
  //       initialValues={ARMTemplateParameters}
  //       validationSchema={armTemplateParametersFormValidationSchema}
  //       validateOnBlur={true}
  //       onSubmit={async (values, {setSubmitting, setErrors}) => {
  //         const input = {...values};
  //         setSubmitting(true);
  //         globalContext.showProcessing();

  //         for (let param of input.templateParameters) {

  //           var idx = values.templateParameters.findIndex(x => x.clientId === param.clientId);
  //           let paramUpdateResult = await ArmTemplateParameterService.update(offerName as string, param);
  //           if (handleSubmissionErrorsForArray(setErrors, setSubmitting, setFormError, 'templateParameters', idx, paramUpdateResult)) {
  //             globalContext.hideProcessing();
  //             return;
  //           }
  //         }

  //         setSubmitting(false);
  //         globalContext.hideProcessing();

  //         toast.success("Success!");

  //         getArmTemplateParameters();
  //         setTimeout(() => {globalContext.setSecondaryFormDirty(false);}, 500);
  //       }}
  //     >
  //       <ArmTemplateParametersFormBody formError={formError}/>
  //     </Formik>
  //   );

  // }

  // type IArmTemplateParametersFormBodyProps = {
  //   formError?: string | null;
  // }

  // const ArmTemplateParametersFormBody: React.FunctionComponent<IArmTemplateFormBodyProps> = (props) => {
  //   const {values, handleChange, handleBlur, touched, errors,handleSubmit, submitForm, dirty} = useFormikContext<IARMTemplateParametersForm>(); // formikProps

  //   const globalContext = useGlobalContext();
  //   const {formError} = props;

  //   useEffect(() => {
  //     globalContext.modifySaveForm(async () => {
  //       await submitForm();
  //     });
  //     // eslint-disable-next-line react-hooks/exhaustive-deps
  //   }, []);


  //   return (
  //     <form style={{width: '100%', marginTop: 20, textAlign: 'left', marginBottom: 0}} autoComplete={"off"}
  //           onSubmit={handleSubmit}>
  //       <h3 style={{textAlign: 'left', fontWeight: 'normal'}}>Parameters</h3>
  //       {formError && <div style={{marginBottom: 15}}><MessageBar messageBarType={MessageBarType.error}>
  //           <div dangerouslySetInnerHTML={{__html: formError}} style={{textAlign: 'left'}}></div>
  //       </MessageBar></div>}

  //       <table style={{borderCollapse: 'collapse', width: '100%'}} cellPadding={10}
  //              className="noborder armparamtable">
  //         <thead>
  //         <tr style={{fontWeight: 'normal', borderBottom: '1px solid #e8e8e8'}}>
  //           <th>
  //             <FormLabel title={"ID"}/>
  //           </th>
  //           <th>
  //             <FormLabel title={"Type"}/>
  //           </th>
  //           <th>
  //             <FormLabel title={"Value"}/>
  //           </th>
  //         </tr>
  //         </thead>
  //         {values.templateParameters.length > 0 ?
  //           <React.Fragment>
  //             <FieldArray
  //               name="templateParameters"
  //               render={arrayHelpers => {                  
  //                 return (
  //                   <React.Fragment>
  //                     <tbody>
  //                     {errors && typeof errors.templateParameters === 'string' ?
  //                       <div>{errors.templateParameters}</div> : null}
  //                     {values.templateParameters.map((value: IARMTemplateParameterModel, idx) => {
  //                       return (
  //                         <tr key={idx}>
  //                           <td>
  //                             <span>{value.name}</span>
  //                           </td>
  //                           <td>
  //                             <span>{value.type}</span>
  //                           </td>
  //                           <td>
  //                             <TextField
  //                               name={`templateParameters.${idx}.value`}                                
  //                               value={value.value}
  //                               onChange={handleChange}
  //                               onBlur={handleBlur}
  //                               errorMessage={_arrayItemErrorMessage(false, globalContext, errors, touched, 'templateParameters', idx, 'value', dirty)}
  //                               placeholder={''}/>
  //                           </td>
  //                         </tr>
  //                       );
  //                     })}
  //                     </tbody>
  //                   </React.Fragment>
  //                 );
  //               }}
  //             />
  //           </React.Fragment>
  //           :
  //           <tbody>
  //           <tr>
  //             <td colSpan={4}>
  //               No Data Found
  //             </td>
  //           </tr>
  //           </tbody>
  //         }
  //       </table>
  //     </form>
  //   );
  // }     
  //#endregion

  return (
    <Stack
      horizontalAlign="center"
      verticalAlign="start"
      verticalFill
      styles={{
        root: {
          width: '90%',
          margin: '0 auto',
          textAlign: 'center',
          color: '#605e5c'
        }
      }}
      gap={15}
    >
      <ProductDeployments />
      
      <AMLWorkSpace/>
    </Stack>
  );

}

export default ProductDetail;