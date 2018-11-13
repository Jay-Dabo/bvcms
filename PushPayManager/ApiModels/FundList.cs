using System.Collections.Generic;

namespace PushPay.ApiModels
{
    public class FundList : BaseResponse
    {
        public IEnumerable<Fund> Items { get; set; }
    }
}
