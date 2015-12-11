using Hyak.Common;
using Microsoft.Azure.Commands.RemoteApp.Common;
using Microsoft.Azure.Commands.ResourceManager.Common;
using Microsoft.Azure.Common.Authentication.Models;
using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Threading;


namespace Microsoft.Azure.Commands.RemoteApp.Cmdlet
{
    public class ArmHttpException : Exception
    {
        public HttpStatusCode Status;

        public string ExceptionMessage;

        public ErrorCategory Category;

        public ExceptionType type;

        public ArmHttpException(ErrorRecordState state)
        {
            Status = state.Status;
            ExceptionMessage = state.ExceptionMessage;
            Category = state.Category;
            type = state.type;
        }
    }

    public class ArmHttpCallUtility
    {
        AzureEnvironment Environment = null;

        RemoteAppManagementClient Client = null;

        public ArmHttpCallUtility(AzureEnvironment env, RemoteAppManagementClient client)
        {
            Environment = env;
            Client = client;
        }

        private string GetArmUri()
        {
            string baseUri = null;

            foreach (KeyValuePair<Microsoft.Azure.Common.Authentication.Models.AzureEnvironment.Endpoint, string> endpoint in Environment.Endpoints)
            {
                if (endpoint.Key == Microsoft.Azure.Common.Authentication.Models.AzureEnvironment.Endpoint.ResourceManager)
                {
                    baseUri = endpoint.Value;
                    break;
                }
            }

            return baseUri;
        }

        public string GetHttpArmResource(string resourceUri)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage();
            CancellationToken cancellationToken = default(CancellationToken);
            HttpResponseMessage result = null;
            HttpStatusCode statusCode = HttpStatusCode.NotImplemented;
           
            string responseContent = null;
            string uri = GetArmUri();


            uri += "subscriptions/" + Uri.EscapeDataString(Client.SubscriptionId) + "/providers/" + resourceUri;
            httpRequest.RequestUri = new Uri(uri);
            httpRequest.Method = new HttpMethod("GET");

            // Set Headers
            httpRequest.Headers.TryAddWithoutValidation("x-ms-client-request-id", Guid.NewGuid().ToString());
            httpRequest.Headers.TryAddWithoutValidation("accept-language", Client.AcceptLanguage);

            // Set credentials
            cancellationToken.ThrowIfCancellationRequested();
            Client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).Wait();


            // Send request
            result = Client.HttpClient.SendAsync(httpRequest, cancellationToken).Result;
            statusCode = result.StatusCode;

            if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.Accepted)
            {
                ErrorRecordState errorState = RemoteAppCollectionErrorState.CreateErrorStateFromHttpStatusCode(statusCode);

                throw new ArmHttpException(errorState);
            }

            responseContent = result.Content.ReadAsStringAsync().Result;

            return responseContent;
        }
    }
}
