using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using crmWebApplication.Models;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.WebServiceClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Crm.Sdk.Messages;
using System.Configuration;

namespace crmWebApplication.BL
{
    public class CrmBL
    {
        public IOrganizationService service;
        string clientId = "3e147e28-4eaf-495d-b6b0-e91ae6090e52";
        string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
        string tenantId = "8c0933d9-84ca-4048-ab76-558ba065baf2";
        string resource = "https://orgd47d8762.crm4.dynamics.com";
        string resourceApi = "https://orgd47d8762.api.crm4.dynamics.com";
        string authorityUrl = "https://login.microsoftonline.com";
        public CrmBL()
        {

            //ConnectToCrm1();
            //ConnectToCrm2();
            //ConnectToCrm3();
            //ConnectToCrm4();
        }


        #region 1
        private void ConnectToCrm1()
        {
            try
            {
                var service = GetCrmService($"{authorityUrl}/{tenantId}", clientId, clientSecret, resource);
                if (service != null)
                {
                    // Add your code to interact with Dynamics 365 CE
                }
                else
                {
                    Console.WriteLine("Failed to connect to Dynamics 365 CE.");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private IOrganizationService GetCrmService(string AuthorityUrl, string clientId, string clientSecret, string resourceUrl)
        {
            try
            {
                var authenticationContext = new AuthenticationContext(AuthorityUrl);
                var credentials = new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(clientId, clientSecret);
                var authResult = authenticationContext.AcquireTokenAsync(resourceUrl, credentials).Result;
                var connectionString = $"ServiceUri={resourceUrl};AuthType=OAuth;AccessToken={authResult.AccessToken};";

                var crmService = new CrmServiceClient(connectionString);
                if (crmService.IsReady)
                {
                    return crmService.OrganizationWebProxyClient != null
                        ? (IOrganizationService)crmService.OrganizationWebProxyClient
                        : crmService.OrganizationServiceProxy;
                }
                Console.WriteLine("Failed to connect: " + crmService.LastCrmError);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return null;
        }
        #endregion


        #region 2
        public async Task<OrganizationWebProxyClient> CreateClient()
        {
            string tokenUrl = $"{authorityUrl}/{tenantId}/oauth2/token";
            var context = new AuthenticationContext(tokenUrl, false);
            var token = await context.AcquireTokenAsync(resource, new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(clientId, clientSecret));
            string serviceUrl = $"{resourceApi}/api/data/v9.2/";
            return new OrganizationWebProxyClient(new Uri(serviceUrl), true)
            {
                HeaderToken = token.AccessToken,
                SdkClientVersion = "9.2"
            };
        }

        public async Task<OrganizationServiceContext> CreateContext()
        {
            var client = await CreateClient();
            return new OrganizationServiceContext(client);
        }

        public async Task ConnectToCrm2()
        {
            try
            {
                var context = await CreateContext();

                // send a test request to verify authentication is working
                var response = (WhoAmIResponse)context.Execute(new WhoAmIRequest());
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion

        #region 3
        async void ConnectToCrm3()
        {
            try
            {
                // Acquire Access Token
                string accessToken = await GetAccessToken();

                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Make Web API Request
                    await MakeRequest(accessToken);
                }
                else
                {
                    Console.WriteLine("Failed to acquire access token.");
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        async Task<string> GetAccessToken()
        {
            try
            {

                string tokenUrl = $"{authorityUrl}/{tenantId}/oauth2/token";
                string grantType = "client_credentials";

                using (var client = new HttpClient())
                {
                    var requestParams = new FormUrlEncodedContent(new[]
                    {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("resource", resourceApi),
                new KeyValuePair<string, string>("grant_type", grantType)
            });

                    var response = await client.PostAsync(tokenUrl, requestParams);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                    return jsonResponse.access_token;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        async Task MakeRequest(string accessToken)
        {
            try
            {


                string apiUrl = $"{resourceApi}/api/data/v9.2/";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    //var response2 = await client.GetAsync(apiUrl);
                    var response = client.GetAsync("WhoAmI").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(content);
                    }
                    else
                    {
                        Console.WriteLine("Request failed: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion


        #region 4
        async public void ConnectToCrm4()
        {
            string authority = $"{authorityUrl}/{tenantId}";
            // Authenticate with Azure AD and obtain access token
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri(authority))
                .Build();

            string[] scopes = new string[] { $"{resource}/user_impersonation" };
            var authResult = await confidentialClientApplication
                .AcquireTokenForClient(scopes)
                .ExecuteAsync();

            var accessToken = authResult.AccessToken;

            // Use the access token to make authenticated requests to Dynamics 365 Web API
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Example: Make a GET request to retrieve data
            var response = await client.GetAsync($"{resourceApi}/api/data/v9.2/");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Process the retrieved data
            }

        }
        #endregion




        public IOrganizationService GetOrganizationService(string userName, string password, string organizationUri)
        {
            string crmConnectionString = $@"
                Url = {organizationUri};
                AuthType = OAuth;
                UserName = {userName};
                Password = {password};
                AppId = ba46a707-c11b-43c5-9b9a-2e1f55ab2b90;
                RedirectUri = app://58145B91-0C36-4500-8554-080854F2AC97;
                LoginPrompt=Auto;

                RequireNewInstance = True";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            service = new CrmServiceClient(crmConnectionString);

            if (((CrmServiceClient)service).LastCrmException != null &&

                (((CrmServiceClient)service).LastCrmException.Message == "OrganizationWebProxyClient is null" ||

                ((CrmServiceClient)service).LastCrmException.Message == "Unable to Login to Dynamics CRM"))

            {


                Console.WriteLine(((CrmServiceClient)service).LastCrmException.Message);

                throw new Exception(((CrmServiceClient)service).LastCrmException.Message);

            }

            return service ?? throw new Exception("Unable to Generate Service Proxy, Please Contact Administrator");

        }


        public HttpResponseMessage get()
        {
            string errorMessage;
            try
            {

                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject("Update User"), System.Text.Encoding.UTF8, "application/json")
                };
            }
            catch (Exception e)
            {

                errorMessage = e.Message;
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(errorMessage), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        public HttpResponseMessage Patch(User user)
        {
            string errorMessage;
            try
            {
                string entityLogicalName = "contact";
                string fieldValue = user.Id;

                var fieldsToUpdate = new Dictionary<string, object>
                {
                    { "firstname", user.firstName },
                    { "lastname", user.lastName },
                    { "emailaddress1", user.email },
                    { "telephone1", user.phone },
                    { "address1_city", user.city }
                };

                Entity record = service.Retrieve(entityLogicalName, Guid.Parse(fieldValue), new ColumnSet(fieldsToUpdate.Keys.ToArray()));

                // Update each field specified in fieldsToUpdate dictionary
                foreach (var fieldToUpdate in fieldsToUpdate)
                {
                    record[fieldToUpdate.Key] = fieldToUpdate.Value;
                }
                service.Update(record);

                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject("Update User"), System.Text.Encoding.UTF8, "application/json")
                };
            }
            catch (Exception e)
            {

                errorMessage = e.Message;
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(errorMessage), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }
        //public HttpResponseMessage UpdateContact(User user)
        //{
        //    string errorMessage;
        //    try
        //    {
        //        string entityLogicalName = "contact";
        //        string fieldName = "ID";
        //        string fieldValue = user.Id;


        //        var fieldsToUpdate = new Dictionary<string, object>
        //        {
        //            { "firstname", user.firstName },
        //            { "lastname", user.lastName },
        //            { "emailaddress1", user.email },
        //            { "telephone1", user.phone },
        //            { "address1_city", user.city }
        //        };

        //        var query = new QueryExpression(entityLogicalName)
        //        {
        //            ColumnSet = new ColumnSet(fieldsToUpdate.Keys.ToArray()),
        //            Criteria = new FilterExpression
        //            {
        //                Conditions =
        //                {
        //                    new ConditionExpression(fieldName, ConditionOperator.Equal, fieldValue)
        //                }
        //            }
        //        };

        //        var entities = _service.RetrieveMultiple(query).Entities;

        //        // Update fields for each retrieved record
        //        foreach (var entity in entities)
        //        {
        //            // Update each field specified in fieldsToUpdate dictionary
        //            foreach (var fieldToUpdate in fieldsToUpdate)
        //            {
        //                entity[fieldToUpdate.Key] = fieldToUpdate.Value;
        //            }

        //            _service.Update(entity);
        //        }
        //        return new HttpResponseMessage()
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Content = new StringContent(JsonConvert.SerializeObject("Update User"), System.Text.Encoding.UTF8, "application/json")
        //        };
        //    }
        //    catch (Exception e)
        //    {

        //        errorMessage = e.Message;
        //        return new HttpResponseMessage()
        //        {
        //            StatusCode = HttpStatusCode.InternalServerError,
        //            Content = new StringContent(JsonConvert.SerializeObject(errorMessage), System.Text.Encoding.UTF8, "application/json")
        //        };
        //    }
        //}
    }
}