# Deploy and publish a hotfix

Previously in this tutorial, we already published and tested our Logistic Regression model as an Azure SaaS offer. In this ariticle, we will show you how can you apply a hotfix to a published AI service.

Publishing a hotfix means:

- It is a critical fix, you want everyone to get it as soon as possible
- Your users' production code should continue to work without any change

In Luna service, you can deploy and publish a hotifx by the following steps:

- Test and check in your code change to your Git repo
- If it is a real-time prediction product, train and deploy the model. Review [this document](./deploy-pre-trained-model.md) for more details.
- If it is a train-your-own-model product, test and publish new Azure Machine Learning pipeline endpoints. Review [this document](./test-and-publish-aml-pipelines.md) for more details.
- In Luna management portal, find the product, deployment and API version you are trying to apply the hotfix to
- Update the real-time prediction API or the pipeline ids and save the changes

Now your hotfix is deployed and available to all the users who is using this API version.

## Next Step

[Deploy and publish a breaking change with version bump](./deploy-a-version-bump.md)
