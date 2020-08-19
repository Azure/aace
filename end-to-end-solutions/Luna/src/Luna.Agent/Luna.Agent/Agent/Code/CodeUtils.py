import os
import tempfile
import requests
import zipfile
import shutil

from Agent.Mgmt.ControlPlane import ControlPlane
from Agent import app

class CodeUtils(object):
    """The class provides Git utilitiy functions"""
    
    @staticmethod
    def getLocalCodeFolder(subscriptionId, productName, deploymentName, apiVersion, lastUpdatedTime, baseDir):
        
        codeDir = os.path.join(baseDir, 'lunaCode', productName, deploymentName, apiVersion)
        if not os.path.isfile(os.path.join(codeDir, 'code', 'MLProject')):
            CodeUtils.downloadAndUnzipToLocal(subscriptionId, apiVersion, codeDir)
        
        os.chdir(os.path.join(codeDir, 'code'))
        return

    @staticmethod
    def downloadAndUnzipToLocal(subscriptionId, apiVersion, codeDir):
        
        zipFileName = os.path.join(codeDir, 'code.zip')
        
        controlPlane = ControlPlane(app.config['CONTROL_PLANE_URL'], app.config['AGENT_ID'], app.config['AGENT_KEY'])
        url = controlPlane.GetProjectFileUrl(subscriptionId, apiVersion)
        response = requests.get(url, allow_redirects=True)

        if response.status_code == 200:
            open(zipFileName, 'wb').write(response.content)

        zipFile = zipfile.ZipFile(zipFileName)
        zipFile.extractall(codeDir)
        zipFile.close()
        os.remove(zipFileName)

        for f in os.scandir(codeDir):
           if f.is_dir():
               os.rename(f.path, os.path.join(codeDir, 'code'))

        return os.path.join(codeDir, 'code')


