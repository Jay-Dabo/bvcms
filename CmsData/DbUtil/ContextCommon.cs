using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmsData.Common
{
    public class AttendMeetingInfo1
    {
        public AttendMeetingInfo2 info;
        public Attend AttendanceOrg;
        public Attend Attendance;
        public Meeting Meeting;
        public List<Attend> VIPAttendance;
        public OrganizationMember BFCMember;
        public Attend BFCAttendance;
        public Meeting BFCMeeting;
        public int path;
    }

    public class AttendMeetingInfo2
    {
        public int? AttendedElsewhere { get; set; }
        public int? MemberTypeId { get; set; }
        //public bool? IsRegularHour { get; set; }
        //public int? ScheduleId { get; set; }
        //public bool? IsSameHour { get; set; }
        public bool? IsOffSite { get; set; }
        public bool? IsRecentVisitor { get; set; }
        public string Name { get; set; }
        public int? BFClassId { get; set; }
    }

    public class TopGiver
    {
        public int PeopleId;
        public string Name;
        public decimal Amount;
    }

    public class RecordAttendInfo
    {
        public string ret { get; set; }
    }
}
