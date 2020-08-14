from sqlalchemy import Column, Integer, String
from Agent import Base

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

    CreatedTime = Column(String)

    BaseUrl = Column(String)

    PrimaryKey = Column(String)

    SecondaryKey = Column(String)