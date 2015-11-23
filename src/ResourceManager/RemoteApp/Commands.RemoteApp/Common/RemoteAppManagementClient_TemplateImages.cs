using Microsoft.Azure.Commands.RemoteApp.Cmdlet;
using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RemoteApp.Common
{
    public partial class RemoteAppManagementClientWrapper
    {

        internal IList<TemplateImage> ListTemplateImages(string location)
        {
            TemplateImageList response = null;

            response = Client.TemplateImage.GetTemplateImages(location);

            if (response != null)
            {
                return response.Value;
            }
            else
            {
                return null;
            }
        }

        internal TemplateImage GetTemplateImage(string location, string templateImageName)
        {
            TemplateImage response = null;

            response = Client.TemplateImage.GetTemplateImage(location, templateImageName);

            return response;
        }

        internal TemplateImage CreateOrUpdateTemplateImage(string location, string templateImageName, TemplateImageCreateDetails details)
        {
            TemplateImage response = null;

            response = Client.TemplateImage.CreateOrUpdate(details, location, templateImageName);

            return response;
        }

        internal void DeleteTemplateImages(string location, string templateImageName)
        {
            Client.TemplateImage.DeleteTemplateImage(location, templateImageName);
        }
            
    }
}
