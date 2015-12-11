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
    [Cmdlet(VerbsCommon.Get, "AzureRmRemoteAppProgram", DefaultParameterSetName = FilterByName), OutputType(typeof(ApplicationDetails))]
    public class GetAzureRemoteAppProgram : RemoteAppArmResourceCmdletBase
    {
        private const string FilterByName = "FilterByName";
        private const string FilterByAlias = "FilterByAlias";

        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name. Wildcards are permitted.")]
        [ValidatePattern(NameValidatorStringWithWildCards)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        [Parameter(Mandatory = false,
            Position = 2,
            HelpMessage = "Name of the program. Wildcards are permitted.",
            ParameterSetName = FilterByName)]
        [ValidateNotNullOrEmpty()]
        public string RemoteAppProgram { get; set; }

        [Parameter(Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Published program alias",
            ParameterSetName = FilterByAlias)]
        [ValidateNotNullOrEmpty()]
        [Alias("ApplicationAlias")]
        public string Alias { get; set; }

        public class ApplicationComparer : IComparer<PublishedApplicationDetails>
        {
            public int Compare(PublishedApplicationDetails first, PublishedApplicationDetails second)
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

                return string.Compare(first.DisplayName, second.DisplayName, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private bool GetAllPublishedApps()
        {
            IEnumerable<PublishedApplicationDetails> response = null;
            List<PublishedApplicationDetails> spList = null;
            PublishedApplicationDetails appDetails = null;
            bool found = false;

            response = RemoteAppClient.GetApplications(ResourceGroupName, CollectionName);

            if (response != null)
            {
                if (ExactMatch)
                {
                    appDetails = response.FirstOrDefault(app => String.Equals(app.DisplayName, RemoteAppProgram, StringComparison.CurrentCultureIgnoreCase));
                    if (appDetails == null)
                    {
                        WriteErrorWithTimestamp("Program: " + RemoteAppProgram + " does not exist in collection " + CollectionName);
                        found = false;
                    }
                    else
                    {
                        WriteObject(appDetails);
                        found = true;
                    }
                }
                else
                {
                    if (UseWildcard)
                    {
                        spList = response.Where(app => Wildcard.IsMatch(app.DisplayName)).ToList();
                    }
                    else
                    {
                        spList = response.ToList();
                    }

                    if (spList != null && spList.Count() > 0)
                    {
                        IComparer<PublishedApplicationDetails> comparer = new ApplicationComparer();
                        spList.Sort(comparer);
                        WriteObject(spList, true);
                        found = true;
                    }
                }
            }

            return found;
        }

        private bool GetPublishedApp()
        {
            PublishedApplicationDetails response = null;
            bool found = false;

            response = RemoteAppClient.GetApplication(ResourceGroupName, CollectionName, Alias);

            if (response != null)
            {
                WriteObject(response);
                found = true;
            }

            return found;
        }


        public override void ExecuteCmdlet()
        {
            bool found = false;

            if (ParameterSetName == FilterByAlias)
            {
                found = GetPublishedApp();
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(RemoteAppProgram))
                {
                    CreateWildcardPattern(RemoteAppProgram);
                }

                found = GetAllPublishedApps();
            }

            if (!found)
            {
                WriteVerboseWithTimestamp(
                    String.Format("Collection {0} has no published program matching: {1}.",
                        CollectionName,
                        RemoteAppProgram)
                    );
            }
        }
    }
}
