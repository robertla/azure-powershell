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
using Microsoft.Azure.Management.RemoteApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureRmRemoteAppUser"), OutputType(typeof(ConsentStatusModel))]
    public class GetAzureRemoteAppUser : RemoteAppArmResourceCmdletBase
    {
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
            HelpMessage = "User name. Wildcard pattern supported.")]
        [ValidateNotNullOrEmpty()]
        public string UserUpn { get; set; }


        public class ServicePrincipalComparer : IComparer<SecurityPrincipalInfo>
        {
            public int Compare(SecurityPrincipalInfo first, SecurityPrincipalInfo second)
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

                return string.Compare(first.User.Name, second.User.Name, StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool GetUsers(bool showAllUsers)
        {
            IList<SecurityPrincipalInfo> response = null;
            ConsentStatusModel model = null;
            bool found = false;

            response = RemoteAppClient.GetUsers(ResourceGroupName, CollectionName);

            if (ExactMatch)
            {
                SecurityPrincipalInfo userconsent = null;

                userconsent = response.FirstOrDefault(user => user.User.SecurityPrincipalType == PrincipalType.User &&
                     String.Equals(user.User.Name, UserUpn, StringComparison.OrdinalIgnoreCase));

                if (userconsent == null)
                {
                    WriteErrorWithTimestamp("User: " + UserUpn + " does not exist in collection " + CollectionName);
                    found = false;
                }
                else
                {
                    model = new ConsentStatusModel(userconsent);
                    WriteObject(model);
                    found = true;
                }
            }
            else
            {
                IEnumerable<SecurityPrincipalInfo> spList = null;

                if (showAllUsers)
                {
                    spList = response.Where(user => user.User.SecurityPrincipalType == PrincipalType.User);
                }
                else
                {
                    spList = response.Where(user => user.User.SecurityPrincipalType == PrincipalType.User &&
                        Wildcard.IsMatch(user.User.Name));
                }

                if (spList != null && spList.Count() > 0)
                {
                    List<SecurityPrincipalInfo> userConsents = new List<SecurityPrincipalInfo>(spList);
                    IComparer<SecurityPrincipalInfo> comparer = new ServicePrincipalComparer();

                    userConsents.Sort(comparer);
                    foreach (SecurityPrincipalInfo consent in spList)
                    {
                        model = new ConsentStatusModel(consent);
                        WriteObject(model);
                    }
                    found = true;
                }
            }

            return found;
        }

        public override void ExecuteCmdlet()
        {
            bool found = false;
            bool showAllUsers = String.IsNullOrWhiteSpace(UserUpn);

            if (showAllUsers == false)
            {
                CreateWildcardPattern(UserUpn);
            }

            found = GetUsers(showAllUsers);

            if (!found)
            {
                WriteVerboseWithTimestamp(String.Format(Commands_RemoteApp.CollectionNotFoundByNameFormat, CollectionName));
            }
        }
    }
}
