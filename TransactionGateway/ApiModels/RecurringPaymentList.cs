using System.Collections.Generic;

namespace TransactionGateway.ApiModels
{
    public class RecurringPaymentList : BaseResponse
    {
        public List<RecurringPaymentList> Items { get; set; }
    }
}
