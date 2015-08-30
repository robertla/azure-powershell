using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.RemoteApp.Common
{
    public partial class RemoteAppManagementClientWrapper
    {
        internal IList<SecurityPrincipalInfo> GetUsers(string ResourceGroupName, string collectionName)
        {
            IList<SecurityPrincipalInfo> response = Client.Collection.GetUsers(collectionName, ResourceGroupName).Value;

            return response;
        }

        internal SecurityPrincipalOperationErrorDetails AddUser(string ResourceGroupName, string collectionName, SecurityPrincipal userToAdd)
        {
            return Client.Collection.AddSecurityPrincipal(userToAdd, collectionName, userToAdd.Name, ResourceGroupName);
        }

        internal SecurityPrincipalOperationErrorDetails DeleteUser(string ResourceGroupName, string collectionName, SecurityPrincipal userToDelete)
        {
            return Client.Collection.DeleteSecurityPrincipal(userToDelete, collectionName, userToDelete.Name, ResourceGroupName);
        }

    }
}
