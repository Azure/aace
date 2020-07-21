from abc import ABCMeta, abstractmethod

import yaml
import io
import argparse
import json

class BaseLunaUtils(object):
    
    __metaclass__ = ABCMeta

    def ParseArguments(self):
        return self._args, self._userInput

    def Init(self, luna_config, run_mode, args, userInput):
        self._luna_config = luna_config
        self._run_mode = run_mode
        self._args= args
        self._userInput = userInput
        self._logger = None

    @property
    def luna_config(self):
        return self._luna_config

    @property
    def run_mode(self):
        return self.run_mode

    @property
    def args(self):
        return self._args

    @property
    def user_input(self):
        return self._userInput

    @property
    def logger(self):
        return self._logger

    @abstractmethod
    def RegisterModel(self, model_path, description, luna_python_model=None):
        """
        register model
        """

    @abstractmethod
    def DeployModel(self):
        """
        deploy model
        """

    @abstractmethod
    def DownloadModel(self, model_path):
        """
        download model
        """