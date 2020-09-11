import os
import tempfile
import requests
import zipfile
import shutil

from Agent.Mgmt.ControlPlane import ControlPlane
from Agent import app
from datetime import datetime, timedelta

class CodeUtils(object):
    """The class provides Git utilitiy functions"""
    
    @staticmethod
    def getLocalCodeFolder(subscriptionId, productName, deploymentName, version, baseDir, controlPlaneUrl):
        
        codeDir = os.path.join(baseDir, 'lunaCode', productName, deploymentName, version.VersionName)
        refreshNeeded = False
        lastUpdatedTimeFile = os.path.join(codeDir, 'code', 'LastUpdatedTime')
        if os.path.isfile(lastUpdatedTimeFile):
            with open(lastUpdatedTimeFile) as file:
                lastUpdatedTimeStr = file.read()
                try:
                    lastUpdatedTime = datetime.strptime(lastUpdatedTimeStr, "%Y-%m-%d %H:%M:%S.%f")
                    if lastUpdatedTime < version.LastUpdatedTime:
                        refreshNeeded = True
                except Exception as e:
                    refreshNeeded = True

        if refreshNeeded:
            shutil.rmtree(codeDir)

        if refreshNeeded or (not os.path.isfile(os.path.join(codeDir, 'code', 'MLproject'))):
            CodeUtils.downloadAndUnzipToLocal(subscriptionId, version.VersionName, codeDir, controlPlaneUrl)
            with open(lastUpdatedTimeFile, 'w') as file:
                file.write(str(version.LastUpdatedTime))
        
        os.chdir(os.path.join(codeDir, 'code'))
        return

    @staticmethod
    def downloadAndUnzipToLocal(subscriptionId, apiVersion, codeDir, controlPlaneUrl):
        if not os.path.exists(codeDir):
            os.makedirs(codeDir)
        zipFileName = os.path.join(codeDir, 'code.zip')
        
        controlPlane = ControlPlane(os.environ['AGENT_ID'], os.environ['AGENT_KEY'])
        url = controlPlane.GetProjectFileUrl(subscriptionId, apiVersion, controlPlaneUrl)
        response = requests.get(url, allow_redirects=True)

        if response.status_code == 200:
            open(zipFileName, 'wb').write(response.content)

        zipFile = zipfile.ZipFile(zipFileName)
        
        # This will handle zip file with a folder or with all files
        try:
            zipFile.getinfo("MLproject")
            codeDir =  os.path.join(codeDir, 'code')
            zipFile.extractall(codeDir)
            zipFile.close()
            os.remove(zipFileName)
            return codeDir
        except KeyError:
            zipFile.extractall(codeDir)
            zipFile.close()
            os.remove(zipFileName)
            for f in os.scandir(codeDir):
                if f.is_dir() and f.name != 'code':
                    os.rename(f.path, os.path.join(codeDir, 'code'))

            return os.path.join(codeDir, 'code')


