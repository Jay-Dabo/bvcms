using CmsData;
using CmsData.Codes;
using CmsWeb.Areas.OnlineReg.Models;
using CmsWeb.Code;
using System;
using System.Linq;
using System.Web.Mvc;
using UtilityExtensions;

namespace CmsWeb.Areas.Dialog.Models
{
    public class MissionSupportModel
    {
        private int? orgId;
        private int? peopleId;
        public MissionSupportModel() { }
        private void Populate()
        {
            var i = (from mm in DbUtil.Db.OrganizationMembers
                     where mm.OrganizationId == OrgId && mm.PeopleId == PeopleId
                     select new
                     {
                         mm.Person.Name,
                         mm.Organization.OrganizationName,
                     }).Single();
            Name = i.Name;
            OrgName = i.OrganizationName;
            if (Goer == null)
            {
                Goer = new CodeInfo(0, GoerList());
            }
        }

        //public OrgMemberModel OrgMemberModel { get { return new OrgMemberModel() { PeopleId = PeopleId, OrgId = OrgId };} }

        public CodeInfo Goer { get; set; }

        public string Group { get; set; }

        public int? OrgId
        {
            get { return orgId; }
            set
            {
                orgId = value;
                if (peopleId.HasValue)
                {
                    Populate();
                }
            }
        }
        public int? PeopleId
        {
            get { return peopleId; }
            set
            {
                peopleId = value;
                if (orgId.HasValue)
                {
                    Populate();
                }
            }
        }
        public string Name { get; set; }
        public string OrgName { get; set; }
        public string CheckNo { get; set; }
        public decimal? AmountGoer { get; set; }
        public decimal? AmountGeneral { get; set; }

        public SelectList GoerList()
        {
            var q = from om in DbUtil.Db.OrganizationMembers
                    where om.OrgMemMemTags.Any(mm => mm.MemberTag.Name == "Goer")
                    where om.OrganizationId == OrgId
                    orderby om.Person.Name2
                    select new CodeValueItem
                    {
                        Id = om.PeopleId,
                        Value = om.Person.Name2,
                    };
            var list = q.ToList();
            list.Insert(0, new CodeValueItem { Id = 0, Value = "(please select a Goer)" });
            return list.ToSelect();
        }

        public string ToGoerName;
        internal void PostContribution()
        {
            var db = DbUtil.Db; // at least reducing to a single static call
            if (!(AmountGeneral > 0) && !(AmountGoer > 0))
            {
                return;
            }

            var org = db.LoadOrganizationById(OrgId);
            var notifyIds = db.NotifyIds(org.GiftNotifyIds);
            var person = db.LoadPersonById(PeopleId ?? 0);
            var setting = db.CreateRegistrationSettings(OrgId ?? 0);
            var fund = setting.DonationFundId;
            if (AmountGoer > 0)
            {
                var goerid = Goer.Value.ToInt();
                db.GoerSenderAmounts.InsertOnSubmit(
                    new GoerSenderAmount
                    {
                        Amount = AmountGoer,
                        GoerId = goerid,
                        Created = DateTime.Now,
                        OrgId = org.OrganizationId,
                        SupporterId = PeopleId ?? 0,
                    });
                var c = person.PostUnattendedContribution(db, AmountGoer ?? 0, fund, $"SupportMissionTrip: org={OrgId}; goer={Goer.Value}", typecode: BundleTypeCode.MissionTrip);
                c.CheckNo = (CheckNo ?? "").Trim().Truncate(20);
                if (PeopleId == goerid)
                {
                    var om = db.OrganizationMembers.Single(mm => mm.PeopleId == goerid && mm.OrganizationId == OrgId);
                    var descriptionForPayment = OnlineRegModel.GetDescriptionForPayment(OrgId, db);
                    om.AddTransaction(db, "Payment", AmountGoer ?? 0, "Payment", pmtDescription: descriptionForPayment);
                }
                // send notices
                var goer = db.LoadPersonById(goerid);
                ToGoerName = "to " + goer.Name;
                db.Email(notifyIds[0].FromEmail, goer, org.OrganizationName + "-donation", $"{AmountGoer:C} donation received from {person.Name}");
                db.LogActivity("OrgMem SupportMissionTrip goer=" + goerid, OrgId, PeopleId);
            }
            if (AmountGeneral > 0)
            {
                db.GoerSenderAmounts.InsertOnSubmit(
                    new GoerSenderAmount
                    {
                        Amount = AmountGeneral,
                        Created = DateTime.Now,
                        OrgId = org.OrganizationId,
                        SupporterId = PeopleId ?? 0
                    });
                var c = person.PostUnattendedContribution(db, AmountGeneral ?? 0, fund, $"SupportMissionTrip: org={OrgId}", typecode: BundleTypeCode.MissionTrip);
                if (CheckNo.HasValue())
                {
                    c.CheckNo = (CheckNo ?? "").Trim().Truncate(20);
                }

                db.LogActivity("OrgMem SupportMissionTrip", OrgId, PeopleId);
            }
            db.SubmitChanges();
        }
    }
}
