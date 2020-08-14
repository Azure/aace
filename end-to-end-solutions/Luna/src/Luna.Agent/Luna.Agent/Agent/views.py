"""
Routes and views for the flask application.
"""

from datetime import datetime
from flask import render_template
from Agent import app, controlPlane
from flask import jsonify, request
from Agent.Code.CodeUtils import CodeUtils
from Agent.AzureML.AzureMLUtils import AzureMLUtils
from datetime import datetime
from uuid import uuid4
import pathlib
from Agent.Data.APISubscription import APISubscription
from Agent.Data.APIVersion import APIVersion
from sqlalchemy.orm import sessionmaker
from Agent import engine, Session
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential
from Agent.Data.AMLWorkspace import AMLWorkspace
import json


@app.route('/api/agentinfo')
def getAgentInfo():
    return jsonify({'name':'myagent', 'key':'mykey'})

@app.route('/api/<subscriptionId>/<operationVerb>', methods=['POST'])
def executeOperation(subscriptionId, operationVerb):
    session = Session()
    sub = session.query(APISubscription).filter_by(SubscriptionId=subscriptionId).first()

    apiVersion = request.args.get('api-version')

    version = session.query(APIVersion).filter_by(DeploymentName=sub.DeploymentName, ProductName=sub.ProductName, VersionName = apiVersion).first()

    if version.VersionSourceType == 'git':
        working_dir = CodeUtils.getLocalCodeFolder(subscriptionId, sub.ProductName, sub.DeploymentName, apiVersion, datetime.utcnow(), pathlib.Path(__file__).parent.absolute())

    workspace = session.query(AMLWorkspace).filter_by(Id = version.AMLWorkspaceId).first()
    amlUtil = AzureMLUtils(workspace)
    opId = amlUtil.runProject(sub.ProductName, sub.DeploymentName, apiVersion, operationVerb, json.dumps(request.json), '', sub.UserId, sub.SubscriptionId)
    return jsonify({'operationId': opId})

@app.route('/api/<subscriptionId>/operations/<operationVerb>/<operationId>', methods=['GET'])
def getOperationStatus(subscriptionId, operationVerb, operationId):
    
    session = Session()
    
    apiVersion = request.args.get('api-version')
    userId = request.args.get('userId')
    
    sub = session.query(APISubscription).filter_by(SubscriptionId=subscriptionId).first()

    version = session.query(APIVersion).filter_by(DeploymentName=sub.DeploymentName, ProductName=sub.ProductName, VersionName = apiVersion).first()

    workspace = session.query(AMLWorkspace).filter_by(Id = version.AMLWorkspaceId).first()
    amlUtil = AzureMLUtils(workspace)
    result = amlUtil.getOperationStatus(operationVerb, operationId, userId, subscriptionId)
    return jsonify(result)

@app.route('/api/<subscriptionId>/operations/<operationVerb>', methods=['GET'])
def listOperations(subscriptionId, operationVerb):
    
    session = Session()
    
    apiVersion = request.args.get('api-version')
    userId = request.args.get('userId')
    
    sub = session.query(APISubscription).filter_by(SubscriptionId=subscriptionId).first()

    version = session.query(APIVersion).filter_by(DeploymentName=sub.DeploymentName, ProductName=sub.ProductName, VersionName = apiVersion).first()

    workspace = session.query(AMLWorkspace).filter_by(Id = version.AMLWorkspaceId).first()
    amlUtil = AzureMLUtils(workspace)
    result = amlUtil.listAllOperations(operationVerb, userId, subscriptionId)
    return jsonify(result)

@app.route('/api/<subscriptionId>/<operationNoun>', methods=['GET'])
def listOperationOutputs(subscriptionId, operationNoun):
    
    session = Session()
    
    apiVersion = request.args.get('api-version')
    userId = request.args.get('userId')
    
    sub = session.query(APISubscription).filter_by(SubscriptionId=subscriptionId).first()

    version = session.query(APIVersion).filter_by(DeploymentName=sub.DeploymentName, ProductName=sub.ProductName, VersionName = apiVersion).first()

    workspace = session.query(AMLWorkspace).filter_by(Id = version.AMLWorkspaceId).first()
    amlUtil = AzureMLUtils(workspace)
    result = amlUtil.listAllOperationOutputs(operationNoun, userId, subscriptionId)
    return jsonify(result)

@app.route('/api/<subscriptionId>/<operationNoun>/<operationId>', methods=['GET'])
def getOperationOutput(subscriptionId, operationNoun, operationId):
    
    session = Session()
    
    apiVersion = request.args.get('api-version')
    userId = request.args.get('userId')
    
    sub = session.query(APISubscription).filter_by(SubscriptionId=subscriptionId).first()

    version = session.query(APIVersion).filter_by(DeploymentName=sub.DeploymentName, ProductName=sub.ProductName, VersionName = apiVersion).first()

    workspace = session.query(AMLWorkspace).filter_by(Id = version.AMLWorkspaceId).first()
    amlUtil = AzureMLUtils(workspace)
    result = amlUtil.getOperationOutput(operationNoun, operationId, userId, subscriptionId)
    return jsonify(result)

@app.route('/api/<subscriptionId>/<parentOperationNoun>/<parentOperationId>/<operationVerb>', methods=['POST'])
def executeChildOperation(subscriptionId, parentOperationNoun, parentOperationId, operationVerb):
    return jsonify({})

