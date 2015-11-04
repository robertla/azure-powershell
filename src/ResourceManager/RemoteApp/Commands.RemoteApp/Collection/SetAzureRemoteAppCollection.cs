// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Management.RemoteApp.Models;
using System;
using System.Management.Automation;
using System.Net;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    [Cmdlet(VerbsCommon.Set, "AzureRemoteAppCollection", DefaultParameterSetName = DescriptionOnly), OutputType(typeof(CollectionCreationDetailsWrapper))]
    public class SetAzureRemoteAppCollection : RemoteAppArmResourceCmdletBase
    {
        private const string DomainJoined = "DomainJoined";
        private const string RdpPropertyOnly = "RdpPropertyOnly";
        private const string DescriptionOnly = "DescriptionOnly";
        private const string PlanOnly = "PlanOnly";

        [Parameter(Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name")]
        [ValidatePattern(NameValidatorString)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        [Parameter(Mandatory = true,
                   ValueFromPipelineByPropertyName = true,
                   ParameterSetName = PlanOnly,
                   HelpMessage = "Plan to use for this collection. Use Get-AzureRemoteAppPlan to see the plans available.")]
        [ValidateNotNullOrEmpty]
        public string Plan { get; set; }

        [Parameter(Mandatory = true,
                   ValueFromPipelineByPropertyName = true,
                   ParameterSetName = DomainJoined,
                   HelpMessage = "Credentials of a user that has permission to add computers to the domain.")]
        [ValidateNotNull]
        public PSCredential Credential { get; set; }

        [Parameter(Mandatory = true,
                   ValueFromPipelineByPropertyName = true,
                   ParameterSetName = DescriptionOnly,
                   HelpMessage = "Description of what this collection is used for.")]
        [ValidateNotNull]
        public string Description { get; set; }

        [Parameter(Mandatory = true,
                   ValueFromPipelineByPropertyName = true,
                   ParameterSetName = RdpPropertyOnly,
                   HelpMessage = "Used to allow RDP redirection.")]
        [ValidateNotNull]
        public string CustomRdpProperty { get; set; }

        public override void ExecuteCmdlet()
        {
            NetworkCredential creds = null;
            CollectionCreationDetailsWrapper details = null;
            CollectionCreationDetailsWrapper response = null;
            Collection collection = null;

            

            collection = FindCollection(ResourceGroupName, CollectionName);
            if (collection == null)
            {
                return;
            }



            details = new CollectionCreationDetailsWrapper()
            {
                ResourceGroupName = ResourceGroupName,
                Location = collection.Location,
                CollectionName = CollectionName,
                BillingPlanName = collection.BillingPlanName,
                TemplateImageName = collection.TemplateImageName,
            };
            if (Credential != null)
            {
                if (collection.AdInfo == null)
                {
                    ErrorRecord er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
                        Commands_RemoteApp.AadInfoCanNotBeAddedToCloudOnlyCollectionMessage,
                        String.Empty,
                        collection,
                        ErrorCategory.InvalidArgument);
                    ThrowTerminatingError(er);
                }

                details.AdInfo = new ActiveDirectoryConfig();

                creds = Credential.GetNetworkCredential();
                details.AdInfo.ServiceAccountUserName = Credential.UserName;
                details.AdInfo.ServiceAccountPassword = creds.Password;
                details.AdInfo.DomainName = collection.AdInfo.DomainName;
                details.AdInfo.OrganizationalUnit = collection.AdInfo.OrganizationalUnit;

            }
            else if (Plan != null)
            {
                details.BillingPlanName = Plan;
            }
            else if (Description != null)
            {
                details.Description = Description;
            }
            else if (CustomRdpProperty != null)
            {
                details.CustomRdpProperty = CustomRdpProperty;
            }
            else
            {
                ErrorRecord er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
                    "At least one parameter must be set with this cmdlet",
                    String.Empty,
                    collection,
                    ErrorCategory.InvalidArgument);
                ThrowTerminatingError(er);
            }

            response = RemoteAppClient.CreateOrUpdateCollection(ResourceGroupName, CollectionName, details);
            if (response != null)
            {
                WriteObject(response);
            }
        }
    }
}
