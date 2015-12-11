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

using LocalModels;
using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureRmRemoteAppTemplateImage"), OutputType(typeof(TemplateImage), typeof(CustomerImage))]
    public class GetAzureRemoteAppTemplateImage : RemoteAppArmCmdletBase
    {
        [Parameter(
            Mandatory = false,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of template image or user vhd.")]
        [ValidateNotNullOrEmpty]
        public string TemplateImageName { get; set; }


        public class TemplateImageComparer : IComparer<TemplateImage>
        {
            public int Compare(TemplateImage first, TemplateImage second)
            {
                if (first == null)
                {
                    if (second == null)
                    {
                        return 0; // both null are equal
                    }
                    else
                    {
                        return -1; // second is greateer
                    }
                }
                else
                {
                    if (second == null)
                    {
                        return 1; // first is greater as it is not null
                    }
                }

                return string.Compare(first.TemplateImageName, second.TemplateImageName, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public class CustomerImageComparer : IComparer<CustomerImage>
        {
            public int Compare(CustomerImage first, CustomerImage second)
            {
                if (first == null)
                {
                    if (second == null)
                    {
                        return 0; // both null are equal
                    }
                    else
                    {
                        return -1; // second is greateer
                    }
                }
                else
                {
                    if (second == null)
                    {
                        return 1; // first is greater as it is not null
                    }
                }

                return string.Compare(first.TemplateImageName, second.TemplateImageName, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private ArmUserVhdArray GetAllUserVhd()
        {
            RemoteAppManagementClient client = this.RemoteAppClient.Client as RemoteAppManagementClient;
            Microsoft.Azure.Common.Authentication.Models.AzureEnvironment environment = this.DefaultProfile.Context.Environment;
            ArmHttpCallUtility armHttpCall = new ArmHttpCallUtility(environment, client);
            ArmUserVhdArray userVhds = null;
            string resourceUri = "Microsoft.ClassicStorage/images?api-version=2015-06-01";
            string responseContent = null;

            try
            {
                responseContent = armHttpCall.GetHttpArmResource(resourceUri);
            }
            catch (ArmHttpException e)
            {
                ErrorRecord er = new ErrorRecord(e, e.ExceptionMessage, e.Category, null);
                WriteError(er);
            }

            userVhds = Newtonsoft.Json.JsonConvert.DeserializeObject<ArmUserVhdArray>(responseContent, client.DeserializationSettings);

            return userVhds;
        }

        private bool WriteAllTemplateImages()
        {
            IEnumerable<TemplateImage> templateImages = null;
            ArmUserVhdArray vhds = null;
            List<TemplateImage> platformList = new List<TemplateImage>();
            List<CustomerImage> customerList = new List<CustomerImage>();
            IComparer<TemplateImage> templateImageComparer = null;
            IComparer<CustomerImage> customerImageComparer = null;
            bool found = false;

            templateImages = RemoteAppClient.ListTemplateImages();
            vhds = GetAllUserVhd();

            if (vhds != null && vhds.Value != null)
            {
                foreach (ArmUserVhdWrapper armVhd in vhds.Value)
                {
                    if (String.Equals(armVhd.Properties.OperatingSystemDisk.OsState, "Generalized", StringComparison.InvariantCultureIgnoreCase) &&
                        String.Equals(armVhd.Properties.OperatingSystemDisk.OperatingSystem, "Windows", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!UseWildcard || Wildcard.IsMatch(armVhd.Name))
                        {
                            customerList.Add(new CustomerImage()
                            {
                                TemplateImageName = armVhd.Name,
                                LocationList = new List<string>()
                                {
                                    new string(armVhd.Location.ToCharArray()),
                                },
                                TemplateImageType = ImageType.CustomerVhd,
                            });
                        }
                    }
                }
            }

            if (templateImages != null && templateImages.Count() > 0)
            {
                foreach (TemplateImage image in templateImages)
                {
                    // Unless we are filtering based off the image name always add the image

                    if (image.TemplateImageType == TemplateImageType.CustomerImage)
                    {
                        if (!UseWildcard || Wildcard.IsMatch(image.TemplateImageName))
                        {
                            customerList.Add(new CustomerImage()
                            {
                                TemplateImageName = image.TemplateImageName,
                                TemplateImageType = CustomerImage.ConvertTemplateImageTypetoUserImageType(image.TemplateImageType),
                                Status = image.Status,
                                LocationList = image.LocationList,
                                UploadCompleteTime = image.UploadCompleteTime,
                                NumberOfLinkedCollections = image.NumberOfLinkedCollections,
                            });
                        }
                    }
                    else
                    {
                        if (!UseWildcard || Wildcard.IsMatch(image.TemplateImageName))
                        {
                            platformList.Add(image);
                        }
                    }
                }

                customerImageComparer = new CustomerImageComparer();
                customerList.Sort(customerImageComparer);
                WriteObject(customerList, true);

                templateImageComparer = new TemplateImageComparer();
                platformList.Sort(templateImageComparer);
                WriteObject(platformList, true);

                found = true;
            }

            return found;
        }

        private bool WriteTemplateImage(string templateImageName)
        {
            TemplateImage templateImage = RemoteAppClient.GetTemplateImage(templateImageName);
            ArmUserVhdArray vhds = null;
            ArmUserVhdWrapper vhd = null;
            bool found = false;

            vhds = GetAllUserVhd();
            vhd = vhds.Value.First((d) => 
                { return String.Equals(d.Properties.OperatingSystemDisk.DiskName, templateImageName, StringComparison.CurrentCultureIgnoreCase); });

            if (vhd != null)
            {
                WriteObject(vhd);
                found = true;
            }

            if (templateImage != null)
            {
                WriteObject(templateImage);
                found = true;
            }

            return found;
        }

        public override void ExecuteCmdlet()
        {
            bool found = false;

            if (!String.IsNullOrWhiteSpace(TemplateImageName))
            {
                CreateWildcardPattern(TemplateImageName);
            }

            if (ExactMatch)
            {
                found = WriteTemplateImage(TemplateImageName);
            }
            else
            {
                found = WriteAllTemplateImages();
            }

            if (!found)
            {
                WriteVerboseWithTimestamp(String.Format(Commands_RemoteApp.CollectionNotFoundByNameFormat, TemplateImageName));
            }        
        }
    }
}
