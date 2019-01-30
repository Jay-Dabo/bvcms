using CmsData;
using CmsData.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionGateway.ApiModels;
using UtilityExtensions;

namespace TransactionGateway
{
    public class PushPayPayment
    {
        private PushpayConnection _pushpay;

        public PushPayPayment(PushpayConnection Pushpay)
        {
            _pushpay = Pushpay;
        }

        public async Task<Payment> GetPayment(string paymentToken, string merchantHandle)
        {
            IEnumerable<Merchant> merchants = await _pushpay.SearchMerchants(merchantHandle);           
            return await _pushpay.GetPayment(merchants.FirstOrDefault().Key, paymentToken);            
        }

        public static string OneTimeRedirect(string PushpayGivingBaseUrl, string Merchant, Person person, CmsData.Organization organization)
        {
            string givingLink = String.Format("{0}/{1}", PushpayGivingBaseUrl, Merchant);
            string sr = String.Format("Org_{0}", organization.OrganizationId);
            string ru = Merchant;
            string rcv = "false";
            string redirectUrl = givingLink
                + "?ru=" + ru
                + "&sr=" + sr
                + "&rcv=" + rcv;

            return redirectUrl;
        }

        public static string OnePageRedirect(string PushpayGivingBaseUrl, string Merchant)
        {
            return String.Format("{0}/{1}rcv=false", PushpayGivingBaseUrl, Merchant);                        
        }

        public static string RecurringGivingRedirect(string PushpayAPIBaseUrl, string Merchant)
        {
            string givingLink = String.Format("{0}/{1}", PushpayAPIBaseUrl, Merchant);
            string sr = String.Format("Org_{0}", "1");
            string ru = Merchant;
            string rcv = "false";
            string redirectUrl = givingLink
                + "?ru=" + ru
                + "&sr=" + sr
                + "&rcv=" + rcv;

            return redirectUrl;
        }                        
    }
}
