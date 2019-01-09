﻿using CmsData.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using UtilityExtensions;

namespace CmsData
{
    public class ChartDTO
    {
        public int? Count;

        public string Name;
    }

    public class LineChartDTO
    {
        public string ChartName;
        public int? Count;
        public int? Count2;
        public int CurYear;
        public string Name;
        public int PreYear;
    }

    public class MonthDTO
    {
        public string Name;
    }

    public class GoogleChartsData
    {
        public List<ChartDTO> GetChartData(int? progId)
        {
            var cn = new SqlConnection(Util.ConnectionString);
            cn.Open();
            if (progId == null)
            {
                progId = 0;
            }

            // var rd = cn.ExecuteReader("ChartMorningWorship", commandType: CommandType.StoredProcedure, commandTimeout: 600);
            var myList = new List<ChartDTO>();
            var cmd = new SqlCommand("ChartMorningWorship", cn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 600
            };
            cmd.Parameters.Add(new SqlParameter("@progId", progId));
            IDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var newItem = new ChartDTO { Name = dr.GetString(0), Count = dr.GetInt32(1) };

                myList.Add(newItem);
            }

            return myList.ToList();
        }

        public List<LineChartDTO> GetAttendanceChartData(int[] orgIds)
        {
            var db = DbUtil.Db; // to avoid calling httpcontext multiple times, create local;

            var api = new APIContributionSearchModel(db);

            var myFinalList = new List<LineChartDTO>();
            var year = DateTime.Now.Year;
            var firstDay = new DateTime(year, 1, 1);
            var lastDay = new DateTime(year, 12, 31);

            var meetingsInOrg = from m in db.Meetings
                                join o in db.Organizations on m.OrganizationId equals o.OrganizationId
                                where m.MeetingDate.Value.Year == DateTime.Now.Year

                            var myList = from m in db.Meetings
                                         where m.MeetingDate.Value.Year == DateTime.Now.Year &&
                                               (from d in db.DivOrgs
                                                join pd in db.ProgDivs on d.DivId equals pd.DivId
                                                where d.OrgId == m.OrganizationId
                                                select m.OrganizationId).Contains(m.OrganizationId)
                                         group m by new { m.MeetingDate.Value.Month }
                into grp
                                         select new ChartDTO
                                         {
                                             Name = grp.First().MeetingDate.Value.ToString("MMM", CultureInfo.InvariantCulture),
                                             Count = Convert.ToInt32(grp.Sum(t => t.MaxCount).Value)
                                         };

            var myList1 = from m in db.Meetings
                          where m.MeetingDate.Value.Year == DateTime.Now.Year - 1 &&
                                (from d in db.DivOrgs
                                 join pd in db.ProgDivs on d.DivId equals pd.DivId
                                 where d.OrgId == m.OrganizationId
                                 select m.OrganizationId).Contains(m.OrganizationId)
                          group m by new { m.MeetingDate.Value.Month }
                into grp
                          select new ChartDTO
                          {
                              Name = grp.First().MeetingDate.Value.ToString("MMM", CultureInfo.InvariantCulture),
                              Count = Convert.ToInt32(grp.Sum(t => t.MaxCount).Value)
                          };

            if (orgIds.IsNotNull())
            {
                if (!(orgIds.Length == 1 && orgIds[0].Equals(0)))
                {
                    myList = from m in db.Meetings
                             where m.MeetingDate.Value.Year == DateTime.Now.Year &&
                                   orgIds.Contains(m.OrganizationId)
                             group m by new { m.MeetingDate.Value.Month }
                        into grp
                             select new ChartDTO
                             {
                                 Name = grp.First().MeetingDate.Value.ToString("MMM", CultureInfo.InvariantCulture),
                                 Count = Convert.ToInt32(grp.Sum(t => t.MaxCount).Value)
                             };

                    myList1 = from m in db.Meetings
                              where m.MeetingDate.Value.Year == DateTime.Now.Year - 1 &&
                                    orgIds.Contains(m.OrganizationId)
                              group m by new { m.MeetingDate.Value.Month }
                        into grp
                              select new ChartDTO
                              {
                                  Name = grp.First().MeetingDate.Value.ToString("MMM", CultureInfo.InvariantCulture),
                                  Count = Convert.ToInt32(grp.Sum(t => t.MaxCount).Value)
                              };
                }
            }

            var myList3 = DateTimeFormatInfo.InvariantInfo.AbbreviatedMonthNames;

            var emptytableQuery = from m in myList3
                                  where m.HasValue()
                                  select new ChartDTO
                                  {
                                      Name = m,
                                      Count = 0
                                  };

            myFinalList = (from e in emptytableQuery
                           join t in myList on e.Name equals t.Name into tm
                           join s in myList1 on e.Name equals s.Name into sm
                           from rdj in tm.DefaultIfEmpty()
                           from sdj in sm.DefaultIfEmpty()
                           select new LineChartDTO
                           {
                               ChartName = "MONTHLY ATTENDANCE ANALYSIS",
                               CurYear = DateTime.Now.Year,
                               PreYear = DateTime.Now.Year - 1,
                               Name = e.Name,
                               Count = rdj == null ? 0 : rdj.Count,
                               Count2 = sdj == null ? 0 : sdj.Count
                           }).ToList();

            return myFinalList;
        }

