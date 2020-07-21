from luna.baseLunaUtils import BaseLunaUtils
from mlflow.pyfunc import PythonModel, PythonModelContext
from mlflow.tracking.client import MlflowClient
from luna.logging.mlflowLunaLogger import MLFlowLunaLogger

import mlflow
import os
import yaml

MLFLOW_MODEL_PATH = "MLFLOW_MODELS"

class MLflowLunaUtils(BaseLunaUtils):

    def Init(self, luna_config, run_mode, args, userInput):
        super().Init(luna_config, run_mode, args, userInput)
        mlflow.set_tracking_uri('databricks')
        if not mlflow.active_run():
            with open(self._luna_config["mlflow"]["test_experiment"]) as file:
                test_exp = yaml.full_load(file)
                mlflow.start_run(experiment_id=test_exp["experiment_id"])
        
        self._logger = MLFlowLunaLogger()

    def RegisterModel(self, model_path, description, luna_python_model=None):
        mlFlowRun = mlflow.active_run()
        if mlFlowRun:
            tags={'userId': self._args.userId, 
                        'productName': self._args.productName, 
                        'deploymentName': self._args.deploymentName, 
                        'apiVersion':self._args.apiVersion,
                        'subscriptionId':self._args.subscriptionId, 
                        'modelId': self._args.modelId}
            mlflow.set_tags(tags)
            mlflow.pyfunc.log_model(artifact_path=model_path, 
            python_model=luna_python_model, 
            artifacts={MLFLOW_MODEL_PATH: model_path}, 
            conda_env=self._luna_config["conda_env"])
            model_uri = "runs:/{run_id}/{artifact_path}".format(run_id=mlFlowRun.info.run_id, artifact_path=model_path)
            mlflow.register_model(
                model_uri,
                self._args.modelId
            )

    def DeployModel(self):
        return
        
    def DownloadModel(self, model_path=""):
        currentRun = mlflow.active_run()
        filter_string = 'tags."modelId" = "{model_id}"'.format(model_id = self._args.modelId)
        client = MlflowClient(tracking_uri='databricks')
        runs = client.search_runs(experiment_ids=currentRun.info.experiment_id, filter_string=filter_string)
        
        target_model_path = os.path.join(os.getcwd(), model_path)

        os.makedirs(target_model_path, exist_ok=True)

        full_model_path = client.download_artifacts(runs[0].info.run_id, "models/artifacts/", target_model_path)
        return full_model_path