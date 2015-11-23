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
    [Cmdlet(VerbsData.Publish, "AzureRmRemoteAppProgram", DefaultParameterSetName = AppId), OutputType(typeof(PublishingOperationResult), typeof(Job))]
    public class PublishAzureRemoteAppProgram : RemoteAppArmResourceCmdletBase
    {
        private const string AppPath = "App Path";
        private const string AppId = "App Id";

        [Parameter(Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name")]
        [ValidatePattern(NameValidatorString)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        [Parameter(Mandatory = true,
            Position = 2,
            ParameterSetName = AppPath,
            HelpMessage = "Virtual file path of the program to be published.")]
        public string FileVirtualPath { get; set; }

        [Parameter(Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = AppId,
            HelpMessage = "Start menu program ID of the program to be published.")]
        public string StartMenuAppId { get; set; }

        [Parameter(Mandatory = true,
            Position = 3,
            HelpMessage = "Unique name which is used to identify this application and properties.")]
        public string ApplicationAlias { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "Command-line argument for the program to be published.")]
        [ValidateNotNullOrEmpty()]
        public string CommandLine { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "Display name of the program to be published.")]
        [ValidateNotNullOrEmpty()]
        public string DisplayName { get; set; }

        public override void ExecuteCmdlet()
        {
            PublishingOperationResult response = null;
            ApplicationDetails appDetails = null;

            string appName = null;
            string appPath = null;
            string iconURI = null;
            IDictionary<string, string> iconPngUris = new Dictionary<string, string>();  // BUGBUG  <int,string>

            switch (ParameterSetName)
            {
                case AppPath:
                    {
                        appName = Path.GetFileNameWithoutExtension(FileVirtualPath);
                        appPath = FileVirtualPath;
                        break;
                    }

                case AppId:
                    {
                        StartMenuApplication startMenu = RemoteAppClient.GetStartMenuApp(ResourceGroupName, CollectionName, StartMenuAppId);

                        appName = startMenu.StartMenuApplicationName;
                        appPath = startMenu.VirtualPath;
                        iconURI = startMenu.IconUri;
                        iconPngUris = startMenu.IconPngUris;
                        break;
                    }
            }

            appDetails = new ApplicationDetails()
            {
                DisplayName = String.IsNullOrWhiteSpace(DisplayName) ? appName : DisplayName,
                VirtualPath = appPath,
                ApplicationAlias = ApplicationAlias,
                IconUri = iconURI,
                IconPngUris = iconPngUris,
                CommandLineArguments = CommandLine,
                AvailableToUsers = true
            };

            response = RemoteAppClient.PublishApp(ResourceGroupName, CollectionName, appDetails);

            if (response != null)
            {
                WriteObject(response, true);
            }
        }
    }
}
