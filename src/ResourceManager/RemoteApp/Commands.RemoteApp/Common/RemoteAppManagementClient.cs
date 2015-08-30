using Microsoft.Azure.Common.Authentication;
using Microsoft.Azure.Common.Authentication.Models;
using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using Microsoft.Rest;
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

        internal string GetSubscriptionId()
        {
            return Client.SubscriptionId;
        }

        internal Uri GetBaseUri()
        {
            return Client.BaseUri;
        }

        #region Collections

        internal IEnumerable<Collection> ListCollections(string groupName)
        {
            CollectionListResult response = Client.Collection.ListResourceGroupCollections(groupName);

            return response.Value;
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
            AccountDetailsWrapperList response = Client.Account.GetAccountInfo();

            return response.Value.FirstOrDefault();
        }

        internal bool SetAccount(AccountDetailsWrapper accountInfo)
        {
            bool accountExists = false;

            AccountDetailsWrapper details = Client.Account.GetAccountInfo().Value.FirstOrDefault();

            if (details != null)
            {
                accountExists = true;

                if (!((details.AccountInfo.WorkspaceName == accountInfo.AccountInfo.WorkspaceName) && (details.AccountInfo.PrivacyUrl == accountInfo.AccountInfo.PrivacyUrl)))
                {
                    if (String.IsNullOrEmpty(accountInfo.AccountInfo.WorkspaceName))
                    {
                        accountInfo.AccountInfo.WorkspaceName = details.AccountInfo.WorkspaceName;
                    }

                    if (accountInfo.AccountInfo.PrivacyUrl == null)
                    {
                        accountInfo.AccountInfo.PrivacyUrl = details.AccountInfo.PrivacyUrl;
                    }

                    AccountDetailsWrapper accountUpdate = new AccountDetailsWrapper
                    {
                        Location = details.Location,
                        AccountInfo = new AccountDetails
                        {
                            IsDesktopEnabled = details.AccountInfo.IsDesktopEnabled,
                            MaxPublishedAppsPerService = details.AccountInfo.MaxPublishedAppsPerService,
                            MaxServices = details.AccountInfo.MaxServices,
                            MaxUsersPerService = details.AccountInfo.MaxUsersPerService,
                            PrivacyUrl = details.AccountInfo.PrivacyUrl,
                            RdWebUrl = details.AccountInfo.RdWebUrl,
                            WorkspaceName = details.AccountInfo.WorkspaceName,
                        },
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

        internal IEnumerable<Session> GetSessionList(string resourceGroupName, string collectionName)
        {
            IEnumerable<Session> response = Client.Collection.SessionList(collectionName, resourceGroupName).Value;

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
