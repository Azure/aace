"""
Routes and views for the flask application.
"""

from datetime import datetime
from flask import render_template
from flask import jsonify, request
from Agent.Code.CodeUtils import CodeUtils
from Agent.AzureML.AzureMLUtils import AzureMLUtils
from Agent.Mgmt.ControlPlane import ControlPlane
from datetime import datetime
from uuid import uuid4
import pathlib
from Agent.Data.APISubscription import APISubscription
from Agent.Data.APIVersion import APIVersion
from sqlalchemy.orm import sessionmaker
from Agent import engine, Session, app, key_vault_client
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential
from Agent.Data.AMLWorkspace import AMLWorkspace
from Agent.Data.AgentUser import AgentUser
from Agent.Data.Publisher import Publisher
from Agent.Exception.LunaExceptions import LunaServerException, LunaUserException
import json, os
from http import HTTPStatus
import requests


@app.route('/api/agentinfo')
def getAgentInfo():
    return jsonify({'name':'myagent', 'key':'mykey'})

def handleExceptions(e):
    if isinstance(e, LunaUserException):
        return e.message, e.http_status_code
    else:
        return 'The server encountered an internal error and was unable to complete your request.', 500

def getMetadata(subscriptionId, isRealTimePredict = False):
    
    apiVersion = request.args.get('api-version')
    if not apiVersion:
        raise LunaUserException(HTTPStatus.BAD_REQUEST, 'The api-version query parameter is not provided.')

    if subscriptionId == 'default':
        subscriptionKey = request.headers.get('api-key')
        if not subscriptionKey:
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, 'The api key is not provided.')

        sub = APISubscription.GetByKey(subscriptionKey)
        if not sub:
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, 'The api key is invalid.')
    else:
        sub = APISubscription.Get(subscriptionId)
        if not sub:
            raise LunaUserException(HTTPStatus.NOT_FOUND, 'The subscription {} does not exist.'.format(subscriptionId))

    version = APIVersion.Get(sub.ProductName, sub.DeploymentName, apiVersion)
    if not version:
        raise LunaUserException(HTTPStatus.NOT_FOUND, 'The api version {} does not exist.'.format(apiVersion))
    
    if not isRealTimePredict:
        if os.environ["AGENT_MODE"] == "SAAS":
            workspace = AMLWorkspace.GetById(version.AMLWorkspaceId)
        elif os.environ["AGENT_MODE"] == "LOCAL":
            if (not sub.AMLWorkspaceId) or sub.AMLWorkspaceId == 0:
                raise LunaServerException(HTTPStatus.METHOD_NOT_ALLOWED, 'There is not an Azure Machine Learning workspace configured for this subscription. Please contact your admin to finish the configuration.'.format(version.AMLWorkspaceId))
            workspace = AMLWorkspace.GetById(sub.AMLWorkspaceId)
        
        if not workspace:
            raise LunaServerException('The workspace with id {} is not found.'.format(version.AMLWorkspaceId))

        publisher = Publisher.Get(sub.PublisherId)
        if version.VersionSourceType == 'git':
            CodeUtils.getLocalCodeFolder(sub.SubscriptionId, sub.ProductName, sub.DeploymentName, version, pathlib.Path(__file__).parent.absolute(), publisher.ControlPlaneUrl)
    else:
        if version.AMLWorkspaceId and version.AMLWorkspaceId != 0:
            workspace = AMLWorkspace.GetById(version.AMLWorkspaceId)
        else:
            workspace = None

    return sub, version, workspace, apiVersion

def getSubscriptionAPIVersionAndWorkspace(subscriptionId, apiVersion):
    sub = APISubscription.Get(subscriptionId)
    version = APIVersion.Get(sub.ProductName, sub.DeploymentName, apiVersion)
    if os.environ["AGENT_MODE"] == "SAAS":
        workspace = AMLWorkspace.GetById(version.AMLWorkspaceId)
    elif os.environ["AGENT_MODE"] == "LOCAL":
        workspace = AMLWorkspace.GetById(sub.AMLWorkspaceId)

    return sub, version, workspace

