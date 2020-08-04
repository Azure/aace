# Add code to the Luna ML project

In this article, we are going to show you how to add training, batch inference and scoring code to the Luna ML project. We will use a sklearn Logistic Regression classification model as an example.

## Update conda environment

Luna and Azure Machine Learning service will create the execution environment from the *conda.yml* file in the base folder of Luna project template. You should add your conda or pip dependencies in the file.

In this tutorial, we will add the sklearn pip package:

```yaml
- pip:
  ...
  sklearn
```

## Create conda environment for local test

In this tutorial, we will run some test locally before deploying the code to Azure Machine Learning service. You need to open a anaconda console, run the following command from the root folder of the Luna ML project template to create the local conda environment after you update your conda.yml file:

```shell
conda env create -f conda.yml
conda activate project_environment
```

## Where to add my code

The only source code file you need to update in the Luna ML project template is *src/luna_publish/LunaPythonModel.py*. There're 5 functions in the LunaPythonModel class:

The following two functions are used for real-time prediction

- load_context: this function will be called every time the container instance/pod started if you or the user deploy the model to a service endpoint. You can use this function to perform some heavy initialization operations.
- predict: this function will be called when user calling the deploy service endpoint API for real-time scoring.

The following two functions are used for model training and batch inference

- train: the function to train a model
- batch_inference: the function to perform batch inference using a model
- set_run_mode: this function is for Luna service usage only. Please don't update or remove it.

## Import modules

You need to add required modules to the *src/luna_publish/LunaPythonModel.py*. In this tutorial, we will be training a Logistic Regression model using sklearn. Add the followings to the code:

```python
from sklearn.linear_model import LogisticRegression
import pandas as pd
import json
import os
import pickle
import requests

from luna.numpyJsonEncoder import NumpyJSONEncoder
```

The NumpyJSONEncoder helps you serialize Numpy data types to JSON strings. You can also implement and use your own JSON encoder.

## Model Training

You can add following code to the *train* function of LunaPythonModel class to train a sklearn Logistic Regression classification model:

```python
train_data = pd.read_csv(user_input["trainingDataSource"])

label_column_name = user_input['labelColumnName'] if 'labelColumnName' in user_input else train_data.columns[-1]
description = user_input['description'] if 'description' in user_input else 'this is my model description'

X = train_data.drop([label_column_name], axis=1)

Y = train_data[label_column_name]

log_reg = LogisticRegression()
log_reg.fit(X, Y)

model_path = 'models'
model_file = os.path.join(model_path, "model.pkl")
pickle.dump(log_reg, open(model_file, 'wb'))

return model_path, description
```

The user_input is an dictionary contains the JSON contain from user API request. In this case, a sample user input will be:

```json
{
    "trainingDataSource": "https://xiwutestai.blob.core.windows.net/lunav2/Iris/data.csv?your_sas_key",
    "labelColumnName": "medv",
    "description": "boston housing price prediction"
}
```

The function will read training data from *trainingDataSource* and train a Logistic Regression model using sklean library.

After you trained and vaidated the model, you can save the model file/files to a local folder (defined as model_path). Luna service will automatically register the model to the Azure Machine Learning workspace with an auto generated model id and return the model id to the user.

## Batch Inference

You can add the following code to the batch_inference function to perform batch inference using a Logistic Regression classification model:

```python
data = pd.read_csv(user_input["dataSource"])
output_filename = user_input["output"]

model_file = os.path.join(model_path, "models", "model.pkl")
model = pickle.load(open(model_file, 'rb'))

y_proba = model.predict(data)

temp_filename = "imputation_result.csv"
with open(temp_filename, "wt") as temp_file:
    pd.DataFrame(y_proba).to_csv(temp_file, header=False)

with open(temp_filename , 'rb') as fh:
    response = requests.put(output_filename,
                        data=fh,
                        headers={
                                    'content-type': 'text/csv',
                                    'x-ms-blob-type': 'BlockBlob'
                                }
                        )

return
```

A sample user input will look like:

```json
{
    "dataSource": "https://xiwutestai.blob.core.windows.net/lunav2/Iris/test.csv?your_sas_key",
    "output": "https://xiwutestai.blob.core.windows.net/lunav2/Iris/result.csv?your_sas_key"
}
```

Luna service will pre-download the model based on user provided model id and save the model files in *model_path* folder.

The function will read the data from the *dataSource*, predict the labels, save the result to a local file and upload it to the *output* Azure blob.

## Real-time scoring

The model can be deployed to a service endpoint (AKS or Azure Container Instances) for real-time scoring. There're two functions to update for real-time scoring:

- If you need to run any code every time when the container or the service started, add it to the *load_context* function.
- If you need to run the code for each user scoring request, add it to the *predict* function.

In this tutorial, you can add the following code to *load_context* function to load the model into memory and save it in *_model* property:

```python
model_file = os.path.join(model_path, 'models/model.pkl')
self._model = pickle.load( open( model_file, "rb" ) )
return
```

Then you can add the following code to the *predict* function to predict the label using the model:

```python
user_input = json.loads(model_input)

scoring_result = {"result": self._model.predict(user_input["records"])}

scoring_result = json.dumps(scoring_result, cls=NumpyJSONEncoder)
return AMLResponse(scoring_result, 200)
```

## Next Step

[Train and deploy a model using sklearn Iris sample data](./deploy-pre-trained-model.md)
