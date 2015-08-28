using Microsoft.Azure.Commands.RemoteApp.Cmdlet;
using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RemoteApp.Common
{
    public partial class RemoteAppManagementClientWrapper
    {
        internal PublishedApplicationDetails GetApplication(string resourceGroupName, string collectionName, string alias)
        {
            return Client.Collection.GetPublishedApp(collectionName, alias, resourceGroupName);
        }

        internal IList<PublishedApplicationDetails> GetApplications(string resourceGroupName, string collectionName)
        {
            return Client.Collection.ListPublishedApp(collectionName, resourceGroupName);
        }

        internal IList<StartMenuApplication> GetStartMenuApps(string resourceGroupName, string collectionName)
        {
            return Client.Collection.ListStartMenuApps(collectionName, resourceGroupName);
        }

        internal StartMenuApplication GetStartMenuApp(string resourceGroupName, string collectionName, string applicationId)
        {
            return Client.Collection.GetStartMenuApp(applicationId, collectionName, resourceGroupName);
        }

        internal PublishingOperationResult PublishApp(string resourceGroupName, string collectionName, ApplicationDetails details)
        {
            PublishedApplicationDetails publishedApp = null; 
            
            publishedApp = Client.Collection.GetPublishedApp(collectionName, details.Alias, resourceGroupName);
            if (publishedApp != null)
            {
                RemoteAppServiceException ex = new RemoteAppServiceException(
                                                Commands_RemoteApp.ApplicationExists,
                                                ErrorCategory.InvalidArgument
                                            );
                throw ex;
            }

            return Client.Collection.PublishOrUpdateApplication(details, collectionName, details.Alias, resourceGroupName);
        }


        internal PublishingOperationResult ModifyApp(string resourceGroupName, string collectionName, ApplicationDetails details)
        {
            return Client.Collection.PublishOrUpdateApplication(details, collectionName, details.Alias, resourceGroupName);
        }

        internal PublishingOperationResult UnpublishApp(string resourceGroupName, string collectionName, string alias)
        {
            return Client.Collection.Unpublish(collectionName, alias, resourceGroupName);
        }
    }
}
