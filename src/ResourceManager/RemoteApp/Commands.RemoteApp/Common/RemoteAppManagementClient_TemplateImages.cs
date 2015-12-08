using Microsoft.Azure.Commands.RemoteApp.Cmdlet;
using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RemoteApp.Common
{
    public partial class RemoteAppManagementClientWrapper
    {

        internal IList<TemplateImage> ListTemplateImages()
        {
            TemplateImageList response = null;

            response = Client.TemplateImage.GetTemplateImages();

            if (response != null)
            {
                return response.Value;
            }
            else
            {
                return null;
            }
        }

        internal TemplateImage GetTemplateImage(string templateImageName)
        {
            TemplateImage response = null;

            response = Client.TemplateImage.GetTemplateImage(templateImageName);

            return response;
        }
    }
}
