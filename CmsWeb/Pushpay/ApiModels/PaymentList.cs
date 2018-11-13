using System.Collections.Generic;

namespace PushPay.ApiModels
{
    public class PaymentList : BaseResponse
    {
        public IEnumerable<Payment> Items { get; set; }
    }
}
