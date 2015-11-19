using Hyak.Common;
using Microsoft.Azure.Commands.RemoteApp.Common;
using Microsoft.Azure.Commands.ResourceManager.Common;
using Microsoft.Azure.Management.RemoteApp.Models;
using System;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    public abstract partial class RemoteAppArmCmdletBase : AzureRMCmdlet
    {
        public RemoteAppArmCmdletBase()
        {

        }

        private RemoteAppManagementClientWrapper _RemoteAppClient = null;

        public RemoteAppManagementClientWrapper RemoteAppClient
        {
            get
            {
                if (_RemoteAppClient == null)
                {
                    _RemoteAppClient = new RemoteAppManagementClientWrapper(DefaultContext, DefaultContext.Subscription);
                }

                return _RemoteAppClient;
            }

            set
            {
                // for testing purpose only
                _RemoteAppClient = value;
            }
        }


        public abstract void ExecuteCmdlet();

        protected override void ProcessRecord()
        {
            try
            {
                this.ExecuteCmdlet();
            }
            catch (Exception e)
            {
                // Handle if this or the inner exception is of type CloudException
                CloudException ce = e as CloudException;
                ErrorRecord er = null;

                if (ce == null)
                {
                    ce = e.InnerException as CloudException;
                }

                if (ce != null)
                {
                    HandleCloudException(null, ce);
                }
                else
                {
                    er = RemoteAppCollectionErrorState.CreateErrorRecordFromException(e, String.Empty, null, ErrorCategory.NotSpecified);

                    ThrowTerminatingError(er);
                }


            }
        }


        private void HandleCloudException(object targetObject, CloudException e)
        {
            CloudRecordState cloudRecord = RemoteAppCollectionErrorState.CreateErrorStateFromCloudException(e, String.Empty, targetObject);
            if (cloudRecord.state.type == ExceptionType.NonTerminating)
            {
                WriteError(cloudRecord.er);
            }
            else
            {
                ThrowTerminatingError(cloudRecord.er);
            }
        }

        public Collection FindCollection(string ResourceGroupName, string collectionName)
        {
            Collection response = null;
            response = RemoteAppClient.Get(ResourceGroupName, collectionName);
            if (response == null)
            {
                WriteErrorWithTimestamp("Collection " + collectionName + " not found in resource group " + ResourceGroupName);
            }
            return response;
        }

    }
}
