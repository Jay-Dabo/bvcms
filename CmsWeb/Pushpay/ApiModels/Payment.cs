using System;

namespace PushPay.ApiModels
{
    public class Payment : BaseResponse
    {
        public string TransactionId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? GivenOn { get; set; }
        public Money Amount { get; set; }
        public Fund Fund { get; set; }
        public Payer Payer { get; set; }
        public Merchant Recipient { get; set; }
        public string Source { get; set; }
        public string PaymentMethodType { get; set; }
        public object RecordedCheck { get; set; }
        public object Card { get; set; }
    }
}