@app.route('/api/<subscriptionId>/<operationNoun>/<operationId>', methods=['DELETE'])
def deleteOperationOutput(subscriptionId, operationNoun, operationId):
    return jsonify({})


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
    return jsonify({
        "subscriptions": [{
            'subscriptionId': '075FCBB4-C2D0-433F-80D2-A3E95006B0BB',
            'subscriptionName' : 'myfirstsub',
            'offerName': 'NLP',
            'planName': 'Basic',
            'hostType': 'selfhost',
            'subscribedTime': '08/05/2020',
            'rootUrl': 'https://10.10.10.10/myfirstsub'
            },
            {
            'subscriptionId': 'DDEC25FA-0F56-4F0E-8DD5-BCDB49409361',
            'subscriptionName' : 'mysecondsub',
            'offerName': 'EDDI',
            'planName': 'Premium',
            'hostType': 'selfhost',
            'subscribedTime': '08/04/2020',
            'rootUrl': 'https://10.10.10.10/mysecondsub'
            }]
        }), 200

@app.route('/api/management/subscriptions/<subscriptionId>', methods=['GET'])
def getSubscription(subscriptionId):
    return jsonify({
        'subscriptionId': subscriptionId,
        'subscriptionName' : 'myfirstsub',
        'offerName': 'NLP',
        'planName': 'Basic',
        'hostType': 'selfhost',
        'subscribedTime': '08/05/2020',
        'rootUrl': 'https://10.10.10.10/myfirstsub',
        'defaultSettings': {
            'amlWorkspace': 'myamlworkspace',
            'computeCluster': 'mycomputecluster',
            'deploymentTargetType': 'aks',
            'aksCluster': 'myakscluster'
            }
        }), 200

@app.route('/api/management/subscriptions/<subscriptionId>', methods=['PUT'])
def createOrUpdateSubscription(subscriptionId):
    return jsonify({}), 202

@app.route('/api/management/subscriptions/<subscriptionId>/users', methods=['GET'])
def listAllSubscriptionUsers(subscriptionId):
    return jsonify({
        'users': [{
            'Name': 'Xiaochen Wu',
            'Id': 'xiwu@microsoft.com'
            },
            {
            'Name': 'Ryan Mott',
            'Id': 'rymott@microsoft.com'
            }]
        }), 200

@app.route('/api/management/subscriptions/<subscriptionId>/users/<userId>', methods=['GET'])
def getSubscriptionUser(subscriptionId, userId):
    return jsonify({
            'Name': 'Xiaochen Wu',
            'Id': 'xiwu@microsoft.com'
            }), 200

@app.route('/api/management/subscriptions/<subscriptionId>/users/<userId>', methods=['PUT'])
def addSubscriptionUser(subscriptionId, userId):
    return jsonify({}), 202

@app.route('/api/management/subscriptions/<subscriptionId>/users/<userId>', methods=['DELETE'])
def removeSubscriptionUser(subscriptionId, userId):
    return jsonify({}), 204


@app.route('/api/management/admins', methods=['GET'])
def listAllAdmins():
    return jsonify({
        'admins': [{
            'Name': 'Echo Wang',
            'Id': 'echowan@microsoft.com'
            },
            {
            'Name': 'Chad Adams',
            'Id': 'chadada@microsoft.com'
            }]
        }), 200

@app.route('/api/management/admins/<userId>', methods=['GET'])
def getAdmin(userId):
    return jsonify({
            'Name': 'Chad Adams',
            'Id': 'chadada@microsoft.com'
        }), 200

@app.route('/api/management/admins/<userId>', methods=['PUT'])
def addAdmin(userId):
    return jsonify({}), 202

@app.route('/api/management/admins/<userId>', methods=['DELETE'])
def removeAdmin(userId):
    return jsonify({}), 204

@app.route('/api/management/amlworkspaces', methods=['GET'])
def listAllAMLWorkspaces():
    return jsonify({
        'workspaces': [{
            'name': 'myamlworkspace',
            'resourceId': '/subscriptions/a6c2a7cc-d67e-4a1a-b765-983f08c0423a/resourceGroups/xiwutest/providers/Microsoft.MachineLearningServices/workspaces/xiwutest',
            'aadApplicationTenantId': '6617475A-B4B7-4578-B8C7-CFEE6A75931B',
            'aadApplicationClientId': 'F7E74366-837D-4EB2-B5FD-29948CD84BC0',
            'aadApplicationClientSecret': 'myclientsecret'
            }
            ]
        }), 200

@app.route('/api/management/amlworkspaces/<workspaceName>', methods=['GET'])
def getAMLWorkspace(workspaceName):
    return jsonify({
            'name': 'myamlworkspace',
            'resourceId': '/subscriptions/a6c2a7cc-d67e-4a1a-b765-983f08c0423a/resourceGroups/xiwutest/providers/Microsoft.MachineLearningServices/workspaces/xiwutest',
            'aadApplicationTenantId': '6617475A-B4B7-4578-B8C7-CFEE6A75931B',
            'aadApplicationClientId': 'F7E74366-837D-4EB2-B5FD-29948CD84BC0',
            'aadApplicationClientSecret': 'myclientsecret',
            'computeClusters': ['mycomputecluster', 'testcomputecluster'],
            'aksClusters': ['myakscluster', 'testakscluster']
            }), 200

@app.route('/api/management/amlworkspaces/<workspaceName>', methods=['PUT'])
def createOrUpdateAMLWorkspace(workspaceName):
    return jsonify({}), 202

@app.route('/api/management/amlworkspaces/<workspaceName>', methods=['DELETE'])
def deleteAMLWorkspace(workspaceName):
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