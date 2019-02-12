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
        private CMSDataContext _db;
        private string _pushpayGivingBaseUrl, _givingLink, _merchantHandle;

        public PushPayPayment(PushpayConnection Pushpay, CMSDataContext db)
        {
            _pushpay = Pushpay;
            _db = db;
            //_pushpayGivingBaseUrl = PushpayGivingBaseUrl;
            _merchantHandle = db.GetSetting("PushpayMerchant", null);
            if (_merchantHandle == null)
            {
                throw new Exception("PushpayMerchant Not Found");
            }
            //_givingLink = $"{_pushpayGivingBaseUrl}/{_merchantHandle}";
        }

        public async Task<Payment> GetPayment(string paymentToken)
        {
            IEnumerable<Merchant> merchants = await _pushpay.SearchMerchants(_merchantHandle);
            return await _pushpay.GetPayment(merchants.FirstOrDefault().Key, paymentToken);
        }

        public async Task<RecurringPayment> GetRecurringPayment(string paymentToken)
        {
            IEnumerable<Merchant> merchants = await _pushpay.SearchMerchants(_merchantHandle);
            return await _pushpay.GetRecurringPayment(merchants.FirstOrDefault().Key, paymentToken);
        }

        public async Task<RecurringPaymentList> GetRecurringPaymentsForAPayer(string payerKey)
        {
            return await _pushpay.GetRecurringPaymentsForAPayer(payerKey);
        }

        //public string OneTimeRedirect(int OrgId)
        //{
        //    return $"{_givingLink}?ru={_merchantHandle}&sr=Org_{OrgId}&rcv=false";
        //}

        //public string OnePageRedirect()
        //{
        //    return $"{_givingLink}?rcv=false";
        //}

        //public string RecurringGivingRedirect(int PeopleId)
        //{
        //    PushPayResolver resolver = new PushPayResolver(_db, _pushpay);
        //    string payerKey = resolver.ResolvePayerKey(PeopleId);
        //    if (string.IsNullOrEmpty(payerKey))
        //    {

        //    }
        //    //string givingLink = $"{PushpayGivingBaseUrl}/{Merchant}";
        //    //string sr = $"Org_{organization.OrganizationId}";
        //    //string ru = Merchant;
        //    //string r = "Monthly";
        //    //string redirectUrl = givingLink
        //    //    + "?ru=" + ru
        //    //    + "&sr=" + sr
        //    //    + "&r=" + r;

        //    return redirectUrl;
        //}
    }
}
