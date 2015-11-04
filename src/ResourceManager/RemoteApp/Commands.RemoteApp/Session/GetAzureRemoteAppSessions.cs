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
    [Cmdlet(VerbsCommon.Get, "AzureRemoteAppSession")]
    public class GetAzureRemoteAppSessions : RemoteAppArmResourceCmdletBase
    {
        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name.")]
        [ValidatePattern(NameValidatorString)]
        public string CollectionName { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "UserUpn. Wildcards are permitted.")]
        [ValidatePattern(UserPrincipalValdatorString)]
        public string UserUpn { get; set; }

        private bool found { get; set; }

        public override void ExecuteCmdlet()
        {
            found = false;

            if (String.IsNullOrWhiteSpace(CollectionName))
            {
                CreateWildcardPattern(CollectionName);
            }

            if (ExactMatch)
            {
                List<Session> sessions = RemoteAppClient.GetSessionList(ResourceGroupName,
                    CollectionName).ToList();

                if (sessions != null)
                {
                    if (UserUpn != null)
                    {
                        foreach (Session session in sessions)
                        {
                            if (session.UserUpn == UserUpn)
                            {
                                found = true;
                                WriteObject(session);
                            }
                        }
                        if (!found)
                        {
                            WriteVerboseWithTimestamp(String.Format(Commands_RemoteApp.SessionNotFoundByUserUpnFormat, UserUpn));
                        }
                    }
                    else
                    {
                        WriteObject(sessions, true);
                    }
                }
            }
            else
            {
                WriteVerboseWithTimestamp(String.Format(Commands_RemoteApp.CollectionNotFoundByNameFormat, CollectionName));
            }
        }
    }
}
