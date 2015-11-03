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
using Microsoft.Azure.Commands.RemoteApp;


namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureRemoteAppCollectionUsageSummary")]
    public class GetAzureRemoteAppCollectionUsageSummary : RemoteAppArmResourceCmdletBase
    {
        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name. Wildcards are permitted.")]
        [ValidatePattern(NameValidatorStringWithWildCards)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Month to get collection usage for in MM format.")]
        [Alias("Month")]
        [ValidatePattern(TwoDigitMonthPattern)]
        public string UsageMonth { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Year to get collection usage for in YYYY format.")]
        [Alias("Year")]
        [ValidatePattern(FullYearPattern)]
        public string UsageYear { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 4,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Upn for a user to get usage information for.")]
        [Alias("Upn")]
        public string UserUpn { get; set; }

        public override void ExecuteRemoteAppCmdlet()
        {
            if (!String.IsNullOrWhiteSpace(CollectionName))
            {
                CreateWildcardPattern(CollectionName);
            }

            DateTime today = DateTime.Now;

            if (String.IsNullOrWhiteSpace(UsageMonth))
            {
                UsageMonth = today.Month.ToString();
            }

            if (String.IsNullOrWhiteSpace(UsageYear))
            {
                UsageYear = today.Year.ToString();
            }

            if (!String.IsNullOrWhiteSpace(UserUpn))
            {
                CollectionUsageSummary usage = RemoteAppClient.GetCollectionUsageForUser(UsageMonth, UsageYear, CollectionName, UserUpn, ResourceGroupName);

                if (usage.UserName != null)
                {
                    WriteObject(usage);
                }
                else
                {
                    WriteObject(String.Format("Usage dtaa not found for UserUpn: {0}", UserUpn));
                }
            }
            else
            {
                CollectionUsageSummaryList usageList = RemoteAppClient.GetCollectionUsage(UsageMonth, UsageYear, CollectionName, ResourceGroupName);

                if (usageList.Value.Count > 0)
                {
                    WriteObject(usageList.Value.Count);
                    WriteObject(usageList.Value);
                }
                else
                {
                    WriteObject(String.Format("No usage data found for colelction: {0}", CollectionName));
                }
            }
        }

    }
}
