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
    [Cmdlet(VerbsCommon.Get, "AzureRmRemoteAppTemplateImage"),OutputType(typeof(TemplateImage),typeof(UserVhdImage))]
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

                return string.Compare(first.TemplateImageName, second.TemplateImageName, StringComparison.OrdinalIgnoreCase);
            }
        }

        public class VHDImageComparer : IComparer<UserVhdImage>
        {
            public int Compare(UserVhdImage first, UserVhdImage second)
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

                return string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase);
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
            List<TemplateImage> customerList = new List<TemplateImage>();
            List<UserVhdImage> userVhdList = new List<UserVhdImage>();
            IComparer<TemplateImage> templateImageComparer = null;
            IComparer<UserVhdImage> userVhdComparer = null;
            bool found = false;

            templateImages = RemoteAppClient.ListTemplateImages();
            vhds = GetAllUserVhd();

            if (vhds != null && vhds.Value != null)
            {
                foreach(ArmUserVhdWrapper armVhd in vhds.Value)
                {
                    if (String.Equals(armVhd.Properties.OperatingSystemDisk.OsState, "Generalized", StringComparison.CurrentCultureIgnoreCase) &&
                        String.Equals(armVhd.Properties.OperatingSystemDisk.OperatingSystem, "Windows", StringComparison.CurrentCultureIgnoreCase))
                    {
                        userVhdList.Add( new UserVhdImage()
                            {
                               Name = armVhd.Name,
                               OsState = armVhd.Properties.OperatingSystemDisk.OsState,
                               DiskName = armVhd.Properties.OperatingSystemDisk.DiskName,
                               OperatingSystem = armVhd.Properties.OperatingSystemDisk.OperatingSystem,
                               VhdUri = armVhd.Properties.OperatingSystemDisk.VhdUri
                            });
                    }
                }

                if (UseWildcard)
                {
                    userVhdList = userVhdList.Where((v => Wildcard.IsMatch(v.Name))).ToList();
                }

                userVhdComparer = new VHDImageComparer();
                userVhdList.Sort();
                WriteObject(userVhdList, true);
            }

            if (templateImages != null && templateImages.Count() > 0)
            {
                if (UseWildcard)
                {
                    templateImages = templateImages.Where(t => Wildcard.IsMatch(t.TemplateImageName));
                }

                foreach (TemplateImage image in templateImages)
                {
                    if (image.TemplateImageType == TemplateImageType.CustomerImage)
                    {
                        customerList.Add(image);
                    }
                    else
                    {
                        platformList.Add(image);
                    }
                }

                templateImageComparer = new TemplateImageComparer();
                customerList.Sort(templateImageComparer);
                WriteObject(customerList, true);

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
                { return String.Equals(d.Properties.OperatingSystemDisk.DiskName, templateImageName, StringComparison.InvariantCultureIgnoreCase); });

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
