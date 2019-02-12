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
using CmsData.Codes;
using CmsData.Registration;

namespace CmsWeb.Areas.Setup.Controllers
{
    [RouteArea("Setup", AreaPrefix = "Pushpay")]
    public class PushpayController : Controller
    {
        //todo: Inheritance chain
        private readonly RequestManager RequestManager;
        private CMSDataContext CurrentDatabase => RequestManager.CurrentDatabase;

        private PushpayConnection _pushpay;
        private string _givingLink;
        private string _merchantHandle;

        public PushpayController(RequestManager requestManager)
        {
            RequestManager = requestManager;
            _pushpay = new PushpayConnection(
                CurrentDatabase.GetSetting("PushPayAccessToken", ""),
                CurrentDatabase.GetSetting("PushPayRefreshToken", ""),
                CurrentDatabase,
                Configuration.Current.PushpayAPIBaseUrl,
                Configuration.Current.PushpayClientID,
                Configuration.Current.PushpayClientSecret,
                Configuration.Current.OAuth2TokenEndpoint,
                Configuration.Current.TouchpointAuthServer,
                Configuration.Current.OAuth2AuthorizeEndpoint);

            _merchantHandle = CurrentDatabase.Setting("PushpayMerchant", null);
            _givingLink = $"{Configuration.Current.PushpayGivingLinkBase}/{_merchantHandle}";
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

        [Route("~/Pushpay/OneTime/{OrgId:int}")]
        public ActionResult OneTime(int OrgId)
        {
            return Redirect($"{_givingLink}?ru={_merchantHandle}&sr=Org_{OrgId}&rcv=false");
        }

        [Route("~/Pushpay/OnePage")]
        public ActionResult OnePage()
        {
            return Redirect($"{_givingLink}?rcv=false");
        }

        [Route("~/Pushpay/RecurringGiving/{PeopleId:int}")]
        public ActionResult RecurringGiving(int PeopleId)
        {
            PushPayResolver resolver = new PushPayResolver(CurrentDatabase, _pushpay);
            string PayerKey = resolver.ResolvePayerKey(PeopleId);
            return Redirect($"{_givingLink}?rcv=false");
        }

        [AllowAnonymous, Route("~/Pushpay/CompletePayment")]
        public async Task<ActionResult> CompletePayment(string paymentToken, string sr)
        {
            try
            {
                int orgId = Int32.Parse(sr.Substring(4));
                SetHeaders2(orgId);
                ViewBag.OrgId = orgId;
                PushPayPayment pushpayPayment = new PushPayPayment(_pushpay, CurrentDatabase);
                PushPayResolver resolver = new PushPayResolver(CurrentDatabase, _pushpay);

                int ManageGivingOrg = (from o in CurrentDatabase.Organizations
                                       where o.RegistrationTypeId == RegistrationTypeCode.ManageGiving
                                       select o.OrganizationId).FirstOrDefault();
                if (ManageGivingOrg == orgId)
                {                                        
                    RecurringPayment recurringPayment = await pushpayPayment.GetRecurringPayment(paymentToken);
                    if (recurringPayment.Schedule != null)
                    {
                        ViewBag.Message = "Thanks for set up your recurring giving.";
                        return View();
                    }
                }

                Payment payment = await pushpayPayment.GetPayment(paymentToken);

                if (payment != null && !resolver.TransactionAlreadyImported(payment))
                {
                    // determine the batch to put the payment in
                    BundleHeader bundle;
                    if (payment.Settlement?.Key.HasValue() == true)
                    {
                        bundle = await resolver.ResolveSettlement(payment.Settlement);
                    }
                    else
                    {
                        // create a new bundle for each payment not part of a PushPay batch or settlement
                        bundle = resolver.CreateBundle(payment.CreatedOn.ToLocalTime(), payment.Amount.Amount, null, null, payment.TransactionId, BundleReferenceIdTypeCode.PushPayStandaloneTransaction);
                    }
                    int? PersonId = resolver.ResolvePersonId(payment.Payer);
                    ContributionFund fund = resolver.ResolveFund(payment.Fund);
                    Contribution contribution = resolver.ResolvePayment(payment, fund, PersonId, bundle);
                    Transaction transaction = resolver.ResolveTransaction(payment, PersonId.Value, orgId);
                }
                ViewBag.Message = "Thank you, your transaction is complete for Online Giving.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Something went wrong";
                CurrentDatabase.LogActivity($"Error in pushpay payment process: {ex.Message}");
                return View("~/Views/Shared/PageError.cshtml");
            }
        }


        private void SetHeaders2(int id)
        {
            var org = CurrentDatabase.LoadOrganizationById(id);
            var shell = "";
            var settings = HttpContext.Items["RegSettings"] as Dictionary<int, Settings>;
            if ((settings == null || !settings.ContainsKey(id)) && org != null)
            {
                var setting = CurrentDatabase.CreateRegistrationSettings(id);
                shell = CurrentDatabase.ContentOfTypeHtml(setting.ShellBs)?.Body;
            }
            if (!shell.HasValue() && settings != null && settings.ContainsKey(id))
                shell = CurrentDatabase.ContentOfTypeHtml(settings[id].ShellBs)?.Body;
            if (!shell.HasValue())
            {
                shell = CurrentDatabase.ContentOfTypeHtml("ShellDefaultBs")?.Body;
                if (!shell.HasValue())
                    shell = CurrentDatabase.ContentOfTypeHtml("DefaultShellBs")?.Body;
            }
            if (shell != null && shell.HasValue())
            {
                shell = shell.Replace("{title}", ViewBag.Title);
                var re = new Regex(@"(.*<!--FORM START-->\s*).*(<!--FORM END-->.*)", RegexOptions.Singleline);
                var t = re.Match(shell).Groups[1].Value.Replace("<!--FORM CSS-->", ViewExtensions2.Bootstrap3Css());
                ViewBag.hasshell = true;
                ViewBag.top = t;
                var b = re.Match(shell).Groups[2].Value;
                ViewBag.bottom = b;
            }
            else
                ViewBag.hasshell = false;
        }
    }
}