@app.route('/predict', methods=['POST'])
@app.route('/<subscriptionId>/predict', methods=['POST'])
def realtimePredict(subscriptionId = 'default'):
    
    sub, version, workspace, apiVersion = getMetadata(subscriptionId, True)
    
    requestUrl = version.RealTimePredictAPI
    headers = {'Content-Type': 'application/json'}
    if version.AuthenticationType == 'Key':
        secret = key_vault_client.get_secret(version.AuthenticationKeySecretName).value
        headers['Authorization'] = 'Bearer {}'.format(secret)
    response = requests.post(requestUrl, json.dumps(request.json), headers=headers)
    if response.ok:
        return response.json(), response.status_code
    return response.text, response.status_code

@app.route('/saas-api/<operationVerb>', methods=['POST'])
@app.route('/api/<subscriptionId>/<operationVerb>', methods=['POST'])
def executeOperation(operationVerb, subscriptionId = 'default'):
    
    sub, version, workspace, apiVersion = getMetadata(subscriptionId)

    amlUtil = AzureMLUtils(workspace)
    if version.VersionSourceType == 'git':
        opId = amlUtil.runProject(sub.ProductName, sub.DeploymentName, apiVersion, operationVerb, json.dumps(request.json), 'na', sub.UserId, sub.SubscriptionId)
    elif version.VersionSourceType == 'amlPipeline':
        url = None
        if operationVerb == 'train':
            url = version.TrainModelAPI

        if url and url != "":
            opId = amlUtil.submitPipelineRun(url, sub.ProductName, sub.DeploymentName, apiVersion, operationVerb, json.dumps(request.json), 'na', sub.UserId, sub.SubscriptionId)
        else:
            return 'The operation {} is not supported'.format(operationVerb)
    
    return jsonify({'operationId': opId})

@app.route('/saas-api/operations/<operationVerb>/<operationId>', methods=['GET'])
@app.route('/api/<subscriptionId>/operations/<operationVerb>/<operationId>', methods=['GET'])
def getOperationStatus(operationVerb, operationId, subscriptionId = 'default'):
    try:
        sub, version, workspace, apiVersion = getMetadata(subscriptionId)
        amlUtil = AzureMLUtils(workspace)
        result = amlUtil.getOperationStatus(operationVerb, operationId, sub.UserId, sub.SubscriptionId)
        if result:
            return jsonify(result)
        else:
            raise LunaUserException(HTTPStatus.NOT_FOUND, 'Object with id {} does not exist.'.format(operationId))
    except Exception as e:
        return handleExceptions(e)

@app.route('/saas-api/operations/<operationVerb>', methods=['GET'])
@app.route('/api/<subscriptionId>/operations/<operationVerb>', methods=['GET'])
def listOperations(operationVerb, subscriptionId='default'):
    
    try:
        sub, version, workspace, apiVersion = getMetadata(subscriptionId)
        amlUtil = AzureMLUtils(workspace)
        result = amlUtil.listAllOperations(operationVerb, sub.UserId, sub.SubscriptionId)
        return jsonify(result)
    except Exception as e:
        return handleExceptions(e)

@app.route('/saas-api/<operationNoun>', methods=['GET'])
@app.route('/api/<subscriptionId>/<operationNoun>', methods=['GET'])
def listOperationOutputs(operationNoun, subscriptionId = 'default'):
    
    try:
        sub, version, workspace, apiVersion = getMetadata(subscriptionId)
        amlUtil = AzureMLUtils(workspace)
        result = amlUtil.listAllOperationOutputs(operationNoun, sub.UserId, sub.SubscriptionId)
        return jsonify(result)
    except Exception as e:
        return handleExceptions(e)

@app.route('/saas-api/<operationNoun>/<operationId>', methods=['GET'])
@app.route('/api/<subscriptionId>/<operationNoun>/<operationId>', methods=['GET'])
def getOperationOutput(operationNoun, operationId, subscriptionId = 'default'):
    
    try:
        sub, version, workspace, apiVersion = getMetadata(subscriptionId)
        amlUtil = AzureMLUtils(workspace)
        result = amlUtil.getOperationOutput(operationNoun, operationId, sub.UserId, sub.SubscriptionId)
        return jsonify(result)
    
    except Exception as e:
        return handleExceptions(e)

