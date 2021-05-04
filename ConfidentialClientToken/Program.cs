using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;


namespace ConfidentialClientToken
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string ClientId = System.Configuration.ConfigurationManager.AppSettings["ida:ClientId"];
            string TenantId = System.Configuration.ConfigurationManager.AppSettings["ida:TenantId"];
            string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["ida:ClientSecret"];
            string AadInstance = System.Configuration.ConfigurationManager.AppSettings["ida:AadInstance"];
            string Scope = System.Configuration.ConfigurationManager.AppSettings["ida:Scope"];

            string MSGraphQuery = "https://graph.microsoft.com/v1.0/users"; // Simulate downstream API call
            string oAuthUrl = string.Format("{0}/{1}/oauth2/v2.0/token", AadInstance, TenantId);

            ////////////////////////////////// Get Bearer Token ////////////////////////////////////////////////////
            HttpClient authClient = new HttpClient();
            HttpRequestMessage authRequest = new HttpRequestMessage(HttpMethod.Post, oAuthUrl);

            var requestContent = string.Format("client_id={0}&scope={1}&client_secret={2}&grant_type=client_credentials", ClientId, Scope, ClientSecret);
            authRequest.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpResponseMessage authResponse = authClient.SendAsync(authRequest).Result;

            var authJson = JsonConvert.DeserializeObject<AuthToken>(authResponse.Content.ReadAsStringAsync().Result);

            Console.WriteLine("Bearer Token : " + authJson.access_token);
            ///////////////////////////////////////////////////////////////////////////////////////////////////////


            //////////////////////// Call Downstream API - In this case MS Graph API //////////////////////////////
            HttpClient gClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, MSGraphQuery);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authJson.access_token);
            HttpResponseMessage response = gClient.SendAsync(request).Result;

            string json = response.Content.ReadAsStringAsync().Result;
            MsGraphUserListResponse users = JsonConvert.DeserializeObject<MsGraphUserListResponse>(json);

            Console.WriteLine("--------");
            
            Console.WriteLine(users.value.First().userPrincipalName);
            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            Console.ReadLine();
        }


        public class AuthToken
        {
            public string token_type { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public int ext_expires_in { get; set; }
        }

        public class MSGraphUser
        {
            [JsonProperty(PropertyName = "@odata.type")]
            public string odataType { get; set; }

            [JsonProperty(PropertyName = "@odata.id")]
            public string odataId { get; set; }

            public List<string> businessPhones { get; set; }
            public string displayName { get; set; }
            public string givenName { get; set; }
            public string jobTitle { get; set; }
            public string mail { get; set; }
            public string mobilePhone { get; set; }
            public string officeLocation { get; set; }
            public string preferredLanguage { get; set; }
            public string surname { get; set; }
            public string userPrincipalName { get; set; }
            public string id { get; set; }
        }

        public class MsGraphUserListResponse
        {
            [JsonProperty(PropertyName = "@odata.context")]
            public string context { get; set; }

            public List<MSGraphUser> value { get; set; }
        }

    }
}
