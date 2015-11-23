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
using Microsoft.Azure.Management.RemoteApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;


using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Common.Authentication;
using Microsoft.Azure.Common.Authentication.Models;
using Microsoft.Rest.Azure;
using Microsoft.Azure.Management.Compute.Models;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.WindowsAzure.Commands.Common.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage.Auth;


namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{

    public class BlobUri
    {
      internal static bool TryParseUri(string sourceUri, string resourceGroupName, out BlobUri sasUri) 
      {
          Uri uri = null;
          string[] segments = null;
          string storageAccountName = null;
          string storageDnsName = null;
          string storageContainerName = null;
          string storageBlobName = null;
          string baseUri = null;

          sasUri = null;

          if (!Uri.TryCreate(sourceUri, UriKind.Absolute, out uri))
          {
              return false;
          }

          segments = uri.DnsSafeHost.ToLower().Split('.');
          if (segments.Count() < 2)  // Host must be a FQDN
          {
              return false;
          }

          // Name of host is the storage account name
          storageAccountName = segments[0];

          // DNS name is network name minus host name
          storageDnsName = String.Join(".", segments, 1, segments.Count() - 1);


          segments = uri.AbsolutePath.ToLower().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

          // Name of host is the storage account name
          storageContainerName = segments[0]; // use System.Web.HttpUtility.UrlDecode

          // Name of blob is network name minus host name
          storageBlobName = String.Join("/", segments, 1, segments.Count() - 1);
          baseUri = uri.Scheme + Uri.SchemeDelimiter + uri.DnsSafeHost;

          sasUri = new BlobUri(uri, storageAccountName, storageDnsName, storageContainerName, storageBlobName, resourceGroupName);
          
          return true;
      }

      public BlobUri(Uri uri, string accountName, string domainName, string containerName, string blobName, string resourceGroupName)
      {
          SourceUri = uri;
          StorageAccountName = accountName;
          StorageDomainName = domainName;
          BlobContainerName = containerName;
          BlobName = blobName;
          ResourceName = resourceGroupName;
      }

        public Uri SourceUri { get; set; }

        public string StorageAccountName { get; set; }

        public string StorageDomainName { get; set; }

        public string BlobContainerName { get; set; }

        public string BlobName { get; set; }

        public string SasUri { get; set; }

        public string ResourceName { get; set; }
    }

    [Cmdlet(VerbsData.Import, "AzureRmRemoteAppTemplateImage", DefaultParameterSetName = AzureVmUpload)]
    public class ImportAzureRemoteAppTemplateImage : RemoteAppArmCmdletBase
    {
        private const string AzureVmUpload = "AzureVmUpload";
        private const string SasUriUpload = "SASUriUpload";

        [Parameter(Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Location where this template image will be stored. Use Get-AzureRemoteAppLocation to see the locations available."
        )]
        public string Location { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name for the template image.")]
        public string TemplateImageName { get; set; }

        [Parameter(Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = AzureVmUpload,
            HelpMessage = "Name of virtual machine with a Sysprep-generalized image in Azure")]
        [Alias("ComputerName")]
        public string AzureVmName { get; set; }

        [Parameter(Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = SasUriUpload,
            HelpMessage = "SAS URI for a Sysprep-generalized VM image in Azure")]
        public string SasUri { get; set; }

        private string GetAzureImageSasUri(string imageUrl, string groupName)
        {
            Uri uri = null;
            StorageManagementClient storageClient = null;
            SharedAccessBlobPolicy sasConstraints = null;
            CloudStorageAccount storageAccount = null;
            CloudPageBlob pageBlob = null;
            ErrorRecord er = null;
            string sasQuery = null;
            string storageAccountName = null;


            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out uri))
            {
                er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
                                     Commands_RemoteApp.FailedToGetSasUriError,
                                     String.Empty,
                                     null,
                                     ErrorCategory.ConnectionError
                                     );

                ThrowTerminatingError(er);
            }

            storageAccountName = uri.DnsSafeHost.ToLower().Split('.')[0];

            storageClient = AzureSession.ClientFactory.CreateClient <StorageManagementClient>(DefaultContext, AzureEnvironment.Endpoint.ResourceManager);
            storageAccount = StorageUtilities.GenerateCloudStorageAccount(storageClient, groupName, storageAccountName);


            pageBlob = new CloudPageBlob(uri, storageAccount.Credentials);
            sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List;
            sasConstraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);  // Sometimes the clocks are 2-3 seconds fast and the SAS is not yet valid when the service tries to use it.
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(5);
            sasQuery = pageBlob.GetSharedAccessSignature(sasConstraints);

            if (sasQuery == null)
            {
                er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
                                     Commands_RemoteApp.FailedToGetSasUriError,
                                     String.Empty,
                                     null,
                                     ErrorCategory.ConnectionError
                                     );

                ThrowTerminatingError(er);
            }

            return uri.AbsoluteUri + sasQuery;
        }

        private void GetAzureOSImageUri(string imageNameUri, out string url, out string resourceGroupName)
        {
            IComputeManagementClient computeClient = null;

            // This will be updating shortly please ignore for now
            url = null;
            resourceGroupName = null;

            computeClient = AzureSession.ClientFactory.CreateClient<ComputeManagementClient>(DefaultContext, AzureEnvironment.Endpoint.ResourceManager);

        }


        private void GetAzureVMImageUri(string vmName, out string url, out string resourceGroupName)
        {
            IComputeManagementClient computeClient = null;
            ListParameters listParameters = new ListParameters();
            VirtualMachineListResponse vmListResult = null;
            Regex resourceNameRegEx = new Regex(@"/subscriptions/\S+/resourceGroups/(?<resourceName>\S+)/providers/\S+", RegexOptions.IgnoreCase);
            ErrorRecord er = null;
            url = null;
            resourceGroupName = null;

            computeClient = AzureSession.ClientFactory.CreateClient<ComputeManagementClient>(DefaultContext, AzureEnvironment.Endpoint.ResourceManager);

            vmListResult = computeClient.VirtualMachines.ListAll(listParameters);

            if (vmListResult != null && vmListResult.VirtualMachines != null)
            {
                foreach (VirtualMachine vm in vmListResult.VirtualMachines)
                {
                    if (String.Equals(vm.OSProfile.ComputerName, vmName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Match resourceName = resourceNameRegEx.Match(vm.Id);

                        if (!resourceName.Success)
                        {
                            er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
                                                 Commands_RemoteApp.FailedToFindVMImage,
                                                 String.Empty,
                                                 null,
                                                 ErrorCategory.ConnectionError
                                                 );

                            ThrowTerminatingError(er);
                        }

                        VerifyWindowsConfiguration(vm);
                        resourceGroupName = resourceName.Groups["resourceName"].Value;
                        url = vm.StorageProfile.OSDisk.VirtualHardDisk.Uri;

                        break;
                    }
                }
            }
        }

        private void VerifyWindowsConfiguration(VirtualMachine vm)
        {
            if (vm.StorageProfile.OSDisk.OperatingSystemType != OperatingSystemTypes.Windows ||
                vm.StorageProfile.ImageReference.Offer != "WindowsServer" ||
                vm.StorageProfile.ImageReference.Publisher != "MicrosoftWindowsServer" ||
                (vm.StorageProfile.ImageReference.Sku != "2012-Datacenter" &&
                 vm.StorageProfile.ImageReference.Sku != "2012-R2-Datacenter"))
            {
                ErrorRecord er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
                                            String.Format(Commands_RemoteApp.InvalidOsTypeErrorFormat, vm.StorageProfile.ImageReference.Sku),
                                            String.Empty,
                                            null,
                                            ErrorCategory.InvalidArgument
                                            );

                ThrowTerminatingError(er);
            }
        }

        public override void ExecuteCmdlet()
        {
            string imageUrl = null;
            string groupName = null;
            string imageName = null;
            string sasUri = null;
            BlobUri uploadParameters = null;
            TemplateImageCreateDetails details = null;
            TemplateImage response = null;

            switch (ParameterSetName)
            {
                case AzureVmUpload:
                    {
                        imageName = AzureVmName.Trim();
                        GetAzureVMImageUri(imageName, out imageUrl, out groupName);
                        break;
                    }

                case SasUriUpload:
                    {
                        imageName = SasUri.Trim();
                        GetAzureOSImageUri(imageName, out imageUrl, out groupName);
                        break;
                    }
            }

            BlobUri.TryParseUri(imageUrl, groupName, out uploadParameters);
            sasUri = GetAzureImageSasUri(imageUrl, groupName);

            details = new TemplateImageCreateDetails()
            {
                SourceImageSasUri = sasUri,
            };

            response = RemoteAppClient.CreateOrUpdateTemplateImage(Location, TemplateImageName, details);

        }
    }
}