@app.route('/saas-api/<parentOperationNoun>/<parentOperationId>/<operationVerb>', methods=['POST'])
@app.route('/api/<subscriptionId>/<parentOperationNoun>/<parentOperationId>/<operationVerb>', methods=['POST'])
def executeChildOperation(parentOperationNoun, parentOperationId, operationVerb, subscriptionId = 'default'):
    
    try:
        sub, version, workspace, apiVersion = getMetadata(subscriptionId)
        amlUtil = AzureMLUtils(workspace)
        if version.VersionSourceType == 'git':
            opId = amlUtil.runProject(sub.ProductName, sub.DeploymentName, apiVersion, operationVerb, json.dumps(request.json), parentOperationId, sub.UserId, sub.SubscriptionId)
        elif version.VersionSourceType == 'amlPipeline':
            if parentOperationNoun != 'models':
                return 'The parent resource type {} is not supported'.format(parentOperationNoun)
            url = None
            if operationVerb == 'batchinference':
                url = version.BatchInferenceAPI
            elif operationVerb == 'deploy':
                url = version.DeployModelAPI

            if url and url != "":
                opId = amlUtil.submitPipelineRun(url, sub.ProductName, sub.DeploymentName, apiVersion, operationVerb, json.dumps(request.json), parentOperationId, sub.UserId, sub.SubscriptionId)
            else:
                return 'The operation {} is not supported'.format(operationVerb)
    
        return jsonify({'operationId': opId})
    
    except Exception as e:
        return handleExceptions(e)

@app.route('/saas-api/<operationNoun>/<operationId>', methods=['DELETE'])
@app.route('/api/<subscriptionId>/<operationNoun>/<operationId>', methods=['DELETE'])
def deleteOperationOutput(operationNoun, operationId, subscriptionId = 'default'):
    return jsonify({})


@app.route('/api/management/refreshMetadata', methods=['POST'])
def refreshMetadata():
    controlPlane = ControlPlane(os.environ['AGENT_ID'], os.environ['AGENT_KEY'])
    controlPlane.UpdateMetadataDatabase()
    return "The metadata database is refreshed", 200

@app.route('/api/management/deploymentTargetTypes', methods=['GET'])
def listAllDeploymentTargetTypes():
    return jsonify({
        'types': [{
                'id': 'aks',
                'displayName': 'Azure Kubernates Service'
            },
            {
                'id': 'aci',
                'displayName': 'Azure Container Instances'
            }]
        }), 200

@app.route('/api/management/subscriptions', methods=['GET'])
def listAllSubscriptions():
    subscriptions = APISubscription.ListAll()
    return jsonify(subscriptions), 200

@app.route('/api/management/subscriptions/<subscriptionId>', methods=['GET'])
def getSubscription(subscriptionId):
    subscription = APISubscription.Get(subscriptionId)
    return jsonify(subscription), 200

@app.route('/api/management/subscriptions/<subscriptionId>', methods=['PUT'])
def createOrUpdateSubscription(subscriptionId):
    """ TODO: do we need this API? """
    subscription = APISubscription(**request.json)
    APISubscription.Update(subscription)
    return jsonify(request.json), 202

@app.route('/api/management/subscriptions/<subscriptionId>/users', methods=['GET'])
def listAllSubscriptionUsers(subscriptionId):
    users = AgentUser.ListAllBySubscriptionId(subscriptionId)
    return jsonify(users), 200

@app.route('/api/management/subscriptions/<subscriptionId>/users/<userId>', methods=['GET'])
def getSubscriptionUser(subscriptionId, userId):
    user = AgentUser.GetUser(subscriptionId, userId)
    return jsonify(user), 200

@app.route('/api/management/subscriptions/<subscriptionId>/users/<userId>', methods=['PUT'])
def addSubscriptionUser(subscriptionId, userId):
    if AgentUser.GetUser(subscriptionId, userId):
        return "The user with user id {userId} already exists in subscription {subscriptionId}".format(userId = userId, subscriptionId = subscriptionId), 409
    user = AgentUser(**request.json)
    if subscriptionId != user.SubscriptionId:
        return "The subscription id in request body doesn't match the subscription id in request url.", 400
    if userId != user.AADUserId:
        return "The user id in request body doesn't match the user id in request url.", 400
    AgentUser.Create(user)
    return jsonify(request.json), 202

@app.route('/api/management/subscriptions/<subscriptionId>/users/<userId>', methods=['DELETE'])
def removeSubscriptionUser(subscriptionId, userId):
    if not AgentUser.GetUser(subscriptionId, userId):
        return "The user with user id {userId} doesn't exist in subscription {subscriptionId}".format(userId = userId, subscriptionId = subscriptionId), 404
    AgentUser.DeleteUser(subscriptionId, userId)
    return jsonify({}), 204


@app.route('/api/management/admins', methods=['GET'])
def listAllAdmins():
    admins = AgentUser.ListAllAdmin()
    return jsonify(admins), 200

