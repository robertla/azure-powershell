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
    [Cmdlet(VerbsCommon.Get, "AzureRemoteAppCollection")]
    public class GetAzureRemoteAppCollection : RemoteAppArmResourceCmdletBase
    {
        [Parameter(
            Mandatory = false,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "RemoteApp collection name. Wildcards are permitted.")]
        [ValidatePattern(NameValidatorStringWithWildCards)]
        [Alias("Name")]
        public string CollectionName { get; set; }

        private bool found = false;

        private bool GetAllCollections()
        {
            IEnumerable<Collection> collections = null;
            IEnumerable<Collection> spList = null;

            collections = RemoteAppClient.ListCollections(ResourceGroupName);

            if (collections != null)
            {
                if (UseWildcard)
                {
                    spList = collections.Where(col => Wildcard.IsMatch(col.Name));
                }
                else
                {
                    spList = collections;
                }

                if (spList != null && spList.Count() > 0)
                {
                    WriteObject(spList, true);
                    found = true;
                }
            }

            return found;
        }

        private bool GetCollection(string collectionName)
        {
            Collection collection = RemoteAppClient.Get(ResourceGroupName, collectionName);

            if (collection != null)
            {
                WriteObject(collection);
                found = true;
            }

            return found;
        }

        public override void ExecuteRemoteAppCmdlet()
        {
            if (!String.IsNullOrWhiteSpace(CollectionName))
            {
                CreateWildcardPattern(CollectionName);
            }

            if (ExactMatch)
            {
                found = GetCollection(CollectionName);
            }
            else
            {
                found = GetAllCollections();
            }

            if (!found)
            {
                WriteVerboseWithTimestamp(String.Format(Commands_RemoteApp.CollectionNotFoundByNameFormat, CollectionName));
            }
        }
    }
}
