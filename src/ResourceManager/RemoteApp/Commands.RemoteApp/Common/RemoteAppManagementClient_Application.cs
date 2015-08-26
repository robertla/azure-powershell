using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using System.Collections.Generic;

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
            return Client.Collection.PublishOrUpdateApplication(details, collectionName, details.Alias, resourceGroupName);

        }

        internal PublishingOperationResult UnpublishApp(string resourceGroupName, string collectionName, string alias)
        {
            return Client.Collection.Unpublish(collectionName, alias, resourceGroupName);

        }
    }
}
