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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Commands.RemoteApp;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    [Cmdlet(VerbsData.Update, "AzureRemoteAppCollection", SupportsShouldProcess = true), OutputType(typeof(CollectionCreationDetailsWrapper))]
    public class UpdateAzureRemoteAppCollection : RemoteAppArmResourceCmdletBase
    {
        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name. Wildcards are permitted.")]
        [ValidatePattern(NameValidatorStringWithWildCards)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        [Parameter(Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the RemoteApp template image."
        )]
        public string ImageName { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "Log off users immediately after the update has successfully completed")]
        public SwitchParameter ForceLogoffWhenUpdateComplete { get; set; }

        public override void ExecuteRemoteAppCmdlet()
        {
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
                TemplateImageName = ImageName,
                WaitBeforeShutdownInMinutes = ForceLogoffWhenUpdateComplete ? -1 : 0
            };

            if (ShouldProcess(CollectionName, Commands_RemoteApp.UpdateCollection))
            {
                response = RemoteAppClient.CreateOrUpdateCollection(ResourceGroupName, CollectionName, details);
            }
            if (response != null)
            {
                WriteObject(response);
            }
        }
    }
}
