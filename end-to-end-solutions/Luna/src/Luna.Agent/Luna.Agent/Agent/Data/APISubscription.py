from sqlalchemy import Column, Integer, String, DateTime, or_
from Agent import Base, Session, app, key_vault_helper
from Agent.Data.AMLWorkspace import AMLWorkspace
from Agent.Data.AgentUser import AgentUser
import os

class APISubscription(Base):
    """description of class"""

    __tablename__ = 'agent_subscriptions'

    Id = Column(Integer)

    SubscriptionId = Column(String, primary_key = True)

    DeploymentName = Column(String)

    ProductName = Column(String)

    ProductType = Column(String)

    UserId = Column(String)

    SubscriptionName = Column(String)

    Status = Column(String)

    HostType = Column(String)

    CreatedTime = Column(DateTime)

    BaseUrl = Column(String)

    PrimaryKeySecretName = Column(String)

    SecondaryKeySecretName = Column(String)

    AMLWorkspaceId = Column(Integer)

    AMLWorkspaceComputeClusterName = Column(String)

    AMLWorkspaceDeploymentTargetType = Column(String)

    AMLWorkspaceDeploymentClusterName = Column(String)

    AgentId = Column(String)

    PublisherId = Column(String)
    
    OfferName = Column(String)

    PlanName = Column(String)

    AMLWorkspaceName = ""

    AvailablePlans = []

    Users = []

    Admins = []

    PrimaryKey = ""

    SecondaryKey = ""
    
    @staticmethod
    def Update(subscription):
        session = Session()
        dbSubscription = session.query(APISubscription).filter_by(SubscriptionId = subscription.SubscriptionId).first()
        workspace = AMLWorkspace.Get(subscription.AMLWorkspaceName)
        dbSubscription.AMLWorkspaceId = workspace.Id
        dbSubscription.AMLWorkspaceComputeClusterName = subscription.AMLWorkspaceComputeClusterName
        dbSubscription.AMLWorkspaceDeploymentTargetType = subscription.AMLWorkspaceDeploymentTargetType
        dbSubscription.AMLWorkspaceDeploymentClusterName = subscription.AMLWorkspaceDeploymentClusterName
        session.commit()
        session.close()
        # update
        return

    @staticmethod
    def Get(subscriptionId):
        """ the function will should only be called in local mode, otherwise, the keys might be out of date! """
        session = Session()
        subscription = session.query(APISubscription).filter_by(SubscriptionId = subscriptionId).first()
        session.close()
        subscription.PrimaryKey = key_vault_helper.get_secret(subscription.PrimaryKeySecretName)
        subscription.SecondaryKey = key_vault_helper.get_secret(subscription.SecondaryKeySecretName)
        if os.environ["AGENT_MODE"] == "LOCAL":
            subscription.Admins = AgentUser.ListAllAdmin()
            subscription.Users = AgentUser.ListAllBySubscriptionId(subscriptionId)
            subscription.AvailablePlans = ["Basic", "Premium"]
        return subscription

    @staticmethod
    def GetByKey(subscriptionKey):
        session = Session()
        secret_name = key_vault_helper.find_secret_name_by_value(subscriptionKey)
        if secret_name:
            subscription = session.query(APISubscription).filter(or_(APISubscription.PrimaryKeySecretName == secret_name, APISubscription.SecondaryKeySecretName == secret_name)).first()            
        else:
            subscription = None
        session.close()
        return subscription

    @staticmethod
    def ListAllByWorkspaceName(workspaceName):
        session = Session()
        workspace = AMLWorkspace.Get(workspaceName)
        subscriptions = session.query(APISubscription).filter_by(AMLWorkspaceId = workspace.Id).all()
        session.close()
        return subscriptions

    @staticmethod
    def ListAll():
        session = Session()
        subscriptions = session.query(APISubscription).all()
        session.close()
        return subscriptions

    @staticmethod
    def MergeWithDelete(subscriptions, publisherId):
        session = Session()
        try:
            dbSubscriptions = session.query(APISubscription).all()
            for dbSubscription in dbSubscriptions:
                if dbSubscription.PublisherId.lower() != publisherId.lower():
                    continue;
                # If the subscription is removed in the control plane, remove it from the agent
                try:
                    next(item for item in subscriptions if item["SubscriptionId"].lower() == dbSubscription.SubscriptionId.lower() and item["PublisherId"].lower() == dbSubscription.PublisherId.lower())
                except StopIteration:
                    session.delete(dbSubscription)

            for subscription in subscriptions:
                dbSubscription = APISubscription(**subscription)
                dbSubscription.PrimaryKeySecretName = 'primarykey-{}'.format(dbSubscription.SubscriptionId)
                dbSubscription.SecondaryKeySecretName = 'secondarykey-{}'.format(dbSubscription.SubscriptionId)
                key_vault_helper.set_secret(dbSubscription.PrimaryKeySecretName, dbSubscription.PrimaryKey)
                key_vault_helper.set_secret(dbSubscription.SecondaryKeySecretName, dbSubscription.SecondaryKey)
                session.merge(dbSubscription)

            session.commit()
        except:
            session.rollback()
            raise

        finally:
            session.close()
