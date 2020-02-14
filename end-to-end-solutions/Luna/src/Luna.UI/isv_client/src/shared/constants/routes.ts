export enum WebRoute {
  Home = '/',
  Offers = '/Offers',  
  ModifyOfferInfo = '/ModifyOffer/:offerName/Info',
  ModifyOfferParameters = '/ModifyOffer/:offerName/Parameters',
  ModifyOfferIpConfigs = '/ModifyOffer/:offerName/IpConfigs',
  ModifyOfferArmTemplates = '/ModifyOffer/:offerName/ArmTemplates',
  ModifyOfferWebHooks = '/ModifyOffer/:offerName/WebHooks',
  ReviewOffer = '/ReviewOffer/:offerName?',
  Subscriptions = '/Subscriptions',
  ModifyOfferPlans = '/ModifyOffer/:offerName/Plans',
  LandingPage = '/LandingPage',
  SubscriptionDetail = '/SubscriptionDetail/:offerName/:subscriptionId'
}