import yaml
import io
import argparse
import json
import os
import requests
import time


TRAINING_URL_FORMAT = "{base_url}/train?api-version={api_version}"
BATCHINFERENCE_URL_FORMAT = "{base_url}/models/{model_id}/batchinference?api-version={api_version}"
DEPLOY_URL_FORMAT = "{base_url}/models/{model_id}/deploy?api-version={api_version}"
GET_TRAINING_OP_URL_FORMAT = "{base_url}/operations/training/{model_id}?api-version={api_version}"
GET_INFERENCE_OP_URL_FORMAT = "{base_url}/operations/inference/{operation_id}?api-version={api_version}"
GET_DEPLOYMENT_OP_URL_FORMAT = "{base_url}/operations/deployment/{endpoint_id}?api-version={api_version}"

GET_MODEL_URL_FORMAT = "{base_url}/models/{model_id}?api-version={api_version}"
GET_ENDPOINT_URL_FORMAT = "{base_url}/endpoints/{endpoint_id}?api-version={api_version}"

class LunaClient(object):
    
    def __init__(self, base_url, key, api_version):
        self._base_url = base_url
        self._key = key
        self._api_version = api_version


    def get_request_header(self):
        return {"Accept": "application/json", "Ocp-Apim-Subscription-Key": self._key}

    def train_model(self, user_input):
        training_url = TRAINING_URL_FORMAT.format(base_url=self._base_url, api_version=self._api_version)
        response = requests.post(training_url, headers=self.get_request_header(), data=json.dumps(user_input))

        if response.status_code == 200:
            return response.json()['modelId']

        return None

    def get_training_operation(self, model_id):
        train_op_url = GET_TRAINING_OP_URL_FORMAT.format(base_url = self._base_url, model_id=model_id, api_version=self._api_version)
        response = requests.get(train_op_url, headers=self.get_request_header())
        if response.status_code == 200:
            return response.json()

        return None
        
    def get_batch_inference_operation(self, operation_id):
        inference_op_url = GET_INFERENCE_OP_URL_FORMAT.format(base_url = self._base_url, operation_id=operation_id, api_version=self._api_version)
        response = requests.get(inference_op_url, headers=self.get_request_header())
        if response.status_code == 200:
            return response.json()

        return None
    
    def get_deployment_operation(self, endpoint_id):
        inference_op_url = GET_DEPLOYMENT_OP_URL_FORMAT.format(base_url = self._base_url, endpoint_id=endpoint_id, api_version=self._api_version)
        response = requests.get(inference_op_url, headers=self.get_request_header())
        if response.status_code == 200:
            return response.json()

        return None

    def get_model(self, model_id):
        model_url = GET_MODEL_URL_FORMAT.format(base_url=self._base_url, model_id=model_id, api_version=self._api_version)
        response = requests.get(model_url, headers=self.get_request_header())
        if response.status_code == 200:
            return response.json()

    
    def get_deployed_endpoint(self, endpoint_id):
        model_url = GET_ENDPOINT_URL_FORMAT.format(base_url=self._base_url, endpoint_id=endpoint_id, api_version=self._api_version)
        response = requests.get(model_url, headers=self.get_request_header())
        if response.status_code == 200:
            return response.json()

    def wait_for_training_completion(self, model_id, timeout_is_seconds = 3600):
        while True:
            time.sleep(10)
            op = self.get_training_operation(model_id)
            if op is None:
                # TODO: what we gonna do here?
                continue
            if op["status"] != 'Running':
                if op["status"] == 'Completed':
                    return self.get_model(model_id)
                else:
                    return op
    
    def wait_for_batch_inference_completion(self, operation_id, timeout_is_seconds = 3600):
        while True:
            time.sleep(10)
            op = self.get_batch_inference_operation(operation_id)
            if op is None:
                # TODO: what we gonna do here?
                continue
            if op["status"] != 'Running':
                return op
    
    def wait_for_deployment_completion(self, endpoint_id, timeout_is_seconds = 3600):
        while True:
            time.sleep(10)
            op = self.get_deployment_operation(endpoint_id)
            if op is None:
                # TODO: what we gonna do here?
                continue
            if op["status"] != 'Running':
                return op
    
    def batch_inference(self, model_id, user_input):
        inference_url = BATCHINFERENCE_URL_FORMAT.format(base_url=self._base_url, model_id=model_id, api_version=self._api_version)
        response = requests.post(inference_url, headers=self.get_request_header(), data=json.dumps(user_input))
        
        if response.status_code == 200:
            return response.json()['operationId']

        return None

    def deploy_model(self, model_id, dns_name_label):
        deploy_url = DEPLOY_URL_FORMAT.format(base_url=self._base_url, model_id=model_id, api_version=self._api_version)
        user_input = {"dns_name_label": dns_name_label}
        response = requests.post(deploy_url, headers=self.get_request_header(), data=json.dumps(user_input))

        if response.status_code == 200:
            return response.json()['endpointId']

        return None

    def score_with_deployed_endpoint(self, endpoint_id, records, verboseMode=False):
        endpoint = self.get_deployed_endpoint(endpoint_id)
        header = {"Content-Type": "application/json", "Authorization": "Bearer "+endpoint["primaryKey"]}

        userInput = {"records": records, "verboseMode": verboseMode}

        response = requests.post(endpoint["scoringUrl"], headers=header, data=json.dumps(userInput))

        return response.text

