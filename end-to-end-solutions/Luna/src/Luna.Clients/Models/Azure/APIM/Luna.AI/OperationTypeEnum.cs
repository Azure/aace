using System;
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
        GetAllBatchInferenceOperationsWithDefaultModel,
        // TYOM
        TrainModel,
        GetAModel,
        GetAllModels,
        BatchInference,
        GetABatchInferenceOperation,
        GetAllBatchInferenceOperations,
        DeployRealTimePredictionEndpoint,
        GetADeployedEndpoint,
        GetAllDeployedEndpoints
    }
}
