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

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureRmRemoteAppVM"), OutputType(typeof(IList<VmDetails>), typeof(LocalModels.RestartVmDetails))]
    public class GetAzureRemoteAppVm : RemoteAppArmResourceCmdletBase
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp resource group name")]
        [ValidatePattern(ResourceGroupValidatorString)]
        public override string ResourceGroupName { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name.")]
        [ValidatePattern(NameValidatorString)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        [Parameter(Mandatory = false,
            Position = 2,
            HelpMessage = "User UPN")]
        [ValidateNotNullOrEmpty()]
        public string UserUpn { get; set; }

        private void WriteVmDetails()
        {
            IList<VmDetails> vmList = null;

            vmList = RemoteAppClient.GetVmDetails(ResourceGroupName, CollectionName);

            if (vmList != null)
            {
                if (String.IsNullOrWhiteSpace(UserUpn))
                {
                    WriteObject(vmList, true);
                }
                else
                {
                    foreach (VmDetails detail in vmList)
                    {
                        foreach (string upn in detail.LoggedOnUserUpn)
                        {
                            if (String.Equals(upn, UserUpn, StringComparison.CurrentCultureIgnoreCase))
                            {
                                LocalModels.RestartVmDetails vmDetails = new LocalModels.RestartVmDetails(detail, CollectionName);
                                WriteObject(vmDetails);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public override void ExecuteCmdlet()
        {
            WriteVmDetails();
        }
    }
}
