using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Azure.Management.RemoteApp.Models;

namespace LocalModels
{
    public class RestartVmDetails : VmDetails
    {
        public RestartVmDetails(VmDetails details, string collectionName)
        {
            LoggedOnUserUpn = details.LoggedOnUserUpn;
            VirtualMachineName = details.VirtualMachineName;
            CollectionName = collectionName;
        }
        public string CollectionName { get; set; }
    }
}
