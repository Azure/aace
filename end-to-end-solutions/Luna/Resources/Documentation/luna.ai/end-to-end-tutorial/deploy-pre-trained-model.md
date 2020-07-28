# Train and deploy a model using sklearn Iris sample data

In this article, we are going to show you how can you train a model and deploy it to a service endpoint in Azure Machine Learning service.

You need to run this if you want to pubish a pre-trained model as a Luna AI service later. If you always require your customer training models using their own data and settings, you can skip this article and jump to [Test and publish Azure Machine Learning pipelines](./use-luna-ml-project-template.md)

## Where's the code

The code to train and deploy a model is located in */src/luna_publish/azureml/train_and_deploy_model.py*

## Model id, endpoint id and more

Luna service will run the model training code and automatically register the model in Azure Machine Learning service. To register and deploy the model, you need to provide the following properties:

- experimentName: the name of Azure Machine Learning experiment
- modelId: the model id will be used to register and identify the model in Azure Machine Learning service
- endpointId: the endpoint id will be used to create and identify the service endpoint where the model is deployed to in Azure Machine Learning service
- serviceEndpointDnsNameLabel: the DNS name label for the deployed service endpoint.
  - If you are deploying to Azure Container Instance, the url will look like:

    ```url
    http://<service_endpoint_dns_name_label>.<region>.azurecontainer.io/score
    ```

  - If you are deploying to AKS, the url will look like:

    ```url
    http://<base_url_of_aks>/api/v1/service/<service_endpoint_dns_name_label>/score
    ```

All these values need to be a string contains lower case letters, number and dash, starts and ends with a letter. The max length of experiment name, model id and endpoint id is 32. The max lenght of service ednpoint DNS name label is 64.

## Input data

You need to define the input data to train the model. In this tutorial, our training function takes 3 parameters:

- trainingDataSource: the blob storage url with SaS key for the training data
- labelColumnName: the name of the label column. Will use the last column is not specified.
- description: the description of the model

Following is an example of the input data for our Logistic Regression model:

```json
{
    "trainingDataSource": "https://xiwutestai.blob.core.windows.net/lunav2/iris/iris.csv?st=2020-07-22T17%3A19%3A10Z&se=2027-10-12T17%3A19%3A00Z&sp=rl&sv=2018-03-28&sr=b&sig=7c%2BaoI8QtdepDHKqJqjjljdBUyDyuL8wbKol2Kn7xaI%3D",
    "description": "Iris prediction"
}
```

## Run python script to train and deploy model

After updated all the properties and the input data, you can run the following command in the conda console to train and deploy the model.

```shell
python src/luna_publish/azureml/train_and_deploy_model.py
```

The script will:

- Start a AML pipeline run to train the model
- Register the model with modelId
- Deploy the model to specified type of service endpoint
- Print out the scoring URL and primary authentication key

Record the scoring URL and key.

## Troubleshoot issues

The script will print out *"Link to Azure Machine Learning Portal"* for the pipeline run. If the run failed, you can open the link and see the error details in Azure Machine Learning workspace. You can find the most relevant error messages by click on the failed component on the graphic design canvas. You can also browse the azureml-logs folder to see other logs.

![aml-workspace-failed-run](../../images/luna.ai/aml-workspace-failed-run.png)

If the deployment step failed and you need more information from the container log to toubleshoot, you can find it in the Azure portal

- Go to azure portal and type in the deployment endpointId in the search textbox. Select the Container instance popped up.

  ![azure-portal-find-aci](../../images/luna.ai/azure-portal-find-aci.png)

- Click on "Containers" in the manu on the left side, select the container with same name with the endpointId in the container list and click on "Logs" in the menu bar below. You will see all the log from the container instance.

  ![azure-portal-aci](../../images/luna.ai/azure-portal-aci.png)

- You can also go the the "Connect" tab and connect to the running container instance for further troubleshooting.

## Test the scoring endpoint

After the training and deployment completed, you can find the endpoint information in your Azure Machine Learning workspace.

- Open the Azure Machine Learning workspace by the pipeline run URL
- Click on "Endpoints" in the menu on left side. Select the endpoint you just deployed
- Go to the "Consume" tab, you will see the REST endpoint and authentiation key

![aml-workspace-get-endpoint](../../images/luna.ai/aml-workspace-get-endpoint.png)

You can test the scoring endpoint with your test data in Postman:

- Open Postman
- Add a new request
- Change the http function to POST
- Paste the scoring URL to the request URL text box
- Click on the Authentication tab, choose "Bearer Token" as Type, paste the key in the "Token" text box
- Click on the Body tab, select "raw", in the content-type dropdown list (default shown as "Text"), choose "JSON"
- Add you test data in the text field. In this tutorial, the scoring function take a 2D array as input with property name "records". Following is an example of the input

    ```json
    {"records":[[5.1,3.5,1.4,0.2]]}
    ```

- Click on "Send" button to send the request
- The response body will show in the bottom. If everything runs correctly, the sample test data should get you [1] as result.

## Next Step

[Test and publish Azure Machine Learning pipelines](./use-luna-ml-project-template.md)
