from abc import ABCMeta, abstractmethod
from luna.azuremlLunaUtils import AzureMLLunaUtils
from luna.mlflowLunaUtils import MLflowLunaUtils
from luna.localLunaUtils import LocalLunaUtils

import yaml
import io
import argparse
import json
import os

MLFLOW_MODEL_PATH = "MLFLOW_MODELS"

class LunaUtils(object):
    
    @staticmethod
    def Create(luna_config_file='luna_config.yml', run_mode='default', parameters={}):
        with open(luna_config_file) as file:
            luna_config = yaml.full_load(file)
        
        args, userInput = LunaUtils.ParseArgument()
        
        if run_mode == 'default':
            run_mode = luna_config['run_mode']

        if run_mode == 'azureml':
            utils = AzureMLLunaUtils()
        elif run_mode == 'mlflow':
            utils = MLflowLunaUtils()
        elif run_mode == 'local':
            utils = LocalLunaUtils()

        utils.Init(luna_config, run_mode, args, userInput)
        return utils

    @staticmethod
    def ParseArgument():
        
        parser = argparse.ArgumentParser("lunaOperation")

        parser.add_argument("--runMode", type=str, help="run mode")
        parser.add_argument("--userInput", type=str, help="input data")
        parser.add_argument("--userId", type=str, help="user id")
        parser.add_argument("--operationId", type=str, help="predecessor id")
        parser.add_argument("--predecessorOperationId", type=str, help="predecessor operation id")
        parser.add_argument("--productName", type=str, help="product name")
        parser.add_argument("--deploymentName", type=str, help="deployment name")
        parser.add_argument("--apiVersion", type=str, help="api version")
        parser.add_argument("--subscriptionId", type=str, help="subscription id")

        args = parser.parse_args()
        userInput = json.loads(args.userInput)
        return args, userInput

    @staticmethod
    def GetModelPath(run_mode, context):
        if run_mode == 'azureml':
            return os.getenv('AZUREML_MODEL_DIR')
        elif run_mode == 'mlflow':
            return context[MLFLOW_MODEL_PATH]
        else:
            return 'models'