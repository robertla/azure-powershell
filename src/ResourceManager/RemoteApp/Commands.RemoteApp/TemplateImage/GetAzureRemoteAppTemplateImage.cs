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
        private ArmUserVhdArray GetAllUserVhd()
        {
            ErrorRecord er = null;
            RemoteAppManagementClient Client = this.RemoteAppClient.Client as RemoteAppManagementClient;
            HttpRequestMessage httpRequest = new HttpRequestMessage();
            CancellationToken cancellationToken = default(CancellationToken);
            HttpResponseMessage result = null;
            HttpStatusCode statusCode = HttpStatusCode.NotImplemented;
            Microsoft.Azure.Common.Authentication.Models.AzureEnvironment environment = this.DefaultProfile.Context.Environment;
            string responseContent = null;
            string uri = null;
            ArmUserVhdArray userVhds = null;

            foreach (KeyValuePair<Microsoft.Azure.Common.Authentication.Models.AzureEnvironment.Endpoint, string> endpoint in environment.Endpoints)
            {
                if (endpoint.Key == Microsoft.Azure.Common.Authentication.Models.AzureEnvironment.Endpoint.ResourceManager)
                {
                    uri = endpoint.Value;
                    break;
                }
            }

            uri += "subscriptions/{subscriptionId}/providers/Microsoft.ClassicStorage/images?api-version=2015-06-01";

            uri = uri.Replace("{subscriptionId}", Uri.EscapeDataString(Client.SubscriptionId));
            httpRequest.RequestUri = new Uri(uri);
            httpRequest.Method = new HttpMethod("GET");

            // Set Headers
            httpRequest.Headers.TryAddWithoutValidation("x-ms-client-request-id", Guid.NewGuid().ToString());
            httpRequest.Headers.TryAddWithoutValidation("accept-language", this.RemoteAppClient.Client.AcceptLanguage);

            // Set credentials
            cancellationToken.ThrowIfCancellationRequested();
            Client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).Wait();


            // Send request
            result = Client.HttpClient.SendAsync(httpRequest, cancellationToken).Result;
            statusCode = result.StatusCode;

            if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.Accepted)
            {
                string msg = "Operation returned an invalid status code: " + statusCode.ToString();
                er = RemoteAppCollectionErrorState.CreateErrorRecordFromString(msg, String.Empty, null, ErrorCategory.NotSpecified);

                ThrowTerminatingError(er);
            }

            responseContent = result.Content.ReadAsStringAsync().Result;
            userVhds = Newtonsoft.Json.JsonConvert.DeserializeObject<ArmUserVhdArray>(responseContent, Client.DeserializationSettings);

            return userVhds;
        }

        private bool WriteAllTemplateImages()
        {
            IEnumerable<TemplateImage> templateImages = null;
            ArmUserVhdArray vhds = null;
            List<TemplateImage> platformList = new List<TemplateImage>();
            List<TemplateImage> customerList = new List<TemplateImage>();
            IComparer<TemplateImage> comparer = null;

            templateImages = RemoteAppClient.ListTemplateImages();
            vhds = GetAllUserVhd();

            if (vhds != null && vhds.Value != null)
            {

                WriteObject(vhds.Value, true);
            }

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
            ArmUserVhdArray vhds = null;
            ArmUserVhdWrapper vhd = null;

            vhds = GetAllUserVhd();
            vhd = vhds.Value.Single((d) => 
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