        public List<LineChartDTO> GetFundChartData(int[] fundIds)
        {
            var api = new APIContributionSearchModel(DbUtil.Db);

            var myFinalList = new List<LineChartDTO>();

            var myList = (from c in DbUtil.Db.Contributions
                          where c.ContributionDate.Value.Year == DateTime.Now.Year
                          group c by new { c.ContributionDate.Value.Month }
                into grp
                          select new ChartDTO
                          {
                              Name = grp.First().ContributionDate.Value.ToString("MMM", CultureInfo.InvariantCulture),
                              Count = Convert.ToInt32(grp.Sum(t => t.ContributionAmount).Value)
                          }).ToList();

            var myList1 = (from ce in DbUtil.Db.Contributions
                           where ce.ContributionDate.Value.Year == DateTime.Now.Year - 1
                           group ce by new { ce.ContributionDate.Value.Month }
                into grpc
                           select new ChartDTO
                           {
                               Name = grpc.First().ContributionDate.Value.ToString("MMM", CultureInfo.InvariantCulture),
                               Count = Convert.ToInt32(grpc.Sum(t => t.ContributionAmount).Value)
                           }).ToList();
            if (fundIds.IsNotNull())
            {
                if (!(fundIds.Length == 1 && fundIds[0].Equals(0)))
                {
                    myList = (from c in DbUtil.Db.Contributions
                              where c.ContributionDate.Value.Year == DateTime.Now.Year &&
                                    fundIds.Contains(c.FundId)
                              group c by new { c.ContributionDate.Value.Month }
                        into grp
                              select new ChartDTO
                              {
                                  Name = grp.First().ContributionDate.Value.ToString("MMM", CultureInfo.InvariantCulture),
                                  Count = Convert.ToInt32(grp.Sum(t => t.ContributionAmount).Value)
                              }).ToList();

                    myList1 = (from ce in DbUtil.Db.Contributions
                               where ce.ContributionDate.Value.Year == DateTime.Now.Year - 1 &&
                                     fundIds.Contains(ce.FundId)
                               group ce by new { ce.ContributionDate.Value.Month }
                        into grpc
                               select new ChartDTO
                               {
                                   Name = grpc.First().ContributionDate.Value.ToString("MMM", CultureInfo.InvariantCulture),
                                   Count = Convert.ToInt32(grpc.Sum(t => t.ContributionAmount).Value)
                               }).ToList();
                }
            }

            var myList3 = DateTimeFormatInfo.InvariantInfo.AbbreviatedMonthNames;

            var emptytableQuery = from m in myList3
                                  where m.HasValue()
                                  select new ChartDTO
                                  {
                                      Name = m,
                                      Count = 0
                                  };
            myFinalList = (from e in emptytableQuery
                           join t in myList on e.Name equals t.Name into tm
                           join s in myList1 on e.Name equals s.Name into sm
                           from rdj in tm.DefaultIfEmpty()
                           from sdj in sm.DefaultIfEmpty()
                           select new LineChartDTO
                           {
                               ChartName = "MONTHLY GIVING ANALYSIS",
                               CurYear = DateTime.Now.Year,
                               PreYear = DateTime.Now.Year - 1,
                               Name = e.Name,
                               Count = rdj == null ? null : rdj.Count,
                               Count2 = sdj == null ? 0 : sdj.Count
                           }).ToList();


            return myFinalList;
        }
    }
}
