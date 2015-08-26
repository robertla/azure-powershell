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
using System.Management.Automation;
using System.Reflection;
using Xunit;

namespace Microsoft.Azure.Commands.RemoteApp.Test.Utilities
{
    /// <summary>
    /// Common helper functions for SqlDatabase UnitTests.
    /// </summary>
    public static class UnitTestHelper
    {
        /// <summary>
        /// Verifies the ConfirmImpact level on a cmdlet.
        /// </summary>
        /// <param name="cmdlet">The cmdlet to check.</param>
        /// <param name="confirmImpact">The expected confirm impact.</param>
        public static void CheckConfirmImpact(Type cmdlet, ConfirmImpact confirmImpact)
        {
            object[] cmdletAttributes = cmdlet.GetCustomAttributes(typeof(CmdletAttribute), true);
            Assert.Equal(1, cmdletAttributes.Length);
            CmdletAttribute attribute = (CmdletAttribute)cmdletAttributes[0];
            Assert.Equal(confirmImpact, attribute.ConfirmImpact);
        }

        /// <summary>
        /// Verifies if a cmdlet is suppose to modify data or not.
        /// </summary>
        /// <param name="cmdlet">The cmdlet to check.</param>
        /// <param name="supportsShouldProcess">Whether or not the cmdlet is expected to modify data.</param>
        public static void CheckCmdletModifiesData(Type cmdlet, bool supportsShouldProcess)
        {
            // If the Cmdlet modifies data, SupportsShouldProcess should be set to true.
            object[] cmdletAttributes = cmdlet.GetCustomAttributes(typeof(CmdletAttribute), true);
            Assert.Equal(1, cmdletAttributes.Length);
            CmdletAttribute attribute = (CmdletAttribute)cmdletAttributes[0];
            Assert.Equal(supportsShouldProcess, attribute.SupportsShouldProcess);

            if (supportsShouldProcess)
            {
                // If the Cmdlet modifies data, there needs to be a Force property to bypass
                // ShouldProcess.
                Assert.True(
                    cmdlet.GetProperty("Force") != null,
                    "Force property is expected for Cmdlets that modifies data.");
            }
        }

        /// <summary>
        /// Asserts that a parameter has the Mandatory and ValueFromPipelineByName flags set correctly
        /// Also checks to ensure that there is a help message.
        /// </summary>
        /// <param name="cmdlet">The cmdlet type to check</param>
        /// <param name="parameterName">The name of the parameter/property to verify</param>
        /// <param name="isMandatory">Whether the parameter should be mandatory or not</param>
        public static void CheckCmdletParameterAttributes(Type cmdlet, string parameterName, bool isMandatory, bool valueFromPipelineByName, int position = -1)
        {
            object[] attributes = GetCustomAttribute(cmdlet, parameterName, typeof(ParameterAttribute));
            Assert.Equal(1, attributes.Length);
            ParameterAttribute paramAttr = attributes[0] as ParameterAttribute;
            Assert.NotNull(paramAttr);
            Assert.Equal(isMandatory, paramAttr.Mandatory);
            Assert.Equal(valueFromPipelineByName, paramAttr.ValueFromPipelineByPropertyName);
            Assert.NotNull(paramAttr.HelpMessage);
            if (position != -1)
            {
                Assert.Equal(position, paramAttr.Position);
            }
        }

        private static object[] GetCustomAttribute(Type cmdlet, string parameterName, Type customAttribute)
        {
            PropertyInfo property = cmdlet.GetProperty(parameterName);
            Assert.NotNull(property);

            object[] attributes = property.GetCustomAttributes(customAttribute, true);

            return attributes;
        }

        /// <summary>
        /// Use reflection to invoke a private member of an object.
        /// </summary>
        /// <param name="instance">The object on which to invoke the method.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="paramerters">An array of parameters for this method.</param>
        /// <returns>The return value for the method.</returns>
        public static object InvokePrivate(
            object instance,
            string methodName,
            params object[] paramerters)
        {
            Type cmdletType = instance.GetType();
            MethodInfo getManageUrlMethod = cmdletType.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            try
            {
                return getManageUrlMethod.Invoke(instance, paramerters);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public static void CheckCmdletParameterValidator(Type cmdlet, string parameterName, Type validatorType, string pattern)
        {
            object[] attributes = GetCustomAttribute(cmdlet, parameterName, validatorType);
            Assert.Equal(1, attributes.Length);

            if (validatorType == typeof(ValidateNotNullOrEmptyAttribute))
            {
                ValidateNotNullOrEmptyAttribute attribute = attributes[0] as ValidateNotNullOrEmptyAttribute;
                Assert.NotNull(attribute);
            }
            else if (validatorType == typeof(ValidatePatternAttribute))
            {
                ValidatePatternAttribute attribute = attributes[0] as ValidatePatternAttribute;
                Assert.NotNull(attribute);
                Assert.Equal(pattern, attribute.RegexPattern);
            }
            else if (validatorType == typeof(AliasAttribute))
            {
                AliasAttribute attribute = attributes[0] as AliasAttribute;
                Assert.NotNull(attribute);
                Assert.Contains(pattern, attribute.AliasNames);
            }
            else
            {
                String message = String.Format("May be invalid validation attribute is used. Otherwise please add a case for the attribute: {0}", validatorType.ToString());
                Assert.False(true, message);
            }
        }
    }
}
