# Deploy a breaking change with version bump

In the previous article, we showed you how to deploy and publish a hotfix to your AI service. In this article, we are going to show you how to update your service when you are introducing a breaking change.

When you are introducing a breaking change or a major feature, you don't want to break your users' product code. In Luna, we allow you deploy this type of changes with a version bump:

- Test and check in your code change to your Git repo
- If it is a real-time prediction product, train and deploy the model. Review [this document](./deploy-pre-trained-model.md) for more details.
- If it is a train-your-own-model product, test and publish new Azure Machine Learning pipeline endpoints. Review [this document](./test-and-publish-aml-pipelines.md) for more details.
- In Luna management portal, find the product and deployment
- Instead of updating an existing API version, create a new API version using the new real-time prediction endpoint URL or the AML pipelines. Review [this document](./publish-ai-service.md) for more details

Now you have a new API version published. All your existing users will continue running the AI service with the original vesion without any impact. When they are ready to test and move to the new version, they can simple do that by updating the api-version query parameter in the requests sent to your AI service.
