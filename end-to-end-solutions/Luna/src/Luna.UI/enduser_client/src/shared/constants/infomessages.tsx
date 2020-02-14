export const Offers =
{
    offer:
    {
        ID:"<b>ID</b>–Create a unique offer ID for each offer in your account. This ID will be visible to customers in the URL of the marketplace offer and the Azure Resource Manager templates (if applicable)"
        + "<br/> <br/> <b>Note:</b> This ID cannot be modified after creation and must correspond to Offer ID specified in the Partner Center. ",
        Alias: "<b>Alias:</b> Enter a descriptive name that we will use to refer to this offer within Luna. Once you select <b>save</b>, you cannot change the offer alias."
        + "<br/> <br/> <b>Note:</b> This name will not be used in the marketplace and is different than the <b>Offer Name</b> <span style='color:red;'>specified in the Partner Center</span> and other values that will be shown to the customers.",
        Version:"<b>Version:</b> The version number of your offer. Customers will see this info on the offer's details page."
        + "<br/> <br/> <b>Note:</b> This must correspond to App version specified on the Properties tab of the Partner Center. ",
        Owners:"<b>Owners:</b> Specify emails of offer owners. "
        + "<br/> <br/> <b>Note:</b> This must correspond to Preview Audience specified on the Preview tab of the Partner Center.",
        HostSubscription:"<b>HostSubscription:</b>Specify tenant ID of host subscription. "
        + "<br/> <br/> <b>Note:</b> This must correspond to the Azure Active Directory tenant ID specified on the Technical configuration tab of the Partner Center.",
        Parameters:"Details of additional parameters needed for the offer (i.e: region, location). Some additional parameters may require a minimum and maximum value (i.e: training iterations). Indicate whether or not a parameter’s value is to be selected from a list by clicking the From List checkbox."        
    },
    ipAddress:"IP blocks should include prefix (i.e: /24) defining all IP addresses for a given resource, while additional prefix in <b> prefix </b> column specifies number of IP addresses allotted to each subscription",
    armTemplates:"Upload ARM templates to specify behavior for subscribe, unsubscribe, suspend, and delete data. If an offer has multiple plans, additional ARM templates should be uploaded to specify the behavior for each plan",
    webHooks:"Include webhooks for additional functionality not doable through ARM templates (i.e: Pause a VM).",
    plans:
    {
        planId:"<b>Plan ID</b> - Create a unique plan ID for each plan in this offer. This ID will be visible to customers in the product URL and Azure Resource Manager templates (if applicable)"+
        "<br/> <br/> <b>Note: </b> This must correspond to the Plan ID specified on the Plan overview tab of the Partner Center.",
        planName:"<b>Plan Name </b> - Create a unique name for each plan in this offer. The plan name is used to differentiate software plans that may be a part of the same offer."+
        "<br/> <br/> <b>Note: </b> This must correspond to the Plan name specified on the Plan overview tab of the Partner Center.",
        restrictedAudience:"<b>Restricted Audience (Tenant IDs)</b> - Assign the audience that will have access to this private plan."+
        "<br/> <br/> <b>Note: </b> This must correspond to the Restricted Audience specified on the Plan overview tab (Plan audience, check “This is a private plan.”) of the partner center."
    }
}