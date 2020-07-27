# Publish an AI service

In this article, we will show you how to publish an AI service using Luna management portal using the service endpoint and AML pipelines we published in the previous steps.

## Access Luna management portal

You can access the Luna management portal at https://*uniqueName*-isvapp.azurewebsites.net/ where *uniqueName* is the unique name you picked when you deploying Luna service. You will need to log in the portal using you AAD account. The AAD account must be added as the admin account during the deployment.

## Publish a real time prediction service

A real time prediction service allows the end user to call the endpoint API for real time scoring using a pre-trained model. In this example, we will use the Logistic Regression model we trained earlier using the Iris sample data.

You can skip this section if you are not planning to publish a real time prediction service.

### Create a real-time prediction product

### Create a real-time prediction deployment

### Create a real-time prediction API version

## Publish a train-your-own-model AI service

A train-your-own-model AI service allows the end user to call the APIs to train models using their own data, do batch inference with their own models and/or deploy the models to real time service endpoint for their online applications.

You can skip this section if you are not planning to publish a train-your-own-model AI service.

### Create a train-your-own-model product

### Register a Azure Machine Learning workspace

### Create a train-your-own-model deployment

### Create a train-your-own-model API version

## Next Step

[Test AI service](./test-ai-service.md)
