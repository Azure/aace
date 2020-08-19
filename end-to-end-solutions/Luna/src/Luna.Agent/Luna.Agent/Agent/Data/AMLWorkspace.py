from sqlalchemy import Column, Integer, String
from Agent import Base, Session
from Agent.AzureML.AzureMLUtils import AzureMLUtils

class AMLWorkspace(Base):
    """description of class"""
    
    __tablename__ = 'agent_amlworkspaces'

    Id = Column(Integer, primary_key = True)

    WorkspaceName = Column(String)

    ResourceId = Column(String)

    AADApplicationId = Column(String)

    AADTenantId = Column(String)

    AADApplicationSecretName = Column(String)

    AADApplicationSecret = Column(String)

    Region = Column(String)

    ComputeClusters = []

    DeploymentClusters = []

    DeploymentTargetTypes = []

    @staticmethod
    def Create(workspace):
        session = Session()
        session.add(workspace)
        session.commit()
        return

    def Update(workspace):
        session = Session()
        dbWorkspace = session.query(AMLWorkspace).filter_by(WorkspaceName = workspace.WorkspaceName).first()
        dbWorkspace.WorkspaceName = workspace.WorkspaceName
        dbWorkspace.AADApplicationId = workspace.AADApplicationId
        if workspace.AADApplicationSecret != "":
            dbWorkspace.AADApplicationSecret = workspace.AADApplicationSecret
        dbWorkspace.AADTenantId = workspace.AADTenantId
        session.commit()
        # update
        return

    @staticmethod
    def Get(workspaceName):
        session = Session()
        workspace = session.query(AMLWorkspace).filter_by(WorkspaceName = workspaceName).first()
        util = AzureMLUtils(workspace)
        workspace.ComputeClusters = util.getComputeClusters()
        workspace.DeploymentClusters = util.getDeploymentClusters()
        workspace.AADApplicationSecret = ""
        workspace.DeploymentTargetTypes = [{
                'id': 'aks',
                'displayName': 'Azure Kubernates Service'
            },
            {
                'id': 'aci',
                'displayName': 'Azure Container Instances'
            }]
        session.close()
        return workspace

    def GetById(workspaceId):
        session = Session()
        workspace = session.query(AMLWorkspace).filter_by(Id = workspaceId).first()
        session.close()
        return workspace

    @staticmethod
    def Exist(workspaceName):
        session = Session()
        return len(session.query(AMLWorkspace).filter_by(WorkspaceName = workspaceName).all()) > 0

    @staticmethod
    def ListAll():
        session = Session()
        workspaces = session.query(AMLWorkspace).all()
        for workspace in workspaces:
            workspace.AADApplicationSecret = ""
        return workspaces

    @staticmethod
    def Delete(workspaceName):
        session = Session()
        workspace = session.query(AMLWorkspace).filter_by(WorkspaceName = workspaceName).first()
        session.delete(workspace)
        session.commit()
        return
