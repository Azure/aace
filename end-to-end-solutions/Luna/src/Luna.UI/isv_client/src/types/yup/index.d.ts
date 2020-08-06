// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
import {ArraySchema, ObjectSchema, default as yup} from 'yup'

declare module 'yup' {
  interface ObjectSchema {
    uniqueProperty(propertyName: string, message: string): ObjectSchema
  }
}