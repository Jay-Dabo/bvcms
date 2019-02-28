using CmsData;
using System;
using System.Collections.Generic;
using System.Linq;
using UtilityExtensions;

namespace CmsWeb.Areas.OnlineReg.Models
{
    public partial class OnlineRegModel
    {
        public bool UserNeedsSelection;

        public IQueryable<Organization> UserSelectedClasses(Organization masterorg)
        {
            if (!masterorg.OrgPickList.HasValue())
            {
                return CurrentDatabase.Organizations.Where(oo => false);
            }

            var cklist = masterorg.OrgPickList.Split(',').Select(oo => oo.ToInt()).ToList();

            var q = from o in CurrentDatabase.Organizations
                    where cklist.Contains(o.OrganizationId)
                    select o;
            return q;

        }

        //todo: this method is referenced from models in views without a currently availably reference to a db context... marking obsolete and moved the other references to a non-static implementation
        [Obsolete("Use the non static version of this method")]
        public static IQueryable<Organization> UserSelectClasses(Organization masterorg)
        {
            if (!masterorg.OrgPickList.HasValue())
            {
                return DbUtil.Db.Organizations.Where(oo => false);
            }

            var cklist = masterorg.OrgPickList.Split(',').Select(oo => oo.ToInt()).ToList();

            var q = from o in DbUtil.Db.Organizations
                    where cklist.Contains(o.OrganizationId)
                    select o;
            return q;
        }

        public List<Organization> OrderedClasses(Organization masterorg)
        {
            if (masterorg == null)
            {
                throw new Exception("masterorg is null in OrderedClasses");
            }

            var cklist = masterorg.OrgPickList.Split(',').Select(oo => oo.ToInt()).ToList();
            var list = UserSelectedClasses(masterorg).ToList();
            var d = new Dictionary<int, int>();
            var n = 0;
            foreach (var i in cklist)
            {
                d.Add(n++, i);
            }

            list = (from o in list
                    join i in d on o.OrganizationId equals i.Value into j
                    from i in j
                    orderby i.Key
                    select o).ToList();
            return list;
        }

        public IEnumerable<ClassInfo> Classes(int? cid)
        {
            return Classes(CurrentDatabase, masterorg, cid ?? 0);
        }

        public static List<ClassInfo> Classes(CMSDataContext db, Organization masterorg, int id)
        {
            var q = from o in OrderedClasses(masterorg)
                    let hasroom = (o.ClassFilled ?? false) == false
                        && ((o.Limit ?? 0) == 0 || o.Limit > o.RegLimitCount(db))
                        && (o.RegistrationClosed ?? false) == false
                        && (o.RegEnd ?? DateTime.MaxValue) > Util.Now
                        && (o.RegStart ?? DateTime.MinValue) <= Util.Now
                    where o.RegistrationTypeId > 0
                    select new ClassInfo
                    {
                        Id = o.OrganizationId,
                        Text = ClassName(o),
                        selected = o.OrganizationId == id,
                        filled = !hasroom
                    };
            var list = q.ToList();
            return list;
        }

        private string ClassName(Organization o)
        {
            var lead = o.LeaderName;
            if (lead.HasValue())
            {
                lead = ": " + lead;
            }

            var loc = o.Location;
            if (loc.HasValue())
            {
                loc = $" ({loc})";
            }

            var dt1 = o.FirstMeetingDate;
            var dt2 = o.LastMeetingDate;
            var dt = "";
            if (dt1.HasValue && dt2.HasValue)
            {
                dt = $", {dt1:MMM d}-{dt2:MMM d}";
            }
            else if (dt1.HasValue)
            {
                dt = $", {dt1:MMM d}";
            }

            return o.Title + lead + dt + loc;
        }

        public class ClassInfo
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public bool selected { get; set; }
            public bool filled { get; set; }
        }
    }
}
