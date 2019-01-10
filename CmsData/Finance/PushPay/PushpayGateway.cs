/* Author: David Carroll
 * Copyright (c) 2008, 2009 Bellevue Baptist Church
 * Licensed under the GNU General Public License (GPL v2)
 * you may not use this code except in compliance with the License.
 * You may obtain a copy of the License at http://bvcms.codeplex.com/license
 */
using System;
using System.Collections.Generic;
using CmsData.Finance;

namespace CmsData
{
    internal class PushpayGateway : IGateway
    {
        private CMSDataContext cMSDataContext;
        private bool testing;

        public PushpayGateway(CMSDataContext cMSDataContext, bool testing)
        {
            this.cMSDataContext = cMSDataContext;
            this.testing = testing;
        }

        public string GatewayType => throw new NotImplementedException();

        public bool CanVoidRefund => throw new NotImplementedException();

        public bool CanGetSettlementDates => throw new NotImplementedException();

        public bool UseIdsForSettlementDates => throw new NotImplementedException();

        public bool CanGetBounces => throw new NotImplementedException();

        public TransactionResponse AuthCreditCard(int peopleId, decimal amt, string cardnumber, string expires, string description, int tranid, string cardcode, string email, string first, string last, string addr, string addr2, string city, string state, string country, string zip, string phone)
        {
            throw new NotImplementedException();
        }

        public TransactionResponse AuthCreditCardVault(int peopleId, decimal amt, string description, int tranid)
        {
            throw new NotImplementedException();
        }

        public void CheckBatchSettlements(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public void CheckBatchSettlements(List<string> transactionids)
        {
            throw new NotImplementedException();
        }

        public BatchResponse GetBatchDetails(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public ReturnedChecksResponse GetReturnedChecks(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public TransactionResponse PayWithCheck(int peopleId, decimal amt, string routing, string acct, string description, int tranid, string email, string first, string middle, string last, string suffix, string addr, string addr2, string city, string state, string country, string zip, string phone)
        {
            throw new NotImplementedException();
        }

        public TransactionResponse PayWithCreditCard(int peopleId, decimal amt, string cardnumber, string expires, string description, int tranid, string cardcode, string email, string first, string last, string addr, string addr2, string city, string state, string country, string zip, string phone)
        {
            throw new NotImplementedException();
        }

        public TransactionResponse PayWithVault(int peopleId, decimal amt, string description, int tranid, string type)
        {
            throw new NotImplementedException();
        }

        public TransactionResponse RefundCheck(string reference, decimal amt, string lastDigits = "")
        {
            throw new NotImplementedException();
        }

        public TransactionResponse RefundCreditCard(string reference, decimal amt, string lastDigits = "")
        {
            throw new NotImplementedException();
        }

        public void RemoveFromVault(int peopleId)
        {
            throw new NotImplementedException();
        }

        public void StoreInVault(int peopleId, string type, string cardNumber, string expires, string cardCode, string routing, string account, bool giving)
        {
            throw new NotImplementedException();
        }

        public string VaultId(int peopleId)
        {
            throw new NotImplementedException();
        }

        public TransactionResponse VoidCheckTransaction(string reference)
        {
            throw new NotImplementedException();
        }

        public TransactionResponse VoidCreditCardTransaction(string reference)
        {
            throw new NotImplementedException();
        }
    }
}
