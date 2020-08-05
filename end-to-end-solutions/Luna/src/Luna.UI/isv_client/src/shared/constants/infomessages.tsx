export const Offers =
{
    offer:
    {
        ID: "<b>ID</b>–Create a unique offer ID for each offer in your account. This ID will be visible to customers in the URL of the marketplace offer and the Azure Resource Manager templates (if applicable)"
            + "<br/> <br/> <b>Note:</b> This ID cannot be modified after creation and must correspond to Offer ID specified in the Partner Center. ",
        Alias: "<b>Alias:</b> Enter a descriptive name that we will use to refer to this offer within Luna. Once you select <b>save</b>, you cannot change the offer alias."
            + "<br/> <br/> <b>Note:</b> This name will not be used in the marketplace and is different than the <b>Offer Name</b> <span style='color:red;'>specified in the Partner Center</span> and other values that will be shown to the customers.",
        Version: "<b>Version:</b> The version number of your offer. Customers will see this info on the offer's details page."
            + "<br/> <br/> <b>Note:</b> This must correspond to App version specified on the Properties tab of the Partner Center. ",
        Owners: "<b>Owners:</b> Specify emails of offer owners. "
            + "<br/> <br/> <b>Note:</b> This must correspond to Preview Audience specified on the Preview tab of the Partner Center.",
        HostSubscription: "<b>HostSubscription:</b>Specify tenant ID of host subscription. "
            + "<br/> <br/> <b>Note:</b> This must correspond to the Azure Active Directory tenant ID specified on the Technical configuration tab of the Partner Center.",
        Parameters: "Configure additional parameters for user subscription. User will provide these information before the application is being provisioned. The parameter can be used later during the provisioning."
    },
    ipAddress: "Pre-allocate ip blocks in your private vNet and configure how many ip addresses needed per subscription. This is needed only when some Azure services are running in your private vNet as a part of the application.",
    armTemplates: "ARM templates can be used to manage Azure resources when user create, update or delete the subscription. ARM template parameters are extracted automatically from uploaded ARM templates. ARM template parameter values will be evaluated as C# expression.",
    webHooks: "Webhooks can be used to define your own business logic when user create, update or delete the subscription. For example, you can define a webhook to send a welcome email to the user when they create the subscription.",
    meters: "Define custom meters and how to collect meter events from telemetry data.",
    plans:
    {
        planId: "<b>Plan ID</b> - Create a unique plan ID for each plan in this offer. This ID will be visible to customers in the product URL and Azure Resource Manager templates (if applicable)" +
            "<br/> <br/> <b>Note: </b> This must correspond to the Plan ID specified on the Plan overview tab of the Partner Center.",
        planName: "<b>Plan Name </b> - Create a unique name for each plan in this offer. The plan name is used to differentiate software plans that may be a part of the same offer." +
            "<br/> <br/> <b>Note: </b> This must correspond to the Plan name specified on the Plan overview tab of the Partner Center.",
        restrictedAudience: "<b>Restricted Audience (Tenant IDs)</b> - Assign the audience that will have access to this private plan." +
            "<br/> <br/> <b>Note: </b> This must correspond to the Restricted Audience specified on the Plan overview tab (Plan audience, check “This is a private plan.”) of the partner center."
    }
}

export const ProductMessages =
{
    product: {
        ProductId: '',
        ProductType: '',
        HostType: '',
        Owner: ''
    },
    deployment: {
        DeploymentName: '',
        Description: ''        
    },
    Version: {
        DeploymentName: '',
        VersionName:'',
        Source:'',
        RealtimePredictAPI:'',
        TrainingAPI:'',
        BatchInferenceAPI:'',
        DeployEndpointAPI:'',
        Authentication:'',
        AMLWorkspace:'',
        Key:'',        
        AdvancedSettings: '',
        ProjectFile:'',
    },
    AMLWorkSpace:
    {
        WorkspaceName:'',
        ResourceId:'',
        AADApplicationId:'',
        AADTenantId:'',
        AADApplicationSecret:'',
        
    },
    LunaWebHookURL:
    {
        HeaderTitle:'You can configure webhooks in Luna service and manage Luna.AI API subscription through Azure Marketplace SaaS offers.'
        +'Please see <a href="#" target="_blank"> Luna documentation </a> for how to configure webhooks in your SaaS offer​',
        SubscribewebhookURL: window.Configs.API_ENDPOINT + 'apisubscriptions/createwithid?ProductName={}&DeploymentName={}&UserId={}&SubscriptionName={}&SubscriptionId={}',
        UnSubscribewebhookURL: window.Configs.API_ENDPOINT + 'apisubscriptions/delete?SubscriptionId={}',
        SuspendwebhookURL: window.Configs.API_ENDPOINT + 'apisubscriptions/suspend?SubscriptionId={}',
    }
}