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

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    [Cmdlet(VerbsCommunications.Send, "AzureRemoteAppSessionMessage")]
    public class SetAzureRemoteAppSessionSendMessage : RemoteAppArmResourceCmdletBase
    {
        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name. Wildcards are permitted.")]
        [ValidatePattern(NameValidatorStringWithWildCards)]
        [Alias("collectionName")]
        public string CollectionName { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "User Upn")]
        [ValidatePattern(NameValidatorStringWithWildCards)]
        [Alias("userUpn")]
        public string UserUpn { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Details of the session message.")]
        [Alias("messageDetails")]
        public string MessageDetails { get; set; }

        public override void ExecuteCmdlet()
        {
            if (String.IsNullOrWhiteSpace(CollectionName))
            {
                CreateWildcardPattern(CollectionName);
            }

            if (ExactMatch)
            {
                SessionSendMessageCommandParameter sess = new SessionSendMessageCommandParameter {
                    Message = MessageDetails
                };

                RemoteAppClient.SetSessionSendMessage(ResourceGroupName, CollectionName, UserUpn, sess);
            }
            else
            {
                WriteVerboseWithTimestamp(String.Format(Commands_RemoteApp.CollectionNotFoundByNameFormat, CollectionName));
            }
        }
    }
}