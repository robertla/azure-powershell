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
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    public abstract class SecurityPrincipalsCmdletBase : RemoteAppArmResourceCmdletBase
    {
        public enum Operation
        {
            Add,
            Remove
        }

        public SecurityPrincipalOperationErrorDetails ProcessUsers(Operation op, string collectionName, PrincipalProviderType provider, string userUpn)
        {
            SecurityPrincipalOperationErrorDetails response = null;
            SecurityPrincipal user = null;

            user = new SecurityPrincipal()
            {
                AadObjectId = null,
                Description = null,
                Name = userUpn,
                SecurityPrincipalType = PrincipalType.User,
                UserIdType = provider
            };

            if (op == Operation.Add)
            {
                response = RemoteAppClient.AddUser(ResourceGroupName, collectionName, user);
            }
            else
            {
                response = RemoteAppClient.DeleteUser(ResourceGroupName, collectionName, user);
            }

            return response;
        }
    }
}
