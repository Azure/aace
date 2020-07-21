from luna.baseLunaUtils import BaseLunaUtils
from luna.logging.localLunaLogger import LocalLunaLogger
import os

class LocalLunaUtils(BaseLunaUtils):
    
    def Init(self, luna_config, run_mode, args, userInput):
        super().Init(luna_config, run_mode, args, userInput)
        self._logger = LocalLunaLogger()
        
    def RegisterModel(self, model_path, description, luna_python_model=None):
        # do nothing
        return
    
    def GetModelPath(self, context):
        return os.getcwd()

    
    def DeployModel(self):
        return
        
    def DownloadModel(self, model_path="models"):
        return os.path.join(os.getcwd(), model_path)