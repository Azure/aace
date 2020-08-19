from sqlalchemy import Column, Integer, String
from Agent import Base, Session

class AgentUser(Base):
    """description of class"""
    
    __tablename__ = 'agent_users'

    Id = Column(Integer, primary_key = True)

    AADUserId = Column(String)

    Description = Column(String)

    Role = Column(String)

    SubscriptionId = Column(String)

    @staticmethod
    def Create(user):
        session = Session()
        session.add(user)
        session.commit()
        return

    @staticmethod
    def ListAllBySubscriptionId(subscriptionId):
        session = Session()
        users = session.query(AgentUser).filter_by(SubscriptionId = subscriptionId).all()
        session.close()
        return users
    
    @staticmethod
    def GetUser(subscriptionId, userId):
        session = Session()
        users = session.query(AgentUser).filter_by(SubscriptionId = subscriptionId, AADUserId = userId).first()
        session.close()
        return users

    @staticmethod
    def ListAllAdmin():
        session = Session()
        users = session.query(AgentUser).filter_by(Role = "Admin").all()
        session.close()
        return users
    
    @staticmethod
    def GetAdmin(userId):
        session = Session()
        users = session.query(AgentUser).filter_by(AADUserId = userId, Role="Admin").first()
        session.close()
        return users

    @staticmethod
    def DeleteUser(subscriptionId, userId):
        session = Session()
        user = AgentUser.GetUser(subscriptionId, userId)
        session.delete(user)
        session.commit()
        session.close()
        return

    @staticmethod
    def DeleteAdmin(userId):
        session = Session()
        admin = AgentUser.GetAdmin(userId)
        session.delete(admin)
        session.commit()
        session.close()
        return