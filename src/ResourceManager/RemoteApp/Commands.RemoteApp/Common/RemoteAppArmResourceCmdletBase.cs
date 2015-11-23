using System;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    public abstract partial class RemoteAppArmResourceCmdletBase : RemoteAppArmCmdletBase
    {
        public RemoteAppArmResourceCmdletBase()
        {

        }

        /// <summary>
        /// Gets or sets the automation account name.
        /// </summary>
        [Parameter(
            Position = 0, 
            Mandatory = true, 
            ValueFromPipelineByPropertyName = true, 
            HelpMessage = "The resource group name.")]
        [ValidatePattern(ResourceGroupValidatorString)]
        public virtual string ResourceGroupName { get; set; }

 
    }
}
