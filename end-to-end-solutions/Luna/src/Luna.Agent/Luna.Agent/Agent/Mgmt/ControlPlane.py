import requests
from Agent.Data.APISubscription import APISubscription
from Agent.Data.Publisher import Publisher

GET_PROJECT_FILE_URL_URL_FORMAT = "{base_url}/api/aiagents/{agent_id}/subscriptions/{subscription_id}/projectFileUrl/{version_name}"
GET_AGENT_SUBSCRIPTIONS_URL_FORMAT = "{base_url}/api/aiagents/{agent_id}/subscriptions"
GET_AGENT_APIVERSIONS_URL_FORMAT = "{base_url}/api/aiagents/{agent_id}/apiVersions"

class ControlPlane(object):
    """description of class"""

    def __init__(self, controlPlaneUrl, agentId, agentKey):
        self._controlPlaneUrl = controlPlaneUrl
        self._agentId = agentId
        self._agentKey = agentKey
        return

    def GetProjectFileUrl(self, subscriptionId, versionName):
        requestUrl = GET_PROJECT_FILE_URL_URL_FORMAT.format(base_url=self._controlPlaneUrl, agent_id=self._agentId, subscription_id=subscriptionId, version_name=versionName)
        response = requests.get(requestUrl, headers=self.GetAuthHeader())
        if response.status_code == 200:
            url = response.text
        return url

    def GetAgentSubscriptions(self):
        requestUrl = GET_AGENT_SUBSCRIPTIONS_URL_FORMAT.format(base_url=self._controlPlaneUrl, agent_id=self._agentId)
        response = requests.get(requestUrl, headers=self.GetAuthHeader())
        if response.status_code == 200:
            subscriptions = response.json()

        return subscriptions

    def GetAgentSubscriptionsFromControlPlane(self, controlPlaneUrl):
        requestUrl = GET_AGENT_SUBSCRIPTIONS_URL_FORMAT.format(base_url=controlPlaneUrl, agent_id=self._agentId)
        response = requests.get(requestUrl, headers=self.GetAuthHeader())
        if response.status_code == 200:
            subscriptions = response.json()

        return subscriptions

    def GetAgentAPIVersions(self):
        requestUrl = GET_AGENT_APIVERSIONS_URL_FORMAT.format(base_url=self._controlPlaneUrl, agent_id=self._agentId)
        response = requests.get(requestUrl, headers=self.GetAuthHeader())
        if response.status_code == 200:
            apiversions = response.json()

        return apiversions

    def GetAuthHeader(self):
        return {"Authorization": self._agentKey}

    def UpdateMetadataDatabase(self):
        publishers = Publisher.ListAll()
        for publisher in publishers:
            subscriptions = self.GetAgentSubscriptionsFromControlPlane(publisher.ControlPlaneUrl)
            APISubscription.MergeWithDelete(subscriptions, publisher.PublisherId)



