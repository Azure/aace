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
        ListAllInferenceOperationsByUserWithDefaultModel,
        // TYOM
        TrainModel,
        ListAllTrainingOperationsByUser,
        GetAllTrainingOperationsByModelIdUser,
        GetAModelByModelIdUserProductDeployment,
        GetAllModelsByUserProductDeployment,
        DeleteAModel,
        BatchInference,
        GetABatchInferenceOperation,
        ListAllInferenceOperationsByUser,
        DeployRealTimePredictionEndpoint,
        GetAllDeployOperationsByEndpointIdUser,
        ListAllDeployOperationsByUser,
        GetAllRealTimeServiceEndpointsByUserProductDeployment,
        GetARealTimeServiceEndpointByEndpointIdUserProductDeployment,
        DeleteAEndpoint
    }
}
