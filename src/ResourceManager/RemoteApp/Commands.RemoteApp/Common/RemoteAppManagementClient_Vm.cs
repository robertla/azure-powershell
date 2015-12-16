using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.RemoteApp.Common
{
    public partial class RemoteAppManagementClientWrapper
    {
        internal IList<VmDetails> GetVmDetails(string resourceGroupName, string collectionName)
        {
            VirtualMachineDetailsListResult virtalMachinesWrapper =  Client.Collection.ListVms(collectionName, resourceGroupName);
            IList<VmDetails> vmDetails = null;

            if (virtalMachinesWrapper != null)
            {
                vmDetails = virtalMachinesWrapper.Value;
            }

            return vmDetails;
        }

        internal void RestartVm(string resourceGroupName, string collectionName, string virtualMachine, VmCommandDetailsWrapper details)
        {
            Client.Collection.RestartVm(details, collectionName, resourceGroupName, virtualMachine);
        }
    }
}
