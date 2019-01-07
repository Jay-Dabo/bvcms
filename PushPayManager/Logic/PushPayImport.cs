using System;
using System.Linq;
using System.Threading.Tasks;
using CmsData;
using CmsData.Codes;
using PushPay.ApiModels;
using System.Data.SqlClient;


namespace PushPay
{
    public class PushPayImport
    {
        private CMSDataContext db;

        private DateTime startDate;
        private int OnBatchPage;
        private bool InitialPass;
        private PushPayLog LastImport;

        //Pushpay
        private PushpayConnection _pushpay;
        private readonly string _access_token;
        private readonly string _refresh_token;
        private readonly string _pushpayAPIBaseUrl;
        private readonly string _pushpayClientID;
        private readonly string _pushpayClientSecret;
        private readonly string _oAuth2AuthorizeEndpoint;
        private readonly string _touchpointAuthServer;
        private readonly string _oAuth2TokenEndpoint;
        
        public PushPayImport(string dbname, string connstr, string pushpayAPIBaseUrl,
            string pushpayClientID, string pushpayClientSecret, string oAuth2AuthorizeEndpoint,
            string touchpointAuthServer, string oAuth2TokenEndpoint)
        {
            var cb = new SqlConnectionStringBuilder(connstr) { InitialCatalog = dbname };
            var host = dbname.Split(new char[] { '_' }, 2)[1];
            db = CMSDataContext.Create(cb.ConnectionString, host, true);

            
            _pushpayAPIBaseUrl = pushpayAPIBaseUrl;
            _pushpayClientID = pushpayClientID;
            _pushpayClientSecret = pushpayClientSecret;
            _oAuth2AuthorizeEndpoint = oAuth2AuthorizeEndpoint;
            _touchpointAuthServer = touchpointAuthServer;
            _oAuth2TokenEndpoint = oAuth2TokenEndpoint;


            if (db.Setting("PushPayEnableImport"))
            {                
                _access_token = db.GetSetting("PushpayAccessToken", "");
                _refresh_token = db.GetSetting("PushpayRefreshToken", "");
                
                _pushpay = new PushpayConnection(_access_token, _refresh_token, db,
                _pushpayAPIBaseUrl,
                _pushpayClientID,
                _pushpayClientSecret,
                _oAuth2AuthorizeEndpoint,
                _touchpointAuthServer,
                _oAuth2TokenEndpoint);
            }

            
        }
        public async Task<int> Run()
        {
            if (_pushpay is null)
                return 0;                        
#if DEBUG
            try
            {
                return await RunInternal();
            }
            catch (Exception ex)
            {
                Console.WriteLine("PushPay error");
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                Environment.Exit(1);
                return 0;
            }
#else
            return await RunInternal();
#endif
        }

        private async Task<int> RunInternal()
        {
            int Count = 0;
            var MerchantList = await _pushpay.GetMerchants();
            foreach (Merchant merchant in MerchantList)
            {
                // initial pass - get last run data and start there
                Init(merchant.Key);

                bool HasBatchesToProcess = true;

                while (HasBatchesToProcess)
                {
                    BatchList batches = await _pushpay.GetBatchesForMerchantSince(merchant.Key, startDate, OnBatchPage);
                    int BatchPages = (batches.TotalPages.HasValue ? (int)batches.TotalPages : 1);

                    foreach (Batch batch in batches.Items)
                    {
                        BundleHeader bundle = ResolveBatch(batch);

                        int OnPaymentPage = 0;
                        bool HasPaymentsToProcess = true;

                        while (HasPaymentsToProcess)
                        {
                            PaymentList payments = await _pushpay.GetPaymentsForBatch(merchant.Key, batch.Key, OnPaymentPage);
                            int PaymentPages = (payments.TotalPages.HasValue ? (int)payments.TotalPages : 1);

                            foreach (Payment payment in payments.Items)
                            {
                                if (!InitialPass || !TransactionAlreadyImported(batch.Key, payment.TransactionId)) {
                                    Console.WriteLine("Payment " + payment.TransactionId);

                                    // resolve the payer, fund, and payment
                                    int? PersonId = ResolvePersonId(payment.Payer);
                                    ContributionFund fund = ResolveFund(payment.Fund);
                                    Contribution contribution = ResolvePayment(payment, fund, PersonId, bundle);

                                    // mark this payment as imported
                                    RecordImportProgress(merchant, batch, payment.TransactionId);
                                }
                            }

                            // done with this page of payments, see if there's more

                            InitialPass = false;
                            if (PaymentPages > OnPaymentPage + 1)
                            {
                                OnPaymentPage++;
                            }
                            else
                            {
                                HasPaymentsToProcess = false;
                            }
                        }
                    }
                    // done with this page of batches, see if there's more
                    if (BatchPages > OnBatchPage + 1)
                    {
                        OnBatchPage++;
                    }
                    else
                    {
                        HasBatchesToProcess = false;
                    }
                }
            }
            return Count;
        }

