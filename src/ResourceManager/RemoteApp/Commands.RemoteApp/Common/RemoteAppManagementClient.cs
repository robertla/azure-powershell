using Microsoft.Azure.Common.Authentication;
using Microsoft.Azure.Common.Authentication.Models;
using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Azure.Commands.RemoteApp.Common
{


    public partial class RemoteAppManagementClientWrapper
    {
        public const string DefaultRemoteAppArmNamespace = "Microsoft.RemoteApp";

        public const string RemoteAppApiVersionValue = "2014-09-01";

        private IRemoteAppManagementClient Client { get; set; }

        internal RemoteAppManagementClientWrapper(AzureProfile profile, AzureSubscription subscription)
        {
            Client = AzureSession.ClientFactory.CreateArmClient<RemoteAppManagementClient>(profile.Context, AzureEnvironment.Endpoint.ResourceManager);
            Client.ArmNamespace = DefaultRemoteAppArmNamespace;
        }

        #region Collections

        internal IEnumerable<Collection> ListCollections(string groupName)
        {
            IEnumerable<Collection> response = Client.Collection.ListResourceGroupCollections(groupName);

            return response;
        }

        internal Collection Get(string ResourceGroupName, string collectionName)
        {
            Collection response = Client.Collection.Get(collectionName, ResourceGroupName);

            return response;
        }

        internal CollectionCreationDetailsWrapper CreateOrUpdateCollection(string ResourceGroupName, string collectionName, CollectionCreationDetailsWrapper createDetails)
        {
            CollectionCreationDetailsWrapper response = Client.Collection.CreateOrUpdate(createDetails, collectionName, ResourceGroupName);



            return response;
        }

        internal void DeleteCollection(string ResourceGroupName, string collectionName)
        {
            Client.Collection.Delete(collectionName, ResourceGroupName);
        }

        #endregion

        #region Accounts

        internal AccountDetailsWrapper GetAccount()
        {
            AccountDetailsWrapper response = Client.Account.GetAccountInfo();

            return response;
        }
        internal IEnumerable<Microsoft.Azure.Management.RemoteApp.Models.Location> GetLocations()
        {
            LocationPropertiesWrapper response = Client.Account.Locations();

            return response.Locations;
        }

        internal IEnumerable<BillingPlan> GetBillingPlans()
        {
            BillingPlanPropertiesWrapper response = Client.Account.Plans();

            return response.Plans;
        }

        internal bool SetAccount(AccountDetailsWrapper accountInfo)
        {
            bool accountExists = false;

            AccountDetailsWrapper details = Client.Account.GetAccountInfo();

            if (details != null)
            {
                accountExists = true;

                if (!((details.WorkspaceName == accountInfo.WorkspaceName) && (details.PrivacyUrl == accountInfo.PrivacyUrl)))
                {
                    if (String.IsNullOrEmpty(accountInfo.WorkspaceName))
                    {
                        accountInfo.WorkspaceName = details.WorkspaceName;
                    }

                    if (accountInfo.PrivacyUrl == null)
                    {
                        accountInfo.PrivacyUrl = details.PrivacyUrl;
                    }

                    AccountDetailsWrapper accountUpdate = new AccountDetailsWrapper
                    {
                        Location = details.Location,
                        IsDesktopEnabled = details.IsDesktopEnabled,
                        MaxPublishedAppsPerService = details.MaxPublishedAppsPerService,
                        MaxServices = details.MaxServices,
                        MaxUsersPerService = details.MaxUsersPerService,
                        PrivacyUrl = details.PrivacyUrl,
                        RdWebUrl = details.RdWebUrl,
                        WorkspaceName = details.WorkspaceName,
                        Tags = new Dictionary<string, string>()
                    };

                    Client.Account.UpdateAccount(accountUpdate);
                }
            }
            return accountExists;
        }

        internal void SetAccountBilling()
        {
            Client.Account.ActivateAccountBilling();
        }

        #endregion

        #region Sessions

        internal IEnumerable<SessionListItemProperties> GetSessionList(string resourceGroupName, string collectionName)
        {
            IEnumerable<SessionListItemProperties> response = Client.Collection.SessionList(collectionName, resourceGroupName).Sessions;

            return response;
        }

        internal void SetSessionLogoff(string resourceGroupName, string collectionName,
            string userUpn)
        {
            Client.Collection.SessionLogOff(collectionName, userUpn, resourceGroupName);
        }

        internal void SetSessionDisconnect(string resourceGroupName, string collectionName,
            string userUpn)
        {
            Client.Collection.SessionDisconnect(collectionName, userUpn, resourceGroupName);
        }

        internal void SetSessionSendMessage(string resourceGroupName, string collectionName, string userUpn,
            SessionSendMessageCommandParameter messageDetails)
        {
            Client.Collection.SessionSendMessage(messageDetails, collectionName, userUpn, resourceGroupName);
        }

        #endregion
    }
}
