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
using System.IO;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    [Cmdlet(VerbsData.Update, "AzureRemoteAppProgram"), OutputType(typeof(PublishingOperationResult), typeof(Job))]
    public class ModifyAzureRemoteAppProgram : RemoteAppArmResourceCmdletBase
    {
        [Parameter(Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name")]
        [ValidatePattern(NameValidatorString)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        [Parameter(Mandatory = true,
            Position = 2,
            HelpMessage = "Unique name which is used to identify this application.")]
        public string ApplicationAlias { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "Optional command-line arguments to be passed to the application.")]
        public string CommandLine { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "Display name of the program to be published.")]
        [ValidateNotNullOrEmpty()]
        public string DisplayName { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "Set to true if you want this application to be shown to users. Set to false if you wish to hide it.")]
        public bool? AvaliableToUsers { get; set; }

        public override void ExecuteRemoteAppCmdlet()
        {
            PublishedApplicationDetails publishedApp = null;
            PublishingOperationResult response = null;
            ApplicationDetails appDetails = null;

            publishedApp = RemoteAppClient.GetApplication(ResourceGroupName, CollectionName, ApplicationAlias);

            if (publishedApp == null)
            {
                RemoteAppServiceException ex = new RemoteAppServiceException(
                                                Commands_RemoteApp.ApplicationNotFound,
                                                ErrorCategory.InvalidArgument
                                            );
                throw ex;
            }

            appDetails = new ApplicationDetails()
            {
               Alias = ApplicationAlias,
               AvailableToUsers = publishedApp.AvailableToUsers,
               CommandLineArguments = publishedApp.CommandLineArguments,
               Name = publishedApp.Name
            };

            if (CommandLine != null)
            {
                appDetails.CommandLineArguments = CommandLine;
            }

            if (!String.IsNullOrWhiteSpace(DisplayName))
            {
                appDetails.Name = DisplayName;
            }

            if (AvaliableToUsers.HasValue)
            {
                appDetails.AvailableToUsers = AvaliableToUsers.Value;
            }

            response = RemoteAppClient.ModifyApp(ResourceGroupName, CollectionName, appDetails);

            if (response != null)
            {
                WriteObject(response, true);
            }
        }
    }
}
