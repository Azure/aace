from azureml.core import Workspace, Run, Experiment
from azureml.core.compute import ComputeTarget, AmlCompute
from azureml.core.environment import Environment
from azureml.core.model import Model, InferenceConfig
from azureml.core.runconfig import RunConfiguration
from azureml.core.webservice import AksWebservice, AciWebservice, Webservice
from azureml.pipeline.core.graph import PipelineParameter
from azureml.pipeline.steps import PythonScriptStep
from azureml.pipeline.core import Pipeline

from luna.baseLunaUtils import BaseLunaUtils
from luna.logging.azuremlLunaLogger import AzureMLLunaLogger

import os
import yaml

class AzureMLLunaUtils(BaseLunaUtils):

    def Init(self, luna_config, run_mode, args, userInput):
        super().Init(luna_config, run_mode, args, userInput)
        self._logger = AzureMLLunaLogger()

    def GetAMLWorkspace(self):
        try:
            run = Run.get_context(allow_offline=False)
            return run.experiment.workspace
        except:
            return Workspace.from_config(path=self._luna_config["azureml"]["test_workspace_path"], 
                _file_name=self._luna_config["azureml"]["test_workspace_file_name"])
    
    def RegisterModel(self, model_path, description, luna_python_model=None):
        ws = self.GetAMLWorkspace()

        Model.register(model_path = model_path,
                       model_name = self._args.modelId,
                       description = description,
                       workspace = ws,
                       tags={'userId': self._args.userId, 
                        'productName': self._args.productName, 
                        'deploymentName': self._args.deploymentName, 
                        'apiVersion':self._args.apiVersion,
                        'subscriptionId':self._args.subscriptionId,
                        'modelId': self._args.modelId})

    def GetDeploymentConfig(self, tags):
        with open(self._luna_config['azureml']['workspace_config']) as file:
            documents = yaml.full_load(file)
            deployment_target = documents['deployment_target']
            if deployment_target == 'aks':
                aks_cluster = documents['aks_cluster']


        with open(self._luna_config['deploy_config']) as file:
            documents = yaml.full_load(file)

            if deployment_target == 'aci':
                deployment_config = AciWebservice.deploy_configuration()
                deployment_config.__dict__.update(documents['azureContainerInstance'])
                deployment_config.dns_name_label = self._userInput["dns_name_label"]
            elif deployment_target == 'aks':
                deployment_config = AksWebservice.deploy_configuration()
                deployment_config.__dict__.update(documents['kubernetes'])
                deployment_config.compute_target_name = aks_cluster
                deployment_config.namespace = self._userInput["dns_name_label"]

            deployment_config.tags = tags
        return deployment_config

    def DeployModel(self):
        
        ws = self.GetAMLWorkspace()
        model = Model(ws, self._args.modelId)

        print(model)

        myenv = Environment.from_conda_specification('scoring', self.luna_config['conda_env'])

        print(self.luna_config['code']['score'])
        print(os.getcwd())

        inference_config = InferenceConfig(entry_script=self.luna_config['code']['score'], source_directory = os.getcwd(), environment=myenv)

        deployment_config = self.GetDeploymentConfig(
            tags={'userId': self._args.userId, 
                'productName': self._args.productName, 
                'deploymentName': self._args.deploymentName, 
                'apiVersion':self._args.apiVersion,
                'subscriptionId':self._args.subscriptionId,
                'modelId': self._args.modelId})
        
        service = Model.deploy(ws, self._args.endpointId, [model], inference_config, deployment_config)
        service.wait_for_deployment(show_output = True)

    def DownloadModel(self, model_path=""):
        ws = self.GetAMLWorkspace()
        model = Model(ws, self._args.modelId)
        full_model_path = os.path.join(os.getcwd(), model_path, "models/artifacts")
        
        os.makedirs(full_model_path, exist_ok=True)
        model.download(target_dir = full_model_path, exist_ok=True)
        return full_model_path
