import {useState} from 'react';
import {Result} from "../../models";

export const arrayItemErrorMessage = (errors: any, touched: any, collectionName: string, collectionIndex: number, propertyName: string): string | undefined => {
  return errors && errors[collectionName] &&
    errors[collectionName][collectionIndex] &&
    touched[collectionName] &&
    touched[collectionName][collectionIndex] &&
    touched[collectionName][collectionIndex][propertyName] &&
    errors[collectionName][collectionIndex][propertyName];
}

export const arrayItemErrorMessageWithoutTouch = (errors: any, touched: any, collectionName: string, collectionIndex: number, propertyName: string): string | undefined => {
  return errors && errors[collectionName] &&
    errors[collectionName][collectionIndex] &&
    errors[collectionName][collectionIndex][propertyName];
}

export const arrayItemErrorMessageFromForm = (errors: any, touched: any, formName: string, collectionName: string, collectionIndex: number, propertyName: string): string | undefined => {
  return errors && errors[formName] && errors[formName][collectionName] &&
    errors[formName][collectionName][collectionIndex] &&
    touched[formName][collectionName] &&
    touched[formName][collectionName][collectionIndex] &&
    touched[formName][collectionName][collectionIndex][propertyName] &&
    errors[formName][collectionName][collectionIndex][propertyName];
}

export const arrayItemErrorMessageWithoutTouchFromForm = (errors: any, touched: any, formName: string, collectionName: string, collectionIndex: number, propertyName: string): string | undefined => {
  return errors && errors[formName] && errors[formName][collectionName] &&
    errors[formName][collectionName][collectionIndex] &&
    errors[formName][collectionName][collectionIndex][propertyName];
}

export const ChildarrayItemErrorMessage = (errors: any, touched: any, collectionName: string, collectionIndex: number
  , childCollectionname: string, childcollectionIndex: number, propertyName?: string): string | undefined => {

  if (propertyName) {
    return errors && errors[collectionName] && errors[collectionName][collectionIndex]&&
      errors[collectionName][collectionIndex][childCollectionname] &&
      errors[collectionName][collectionIndex][childCollectionname][childcollectionIndex] &&
      errors[collectionName][collectionIndex][childCollectionname][childcollectionIndex][propertyName] &&
      touched[collectionName] &&
      touched[collectionName][collectionIndex] &&
      touched[collectionName][collectionIndex][childCollectionname] &&
      touched[collectionName][collectionIndex][childCollectionname][childcollectionIndex] &&
      touched[collectionName][collectionIndex][childCollectionname][childcollectionIndex][propertyName] &&
      errors[collectionName][collectionIndex][childCollectionname][childcollectionIndex][propertyName];
  }
  else {
    return errors && errors[collectionName] && errors[collectionName][collectionIndex]&&
      errors[collectionName][collectionIndex][childCollectionname] &&
      errors[collectionName][collectionIndex][childCollectionname][childcollectionIndex] &&
      touched[collectionName] &&
      touched[collectionName][collectionIndex] &&
      touched[collectionName][collectionIndex][childCollectionname] &&
      touched[collectionName][collectionIndex][childCollectionname][childcollectionIndex] &&
      errors[collectionName][collectionIndex][childCollectionname][childcollectionIndex];
  }


}

export const ChildarrayItemErrorMessageWithoutTouch = (errors: any, touched: any, collectionName: string, collectionIndex: number
  , childCollectionname: string, childcollectionIndex: number, propertyName?: string): string | undefined => {
  if (propertyName) {
    return errors && errors[collectionName] && errors[collectionName][collectionIndex] &&
      errors[collectionName][collectionIndex][childCollectionname] &&
      errors[collectionName][collectionIndex][childCollectionname][childcollectionIndex] &&
      errors[collectionName][collectionIndex][childCollectionname][childcollectionIndex][propertyName] &&
      errors[collectionName][collectionIndex][childCollectionname][childcollectionIndex][propertyName];
  }
  else {
    return errors && errors[collectionName] && errors[collectionName][collectionIndex] &&
      errors[collectionName][collectionIndex][childCollectionname] &&
      errors[collectionName][collectionIndex][childCollectionname][childcollectionIndex] &&
      errors[collectionName][collectionIndex][childCollectionname][childcollectionIndex];
  }
}

