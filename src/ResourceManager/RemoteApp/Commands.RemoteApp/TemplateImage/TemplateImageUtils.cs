using Microsoft.Azure.Management.RemoteApp.Models;
using Microsoft.Azure.Management.RemoteApp;
using System;
using System.Collections.Generic;
using System.Management.Automation;

using Microsoft.Azure.Management.Compute;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{

    public enum Operation
    {
        Create,
        Update,
        Remove,
        Resume,
    }

    public class TemplateImageCreateDetails // BUGBUG
    {
       public string name;

       public string location;

       public string sourceImageSasUri;
    }

    public class TemplateImageUtils
    {
        public static void ImportTemplateImage(string imageName, string location)
        {
#if false
            TemplateImageResult response = null;
            TemplateImageDetails details = null;

            FilterTemplateImage(imageName, Operation.Create);

            details = new TemplateImageDetails()
            {
                Name = ImageName,
                Region = Location,
                SourceImageSasUri = GetAzureVmSasUri(AzureVmImageName)
            };

//            response = CallClient(() => Client.TemplateImages.Set(details), Client.TemplateImages);
            response = RemoteAppClient.CreateOrUpdateCollection(ResourceGroupName, CollectionName, details);

            if (response != null)
            {
                WriteObject(response.TemplateImage);
            }
#endif
        }


        public static TemplateImage FilterTemplateImage(string TemplateImageName, Operation op)
        {
            TemplateImage matchingTemplate = null;
            string errorMessage = null;
            ErrorCategory category = ErrorCategory.NotSpecified;

            //IEnumerable<TemplateImage> response = null;

            //response = RemoteAppClient.GetTemplateImages();

//            response = CallClient_ThrowOnError(() => Client.TemplateImages.List());

            //foreach (TemplateImage template in response.RemoteAppTemplateImageList)
            //{
            //    if (String.Equals(template.Name, TemplateImageName, StringComparison.OrdinalIgnoreCase))
            //    {
            //        matchingTemplate = template;
            //        break;
            //    }
            //}

            switch (op)
            {
                case Operation.Remove:
                case Operation.Update:
                    {
                        if (matchingTemplate == null)
                        {
                            errorMessage = String.Format("Template {0} does not exist.", TemplateImageName);
                            category = ErrorCategory.ObjectNotFound;
                        }
                        break;
                    }
                case Operation.Create:
                    {
                        if (matchingTemplate != null)
                        {
                            errorMessage = String.Format("There is an existing template named {0}.", TemplateImageName);
                            category = ErrorCategory.ResourceExists;
                        }
                        break;
                    }
            }

            if (errorMessage != null)
            {
                throw new RemoteAppServiceException(errorMessage, category);
            }

            return matchingTemplate;
        }

        //private string GetVmImageUri(string imageName)
        //{
        //    ComputeManagementClient computeClient = new ComputeManagementClient(this.Client.Credentials, this.Client.BaseUri);
        //    VirtualMachineVMImageListResponse vmList = null;
        //    ErrorRecord er = null;
        //    string imageUri = null;

        //    try
        //    {
        //        computeClient.VirtualMachineImages.Get
        //        vmList = computeClient.VirtualMachineVMImages.List();
        //    }
        //    catch (Exception ex)
        //    {
        //        er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
        //                        ex.Message,
        //                        String.Empty,
        //                        Client.TemplateImages,
        //                        ErrorCategory.InvalidArgument
        //                        );

        //        ThrowTerminatingError(er);
        //    }

        //    foreach (VirtualMachineVMImageListResponse.VirtualMachineVMImage image in vmList.VMImages)
        //    {
        //        if (string.Compare(image.Name, imageName, true) == 0)
        //        {
        //            if (image.OSDiskConfiguration != null)
        //            {
        //                ValidateImageOsType(image.OSDiskConfiguration.OperatingSystem);
        //                ValidateImageMediaLink(image.OSDiskConfiguration.MediaLink);

        //                imageUri = image.OSDiskConfiguration.MediaLink.AbsoluteUri;
        //                break;
        //            }
        //            else
        //            {
        //                er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
        //                        string.Format(Commands_RemoteApp.NoOsDiskFoundErrorFormat, imageName),
        //                        String.Empty,
        //                        Client.TemplateImages,
        //                        ErrorCategory.InvalidArgument
        //                        );

        //                ThrowTerminatingError(er);
        //            }
        //        }
        //    }

        //    if (imageUri == null)
        //    {
        //        er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(
        //                            string.Format(Commands_RemoteApp.NoVmImageFoundErrorFormat, imageName),
        //                            String.Empty,
        //                            Client.TemplateImages,
        //                            ErrorCategory.InvalidArgument
        //                            );

        //        ThrowTerminatingError(er);
        //    }

        //    return imageUri;
        //}
    }
}
