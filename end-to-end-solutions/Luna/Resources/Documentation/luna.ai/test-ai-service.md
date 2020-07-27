# Test the AI service

In this article, we are going to show you how can you test the AI services you published in the previous steps.

There are two different ways to run the test: calling APIs using Postman collection or using Luna client library

## Run test using Postman collection

### Download and configure the Postman collection

We created a Postman collection which you can use to test your AI service. You can download the collection from [here](https://www.getpostman.com/collections/92eec92e800414e8cece).

After download the collection, you need to update the variables in the collection.

#### Must updated variables

- unique_name: the unique name you used when deploying Luna service
- user_id: your AAD account

#### Other variables

You can leave the other variables with the default values if you were following our tutorial and publishing your products, deployments and API versions using the default names we were suggesting. If you used your own name, you need update the corresponding variable values.

### Run tests

After updating all the variables, you can start to run the test by running the requests from the top. Few things to pay attention to:

- All the OPTIONS requests are used as comments. Don't run those requests.
- After model training and deployment, keep trying the get operations until the status is "Completed".
- After the batch inference completed, check the result file uploaded to Azure storage account and make sure the result is correct.

## Run tests using Luna client library

### Subscribe the AI service

### Run tests in the sample python notebook