export default function useFunctionAsState(fn) {

  const [val, setVal] = useState(() => fn);

  function setFunc(fn) {
    setVal(() => fn);
  }

  return [val, setFunc];

}

export const handleSubmissionErrorsForArray = (setErrors: any, setSubmitting: any, setFormError: any, formKey: string, itemIndex: number, result: Result<any> ) : boolean => {
  return handleSubmissionErrorsGeneral(setErrors, setSubmitting, setFormError, formKey, true, itemIndex, result);
};

export const handleSubmissionErrorsForForm = (setErrors: any, setSubmitting: any, setFormError: any, formKey: string, result: Result<any> ) : boolean => {
  return handleSubmissionErrorsGeneral(setErrors, setSubmitting, setFormError, formKey, false, 0, result);
};

export const handleSubmissionErrorsGeneral = (setErrors: any, setSubmitting: any, setFormError: any, formKey: string, isArray: boolean, idx: number, result: Result<any> ) : boolean => {

  setSubmitting(false);

  if (result.hasErrors) {
    let errorObj = {};
    let setupValidationElement = false;

    for(let err of result.errors) {
      if (Object.keys(err).length === 0) {
        alert('Encountered an error parsing the returned error values');
        return true;
      }

      // need to handle multiple validation errors and or validation and method errors from the server
      //TODO address this
      //let key = Object.keys(err)[0]; // field with validation errors'

      let errorDetailItem = err;

      let key = errorDetailItem.target.replace('$.', '');

      if (isArray) {

        if (!setupValidationElement) {
          setupValidationElement = true;
          errorObj[formKey] = [];
          // Pad our array of errors so that formik can associate our error with the correct element in the array
          for (var i = 0; i < idx; i++)
            errorObj[formKey].push(undefined);

          let tempError = {};

          tempError[key] = errorDetailItem.message;
          errorObj[formKey].push(tempError);
        } else {
          errorObj[formKey][errorObj[formKey].length - 1][key] = errorDetailItem.message;
        }

      } else {
        errorObj[formKey] = {};
        errorObj[formKey][key] = errorDetailItem.message;
      }

      if (key === "method_error")
        setFormError(errorDetailItem.message);
      else
        setErrors(errorObj);
    }
    return true;
  }

  if (result.success)
    return false;
  else
    return true; // if we get to this point, that means a global error has already been shown to the user
}

//Below method is just for testing error response
export const TesthandleSubmissionErrorsGeneral = (setErrors: any, setFormError: any, formKey: string, isArray: boolean, idx: number, result: Result<any> ) : boolean => {
/*
  if (result.hasErrors) {
    let errorObj = {};
    let setupValidationElement = false;

    for(let err of result.errors) {
      if (Object.keys(err).length === 0) {
        alert('Encountered an error parsing the returned error values');
        return true;
      }

      // need to handle multiple validation errors and or validation and method errors from the server
      //TODO address this
      let key = Object.keys(err)[0]; // field with validation errors'

      if (isArray) {

        if (!setupValidationElement) {
          setupValidationElement = true;
          errorObj[formKey] = [];
          // Pad our array of errors so that formik can associate our error with the correct element in the array
          for (var i = 0; i < idx; i++)
            errorObj[formKey].push(undefined);

          let tempError = {};
          tempError[key] = Object.values(err)[0].join(', ');
          errorObj[formKey].push(tempError);
        } else {
          errorObj[formKey][errorObj[formKey].length - 1][key] = Object.values(err)[0].join(', ');
        }

      } else {
        errorObj[formKey] = {};
        errorObj[formKey][key] = Object.values(err)[0].join(', ');
      }

      if (key === "method_error")
        setFormError(Object.values(err)[0].join('<br/>'));
      else
        setErrors(errorObj);
    }
    return true;
  }

  if (result.success)
    return false;
  else*/
    return true; // if we get to this point, that means a global error has already been shown to the user
}