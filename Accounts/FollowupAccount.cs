using System;
using Microsoft.Xrm.Sdk;

namespace Accounts
{
    /// <summary>
    /// Create an asynchronous plug-in, registered on the Create Message of the account table (i.e entity here).
    /// This plug-in will create a task activity that will remind the creator (owner) 
    /// of the account to follow up one week later 
    /// </summary>


    public class FollowupAccount : IPlugin
    {
        public FollowupAccount(string unsecure, string secure)
        {

        }
        
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity account)
            {
                // Obtain the organization service reference which you will need for  
                // web service calls.  A
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    tracingService.Trace(account.Id.ToString());

                    Entity followup = new Entity("task");
                    followup["subject"] = "Send e-mail to the new customer.";
                    followup["description"] =
                        "Follow up with the customer. Check if there are any new issues that need resolution.";
                    followup["scheduledstart"] = DateTime.Now.AddDays(7);
                    followup["scheduledend"] = DateTime.Now.AddDays(7);
                    followup["category"] = context.PrimaryEntityName;

                    // Refer to the account in the task activity.
                    if (context.OutputParameters.Contains("id"))
                    {
                        Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        string regardingobjectidType = "account";

                        followup["regardingobjectid"] =
                        new EntityReference(regardingobjectidType, regardingobjectid);

                        // Create the task in Microsoft Dynamics CRM.
                        tracingService.Trace($"{this.GetType().Name}: Creating the task activity.");
                        service.Create(followup);
                    }

                }
                catch (Exception ex)
                {
                    tracingService.Trace($"{this.GetType().Name} Error: {0}", ex.ToString());
                    throw;
                }

            }

            }
    }
}
