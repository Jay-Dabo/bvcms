using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CmsData;
using PushPay.ApiModels;
using PushPay.Enums;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Pushpay.Configuration;


namespace PushPay
{
    /// <summary>
    ///     Centralized logic for communicating with the Pushpay payment server
    /// </summary>
    public class PushpayConnection
    {
        private ApiClient _client;
        private string accessToken;
        private string refreshToken;
        private CMSDataContext db;

        public PushpayConnection(string access_token, string refresh_token, CMSDataContext db_context)
        {
            accessToken = access_token;
            refreshToken = refresh_token;
            db = db_context;
        }

        /// <summary>
        ///     Helper method to create a client connection
        /// </summary>
        /// <returns></returns>
        private async Task<ApiClient> CreateClient(string _pushpayAPIBaseUrl)
        {
            if (_client == null)
            {
                string baseUrl = _pushpayAPIBaseUrl;
                if (string.IsNullOrWhiteSpace(baseUrl)) RaiseError(new Exception("Please provide a PushpayAPIBaseUrl in your configuration AppSettings"));
                _client = new ApiClient(baseUrl);

                // Authenticate
                string token = await AuthenticateClient(Configuration.Current.PushpayClientID, Configuration.Current.PushpayClientSecret, Configuration.Current.OAuth2AuthorizeEndpoint);
                _client.SetBearerToken(token);
            }
            return _client;
        }

        /// <summary>
        ///     Helper method to authenticate the client. Run this on client creation and if the bearer token expires
        /// </summary>
        /// <returns></returns>
        public async Task<string> AuthenticateClient(string _pushpayClientID, string _pushpayClientSecret, string _oAuth2TokenEndpoint)
        {
            if (string.IsNullOrWhiteSpace(_pushpayClientID) || string.IsNullOrWhiteSpace(_pushpayClientSecret))
            {
                RaiseError(new Exception("Please provide Pushpay client ID and secret tokens in your web.config"));
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                RaiseError(new Exception("No refresh token set on this DB. Please authenticate with PushPay first"));
            }

            // Update the access token required by Pushpay
            string newAccessToken = null;
            string newRefreshToken = null;

            // exchange old refresh token for a new access and refresh token
            Dictionary<string, string> post = null;
            post = new Dictionary<string, string>
                {
                    {"grant_type", "refresh_token"}
                    ,{"refresh_token", refreshToken}
                };

            var client = new HttpClient();

            // Setting a "basic auth" header
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", _pushpayClientID, _pushpayClientSecret)
                        )));

            var postContent = new FormUrlEncodedContent(post);
            var response = await client.PostAsync(_oAuth2TokenEndpoint, postContent);
            var content = await response.Content.ReadAsStringAsync();

            // received tokens from authorization server
            var json = JObject.Parse(content);
            newAccessToken = json["access_token"].ToString();
            newRefreshToken = json["refresh_token"].ToString();

            if (json["refresh_token"] == null || json["access_token"] == null)
            {
                RaiseError(new Exception("Failed to retrieve access token, error was: " + response.ReasonPhrase));
            }
            db.SetSetting("PushPayAccessToken", newAccessToken);
            db.SetSetting("PushPayRefreshToken", newRefreshToken);
            Console.WriteLine("PushPay authenticated!");
            return newAccessToken;
        }


        /// <summary>
        ///     Centralized error handling
        /// </summary>
        /// <param name="ex"></param>
        private void RaiseError(Exception ex)
        {
            throw ex;
        }
        
        public async Task<IEnumerable<Merchant>> GetMerchants()
        {
            ApiClient client = await CreateClient(Configuration.Current.PushpayAPIBaseUrl);
            MerchantList result = await client.Init("my/merchants", "Loading merchant list").Execute<MerchantList>();
            return result.Items;
        }

        public async Task<Merchant> GetMerchant(string merchantKey)
        {
            ApiClient client = await CreateClient(Configuration.Current.PushpayAPIBaseUrl);
            Merchant result = await client.Init($"merchant/{merchantKey}", "Loading merchant details").SetMethod(RequestMethodTypes.GET).Execute<Merchant>();
            return result;
        }
        
        public async Task<BatchList> GetBatchesForMerchantSince(string merchantKey, DateTime startDate, int page = 0)
        {
            // GET /v1/merchant/{merchantKey}/batches?from=2018-01-01T00:00:00Z
            ApiClient client = await CreateClient(Configuration.Current.PushpayAPIBaseUrl);
            BatchList result = await client.Init($"merchant/{merchantKey}/batches?from={startDate}&page={page}", "Loading batches").SetMethod(RequestMethodTypes.GET).Execute<BatchList>();
            return result;
        }

        public async Task<FundList> GetFundsForMerchant(string merchantKey)
        {
            // GET /v1/merchant/{merchantKey}/funds
            ApiClient client = await CreateClient(Configuration.Current.PushpayAPIBaseUrl);
            FundList result = await client.Init($"merchant/{merchantKey}/funds", "Loading funds").SetMethod(RequestMethodTypes.GET).Execute<FundList>();
            return result;
        }

        public async Task<PaymentList> GetPaymentsForBatch(string merchantKey, string batchKey, int page = 0)
        {
            // GET /v1/merchant/{merchantKey}/batch/{batchKey}/payments
            ApiClient client = await CreateClient(Configuration.Current.PushpayAPIBaseUrl);
            PaymentList result = await client.Init($"merchant/{merchantKey}/batch/{batchKey}/payments?page={page}", "Loading payments").SetMethod(RequestMethodTypes.GET).Execute<PaymentList>();
            return result;
        }
    }
}
