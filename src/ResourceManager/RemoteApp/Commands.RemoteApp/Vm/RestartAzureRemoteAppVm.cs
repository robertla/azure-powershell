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
    [Cmdlet("Restart", "AzureRmRemoteAppVM", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RestartAzureRemoteAppVm : RemoteAppArmResourceCmdletBase
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
            HelpMessage = "RemoteApp collection name")]
        [ValidatePattern(NameValidatorString)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        [Parameter(Mandatory = true,
            Position = 2,
             HelpMessage = "User UPN")]
        public string UserUpn { get; set; }
   
        [Parameter(Mandatory = false,
            Position = 3,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Warning message shown to users connected to the VM before they are logged off")]
        public string LogoffMessage { get; set; }

        [Parameter(Mandatory = false,
            Position = 4,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Time to wait before logging off users on the VM (default is 60 seconds)")]
        public int LogoffWaitTimeInSeconds { get; set; }

        private VmDetails GetVm()
        {
            IList<VmDetails> vmList = null;
            VmDetails vm = null;

            vmList = RemoteAppClient.GetVmDetails(ResourceGroupName, CollectionName);

            if (vmList != null)
            {
                foreach (VmDetails detail in vmList)
                {
                    foreach (string upn in detail.LoggedOnUserUpn)
                    {
                        if (String.Equals(upn, UserUpn, StringComparison.CurrentCultureIgnoreCase))
                        {
                            vm = detail;
                            break;
                        }
                    }
                }
            }

            return vm;
        }

        private void RestartVm(VmDetails vm)
        {
            VmCommandDetailsWrapper restartParameter = null;
            string otherLoggedInUsers = null;
            string warningMessage = null;
            string warningCaption = null;

            if (vm.LoggedOnUserUpn.Count > 1)
            {
                otherLoggedInUsers = String.Join("\n", vm.LoggedOnUserUpn.Where(u => !String.Equals(u, UserUpn)).ToArray());

                warningMessage = string.Format(Commands_RemoteApp.RestartVmWarningMessageFormat, UserUpn, vm.VirtualMachineName, otherLoggedInUsers);
                warningCaption = string.Format(Commands_RemoteApp.RestartVmWarningCaptionFormat, vm.VirtualMachineName);

                if (!ShouldProcess(null, Commands_RemoteApp.GenericAreYouSureQuestion, warningCaption))
                {
                    return; 
                }
            }

            restartParameter = new VmCommandDetailsWrapper();
            restartParameter.Location = vm.Location;
            restartParameter.LogoffWaitTimeInSeconds = LogoffWaitTimeInSeconds <= 0 ? 60 : LogoffWaitTimeInSeconds;
            restartParameter.LogoffMessage = string.IsNullOrEmpty(LogoffMessage) ? string.Format(Commands_RemoteApp.DefaultLogoffMessage, restartParameter.LogoffWaitTimeInSeconds) : LogoffMessage;

            RemoteAppClient.RestartVm(ResourceGroupName, CollectionName, vm.VirtualMachineName, restartParameter);
        }

        public override void ExecuteCmdlet()
        {
            VmDetails vm = GetVm();

            if (vm == null)
            {
                WriteWarning(string.Format(Commands_RemoteApp.NoVmInCollectionForUserFormat, UserUpn, CollectionName));
                return;
            }

            RestartVm(vm);
        }
    }
}
