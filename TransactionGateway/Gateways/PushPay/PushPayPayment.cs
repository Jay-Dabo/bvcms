using CmsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionGateway.ApiModels;

namespace TransactionGateway
{
    public class PushPayPayment
    {
        private PushpayConnection _pushpay;
        private CMSDataContext _db;
        private string _merchantHandle;

        public PushPayPayment(PushpayConnection Pushpay, CMSDataContext db)
        {
            _pushpay = Pushpay;
            _db = db;
            _merchantHandle = db.GetSetting("PushpayMerchant", null);
            if (_merchantHandle == null)
            {
                throw new Exception("PushpayMerchant Not Found");
            }
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

        public async Task<IEnumerable<RecurringPayment>> GetRecurringPaymentsForAPayer(string payerKey)
        {
            return await _pushpay.GetRecurringPaymentsForAPayer(payerKey);
        }
    }
}
