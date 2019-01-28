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
        private CMSDataContext _db;
        private const string PushPayKey = "PushPayKey";

        private PushpayConnection _pushpay;
        private readonly string _access_token;
        private readonly string _refresh_token;
        private readonly string _pushpayAPIBaseUrl;
        private readonly string _pushpayClientID;
        private readonly string _pushpayClientSecret;
        private readonly string _oAuth2AuthorizeEndpoint;
        private readonly string _touchpointAuthServer;
        private readonly string _oAuth2TokenEndpoint;

        public PushPayPayment(CMSDataContext CurrentDatabase, string PushpayAPIBaseUrl,
                string PushpayClientID,
                string PushpayClientSecret,
                string OAuth2TokenEndpoint,
                string TouchpointAuthServer,
                string OAuth2AuthorizeEndpoint)
        {
            _db = CurrentDatabase;
            _refresh_token = CurrentDatabase.GetSetting("PushPayRefreshToken","");
            _pushpay = new PushpayConnection(_access_token, _refresh_token, _db,
            PushpayAPIBaseUrl,
                PushpayClientID,
                PushpayClientSecret,
                OAuth2TokenEndpoint,
                TouchpointAuthServer,
                OAuth2AuthorizeEndpoint);
        }

        public async Task<Payment> GetPayment(string paymentToken, string merchantHandle)
        {
            IEnumerable<Merchant> merchants = await _pushpay.SearchMerchants(merchantHandle);           
            return await _pushpay.GetPayment(merchants.FirstOrDefault().Key, paymentToken);            
        }

        public static string OneTimeRedirect(string PushpayAPIBaseUrl, string Merchant, Person person, CmsData.Organization organization)
        {
            string givingLink = String.Format("{0}/{1}", PushpayAPIBaseUrl, Merchant);
            string sr = String.Format("Org_{0}", organization.OrganizationId);
            string ru = Merchant;
            string rcv = "false";
            string redirectUrl = givingLink
                + "?ru=" + ru
                + "&sr=" + sr
                + "&rcv=" + rcv;

            return redirectUrl;
        }

        public static string OnePageRedirect(string PushpayAPIBaseUrl, string Merchant)
        {
            return String.Format("{0}/{1}rcv=false", PushpayAPIBaseUrl, Merchant);                        
        }

        public static string RecurringGivingRedirect(string PushpayAPIBaseUrl, string Merchant)
        {
            string givingLink = String.Format("{0}/{1}", PushpayAPIBaseUrl, Merchant);
            string sr = String.Format("Org_{0}", "1");
            string ru = Merchant;
            string rcv = "false";
            string redirectUrl = givingLink
                + "?ru=" + ru
                + "&sr=" + sr
                + "&rcv=" + rcv;

            return redirectUrl;
        }

        public int? ResolvePersonId(Payer payer, CMSDataContext db)
        {
            // take a pushpay payer and find or create a touchpoint person
            bool hasKey = payer.Key.HasValue();
            bool hasEmail = payer.emailAddress.HasValue();
            bool hasMobileNumber = payer.mobileNumber.HasValue();
            bool hasFirstAndLastName = payer.firstName.HasValue() && payer.lastName.HasValue();

            if (!hasKey &&
                !hasEmail &&
                !hasMobileNumber &&
                !hasFirstAndLastName)
            {
                // can't resolve - typically due to an anonymous donation
                return null;
            }

            IQueryable<Person> people = db.People.AsQueryable();

            // first look for an already established person link
            int? PersonId = null;

            if (hasKey)
            {
                PersonId = db.PeopleExtras.Where(p => p.Field == PushPayKey && p.StrValue == payer.Key).Select(p => p.PeopleId).SingleOrDefault();
                if (PersonId.HasValue && PersonId != 0)
                {
                    return PersonId;
                }
            }

            IQueryable<Person> result = null;
            Func<bool> hasResults = delegate () { return result != null && result.Any(); };

            if (hasEmail && hasFirstAndLastName)
            {
                result = people.Where(p => p.EmailAddress == payer.emailAddress
                    && p.FirstName == payer.firstName && p.LastName == payer.lastName);
            }

            if (hasEmail && !hasResults())
            {
                result = people.Where(p => p.EmailAddress == payer.emailAddress);
            }

            var cellPhone = payer.mobileNumber.GetDigits();
            if (hasMobileNumber && hasFirstAndLastName && !hasResults())
            {
                result = people.Where(p => p.CellPhone == cellPhone
                    && p.FirstName == payer.firstName && p.LastName == payer.lastName);
            }

            if (hasMobileNumber && !hasResults())
            {
                result = people.Where(p => p.CellPhone == cellPhone);
            }

            if (hasFirstAndLastName && !hasResults())
            {
                result = people.Where(p => p.FirstName == payer.firstName && p.LastName == payer.lastName);
            }

            if (hasResults())
            {
                PersonId = result.OrderBy(p => p.CreatedDate).Select(p => p.PeopleId).First();
            }
            else
            {
                Person person = Person.Add(db, null, payer.firstName, null, payer.lastName, null);
                person.EmailAddress = payer.emailAddress;
                person.CellPhone = payer.mobileNumber;
                person.Comments = "Added in context of PushPayImport because record was not found";
                db.SubmitChanges();
                PersonId = person.PeopleId;
            }
            // add extra value
            if (payer.Key.HasValue())
            {
                db.AddExtraValueData(PersonId, PushPayKey, payer.Key, null, null, null, null);
            }
            return PersonId;
        }

        public ContributionFund ResolveFund(Fund fund, CMSDataContext db)
        {
            // take a pushpay fund and find or create a touchpoint fund
            IQueryable<ContributionFund> funds = db.ContributionFunds.AsQueryable();

            var result = from f in funds
                         where f.FundName == fund.Name
                         where f.FundStatusId > 0
                         orderby f.FundStatusId
                         orderby f.FundId descending
                         select f;
            if (result.Any())
            {
                int id = result.Select(f => f.FundId).First();
                return db.ContributionFunds.SingleOrDefault(f => f.FundId == id);
            }
            else
            {
                var max_id = from fn in funds
                             orderby fn.FundId descending
                             select fn.FundId + 1;
                int fund_id = max_id.FirstOrDefault();

                ContributionFund f = new ContributionFund
                {
                    FundId = fund_id,
                    FundName = fund.Name,
                    FundStatusId = 1,
                    CreatedDate = DateTime.Now.Date,
                    CreatedBy = 1,
                    FundDescription = fund.Name,
                    NonTaxDeductible = !fund.taxDeductible
                };
                db.ContributionFunds.InsertOnSubmit(f);
                db.SubmitChanges();
                return f;
            }
        }

        //public async Task<BundleHeader> ResolveSettlement(Settlement settlement, CMSDataContext db)
        //{
        //    // take a pushpay settlement and find or create a touchpoint bundle
        //    IQueryable<BundleHeader> bundles = db.BundleHeaders.AsQueryable();

        //    var result = from b in bundles
        //                 where b.ReferenceId == settlement.Key
        //                 where b.ReferenceIdType == BundleReferenceIdTypeCode.PushPaySettlement
        //                 select b;
        //    if (result.Any())
        //    {
        //        int id = result.Select(b => b.BundleHeaderId).SingleOrDefault();
        //        return db.BundleHeaders.SingleOrDefault(b => b.BundleHeaderId == id);
        //    }
        //    else
        //    {
        //        if (settlement.TotalAmount == null || settlement.TotalAmount.Amount == 0)
        //        {
        //            settlement = await _pushpay.GetSettlement(settlement.Key);
        //        }
        //        return CreateBundle(settlement.EstimatedDepositDate.ToLocalTime(), settlement.TotalAmount?.Amount, null, null, settlement.Key, BundleReferenceIdTypeCode.PushPaySettlement);
        //    }
        //}

        //public BundleHeader CreateBundle(DateTime CreatedOn, decimal? BundleTotal, decimal? TotalCash, decimal? TotalChecks, string RefId, int? RefIdType)
        //{
        //    // create a touchpoint bundle
        //    BundleHeader bh = new BundleHeader
        //    {
        //        ChurchId = 1,
        //        CreatedBy = 1,
        //        CreatedDate = CreatedOn,
        //        RecordStatus = false,
        //        BundleStatusId = BundleStatusCode.OpenForDataEntry,
        //        ContributionDate = CreatedOn,
        //        BundleHeaderTypeId = BundleTypeCode.Online,
        //        DepositDate = null,
        //        BundleTotal = BundleTotal,
        //        TotalCash = TotalCash,
        //        TotalChecks = TotalChecks,
        //        ReferenceId = RefId,
        //        ReferenceIdType = RefIdType
        //    };
        //    db.BundleHeaders.InsertOnSubmit(bh);
        //    db.SubmitChanges();
        //    return bh;
        //}
    }
}
