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

using System;
using System.Text.RegularExpressions;

namespace LocalModels
{
    public class Collection : Microsoft.Azure.Management.RemoteApp.Models.Collection
    {
        public DateTime LastModifiedLocalTime { get; set; }

        public string ResourceGroupName { get; set; }

        public Collection(Microsoft.Azure.Management.RemoteApp.Models.Collection col)
        {
            Regex resourceNameRegEx = new Regex(@"/subscriptions/\S+/resourceGroups/(?<resourceGroupName>\S+)/providers/\S+", RegexOptions.IgnoreCase);
            Match resourceGroupName = resourceNameRegEx.Match(col.Id);

            AdInfo = col.AdInfo;
            BillingPlanName = col.BillingPlanName;
            CollectionType = col.CollectionType;
            CustomRdpProperty = col.CustomRdpProperty;
            Description = col.Description;
            DnsServers = col.DnsServers;
            LastErrorCode = col.LastErrorCode;
            LastModifiedTimeUtc = col.LastModifiedTimeUtc;
            MaxSessions = col.MaxSessions;
            Mode = col.Mode;
            CollectionName = col.CollectionName;
            OfficeType = col.OfficeType;
            ReadyForPublishing = col.ReadyForPublishing;
            Location = col.Location;
            SessionWarningThreshold = col.SessionWarningThreshold;
            Status = col.Status;
            SubnetName = col.SubnetName;
            TemplateImageName = col.TemplateImageName;
            TrialOnly = col.TrialOnly;
            VnetName = String.IsNullOrWhiteSpace(col.VnetName) || col.VnetName.StartsWith("simplevnet-", StringComparison.InvariantCultureIgnoreCase) ? "" : col.VnetName;
            
            if (col.LastModifiedTimeUtc.HasValue) 
            {
                LastModifiedLocalTime = col.LastModifiedTimeUtc.Value.ToLocalTime();
            }

            if (resourceGroupName.Success)
            {
                ResourceGroupName = resourceGroupName.Groups["resourceGroupName"].Value;
            }
        }
    }
}
