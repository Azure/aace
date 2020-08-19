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
from Agent import engine, Session, app
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential
from Agent.Data.AMLWorkspace import AMLWorkspace
from Agent.Data.AgentUser import AgentUser
import json


@app.route('/api/agentinfo')
def getAgentInfo():
    return jsonify({'name':'myagent', 'key':'mykey'})

def getSubscriptionAPIVersionAndWorkspace(subscriptionId, apiVersion):
    sub = APISubscription.Get(subscriptionId)
    version = APIVersion.Get(sub.ProductName, sub.DeploymentName, apiVersion)
    if app.config["AGENT_MODE"] == "SAAS":
        workspace = AMLWorkspace.GetById(version.AMLWorkspaceId)
    elif app.config["AGENT_MODE"] == "LOCAL":
        workspace = AMLWorkspace.GetById(sub.AMLWorkspaceId)

    return sub, version, workspace

@app.route('/api/<subscriptionId>/<operationVerb>', methods=['POST'])
def executeOperation(subscriptionId, operationVerb):
    
    apiVersion = request.args.get('api-version')
    userId = request.args.get('userId')
    sub, version, workspace = getSubscriptionAPIVersionAndWorkspace(subscriptionId, apiVersion)

    amlUtil = AzureMLUtils(workspace)
    if version.VersionSourceType == 'git':
        working_dir = CodeUtils.getLocalCodeFolder(subscriptionId, sub.ProductName, sub.DeploymentName, apiVersion, datetime.utcnow(), pathlib.Path(__file__).parent.absolute())
        opId = amlUtil.runProject(sub.ProductName, sub.DeploymentName, apiVersion, operationVerb, json.dumps(request.json), 'na', sub.UserId, sub.SubscriptionId)
    elif version.VersionSourceType == 'amlPipeline':
        url = None
        if operationVerb == 'train':
            url = version.TrainModelAPI
        elif operationVerb == 'inference':
            url = version.BatchInferenceAPI
        elif operationVerb == 'deploy':
            url = version.DeployModelAPI

        if url and url != "":
            opId = amlUtil.submitPipelineRun(url, sub.ProductName, sub.DeploymentName, apiVersion, operationVerb, json.dumps(request.json), '', sub.UserId, sub.SubscriptionId)
        else:
            return 'The operation {} is not supported'.format(operationVerb)
    
    return jsonify({'operationId': opId})

@app.route('/api/<subscriptionId>/operations/<operationVerb>/<operationId>', methods=['GET'])
def getOperationStatus(subscriptionId, operationVerb, operationId):
    
    apiVersion = request.args.get('api-version')
    userId = request.args.get('userId')
    sub, version, workspace = getSubscriptionAPIVersionAndWorkspace(subscriptionId, apiVersion)

    CodeUtils.getLocalCodeFolder(subscriptionId, sub.ProductName, sub.DeploymentName, apiVersion, datetime.utcnow(), pathlib.Path(__file__).parent.absolute())
    amlUtil = AzureMLUtils(workspace)
    result = amlUtil.getOperationStatus(operationVerb, operationId, userId, subscriptionId)
    return jsonify(result)

@app.route('/api/<subscriptionId>/operations/<operationVerb>', methods=['GET'])
def listOperations(subscriptionId, operationVerb):
    
    apiVersion = request.args.get('api-version')
    userId = request.args.get('userId')
    
    sub, version, workspace = getSubscriptionAPIVersionAndWorkspace(subscriptionId, apiVersion)
    CodeUtils.getLocalCodeFolder(subscriptionId, sub.ProductName, sub.DeploymentName, apiVersion, datetime.utcnow(), pathlib.Path(__file__).parent.absolute())
    amlUtil = AzureMLUtils(workspace)
    result = amlUtil.listAllOperations(operationVerb, userId, subscriptionId)
    return jsonify(result)

@app.route('/api/<subscriptionId>/<operationNoun>', methods=['GET'])
def listOperationOutputs(subscriptionId, operationNoun):
    
    apiVersion = request.args.get('api-version')
    userId = request.args.get('userId')
    
    sub, version, workspace = getSubscriptionAPIVersionAndWorkspace(subscriptionId, apiVersion)
    CodeUtils.getLocalCodeFolder(subscriptionId, sub.ProductName, sub.DeploymentName, apiVersion, datetime.utcnow(), pathlib.Path(__file__).parent.absolute())

    amlUtil = AzureMLUtils(workspace)
    result = amlUtil.listAllOperationOutputs(operationNoun, userId, subscriptionId)
    return jsonify(result)

@app.route('/api/<subscriptionId>/<operationNoun>/<operationId>', methods=['GET'])
def getOperationOutput(subscriptionId, operationNoun, operationId):
    
    apiVersion = request.args.get('api-version')
    userId = request.args.get('userId')
    
    sub, version, workspace = getSubscriptionAPIVersionAndWorkspace(subscriptionId, apiVersion)
    CodeUtils.getLocalCodeFolder(subscriptionId, sub.ProductName, sub.DeploymentName, apiVersion, datetime.utcnow(), pathlib.Path(__file__).parent.absolute())

    amlUtil = AzureMLUtils(workspace)
    result = amlUtil.getOperationOutput(operationNoun, operationId, userId, subscriptionId)
    return jsonify(result)




@app.route('/api/<subscriptionId>/<parentOperationNoun>/<parentOperationId>/<operationVerb>', methods=['POST'])
def executeChildOperation(subscriptionId, parentOperationNoun, parentOperationId, operationVerb):
    return jsonify({})

@app.route('/api/<subscriptionId>/<operationNoun>/<operationId>', methods=['DELETE'])
def deleteOperationOutput(subscriptionId, operationNoun, operationId):
    return jsonify({})


@app.route('/api/management/refreshMetadata', methods=['POST'])
def refreshMetadata():
    controlPlane = ControlPlane(app.config['CONTROL_PLANE_URL'], app.config['AGENT_ID'], app.config['AGENT_KEY'])
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