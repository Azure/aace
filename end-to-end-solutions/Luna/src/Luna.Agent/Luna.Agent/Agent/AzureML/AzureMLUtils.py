from uuid import uuid4
from azureml.core import Workspace, Experiment, Model
from azureml.core.webservice import AksWebservice, Webservice, AciWebservice
from azureml.pipeline.core import PublishedPipeline
from azureml.core.authentication import ServicePrincipalAuthentication
from luna import utils
from Agent import key_vault_client
import json
from luna import utils
import tempfile
import os

class AzureMLUtils(object):
    """The utlitiy class to execute and monitor runs in AML"""
    
    def get_workspace_info_from_resource_id(self, resource_id):
        infoList = resource_id.split('/')
        subscriptionId = infoList[2]
        resourceGroupName = infoList[4]
        workspaceName = infoList[-1]
        return subscriptionId, resourceGroupName, workspaceName

    def __init__(self, workspace):
        if workspace.AADApplicationSecret:
            secret = workspace.AADApplicationSecret
        else:
            secret = key_vault_client.get_secret(workspace.AADApplicationSecretName).value
        auth = ServicePrincipalAuthentication(
            tenant_id = workspace.AADTenantId,
            service_principal_id = workspace.AADApplicationId,
            service_principal_password = secret)
        subscriptionId, resourceGroupName, workspaceName = self.get_workspace_info_from_resource_id(workspace.ResourceId)
        ws = Workspace(subscriptionId, resourceGroupName, workspaceName, auth)
        self._workspace = ws
        
    def get_pipeline_id_from_url(self, url):
        list = url.split('/')
        return list[-1]

    def submitPipelineRun(self, pipelineUrl, productName, deploymentName, apiVersion, entryPoint, userInput, predecessorOperationId, userId, subscriptionId):
        operationId = str('a' + uuid4().hex[1:])
        experimentName = subscriptionId
        exp = Experiment(self._workspace, experimentName)
        tags={'userId': userId, 
              'productName': productName, 
              'deploymentName': deploymentName, 
              'apiVersion': apiVersion,
              'operationName': entryPoint,
              'operationId': operationId,
              'subscriptionId': subscriptionId,
              'predecessorOperationId': predecessorOperationId}
        pipeline = PublishedPipeline.get(workspace = self._workspace, id = self.get_pipeline_id_from_url(pipelineUrl))
        exp.submit(pipeline, tags)
        return operationId

    def runProject(self, productName, deploymentName, apiVersion, operationVerb, userInput, predecessorOperationId, userId, subscriptionId):
        operationId = str('a' + uuid4().hex[1:])
        experimentName = subscriptionId
        entryPoint = utils.GetOperationNameByVerb(operationVerb)
        run_id = utils.RunProject(azureml_workspace = self._workspace, 
                                entry_point = entryPoint, 
                                experiment_name = experimentName, 
                                parameters={'operationId': operationId, 
                                            'userId': userId,
                                            'userInput': userInput,
                                            'productName': productName,
                                            'deploymentName': deploymentName,
                                            'apiVersion': apiVersion,
                                            'subscriptionId': subscriptionId,
                                            'predecessorOperationId': predecessorOperationId}, 
                                tags={'userId': userId, 
                                        'productName': productName, 
                                        'deploymentName': deploymentName, 
                                        'apiVersion': apiVersion,
                                        'operationName': entryPoint,
                                        'subscriptionId': subscriptionId,
                                        'predecessorOperationId': predecessorOperationId,
                                        'operationId': operationId})
        return operationId

    def getOperationStatus(self, operationVerb, operationId, userId, subscriptionId):
        experimentName = subscriptionId
        exp = Experiment(self._workspace, experimentName)
        operationName = utils.GetOperationNameByVerb(operationVerb)
        tags = {'userId': userId,
                'operationId': operationId,
                'operationName': operationName,
                'subscriptionId': subscriptionId}
        runs = exp.get_runs(type='azureml.PipelineRun', tags=tags)
        run = next(runs)
        result = {'operationId': operationId,
                  'status': run.status
            }
        return result

    def listAllOperations(self, operationVerb, userId, subscriptionId):
        experimentName = subscriptionId
        operationName = utils.GetOperationNameByVerb(operationVerb)
        exp = Experiment(self._workspace, experimentName)
        tags = {'userId': userId,
                'operationName': operationName,
                'subscriptionId': subscriptionId}
        runs = exp.get_runs(type='azureml.PipelineRun', tags=tags)
        run = next(runs)
        resultList = []
        while True:
            result = {'operationId': run.tags["operationId"],
                    'status': run.status
                }
            resultList.append(result)
            try:
                run = next(runs)
            except StopIteration:
                break
        return resultList

    def getOperationOutput(self, operationNoun, operationId, userId, subscriptionId):
        operationName = utils.GetOperationNameByNoun(operationNoun)
        

        if operationName == 'train':
            
            tags = {'userId': userId,
                    'modelId': operationId,
                    'subscriptionId': subscriptionId}
            models = Model.list(self._workspace, tags = tags)
            model = models[0]
            result = {'id': model.id,
                      'description': model.description,
                      'created_time': model.created_time}
            return result

        if operationName == 'deploy':
            
            tags = {'userId': userId,
                    'modelId': operationId,
                    'endpointId': subscriptionId}
            endpoints = Webservice.list(self._workspace, tags = tags)
            endpoint = endpoints[0]
            result = {'id': operationId,
                      'description': model.description,
                      'created_time': model.created_time}

            result.scoring_uri = endpoint.scoring_uri
            primaryKey, secondaryKey = endpoint.get_keys()
            result.primary_key = primaryKey
            result.secondary_key = secondaryKey

            return result
        
        tags = {'userId': userId,
                'operationId': operationId,
                'operationName': operationName,
                'subscriptionId': subscriptionId}

        experimentName = subscriptionId
        exp = Experiment(self._workspace, experimentName)
        runs = exp.get_runs(type='azureml.PipelineRun', tags=tags)
        run = next(runs)
        child_runs = run.get_children()
        child_run = next(child_runs)
        outputType = utils.GetOutputType(operationName)
        if outputType == 'json':
            with tempfile.TemporaryDirectory() as tmp:
                path = os.path.join(tmp, 'output.json')
                files = child_run.download_file('/outputs/output.json', path)
                with open(path) as file:
                    return json.load(file)

    def listAllOperationOutputs(self, operationNoun, userId, subscriptionId):
        operationName = utils.GetOperationNameByNoun(operationNoun)
        experimentName = subscriptionId
        exp = Experiment(self._workspace, experimentName)
        tags = {'userId': userId,
                'operationName': operationName,
                'subscriptionId': subscriptionId}
        runs = exp.get_runs(type='azureml.PipelineRun', tags=tags)
        run = next(runs)
        results = []
        while True:
            child_runs = run.get_children()
            child_run = next(child_runs)

            files = child_run.get_file_names()
            results.append({"operationId": run.tags["operationId"], "files": files})
            try:
                run = next(runs)
            except StopIteration:
                break
        return results

    def deleteOperationOutput(self, productName, deploymentName, apiVersion, operationName, operationId, userId, subscriptionId):
        return

    def getComputeClusters(self):
        clusters = self._workspace.compute_targets
        computeClusters = []
        for cluster in clusters.values():
            if cluster.type == "AmlCompute":
                computeClusters.append(cluster.name)
        return computeClusters

    def getDeploymentClusters(self):
        clusters = self._workspace.compute_targets
        deploymentClusters = []
        for cluster in clusters.values():
            if cluster.type == "AKS":
                deploymentClusters.append(cluster.name)
        return deploymentClusters