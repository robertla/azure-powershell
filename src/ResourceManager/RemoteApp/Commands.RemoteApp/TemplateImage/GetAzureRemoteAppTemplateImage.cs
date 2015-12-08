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
    [Cmdlet(VerbsCommon.Get, "AzureRmRemoteAppTemplateImage")]
    public class GetAzureRemoteAppTemplateImage : RemoteAppArmCmdletBase
    {
        [Parameter(
            Mandatory = false,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of template image.")]
        [ValidateNotNullOrEmpty]
        public string TemplateImageName { get; set; }


        private bool found = false;


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

        private bool WriteAllTemplateImages()
        {
            IEnumerable<TemplateImage> templateImages = null;
            List<TemplateImage> platformList = null;
            List<TemplateImage> customerList = null;
            IComparer<TemplateImage> comparer = null;

            templateImages = RemoteAppClient.ListTemplateImages();

            if (templateImages != null && templateImages.Count() > 0)
            {
                if (UseWildcard)
                {
                    templateImages = templateImages.Where(t => Wildcard.IsMatch(t.TemplateImageName));
                }

                comparer = new TemplateImageComparer();

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

                customerList.Sort(comparer);
                WriteObject(customerList, true);

                platformList.Sort(comparer);
                WriteObject(platformList, true);

                found = true;
            }

            return found;
        }

        private bool WriteTemplateImage(string templateImageName)
        {
            TemplateImage templateImage = RemoteAppClient.GetTemplateImage(templateImageName);

            if (templateImage != null)
            {
                WriteObject(templateImage);
                found = true;
            }

            return found;
        }

        public override void ExecuteCmdlet()
        {
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
