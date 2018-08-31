﻿using System;
using System.Collections.Generic;

namespace CmsWeb.Areas.Coordinator.Models
{
    public class CheckinScheduleDto
    {
        public int OrgScheduleId { get; set; }
        public DateTime? NextMeetingDate { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public int SubgroupId { get; set; }
        public string SubgroupName { get; set; }
        public int CheckInCapacity { get; set; }
        public bool CheckInOpen { get; set; }
        public int CheckInCapacityDefault { get; set; }
        public bool CheckInOpenDefault { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public int AttendeeWorkerCount { get; set; }
        public int AttendeeMemberCount { get; set; }
        public List<CheckinAttendeeDto> Attendees { get; set; }
    }
}
