from abc import ABCMeta, abstractmethod


class BaseLunaLogger(object):
    
    __metaclass__ = ABCMeta

    @abstractmethod
    def log_metric(self, key, value):
        """
        log numeric metric
        """

    @abstractmethod
    def log_metrics(self, metrics):
        """
        log numeric metrics
        """

    @abstractmethod
    def upload_artifacts(self, local_path, upload_path):
        """
        upload artifacts (files) to the run
        """