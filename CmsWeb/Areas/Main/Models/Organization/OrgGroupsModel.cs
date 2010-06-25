﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CmsData;
using System.Web.Mvc;
using UtilityExtensions;
using CMSPresenter;
using System.Text.RegularExpressions;
using System.Collections;

namespace CMSWeb.Models
{
    public class OrgGroupsModel
    {
        public int orgid { get; set; }
        public int? groupid { get; set; }
        public string GroupName { get; set; }
        public string ingroup { get; set; }
        public string notgroup { get; set; }
        public bool notgroupactive { get; set; }

        public string OrgName
        {
            get { return DbUtil.Db.LoadOrganizationById(orgid).OrganizationName; }
        }
        

        public int memtype { get; set; }

        private IList<int> list = new List<int>();
        public IList<int> List
        {
            get { return list; }
            set { list = value; }
        }
        public SelectList Groups()
        {
            var q = from g in DbUtil.Db.MemberTags
                    where g.OrgId == orgid
                    orderby g.Name
                    select new
                    {
                        Value = g.Id,
                        Text = g.Name,
                    };
            var list = q.ToList();
            list.Insert(0, new { Value = 0, Text = "(not specified)" });
            return new SelectList(list, "Value", "Text", groupid);
        }
        private List<SelectListItem> mtypes;
        private List<SelectListItem> MemberTypes()
        {
            if (mtypes == null)
            {
                var q = from om in DbUtil.Db.OrganizationMembers
                        where om.OrganizationId == orgid
                        where om.Pending ?? false == false
                        where om.MemberTypeId != (int)OrganizationMember.MemberTypeCode.InActive
                        group om by om.MemberType into g
                        orderby g.Key.Description
                        select new SelectListItem
                        {
                            Value = g.Key.Id.ToString(),
                            Text = g.Key.Description,
                        };
                mtypes = q.ToList();
            }
            return mtypes;
        }
        public IEnumerable<SelectListItem> MemberTypeCodesWithNotSpecified()
        {
            var mt = MemberTypes().ToList();
            mt.Insert(0, new SelectListItem { Value = "0", Text = "(not specified)" });
            return mt;
        }

        public int count;
        public IEnumerable<PersonInfo> FetchOrgMemberList()
        {
            var q = OrgMembers();
            if (memtype != 0)
                q = q.Where(om => om.MemberTypeId == memtype);
            if (ingroup.HasValue())
                q = q.Where(om => om.OrgMemMemTags.Any(omt => omt.MemberTag.Name.StartsWith(ingroup)));
            if (notgroupactive)
                if (notgroup.HasValue())
                    q = q.Where(om => !om.OrgMemMemTags.Any(omt => omt.MemberTag.Name.StartsWith(notgroup)));
                else
                    q = q.Where(om => om.OrgMemMemTags.Count() == 0);

            count = q.Count();
            var q2 = from m in q
                     let p = m.Person
                     let ck = m.OrgMemMemTags.Any(g => g.MemberTagId == groupid.ToInt())
                     orderby !ck, p.Name2
                     select new PersonInfo
                     {
                         PeopleId = m.PeopleId,
                         Name = p.Name,
                         LastName = p.LastName,
                         JoinDate = p.JoinDate,
                         BirthDate = p.DOB,
                         Address = p.PrimaryAddress,
                         CityStateZip = p.CityStateZip5,
                         HomePhone = p.HomePhone.FmtFone(),
                         CellPhone = p.CellPhone.FmtFone(),
                         WorkPhone = p.WorkPhone.FmtFone(),
                         Email = p.EmailAddress,
                         Age = p.Age,
                         MemberStatus = p.MemberStatus.Description,
                         ischecked = ck,
                         Gender = p.Gender.Description,
                         Request = m.Request,
                         Groups = m.OrgMemMemTags.Select(mt => new GroupInfo { Name = mt.MemberTag.Name, Count = mt.MemberTag.OrgMemMemTags.Count() }).OrderBy(s => s.Name)
                     };
            return q2;
        }
        public IQueryable<OrganizationMember> OrgMembers()
        {
            var q = from om in DbUtil.Db.OrganizationMembers
                    where om.OrganizationId == orgid
                    //where om.OrgMemMemTags.Any(g => g.MemberTagId == sg) || (sg ?? 0) == 0
                    select om;
            return q;
        }
        public class GroupInfo
        {
            public string Name { get; set; }
            public int Count { get; set; }
        }
        public class PersonInfo
        {
            public int PeopleId { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public DateTime? JoinDate { get; set; }
            public string Email { get; set; }
            public string BirthDate { get; set; }
            public string Address { get; set; }
            public string CityStateZip { get; set; }
            public string HomePhone { get; set; }
            public string CellPhone { get; set; }
            public string WorkPhone { get; set; }
            public int? Age { get; set; }
            public string MemberStatus { get; set; }
            public string Gender { get; set; }
            public string Request { get; set; }
            public IEnumerable<GroupInfo> Groups { get; set; }
            public string GroupsDisplay
            {
                get
                {
                    var s = string.Join(",~", Groups.Select(g => "{0}({1})".Fmt(g.Name, g.Count)).ToArray());
                    return s.Replace(" ", "&nbsp;").Replace(",~", "<br />\n");
                }
            }
            public bool ischecked { get; set; }
            public string IsInGroup()
            {
                return ischecked ? "style='color:blue;'" : "";
            }
            public string ToolTip
            {
                get
                {
                    return "{0} ({1})|Cell Phone: {2}|Work Phone: {3}|Home Phone: {4}|BirthDate: {5:d}|Join Date: {6:d}|Status: {7}|Email: {8}"
                        .Fmt(Name, PeopleId, CellPhone, WorkPhone, HomePhone, BirthDate, JoinDate, MemberStatus, Email);
                }
            }
        }
    }
}
