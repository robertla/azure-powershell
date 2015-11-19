using Microsoft.Azure.Commands.RemoteApp.Cmdlet;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Commands.RemoteApp.Test.UnitTests
{
    public class GetAzureRemoteAppCollectionTest
    {
        [Fact]
        public void GetCollectionListTest()
        {
            Utilities.UnitTestHelper.CheckCmdletParameterAttributes(typeof(GetAzureRemoteAppCollection), "ResourceGroupName", true, true, 0);
            Utilities.UnitTestHelper.CheckCmdletParameterValidator(typeof(GetAzureRemoteAppCollection), "ResourceGroupName", typeof(ValidateNotNullOrEmptyAttribute), "");
            Utilities.UnitTestHelper.CheckCmdletParameterAttributes(typeof(GetAzureRemoteAppCollection), "CollectionName", false, true);
            Utilities.UnitTestHelper.CheckCmdletParameterValidator(typeof(GetAzureRemoteAppCollection), "CollectionName", typeof(ValidatePatternAttribute), @"^[?*A-Za-z0-9\u007F-\uFFFF]{1,13}$");
            Utilities.UnitTestHelper.CheckCmdletParameterValidator(typeof(GetAzureRemoteAppCollection), "CollectionName", typeof(AliasAttribute), "Name");
        }
    }
}
