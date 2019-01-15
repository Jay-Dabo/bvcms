using System.Collections.Generic;

namespace TransactionGateway.ApiModels
{
    public class PaymentList : BaseResponse
    {
        public IEnumerable<Payment> Items { get; set; }
    }
}
