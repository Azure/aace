from sqlalchemy import Column, Integer, String
from Agent import Base

class AMLWorkspace(Base):
    """description of class"""
    
    __tablename__ = 'AMLWorkspaces'

    Id = Column(Integer, primary_key = True)

    WorkspaceName = Column(String)

    ResourceId = Column(String)

    AADApplicationId = Column(String)

    AADTenantId = Column(String)

    AADApplicationSecretName = Column(String)

    Region = Column(String)


