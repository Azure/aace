# Distributed training of Image segmentation on Azure ML

The repo will show how to complete distributional training of image segmentation on Azure ML.

## Platform

We complete the distributional training in Azure ML by using mutiple nodes and mutiple GPU's per node.

[Azure Machine Learning](https://azure.microsoft.com/en-us/services/machine-learning/)

[Azure ML SDK](https://docs.microsoft.com/en-us/python/api/overview/azure/ml/?view=azure-ml-py)

To run the notebook, you need to have/create:
1. Create/have Azure subscription
2. Create/have Azure storage
3. Create/have Azure ML workspace
4. (Optional) Create/have Azure ML compute target (4 nodes of STANDARD_NC24) - this can be created in notebook.

## Dataset

We used the data from a kaggle project:

https://www.kaggle.com/c/airbus-ship-detection

The project is for segmenting ships from sattelite images. We used their train_v2 data.

To run the notebook, you need to:
1. create a container in Azure storage.
2. Upload "train_v2" into the container with folder name "airbus"

## Package
We used a package "Fast.AI". It can use less codes to create deep learning model and train the model. For example, we used 3 lines for the image classfication:

>data = ImageDataBunch.from_folder(data_folder, train=".", valid_pct=0.2, ds_tfms=get_transforms(), size=sz, bs = bs, num_workers=8).normalize(imagenet_stats)

>learn = cnn_learner(data, models.resnet34, metrics=dice)

>learn.fit_one_cycle(5, slice(1e-5), pct_start=0.8)

Fast.AI supports computer vision (CNN and U-Net), and NLP (transformer). Please find details in their website.

https://www.fast.ai/

You can install it by:

>pip install fastai

## Distributional training

Fasi.AI only support the NCCL backend distributional training, which is not natively supported by Azure ML. We used a script "azureml_adapter.py" to help complete the NCCL initialization on Azure ML.
