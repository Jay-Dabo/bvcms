﻿using CmsData;
using CmsData.Codes;
using CmsData.Registration;
using CmsWeb.Common;
using CmsWeb.Lifecycle;
using CmsWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using TransactionGateway;
using TransactionGateway.ApiModels;
using UtilityExtensions;

namespace CmsWeb.Areas.Setup.Controllers
{
    [RouteArea("Setup", AreaPrefix = "Pushpay")]
    public class PushpayController : Controller
    {
        //todo: Inheritance chain
        private readonly RequestManager RequestManager;
        private CMSDataContext CurrentDatabase => RequestManager.CurrentDatabase;

        private PushpayConnection _pushpay;
        private PushpayPayment _pushpayPayment;
        private PushpayResolver _resolver;
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
            _pushpayPayment = new PushpayPayment(_pushpay, CurrentDatabase);
            _resolver = new PushpayResolver(_pushpay, CurrentDatabase);

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

        [Route("~/Pushpay/OneTime/{PeopleId:int}/{OrgId:int}")]
        public ActionResult OneTime(int PeopleId, int OrgId)
        {
            string mobile = CurrentDatabase.People.Where(p => p.PeopleId == PeopleId).FirstOrDefault().CellPhone;
            return Redirect($"{_givingLink}?ru={_merchantHandle}&sr=Org_{OrgId}&rcv=false&up={mobile}");
        }

        [Route("~/Pushpay/OnePage")]
        public ActionResult OnePage()
        {
            return Redirect($"{_givingLink}?rcv=false");
        }

        [Route("~/Pushpay/NewRecurringGiving/{PeopleId:int}/{OrgId:int}")]
        public ActionResult NewRecurringGiving(int PeopleId, int OrgId)
        {
            string mobile = CurrentDatabase.People.Where(p => p.PeopleId == PeopleId).FirstOrDefault().CellPhone;
            return Redirect($"{_givingLink}?ru={_merchantHandle}&sr=Org_{OrgId}&r=monthly&up={mobile}");
        }

        [Route("~/Pushpay/RecurringManagment/{PeopleId:int}/{OrgId:int}")]
        public async Task<ActionResult> RecurringManagment(int PeopleId, int OrgId)
        {
            ViewBag.PeopleId = PeopleId;
            ViewBag.OrgId = OrgId;
            string payerKey = _resolver.ResolvePayerKey(PeopleId);
            IEnumerable<RecurringPayment> rpList = await _pushpayPayment.GetRecurringPaymentsForAPayer(payerKey);
            if (rpList == null || rpList.Count() == 0)
            {
                string mobile = CurrentDatabase.People.Where(p => p.PeopleId == PeopleId).FirstOrDefault().CellPhone;
                return NewRecurringGiving(PeopleId, OrgId);
            }
            List<RecurringManagment> model = new List<RecurringManagment>();
            foreach (var item in rpList)
            {
                RecurringManagment mg = new RecurringManagment()
                {
                    NextPayment = item.Schedule.NextPaymentDate,
                    Amount = item.Amount.Amount,
                    Fund = item.Fund.Name,
                    Frequency = item.Schedule.Frequency,
                    LinkToEdit = item.Links["donorviewrecurringpayment"].Href
                };
                model.Add(mg);
            }
            return View(model);
        }

        [AllowAnonymous, Route("~/Pushpay/CompletePayment")]
        public async Task<ActionResult> CompletePayment(string paymentToken, string sr)
        {
            try
            {
                int orgId = Int32.Parse(sr.Substring(4));
                SetHeaders2(orgId);
                ViewBag.OrgId = orgId;

                int ManageGivingOrg = (from o in CurrentDatabase.Organizations
                                       where o.RegistrationTypeId == RegistrationTypeCode.ManageGiving
                                       select o.OrganizationId).FirstOrDefault();
                if (ManageGivingOrg == orgId)
                {
                    RecurringPayment recurringPayment = await _pushpayPayment.GetRecurringPayment(paymentToken);
                    if (recurringPayment.Schedule != null)
                    {
                        int? PersonId = _resolver.ResolvePersonId(recurringPayment.Payer);
                        ViewBag.Message = "Thanks for set up your recurring giving.";
                        return View();
                    }
                }

                Payment payment = await _pushpayPayment.GetPayment(paymentToken);

                if (payment != null && !_resolver.TransactionAlreadyImported(payment))
                {
                    // determine the batch to put the payment in
                    BundleHeader bundle;
                    if (payment.Settlement?.Key.HasValue() == true)
                    {
                        bundle = await _resolver.ResolveSettlement(payment.Settlement);
                    }
                    else
                    {
                        // create a new bundle for each payment not part of a PushPay batch or settlement
                        bundle = _resolver.CreateBundle(payment.CreatedOn.ToLocalTime(), payment.Amount.Amount, null, null, payment.TransactionId, BundleReferenceIdTypeCode.PushPayStandaloneTransaction);
                    }
                    int? PersonId = _resolver.ResolvePersonId(payment.Payer);
                    ContributionFund fund = _resolver.ResolveFund(payment.Fund);
                    Contribution contribution = _resolver.ResolvePayment(payment, fund, PersonId, bundle);
                    Transaction transaction = _resolver.ResolveTransaction(payment, PersonId.Value, orgId);
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