        private Contribution ResolvePayment(Payment payment, ContributionFund fund, int? PersonId, BundleHeader bundle)
        {
            // take a pushpay payment and create a touchpoint payment
            IQueryable<Contribution> contributions = db.Contributions.AsQueryable();

            var result = from c in contributions
                         where c.ContributionDate == payment.CreatedOn
                         where c.ContributionAmount == payment.Amount.Amount
                         where c.MetaInfo == "PushPay Transaction #" + payment.TransactionId
                         select c;
            int count = result.Count();
            if (count == 1)
            {
                int id = result.Select(c => c.ContributionId).SingleOrDefault();
                return db.Contributions.SingleOrDefault(c => c.ContributionId == id);
            }
            else
            {
                Contribution c = new Contribution
                {
                    PeopleId = PersonId,
                    ContributionDate = payment.CreatedOn,
                    ContributionAmount = payment.Amount.Amount,
                    ContributionTypeId = (fund.NonTaxDeductible == true) ? ContributionTypeCode.NonTaxDed : ContributionTypeCode.Online,
                    ContributionStatusId = (payment.Amount.Amount >= 0) ? ContributionStatusCode.Recorded : ContributionStatusCode.Reversed,
                    Origin = ContributionOriginCode.PushPay,
                    CreatedDate = DateTime.Now,
                    FundId = fund.FundId,

                    MetaInfo = "PushPay Transaction #" + payment.TransactionId
                };
                db.Contributions.InsertOnSubmit(c);
                db.SubmitChanges();

                // assign contribution to bundle
                BundleDetail bd = new BundleDetail
                {
                    BundleHeaderId = bundle.BundleHeaderId,
                    ContributionId = c.ContributionId,
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now
                };
                db.BundleDetails.InsertOnSubmit(bd);
                db.SubmitChanges();
                return c;
            }

        }

        private BundleHeader ResolveBatch(Batch batch)
        {
            // take a pushpay batch and find or create a touchpoint bundle
            BundleHeader bh = new BundleHeader
            {
                ChurchId = 1,
                CreatedBy = 1,
                CreatedDate = batch.CreatedOn,
                RecordStatus = false,
                BundleStatusId = BundleStatusCode.OpenForDataEntry,
                ContributionDate = batch.CreatedOn,
                BundleHeaderTypeId = BundleTypeCode.Online,
                DepositDate = null,
                BundleTotal = batch.TotalAmount.Amount,
                TotalCash = batch.TotalCashAmount.Amount,
                TotalChecks = batch.TotalCheckAmount.Amount
            };
            db.BundleHeaders.InsertOnSubmit(bh);
            db.SubmitChanges();
            return bh;
        }

        private int? ResolvePersonId(Payer payer)
        {
            // take a pushpay payer and find or create a touchpoint person
            IQueryable<Person> people = db.People.AsQueryable();

            if (String.IsNullOrEmpty(payer.emailAddress))
            {
                // can't resolve - typically due to an anonymous donation
                return null;
            }

            var result = from p in people
                         where p.EmailAddress == payer.emailAddress
                         select p;
            int count = result.Count();
            if (count == 1)
            {
                int id = result.Select(p => p.PeopleId).SingleOrDefault();
                return id;
            }
            else
            {

                Person p = Person.Add(db, null, payer.firstName, null, payer.lastName, null);
                p.EmailAddress = payer.emailAddress;
                p.CellPhone = payer.mobileNumber;

                db.SubmitChanges();
                return p.PeopleId;
            }
        }

        private ContributionFund ResolveFund(Fund fund)
        {
            // take a pushpay fund and find or create a touchpoint fund
            IQueryable<ContributionFund> funds = db.ContributionFunds.AsQueryable();

            var result = from f in funds
                         where f.FundName == fund.Name
                         select f;
            int count = result.Count();
            if (count == 1)
            {
                int id = result.Select(f => f.FundId).SingleOrDefault();
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

        private bool TransactionAlreadyImported(string batchKey, string transactionId)
        {
            // check if a transaction has already been imported
            IQueryable<PushPayLog> logs = db.PushPayLogs.AsQueryable();

            var result = (from l in logs
                          where l.BatchKey == batchKey
                          where l.TransactionId == transactionId
                          select l).Any();
            return result;
        }

        private void Init(string merchantkey)
        {
            InitialPass = true;

            // load initial import status so we can start where we left off
            IQueryable<PushPayLog> logs = db.PushPayLogs.AsQueryable();

            var result = (from l in logs
                         where l.MerchantKey == merchantkey
                         orderby l.Id descending
                         select l).Take(1);
            int count = result.Count();
            if (count == 1)
            {
                long id = result.Select(l => l.Id).SingleOrDefault();
                LastImport = db.PushPayLogs.SingleOrDefault(l => l.Id == id);
                startDate = (DateTime)LastImport.BatchDate;
                OnBatchPage = (int)LastImport.BatchPage;
            }
            else
            {
                startDate = new DateTime(1970, 1, 1);
                OnBatchPage = 0;
            }
        }

        private void RecordImportProgress(Merchant merchant, Batch batch, string transactionId)
        {
            // record our import status so that we can recover if need be
            PushPayLog log = new PushPayLog
            {
                BatchPage = OnBatchPage,
                BatchDate = batch.CreatedOn,
                BatchKey = batch.Key,
                MerchantKey = merchant.Key,
                TransactionId = transactionId,
                ImportDate = DateTime.Now
            };
            db.PushPayLogs.InsertOnSubmit(log);
            db.SubmitChanges();
        }
    }
}
