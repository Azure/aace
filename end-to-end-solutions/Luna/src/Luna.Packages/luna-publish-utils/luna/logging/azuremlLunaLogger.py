from luna.logging.baseLunaLogger import BaseLunaLogger
from azureml.core import Workspace, Run, Experiment
import os

class AzureMLLunaLogger(BaseLunaLogger):
    
    def log_metric(self, key, value):
        run = Run.get_context(allow_offline=False)
        run.log(key, value)

    def log_metrics(self, metrics):
        run = Run.get_context(allow_offline=False)
        for key in metrics:
            run.log(key, metrics[key])

    def upload_artifacts(self, local_path, upload_path):
        run = Run.get_context(allow_offline=False)
        if os.path.isdir(local_path):
            run.upload_folder(upload_path, local_path)
        elif os.path.isfile(local_path):
            run.upload_file(upload_path, local_path)