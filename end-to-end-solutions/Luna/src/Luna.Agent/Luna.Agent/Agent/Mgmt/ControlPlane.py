import requests

GET_PROJECT_FILE_URL_URL_FORMAT = "{base_url}/api/aiagents/{agent_id}/subscriptions/{subscription_id}/projectFileUrl/{version_name}"

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

    def GetAuthHeader(self):
        return {"Authorization": self._agentKey}


