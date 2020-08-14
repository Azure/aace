from sqlalchemy import Column, Integer, String
from Agent import Base

class APIVersion(Base):
    """description of class"""
    
    __tablename__ = 'agent_apiversions'

    Id = Column(Integer, primary_key = True)

    DeploymentName = Column(String)

    ProductName = Column(String)

    VersionName = Column(String)

    RealTimePredictAPI = Column(String)

    TrainModelAPI = Column(String)
    
    BatchInferenceAPI = Column(String)

    DeployModelAPI = Column(String)

    AuthenticationType = Column(String)

    CreatedTime = Column(String)

    LastUpdatedTime = Column(String)

    VersionSourceType = Column(String)

    ProjectFileUrl = Column(String)

    AMLWorkspaceId = Column(Integer)

