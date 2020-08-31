// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Azure
{
    public enum OperationTypeEnum
    {
        // RTP
        RealTimePrediction,
        // BI
        BatchInferenceWithDefaultModel,
        GetBatchInferenceOperationWithDefaultModel,
        ListAllInferenceOperationsByUserWithDefaultModel,
        // TYOM
        TrainModel,
        ListAllTrainingOperationsByUser,
        GetTrainingOperationByModelIdUser,
        GetModelByModelIdUserProductDeployment,
        GetAllModelsByUserProductDeployment,
        DeleteModel,
        BatchInference,
        GetBatchInferenceOperation,
        ListAllInferenceOperationsByUser,
        DeployRealTimePredictionEndpoint,
        GetDeployOperationByEndpointIdUser,
        ListAllDeployOperationsByUser,
        GetAllRealTimeServiceEndpointsByUserProductDeployment,
        GetARealTimeServiceEndpointByEndpointIdUserProductDeployment,
        DeleteEndpoint
    }
}
