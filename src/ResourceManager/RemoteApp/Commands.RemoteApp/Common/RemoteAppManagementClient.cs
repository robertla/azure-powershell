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
            CollectionListResult response = null;

            if (String.IsNullOrEmpty(groupName))
            {
                response = Client.Collection.ListCollections();
            }
            else
            {
                response = Client.Collection.ListResourceGroupCollections(groupName);
            }

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

        internal CollectionUsageSummary GetCollectionUsageForUser(string usageMonth, string usageYear, string collectionName, string userUpn, string resourceGroupName)
        {
            return Client.Collection.GetUsageSummary(usageMonth, usageYear, collectionName, userUpn, resourceGroupName);
        }

        internal CollectionUsageSummaryList GetCollectionUsage(string usageMonth, string usageYear, string collectionName, string resourceGroupName)
        {
            return Client.Collection.GetUsageSummaryList(usageMonth, usageYear, collectionName, resourceGroupName);
        }

        internal UsageDetailsInfo GetCollectionUsageDetails(string usageMonth, string usageYear, string collectionName, string resourceGroupName)
        {
            BillingDate date = new BillingDate
            {
                Year = usageYear,
                Month = usageMonth
            };

            return Client.Collection.GetUsageDetails(collectionName, resourceGroupName, date);
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

                if (!(String.Equals(details.AccountInfo.WorkspaceName, accountInfo.AccountInfo.WorkspaceName) && String.Equals(details.AccountInfo.PrivacyUrl, accountInfo.AccountInfo.PrivacyUrl)))
                {
                    accountInfo.Location = details.Location;
                    accountInfo.Tags = new Dictionary<string, string>();

                    if (String.IsNullOrEmpty(accountInfo.AccountInfo.WorkspaceName))
                    {
                        accountInfo.AccountInfo.WorkspaceName = details.AccountInfo.WorkspaceName;
                    }

                    if (accountInfo.AccountInfo.PrivacyUrl == null)
                    {
                        accountInfo.AccountInfo.PrivacyUrl = details.AccountInfo.PrivacyUrl;
                    }

                    Client.Account.UpdateAccount(accountInfo);
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
