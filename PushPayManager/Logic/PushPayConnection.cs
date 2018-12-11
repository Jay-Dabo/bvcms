using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CmsData;
using PushPay.ApiModels;
using PushPay.Enums;
using PushPay.Entities;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

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
        private string _pushpayAPIBaseUrl;
        private string _pushpayClientID, _pushpayClientSecret, _oAuth2AuthorizeEndpoint, _touchpointAuthServer, _oAuth2TokenEndpoint;

        public PushpayConnection(string access_token, string refresh_token, CMSDataContext db_context,
            string pushpayAPIBaseUrl, string pushpayClientID, string pushpayClientSecret, string oAuth2AuthorizeEndpoint,
            string touchpointAuthServer, string oAuth2TokenEndpoint)
        {
            accessToken = access_token;
            refreshToken = refresh_token;
            db = db_context;
            _pushpayAPIBaseUrl = pushpayAPIBaseUrl;
            _pushpayClientID = pushpayClientID;
            _pushpayClientSecret = pushpayClientSecret;
            _oAuth2AuthorizeEndpoint = oAuth2AuthorizeEndpoint;
            _touchpointAuthServer = touchpointAuthServer;
            _oAuth2TokenEndpoint = oAuth2TokenEndpoint;
        }

        /// <summary>
        ///     Helper method to create a client connection
        /// </summary>
        /// <returns></returns>
        private async Task<ApiClient> CreateClient()
        {
            if (_client == null)
            {
                string baseUrl = _pushpayAPIBaseUrl;
                if (string.IsNullOrWhiteSpace(baseUrl)) RaiseError(new Exception("Please provide a PushpayAPIBaseUrl in your configuration AppSettings"));
                _client = new ApiClient(baseUrl);

                // Authenticate
                string token = await AuthenticateClient();
                _client.SetBearerToken(token);
            }
            return _client;
        }

        /// <summary>
        ///     Helper method to authenticate the client. Run this on client creation and if the bearer token expires
        /// </summary>
        /// <returns></returns>
        public async Task<string> AuthenticateClient()
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

        public async Task<AccessToken> AuthorizationCodeCallback(string _authCode)
        {


            // exchange authorization code at authorization server for an access and refresh token
            Dictionary<string, string> post = null;
            post = new Dictionary<string, string>
            {
                { "client_id", _pushpayClientID}
                ,{"client_secret", _pushpayClientSecret}
                ,{"grant_type", "authorization_code"}
                ,{"code", _authCode}
                ,{"redirect_uri", _touchpointAuthServer}
            };

            var client = new HttpClient();
            //Setting a "basic auth" header
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
            var _accessToken = new AccessToken();
            // exchange code for tokens from authorization server
            try
            {
                var json = JObject.Parse(content);
                _accessToken.access_token = json["access_token"].ToString();
                _accessToken.token_type = json["token_type"].ToString();
                _accessToken.expires_in = Convert.ToInt64(json["expires_in"].ToString());
                if (json["refresh_token"] != null)
                    _accessToken.refresh_token = json["refresh_token"].ToString();
            }
            catch (Exception ex)
            {
                RaiseError(new Exception("Failed to retrieve access token, error was: " + ex.Message));
            }
            if (_accessToken != null)
                return _accessToken;
            else return null;
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
            ApiClient client = await CreateClient();
            MerchantList result = await client.Init("my/merchants", "Loading merchant list").Execute<MerchantList>();
            return result.Items;
        }

        public async Task<Merchant> GetMerchant(string merchantKey)
        {
            ApiClient client = await CreateClient();
            Merchant result = await client.Init($"merchant/{merchantKey}", "Loading merchant details").SetMethod(RequestMethodTypes.GET).Execute<Merchant>();
            return result;
        }
        
        public async Task<BatchList> GetBatchesForMerchantSince(string merchantKey, DateTime startDate, int page = 0)
        {
            // GET /v1/merchant/{merchantKey}/batches?from=2018-01-01T00:00:00Z
            ApiClient client = await CreateClient();
            BatchList result = await client.Init($"merchant/{merchantKey}/batches?from={startDate}&page={page}", "Loading batches").SetMethod(RequestMethodTypes.GET).Execute<BatchList>();
            return result;
        }

        public async Task<FundList> GetFundsForMerchant(string merchantKey)
        {
            // GET /v1/merchant/{merchantKey}/funds
            ApiClient client = await CreateClient();
            FundList result = await client.Init($"merchant/{merchantKey}/funds", "Loading funds").SetMethod(RequestMethodTypes.GET).Execute<FundList>();
            return result;
        }

        public async Task<PaymentList> GetPaymentsForBatch(string merchantKey, string batchKey, int page = 0)
        {
            // GET /v1/merchant/{merchantKey}/batch/{batchKey}/payments
            ApiClient client = await CreateClient();
            PaymentList result = await client.Init($"merchant/{merchantKey}/batch/{batchKey}/payments?page={page}", "Loading payments").SetMethod(RequestMethodTypes.GET).Execute<PaymentList>();
            return result;
        }
    }
}
