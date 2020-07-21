from luna.logging.baseLunaLogger import BaseLunaLogger

class LocalLunaLogger(BaseLunaLogger):
    
    def log_metric(self, key, value):
        """
        log numeric metric
        """

    def log_metrics(self, metrics):
        """
        log numeric metrics
        """

    def upload_artifacts(self, local_path, upload_path):
        """
        upload artifacts (files) to the run
        """