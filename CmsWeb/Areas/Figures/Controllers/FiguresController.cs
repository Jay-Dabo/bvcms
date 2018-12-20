using CmsData;
using CmsWeb.Lifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CmsWeb.Areas.Figures.Controllers
{
    public class FiguresController : CMSBaseController
    {
        public class LookupItem
        {
            public string ItemType { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class FiguresLookupItems
        {
            public IEnumerable<LookupItem> Programs { get; set; }
            public IEnumerable<LookupItem> Divisions { get; set; }
            public IEnumerable<LookupItem> Organizations { get; set; }
            public IEnumerable<LookupItem> ContributionFunds { get; set; }
        }

        public FiguresController(IRequestManager requestManager) : base(requestManager)
        {
        }

        public ActionResult Index()
        {
            var model = GetLookupItems();
            return View(model);
        }

        public ActionResult RefineAttendanceView()
        {
            var model = GetLookupItems();
            return View(model);
        }

        public ActionResult RefineFundView()
        {
            var model = GetLookupItems();
            return View(model);
        }

        public ActionResult GetMapData(int? progId)
        {
            var test = new GoogleChartsData();
            return Json(test.GetChartData(progId).ToList(), JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        public ActionResult ChartDisplayView(int[] fundIdsArr)
        {
            var test = new GoogleChartsData();
            var temp = test.GetFundChartData(fundIdsArr).ToList();
            return View(temp);
        }

        [ChildActionOnly]
        public ActionResult AttendanceChartDisplayView(int[] orgIdsArr)
        {
            var test = new GoogleChartsData();
            var temp = test.GetAttendanceChartData(orgIdsArr).ToList();
            return View(temp);
        }

        private FiguresLookupItems GetLookupItems()
        {
            var cacheKey = $"{CurrentDatabase.Host}|FiguresLookupItems";
            var cachedItems = (FiguresLookupItems)CurrentHttpContext.Cache.Get(cacheKey);

            if (cachedItems != null)
            {
                return cachedItems;
            }

            var items = (from f in CurrentDatabase.Programs
                         join d in CurrentDatabase.Divisions on f.Id equals d.ProgId
                         join o in CurrentDatabase.Organizations on d.Id equals o.DivisionId
                         select new
                         {
                             ProgramId = f.Id,
                             ProgramName = f.Name,
                             DivisionId = d.Id,
                             DivisionName = d.Name,
                             OrganizationId = o.OrganizationId,
                             OrganizationName = o.OrganizationName
                         }).ToList();

            cachedItems = new FiguresLookupItems
            {
                Programs = items.Select(x => new LookupItem { Id = x.ProgramId, Name = x.ProgramName }).Distinct().ToList(),
                Divisions = items.Select(x => new LookupItem { Id = x.DivisionId, Name = x.DivisionName }).Distinct().ToList(),
                Organizations = items.Select(x => new LookupItem { Id = x.OrganizationId, Name = x.OrganizationName }).Distinct().ToList(),
                ContributionFunds = CurrentDatabase.ContributionFunds.Select(x => new LookupItem { Id = x.FundId, Name = x.FundName }).ToList()
            };

            CurrentHttpContext.Cache.Insert(cacheKey, cachedItems, null, DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(5));

            return cachedItems;
        }
    }
}
