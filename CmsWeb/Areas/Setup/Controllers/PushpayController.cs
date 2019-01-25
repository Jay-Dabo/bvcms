using CmsData;
using CmsWeb.Common;
using CmsWeb.Lifecycle;
using CmsWeb.Pushpay.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using UtilityExtensions;
using TransactionGateway;
using TransactionGateway.ApiModels;
using CmsData.Finance;

namespace CmsWeb.Areas.Setup.Controllers
{
    [RouteArea("Setup", AreaPrefix = "Pushpay")]
    public class PushpayController : Controller
    {
        //todo: Inheritance chain
        private readonly RequestManager RequestManager;
        private CMSDataContext CurrentDatabase => RequestManager.CurrentDatabase;

        private PushpayConnection _pushpay;

        public PushpayController(RequestManager requestManager)
        {
            RequestManager = requestManager;
            _pushpay = new PushpayConnection("", "", CurrentDatabase,
                Configuration.Current.PushpayAPIBaseUrl,
                Configuration.Current.PushpayClientID,
                Configuration.Current.PushpayClientSecret,
                Configuration.Current.OAuth2TokenEndpoint,
                Configuration.Current.TouchpointAuthServer,
                Configuration.Current.OAuth2AuthorizeEndpoint);
        }

        /// <summary>
        ///     Opens the developer console in a separate VIEW
        /// </summary>
        /// <returns></returns>
        public ActionResult DeveloperConsole()
        {
            return View();
        }

        /// <summary>
        ///     Entry point / home page into the application
        /// </summary>
        /// <returns></returns>
        [Route("~/Pushpay")]
        public ActionResult Index()
        {
            string redirectUrl = Configuration.Current.OAuth2AuthorizeEndpoint
                + "?client_id=" + Configuration.Current.PushpayClientID
                + "&response_type=code"
                + "&redirect_uri=" + Configuration.Current.TouchpointAuthServer
                + "&scope=" + Configuration.Current.PushpayScope
                + "&state=" + CurrentDatabase.Host; //Get  xsrf_token:tenantID

            return Redirect(redirectUrl);
        }

        [AllowAnonymous, Route("~/Pushpay/Complete")]
        public async Task<ActionResult> Complete(string state)
        {
            string redirectUrl;
            var tenantHost = state;
#if DEBUG
            redirectUrl = "http://" + Configuration.Current.TenantHostDev + "/Pushpay/Save";
#else
            redirectUrl = "https://" + tenantHost + "." + Configuration.Current.OrgBaseDomain + "/Pushpay/Save";
#endif            

            //Received authorization code from authorization server
            var authorizationCode = Request["code"];
            if (authorizationCode != null && authorizationCode != "")
            {
                //Get code returned from Pushpay                
                var at = await _pushpay.AuthorizationCodeCallback(authorizationCode);
                return Redirect(redirectUrl + "?_at=" + at.access_token + "&_rt=" + at.refresh_token);
            }
            return Redirect("~/Home/Index");
        }


        [Route("~/Pushpay/Save")]
        public ActionResult Save(string _at, string _rt)
        {
            string idAccessToken = "PushpayAccessToken", idRefreshToken = "PushpayRefreshToken";
            //var dbContext = Db;
            //var m = CurrentDatabase.Settings.AsQueryable();
            if (!Regex.IsMatch(idAccessToken, @"\A[A-z0-9-]*\z"))
            {
                return View("Invalid characters in setting id");
            }

            if (!CurrentDatabase.Settings.Any(s => s.Id == idAccessToken))
            {
                //Create access token
                var s = new Setting { Id = idAccessToken, SettingX = _at };
                CurrentDatabase.Settings.InsertOnSubmit(s);
                CurrentDatabase.SubmitChanges();
                CurrentDatabase.SetSetting(idAccessToken, _at);
            }
            else
            { // Update access token
                CurrentDatabase.SetSetting(idAccessToken, _at);
                CurrentDatabase.SubmitChanges();
                DbUtil.LogActivity($"Edit Setting {idAccessToken} to {_at}", userId: Util.UserId);
            }
            if (!CurrentDatabase.Settings.Any(s => s.Id == idRefreshToken))
            { //Create refresh token
                var s = new Setting { Id = idRefreshToken, SettingX = _rt };
                CurrentDatabase.Settings.InsertOnSubmit(s);
                CurrentDatabase.SubmitChanges();
                CurrentDatabase.SetSetting(idRefreshToken, _rt);
            }
            else
            { // Update refresh token
                CurrentDatabase.SetSetting(idRefreshToken, _rt);
                CurrentDatabase.SubmitChanges();
                DbUtil.LogActivity($"Edit Setting {idRefreshToken} to {_rt}", userId: Util.UserId);
            }

            return RedirectToAction("Finish");
        }

        [Route("~/Pushpay/Finish")]
        public ActionResult Finish()
        { return View(); }

        [AllowAnonymous, Route("~/Pushpay/CompletePayment")]
        public async Task<ActionResult> CompletePayment(string paymentToken, string rc)
        {
            var _pushpayPayment = new PushPayPayment(CurrentDatabase, Configuration.Current.PushpayAPIBaseUrl,
                Configuration.Current.PushpayClientID,
                Configuration.Current.PushpayClientSecret,
                Configuration.Current.OAuth2TokenEndpoint,
                Configuration.Current.TouchpointAuthServer,
                Configuration.Current.OAuth2AuthorizeEndpoint);

            Payment payment = await _pushpayPayment.GetPayment(paymentToken, CurrentDatabase.GetSetting("PushpayMerchant", ""));

            //if (payment.Settlement?.Key.HasValue() == true)
            //{
            //     var bundle = await ResolveSettlement(payment.Settlement);
            //}

            int? peopleid = _pushpayPayment.ResolvePersonId(payment.Payer, CurrentDatabase);
            ContributionFund fund = _pushpayPayment.ResolveFund(payment.Fund, CurrentDatabase);


            if (payment != null)
            {
                //var ti = new Transaction
                //{
                //    Name = r.Payer.fullName,
                //    First = r.Payer.firstName,
                //    MiddleInitial = "",
                //    Last = r.Payer.lastName,
                //    Suffix = "",
                //    Donate = t.Donate,
                //    Amtdue = payment.Amount,
                //    Amt = payment.Amount.Amount,
                //    Emails = payment.Payer.emailAddress,
                //    Testing = t.Testing,
                //    Description = t.Description,
                //    OrgId = t.OrgId,
                //    Url = t.Url,
                //    Address = t.Address,
                //    TransactionGateway = "Pushpay",
                //    City = t.City,
                //    State = t.State,
                //    Zip = t.Zip,
                //    DatumId = t.DatumId,
                //    Phone = t.Phone,
                //    OriginalId = t.OriginalId ?? t.Id,
                //    Financeonly = t.Financeonly,
                //    TransactionDate = Util.Now,
                //    PaymentType = t.PaymentType,
                //    LastFourCC = t.LastFourCC,
                //    LastFourACH = t.LastFourACH
                //};

                //var staff = CurrentDatabase.StaffPeopleForOrg(34);
                //var body = GivingConfirmation.PostAndBuild(CurrentDatabase, staff, p.person, p.setting.Body, 34, p.FundItemsChosen(), Transaction, desc,
                //    p.setting.DonationFundId);
            }
            
            return View();
        }

    }
}