@app.route('/api/management/admins/<userId>', methods=['GET'])
def getAdmin(userId):
    admin = AgentUser.GetAdmin(userId)
    if not admin:
        return "The admin with user id {userId} doesn't exist.".format(userId = userId), 404
    return jsonify(admin), 200

@app.route('/api/management/admins/<userId>', methods=['PUT'])
def addAdmin(userId):
    if AgentUser.GetAdmin(userId):
        return "The admin with user id {userId} already exists.".format(userId = userId), 409
    user = AgentUser(**request.json)

    if user.Role != "Admin":
        return "The role of the admin user must be Admin.", 400
    if userId != user.AADUserId:
        return "The user id in request body doesn't match the user id in request url.", 400
    AgentUser.Create(user)
    return jsonify(request.json), 202

@app.route('/api/management/admins/<userId>', methods=['DELETE'])
def removeAdmin(userId):
    if not AgentUser.GetAdmin(userId):
        return "The admin with user id {userId} doesn't exist.".format(userId = userId), 404
    AgentUser.DeleteAdmin(userId)
    return jsonify({}), 204

@app.route('/api/management/publishers', methods=['GET'])
def listAllPublishers():
    publishers = Publisher.ListAll()
    return jsonify(publishers), 200

@app.route('/api/management/publishers/<publisherId>', methods=['GET'])
def getPublisher(publisherId):
    publisher = Publisher.Get(publisherId)
    if not publisher:
        return "The publisher with id {publisherId} doesn't exist.".format(publisherId = publisherId), 404
    return jsonify(publisher), 200

@app.route('/api/management/publishers/<publisherId>', methods=['PUT'])
def addPublisher(publisherId):
    if Publisher.Get(publisherId):
        return "The publisher with id {publisherId} already exists.".format(publisherId = publisherId), 409
    publisher = Publisher(**request.json)

    if publisherId != publisher.PublisherId:
        return "The id in request body doesn't match the publisher id in request url.", 400
    Publisher.Create(publisher)
    return jsonify(request.json), 202

@app.route('/api/management/publishers/<publisherId>', methods=['DELETE'])
def removePublisher(publisherId):
    if not Publisher.Get(publisherId):
        return "The publisher with id {publisherId} doesn't exist.".format(publisherId = publisherId), 404
    Publisher.Delete(publisherId)
    return jsonify({}), 204

@app.route('/api/management/amlworkspaces', methods=['GET'])
def listAllAMLWorkspaces():
    workspaces = AMLWorkspace.ListAll()
    return jsonify(workspaces), 200

@app.route('/api/management/amlworkspaces/<workspaceName>', methods=['GET'])
def getAMLWorkspace(workspaceName):
    workspace = AMLWorkspace.Get(workspaceName)
    if workspace:
        return jsonify(workspace), 200
    else:
        return "Can not find the workspace with name {}".format(workspaceName), 404

@app.route('/api/management/amlworkspaces/<workspaceName>', methods=['PUT'])
def createOrUpdateAMLWorkspace(workspaceName):
    workspace = AMLWorkspace(**request.json)
    if workspaceName != workspace.WorkspaceName:
        return "The workspace name in request body doesn't match the workspace name in request url.", 400
    if AMLWorkspace.Exist(workspaceName):
        AMLWorkspace.Update(workspace)
        return jsonify(request.json), 200
    else:
        AMLWorkspace.Create(workspace)
        return jsonify(request.json), 202

@app.route('/api/management/amlworkspaces/<workspaceName>', methods=['DELETE'])
def deleteAMLWorkspace(workspaceName):
    if not AMLWorkspace.Exsit(workspaceName):
        return "Workspace with name {} doesn't exist.".format(workspaceName), 404

    if len(APISubscription.ListAllByWorkspaceName(workspaceName)) != 0:
        return "The workspace {} is still being used by API subscription. Reconfigure the subscriptions before deleting the workspace.".format(workspaceName), 409
    AMLWorkspace.Delete(workspaceName)
    return jsonify({}), 204

@app.route('/api/management/subscriptions/<subscriptionId>/availablePlans', methods=['GET'])
def getAvailablePlans(subscriptionId):
    return jsonify({
        'plans': ['Basic', 'Premium']
        }), 200

@app.route('/')
@app.route('/home')
def home():
    """Renders the home page."""
    return jsonify("This is an API Service.")