from luna.logging.baseLunaLogger import BaseLunaLogger

import mlflow
import os

class MLFlowLunaLogger(BaseLunaLogger):
    
    def log_metric(self, key, value):
        mlflow.log_metric(key, value)

    def log_metrics(self, metrics):
        mlflow.log_metrics(metrics)

    def upload_artifacts(self, local_path, upload_path):
        if os.path.isdir(local_path):
            mlflow.log_artifacts(local_path, upload_path)
        elif os.path.isfile(local_path):
            mlflow.log_artifact(local_path, upload_path)