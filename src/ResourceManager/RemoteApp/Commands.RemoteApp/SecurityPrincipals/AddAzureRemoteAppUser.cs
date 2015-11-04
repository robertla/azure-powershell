// ----------------------------------------------------------------------------------
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
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureRemoteAppUser"), OutputType(typeof(SecurityPrincipalOperationErrorDetails))]
    public class AddAzureRemoteAppUser : SecurityPrincipalsCmdletBase
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
            HelpMessage = "The security principal provider type."
            )]
        public PrincipalProviderType Provider { get; set; }

        [Parameter(Mandatory = true,
            Position = 3,
            ValueFromPipeline = false,
            HelpMessage = "A user UPN to add to the RemoteApp collection.")]
        [ValidatePattern(UserPrincipalValdatorString)]
        public string UserUpn { get; set; }

        public override void ExecuteCmdlet()
        {
            SecurityPrincipalOperationErrorDetails response = null;

            response = ProcessUsers(Operation.Add, CollectionName, Provider, UserUpn);

            if (response != null)
            {
                WriteObject(response, true);
            }
        }
    }
}

