using System.Collections.Generic;

namespace PushPay.ApiModels
{
    public class MerchantList : BaseResponse 
    {
        public IEnumerable<Merchant> Items { get; set; }
    }
}
