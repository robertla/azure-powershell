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
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Network.Models;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Azure.Commands.RemoteApp;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    [Cmdlet(VerbsCommon.New, "AzureRemoteAppCollection", DefaultParameterSetName = NoDomain), OutputType(typeof(CollectionCreationDetailsWrapper))]
    public class NewAzureRemoteAppCollection : RemoteAppArmResourceCmdletBase
    {
        private const string DomainJoined = "DomainJoined";
        private const string NoDomain = "NoDomain";
        private const string AzureVNet = "AzureVNet";

        [Parameter(Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name")]
        [ValidatePattern(NameValidatorString)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        [Parameter(Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the RemoteApp template image."
        )]
        public string ImageName { get; set; }

        [Parameter(Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Plan to use for this collection. Use Get-AzureRemoteAppPlan to see the plans available."
        )]
        public string Plan { get; set; }

        [Parameter(Mandatory = true,
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = NoDomain,
            HelpMessage = "Location in which this collection will be created. Use Get-AzureRemoteAppLocation to see the locations available."
        )]
        public string Location { get; set; }

        [Parameter(
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the RemoteApp or Azure VNet to create the collection in."
        )]
        [Parameter(Mandatory = true, Position = 3, ParameterSetName = DomainJoined)]
        [Parameter(Mandatory = true, Position = 3, ParameterSetName = AzureVNet)]
        public string VNetName { get; set; }

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = AzureVNet,
            HelpMessage = "For Azure VNets only, a comma-separated list of DNS servers for the VNet."
        )]
        [ValidateNotNullOrEmpty]
        public string DnsServers { get; set; }

        [Parameter(Mandatory = true,
            Position = 6,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = AzureVNet,
            HelpMessage = "For Azure VNets only, the name of the subnet."
        )]
        [ValidateNotNullOrEmpty]
        public string SubnetName { get; set; }

        [Parameter(
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the on-premise domain to join the RD Session Host servers to."
        )]
        [Parameter(Mandatory = true, Position = 4, ParameterSetName = DomainJoined)]
        [Parameter(Mandatory = true, Position = 4, ParameterSetName = AzureVNet)]
        [ValidatePattern(DomainNameValidatorString)]
        public string Domain { get; set; }

        [Parameter(
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The users credentials that has permission to add computers to the domain."
        )]
        [Parameter(Mandatory = true, Position = 5, ParameterSetName = DomainJoined)]
        [Parameter(Mandatory = true, Position = 5, ParameterSetName = AzureVNet)]
        public PSCredential Credential { get; set; }

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of your organizational unit to join the RD Session Host servers, e.g. OU=MyOu,DC=MyDomain,DC=ParentDomain,DC=com. Attributes such as OU, DC, etc. must be in uppercase."
        )]
        [Parameter(ParameterSetName = DomainJoined)]
        [Parameter(ParameterSetName = AzureVNet)]
        [ValidatePattern(OrgIDValidatorString)]
        public string OrganizationalUnit { get; set; }

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Description of what this collection is used for."
        )]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Used to allow RDP redirection."
        )]
        [ValidateNotNullOrEmpty]
        public string CustomRdpProperty { get; set; }

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Sets the resource type of the collection."
        )]
        public CollectionMode? ResourceType { get; set; }

        public override void ExecuteRemoteAppCmdlet()
        {
            CollectionCreationDetailsWrapper createDetails = new CollectionCreationDetailsWrapper(){
                CollectionCreationDetailsWrapperName = CollectionName,
                TemplateImageName = ImageName,
                Region = Location,
                Location = Location,
                BillingPlanName = Plan,
                Description = Description,
                CustomRdpProperty = CustomRdpProperty,
                Mode = (ResourceType == null || ResourceType == CollectionMode.Unassigned) ? CollectionMode.Apps : ResourceType.Value,
                Tags = new Dictionary<string, string>(),
                ResourceGroupName = ResourceGroupName
            };

            CollectionCreationDetailsWrapper response = RemoteAppClient.CreateOrUpdateCollection(ResourceGroupName, CollectionName, createDetails);

            if (response != null)
            {
                WriteObject(response);
            }
        }

    }
}
