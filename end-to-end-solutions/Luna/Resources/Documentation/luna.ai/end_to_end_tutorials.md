# Luna.ai end to end tutorial

## Overview

In this tutorial, we will show you how to use Project Luna, Azure Machine Learning Service and Azure Marketplace to package and publish ML models and algorithms into AI services and sell through Microsoft.

We are going to use a simple sklearn Linear Regression classification model as an example in this tutorial. By end of the tutorial, you will have:

- An Azure Marketplace SaaS offer which allows user to train and use Linear Regression classification models.
- Two plans in your SaaS offer:
  - First plan provides a pre-trained model using the sklearn iris dataset. It can be used for trial or demo purposes.
  - Second plan allows user to train classification models using their own data, use the model to do batch inference or deploy the model to a service endpoint for real-time scoring
- Usage based billing for the train-your-own-model plan

## Get Started

- [Get ready to start](./get-ready.md)
- [Deploy Luna service to your Azure subscription](./setup-luna.md)
- [Create Azure Machine Learning service and configure compute resources](./create-and-configure-aml.md)

## Work on your code

- [Create a ML project using Luna.AI project template](./use-luna-ml-project-template.md)
- [Add your code to the ML project](./add-ml-code.md)
- [Train and deploy a model using sklearn Iris sample data](./deploy-pre-trained-model.md)
- [Test and publish Azure Machine Learning pipelines](./use-luna-ml-project-template.md)

## Publish an AI service

- [Publish an AI service](./publish-ai-service.md)
- [Test AI service](./test-ai-service.md)
  
## Publish a SaaS offer

- [Publish an SaaS offer](./publish-saas-offer.md)
- [Send a welcome letter using webhook](./send-welcome-letter-using-webhook.md)
- [Test SaaS offer](./test-ai-service.md)
- [Config usage based billing](./config-meter-based-billing.md)

## Management and Maintenance

- [Deploy and publish a hotfix](./deploy-a-hotfix.md)
- [Deploy and publish a breaking change with version bump](./deploy-a-version-bump.md)
