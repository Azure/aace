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
        GetABatchInferenceOperationWithDefaultModel,
        ListAllInferenceOperationsByUserWithDefaultModel,
        // TYOM
        TrainModel,
        ListAllTrainingOperationsByUser,
        GetATrainingOperationsByModelIdUser,
        GetAModelByModelIdUserProductDeployment,
        GetAllModelsByUserProductDeployment,
        DeleteAModel,
        BatchInference,
        GetABatchInferenceOperation,
        ListAllInferenceOperationsByUser,
        DeployRealTimePredictionEndpoint,
        GetADeployOperationByEndpointIdUser,
        ListAllDeployOperationsByUser,
        GetAllRealTimeServiceEndpointsByUserProductDeployment,
        GetARealTimeServiceEndpointByEndpointIdUserProductDeployment,
        DeleteAEndpoint
    }
}
