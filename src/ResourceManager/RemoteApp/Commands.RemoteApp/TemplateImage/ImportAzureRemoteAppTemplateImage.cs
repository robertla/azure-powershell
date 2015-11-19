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

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{

    public class ImageSasUri
    {
      public ImageSasUri(Uri sourceUri, string resourceGroupName)
        {
            SourceUri = sourceUri;
            ResourceName = resourceGroupName;
        }

        public Uri SourceUri { get; set; }

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
            ParameterSetName = AzureVmUpload,
            HelpMessage = "Sysprep-generalized VM image name in Azure")]
        [Alias("ComputerName")]
        public string AzureVmImageName { get; set; }

        [Parameter(Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = SasUriUpload,
            HelpMessage = "SAS URI for a Sysprep-generalized VM image in Azure")]
        public string SasUri { get; set; }

        [Parameter(Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Location in which this template image will be stored. Use Get-AzureRemoteAppLocation to see the locations available."
        )]
        public string Location { get; set; }

        public override void ExecuteCmdlet()
        {
            string imageName = null;
            string sasUri = null;
            ImageSasUri uploadParameters = null;
            TemplateImageCreateDetails details = null;
            TemplateImage response = null;

            switch (ParameterSetName)
            {
               case AzureVmUpload:
               {
                  imageName = AzureVmImageName.Trim();
                  uploadParameters = GetAzureVMImageUri(imageName);
                  break;
               }
               
                case SasUriUpload:
                {
                   imageName = SasUri.Trim();
                   uploadParameters = GetAzureOSImageUri(imageName);
                   break;
                }
            }

            sasUri = GetAzureImageSasUri(uploadParameters);

/// Testing only
/// 
            Uri uri = new Uri(sasUri);
            CloudPageBlob blob = new CloudPageBlob(uri);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            blob.DownloadToStream(ms);
            /// Testing only
            /// 

            details = new TemplateImageCreateDetails()
            {
                name = imageName,
                location = uploadParameters.ResourceName,
                sourceImageSasUri = sasUri,
            };

            response = RemoteAppClient.SetTemplateImage(details);
        
        }


        // https://github.com/Azure/azure-powershell/pull/1041


        //$vm2 = Get-AzureRmResourceGroup | Get-AzureRmVM

        // https://lqportalvhdsb65fscggcwhm.blob.core.windows.net/vhds/w00f-2042-529359-os-2015-11-16.vhd
        // https://9hportalvhds9q3g9nzy4w4p.blob.core.windows.net/vhds/kwiggle2-kwiggle2-2015-11-16.vhd

        //    TypeName: Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine

        //$vm2.StorageProfile.OSDisk.VirtualHardDisk
        //    Microsoft.Azure.Management.Compute.Models.OSDisk
        //$vm2.StorageProfile.ImageReference
        //    TypeName: Microsoft.Azure.Management.Compute.Models.ImageReference

        //Get-AzureRmVMImageOffer
        //Get-AzureRmVMImagePublisher
        //Get-AzureRmVMImageSku
        //Get-AzureRmVMImagePublisher  -Location westus | ? PublisherName -like *microsoft*
        //Get-AzureRmVMImageOffer -Location westus -PublisherName MicrosoftWindowsServer
        //Get-AzureRmVMImageSku -Location westus -PublisherName MicrosoftWindowsServer -Offer WindowsServer
        //Get-AzureRmVMImageSku -Location westus -PublisherName MicrosoftWindowsServer -Offer WindowsServer | ? Skus -eq 2012-R2-Datacenter
        //    TypeName: Microsoft.Azure.Commands.Compute.Models.PSVirtualMachineImageSku

        private string GetAzureImageSasUri(ImageSasUri uploadParameters)
        {
            StorageManagementClient storageClient = null;
            SharedAccessBlobPolicy sasConstraints = null;
            CloudStorageAccount storageAccount = null;
            CloudPageBlob pageBlob = null;
            ErrorRecord er = null;
            string sasUri = null;
            string storageAccountName = null;

            storageAccountName = uploadParameters.SourceUri.Authority.Split('.')[0];
            storageClient = AzureSession.ClientFactory.CreateClient <StorageManagementClient>(DefaultContext, AzureEnvironment.Endpoint.ResourceManager);
            storageAccount = StorageUtilities.GenerateCloudStorageAccount(storageClient, uploadParameters.ResourceName, storageAccountName);

            pageBlob = new CloudPageBlob(uploadParameters.SourceUri, storageAccount.Credentials);
            sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read;
            sasConstraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);  // Sometimes the clocks are 2-3 seconds fast and the SAS is not yet valid when the service tries to use it.
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(5);
            sasUri = pageBlob.GetSharedAccessSignature(sasConstraints);

            if (sasUri == null)
            {
                er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
                                     Commands_RemoteApp.FailedToGetSasUriError,
                                     String.Empty,
                                     null,
                                     ErrorCategory.ConnectionError
                                     );

                ThrowTerminatingError(er);
            }

            return uploadParameters.SourceUri.AbsoluteUri + sasUri;
        }

        private ImageSasUri GetAzureOSImageUri(string imageNameUri)
        {
            IComputeManagementClient computeClient = null;
            Uri uri = null;
            ImageSasUri uploadParameters = null;

            computeClient = AzureSession.ClientFactory.CreateArmClient<ComputeManagementClient>(DefaultContext, AzureEnvironment.Endpoint.ResourceManager);
//            computeClient.VirtualMachineImages.Get()

            if (Uri.TryCreate(imageNameUri, UriKind.Absolute, out uri) == false)
            {
                ;
            }

//            uploadParameters = new ImageSasUri(uri, resourceName.Groups["resourceName"].Value);

            return uploadParameters;
        }


        private ImageSasUri GetAzureVMImageUri(string vmName)
        {
            IComputeManagementClient computeClient = null;
            Page<VirtualMachine> vmList = null;
            Regex resourceNameRegEx = new Regex(@"/subscriptions/\S+/resourceGroups/(?<resourceName>\S+)/providers/\S+", RegexOptions.IgnoreCase);
            ImageSasUri uploadParameters = null;
            ErrorRecord er = null;

            computeClient = AzureSession.ClientFactory.CreateArmClient<ComputeManagementClient>(DefaultContext, AzureEnvironment.Endpoint.ResourceManager);
            vmList = computeClient.VirtualMachines.ListAll();

            foreach (VirtualMachine vm in vmList)
            {
                if (String.Equals(vm.OsProfile.ComputerName,  vmName, StringComparison.CurrentCultureIgnoreCase))
                {
                   VirtualHardDisk disk = null;
                   Match resourceName = resourceNameRegEx.Match(vm.Id);
                   string groupName = null;
                   
                   if (resourceName.Success)
                   {
                      groupName =  resourceName.Groups["resourceName"].Value;
                   }

                    VerifyWindowsConfiguration(vm);
                    disk = vm.StorageProfile.OsDisk.Vhd;

                    uploadParameters = new ImageSasUri(new Uri(disk.Uri), resourceName.Groups["resourceName"].Value);
                    break;
                }
            }

            if (uploadParameters == null)
            {
                er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
                                     Commands_RemoteApp.FailedToFindVMImage,
                                     String.Empty,
                                     null,
                                     ErrorCategory.ConnectionError
                                     );

                ThrowTerminatingError(er);
            }

            return uploadParameters;
        }

        private void VerifyWindowsConfiguration(VirtualMachine vm)
        {
            if (vm.StorageProfile.OsDisk.OsType != OperatingSystemTypes.Windows ||
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
    }
}
