using Microsoft.Azure.Commands.RemoteApp.Cmdlet;
using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RemoteApp.Common
{
    public partial class RemoteAppManagementClientWrapper
    {
        // BUGBUG
        internal IEnumerable<TemplateImage> GetTemplateImages()
        {

//            return Client.TemplateImage.GetTemplateImages();
            return null;
        }

        internal TemplateImage SetTemplateImage(TemplateImageCreateDetails details)
        {
            return null;
        }

        internal void DeleteTemplateImages()
        {
        }
            
    }
}
