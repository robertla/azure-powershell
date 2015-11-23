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
    [Cmdlet(VerbsCommon.Get, "AzureRmRemoteAppStartMenuProgram"), OutputType(typeof(StartMenuApplication))]
    public class GetStartMenuProgram : RemoteAppArmResourceCmdletBase
    {
        [Parameter(Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name")]
        [ValidatePattern(NameValidatorString)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        [Parameter(Mandatory = false,
            Position = 2,
            HelpMessage = "Unique alias of the program, Wildcards are permitted.")]
        [ValidateNotNullOrEmpty()]
        public string ProgramName { get; set; }

        public class ApplicationComparer : IComparer<StartMenuApplication>
        {
            public int Compare(StartMenuApplication first, StartMenuApplication second)
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

                return string.Compare(first.StartMenuApplicationName, second.StartMenuApplicationName, StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool GetStartMenuApps(bool getAllPrograms)
        {
            IList<StartMenuApplication> response = null;
            bool found = false;

            response = RemoteAppClient.GetStartMenuApps(ResourceGroupName, CollectionName);

            if (response != null)
            {
                if (ExactMatch)
                {
                    StartMenuApplication application = null;
                    application = response.FirstOrDefault(app => String.Equals(app.StartMenuApplicationName, ProgramName, StringComparison.InvariantCultureIgnoreCase));

                    if (application == null)
                    {
                        WriteErrorWithTimestamp("Program: " + ProgramName + " does not exist in collection " + CollectionName);
                        found = false;
                    }
                    else
                    {
                        WriteObject(application);
                        found = true;
                    }
                }
                else
                {
                    List<StartMenuApplication> matchingApps = null;
                    if (getAllPrograms)
                    {
                        matchingApps = response.ToList();
                    }
                    else if (UseWildcard)
                    {
                        matchingApps = response.Where(app => Wildcard.IsMatch(app.StartMenuApplicationName)).ToList(); ;
                    }

                    if (matchingApps != null && matchingApps.Count() > 0)
                    {
                        IComparer<StartMenuApplication> comparer = new ApplicationComparer();
                        matchingApps.Sort(comparer);
                        WriteObject(matchingApps, true);
                        found = true;
                    }
                }
            }
            return found;
        }

        public override void ExecuteCmdlet()
        {
            bool found = false;
            bool getAllPrograms = true;

            if (!String.IsNullOrWhiteSpace(ProgramName))
            {
                CreateWildcardPattern(ProgramName);
                getAllPrograms = false;
            }

            found = GetStartMenuApps(getAllPrograms);

            if (!found && !getAllPrograms)
            {
                WriteVerboseWithTimestamp(String.Format("Program '{0}' was not found in Collection '{1}'.", ProgramName, CollectionName));
            }
        }
    }
}