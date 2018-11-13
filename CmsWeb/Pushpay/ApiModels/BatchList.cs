using System.Collections.Generic;

namespace PushPay.ApiModels
{
    public class BatchList : BaseResponse
    {
        public IEnumerable<Batch> Items { get; set; }
    }
}
