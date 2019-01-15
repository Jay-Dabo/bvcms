using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CmsData;
using CmsData.Codes;
using TransactionGateway.ApiModels;


namespace TransactionGateway
{
    public class PushPayPayment
    {
        private CMSDataContext db;
    
        private PushpayConnection _pushpay;
        private readonly string _access_token;
        private readonly string _refresh_token;
        private readonly string _pushpayAPIBaseUrl;
        private readonly string _pushpayClientID;
        private readonly string _pushpayClientSecret;
        private readonly string _oAuth2AuthorizeEndpoint;
        private readonly string _touchpointAuthServer;
        private readonly string _oAuth2TokenEndpoint;

        public PushPayPayment()
        {

        }
    }
}
