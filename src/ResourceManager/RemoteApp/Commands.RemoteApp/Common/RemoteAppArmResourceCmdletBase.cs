using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Management.RemoteApp;
using System.Management.Automation;
using Microsoft.Azure.Commands.RemoteApp.Common;
using Hyak.Common;
using System.Net;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    public abstract partial class RemoteAppArmResourceCmdletBase : RemoteAppArmCmdletBase
    {
        public RemoteAppArmResourceCmdletBase()
        {

        }

        protected WildcardPattern Wildcard { get; private set; }

        protected bool UseWildcard
        {
            get { return Wildcard != null; }
        }

        protected bool ExactMatch { get; private set; }

        protected void CreateWildcardPattern(string name)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(name))
                {
                    ExactMatch = !WildcardPattern.ContainsWildcardCharacters(name);

                    Wildcard = new WildcardPattern(name, WildcardOptions.IgnoreCase);
                }
            }
            catch (WildcardPatternException e)
            {
                ErrorRecord er = new ErrorRecord(e, "", ErrorCategory.InvalidArgument, Wildcard);
                ThrowTerminatingError(er);
            }
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

        public abstract void ExecuteRemoteAppCmdlet();

        public override void ExecuteCmdlet()
        {
            try
            {
                this.ExecuteRemoteAppCmdlet();
            }
            catch (CloudException)
            {
                // TODO: add code to display properly formatted error information here, if desired
                throw;
            }
        }
    }
}
