/* 
 * Copyright (c) 2008, 2009 Bellevue Baptist Church
 * Licensed under the GNU General Public License (GPL v2)
 * you may not use this code except in compliance with the License.
 * You may obtain a copy of the License at http://bvcms.codeplex.com/license
 */
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using CmsData.Common;

namespace CmsData
{
    public interface IDataContext
    {           
        int NextTagId { get; }
        Table<ActivityLog> ActivityLogs { get;}
        Table<Address> Addresses {get;}
        Table<AddressType> AddressTypes { get; }
        Table<AddToOrgFromTagRun> AddToOrgFromTagRuns { get; }
        Table<ApiSession> ApiSessions { get; }
        Table<Attend> Attends { get; }
        Table<AttendCredit> AttendCredits { get; }
        Table<AttendType> AttendTypes { get; }
        Table<Audit> Audits { get; }
        Table<AuditValue> AuditValues { get; }
        Table<BackgroundCheckLabel> BackgroundCheckLabels { get; }
        Table<BackgroundCheckMVRCode> BackgroundCheckMVRCodes { get; }
        Table<BackgroundCheck> BackgroundChecks { get; }
        Table<BaptismStatus> BaptismStatuses { get; }
        Table<BaptismType> BaptismTypes { get; }
        Table<BuildingAccessType> BuildingAccessTypes { get; }
        Table<BundleDetail> BundleDetails { get; }
        Table<BundleHeader> BundleHeaders { get; }
        Table<BundleHeaderType> BundleHeaderTypes { get; }
        Table<BundleStatusType> BundleStatusTypes { get; }
        Table<Campu> Campus { get; }
        Table<CardIdentifier> CardIdentifiers { get; }
        Table<ChangeDetail> ChangeDetails { get; }
        Table<ChangeLog> ChangeLogs { get; }
        Table<CheckedBatch> CheckedBatches { get; }
        Table<CheckInActivity> CheckInActivities { get; }
        Table<CheckInSetting> CheckInSettings { get; }
        Table<CheckInTime> CheckInTimes { get; }
        Table<ChurchAttReportId> ChurchAttReportIds { get; }
        Table<Contact> Contacts { get; }
        Table<Contactee> Contactees { get; }
        Table<ContactExtra> ContactExtras { get; }
        Table<Contactor> Contactors { get; }
        Table<ContactPreference> ContactPreferences { get; }
        Table<ContactReason> ContactReasons { get; }
        Table<ContactType> ContactTypes { get; }
        Table<Content> Contents { get; }
        Table<ContentKeyWord> ContentKeyWords { get; }
        Table<Contribution> Contributions { get; }
        Table<ContributionFund> ContributionFunds { get; }
        Table<ContributionsRun> ContributionsRuns { get; }
        Table<ContributionStatus> ContributionStatuses { get; }
        Table<ContributionType> ContributionTypes { get; }
        Table<Country> Countries { get; }
        Table<Coupon> Coupons { get; }
        Table<CustomColumn> CustomColumns { get; }
        Table<DecisionType> DecisionTypes { get; }
        Table<DeleteMeetingRun> DeleteMeetingRuns { get; }
        Table<Division> Divisions { get; }
        Table<DivOrg> DivOrgs { get; }
        Table<Downline> Downlines { get; }
        Table<DownlineLeader> DownlineLeaders { get; }
        Table<DropType> DropTypes { get; }
        Table<Duplicate> Duplicates { get; }
        Table<DuplicatesRun> DuplicatesRuns { get; }
        Table<EmailLink> EmailLinks { get; }
        Table<EmailLog> EmailLogs { get; }
        Table<EmailOptOut> EmailOptOuts { get; }
        Table<EmailQueue> EmailQueues { get; }
        Table<EmailQueueTo> EmailQueueTos { get; }
        Table<EmailQueueToFail> EmailQueueToFails { get; }
        Table<EmailResponse> EmailResponses { get; }
        Table<EmailToText> EmailToTexts { get; }
        Table<EnrollmentTransaction> EnrollmentTransactions { get; }
        Table<EntryPoint> EntryPoints { get; }
        Table<EnvelopeOption> EnvelopeOptions { get; }
        Table<ExtraDatum> ExtraDatas { get; }
        Table<Family> Families { get; }
        Table<FamilyCheckinLock> FamilyCheckinLocks { get; }
        Table<FamilyExtra> FamilyExtras { get; }
        Table<FamilyMemberType> FamilyMemberTypes { get; }
        Table<FamilyPosition> FamilyPositions { get; }
        Table<FamilyRelationship> FamilyRelationships { get; }
        Table<Gender> Genders { get; }
        Table<GeoCode> GeoCodes { get; }
        Table<GoerSenderAmount> GoerSenderAmounts { get; }
        Table<GoerSupporter> GoerSupporters { get; }
        Table<InterestPoint> InterestPoints { get; }
        Table<IpLog> IpLogs { get; }
        Table<IpLog2> IpLog2s { get; }
        Table<IpWarmup> IpWarmups { get; }
        Table<JoinType> JoinTypes { get; }
        Table<LabelFormat> LabelFormats { get; }
        Table<LongRunningOp> LongRunningOps { get; }
        Table<LongRunningOperation> LongRunningOperations { get; }
        Table<ManagedGiving> ManagedGivings { get; }
        Table<MaritalStatus> MaritalStatuses { get; }
        Table<MeetingExtra> MeetingExtras { get; }
        Table<Meeting> Meetings { get; }
        Table<MeetingType> MeetingTypes { get; }
        Table<MemberDocForm> MemberDocForms { get; }
        Table<MemberLetterStatus> MemberLetterStatuses { get; }
        Table<MemberStatus> MemberStatuses { get; }
        Table<MemberTag> MemberTags { get; }
        Table<MemberType> MemberTypes { get; }
        Table<MergeHistory> MergeHistories { get; }
        Table<Ministry> Ministries { get; }
        Table<MobileAppAction> MobileAppActions { get; }
        Table<MobileAppActionType> MobileAppActionTypes { get; }
        Table<MobileAppAudioType> MobileAppAudioTypes { get; }
        Table<MobileAppBuilding> MobileAppBuildings { get; }
        Table<MobileAppDevice> MobileAppDevices { get; }
        Table<MobileAppFloor> MobileAppFloors { get; }
        Table<MobileAppIcon> MobileAppIcons { get; }
        Table<MobileAppIconSet> MobileAppIconSets { get; }
        Table<MobileAppPushRegistration> MobileAppPushRegistrations { get; }
        Table<MobileAppRoom> MobileAppRooms { get; }
        Table<MobileAppVideoType> MobileAppVideoTypes { get; }
        Table<NewMemberClassStatus> NewMemberClassStatuses { get; }
        Table<Number> Numbers { get; }
        Table<OneTimeLink> OneTimeLinks { get; }
        Table<OrganizationExtra> OrganizationExtras { get; }
        Table<OrganizationMember> OrganizationMembers { get; }
        Table<Organization> Organizations { get; }
        Table<OrganizationStatus> OrganizationStatuses { get; }
        Table<OrganizationType> OrganizationTypes { get; }
        Table<OrgContent> OrgContents { get; }
        Table<OrgFilter> OrgFilters { get; }
        Table<OrgMemberExtra> OrgMemberExtras { get; }
        Table<OrgMemMemTag> OrgMemMemTags { get; }
        Table<OrgSchedule> OrgSchedules { get; }
        Table<Origin> Origins { get; }
        Table<PaymentInfo> PaymentInfos { get; }
        Table<Person> People { get; }
        Table<PeopleCanEmailFor> PeopleCanEmailFors { get; }
        Table<PeopleExtra> PeopleExtras { get; }
        Table<Picture> Pictures { get; }
        Table<PostalLookup> PostalLookups { get; }
        Table<Preference> Preferences { get; }
        Table<PrevOrgMemberExtra> PrevOrgMemberExtras { get; }
        Table<PrintJob> PrintJobs { get; }
        Table<ProgDiv> ProgDivs { get; }
        Table<Program> Programs { get; }
        Table<Promotion> Promotions { get; }
        Table<PushPayLog> PushPayLogs { get; }
        Table<QBConnection> QBConnections { get; }
        Table<Query> Queries { get; }
        Table<RecReg> RecRegs { get; }
        Table<RecurringAmount> RecurringAmounts { get; }
        Table<RegistrationDatum> RegistrationDatas { get; }
        Table<RelatedFamily> RelatedFamilies { get; }
        Table<RepairTransactionsRun> RepairTransactionsRuns { get; }
        Table<ResidentCode> ResidentCodes { get; }
        Table<Resource> Resources { get; }
        Table<ResourceAttachment> ResourceAttachments { get; }
        Table<ResourceCategory> ResourceCategories { get; }
        Table<ResourceOrganization> ResourceOrganizations { get; }
        Table<ResourceOrganizationType> ResourceOrganizationTypes { get; }
        Table<ResourceType> ResourceTypes { get; }
        Table<Role> Roles { get; }
        Table<RssFeed> RssFeeds { get; }
        Table<SecurityCode> SecurityCodes { get; }
        Table<Setting> Settings { get; }
        Table<SMSGroupMember> SMSGroupMembers { get; }
        Table<SMSGroup> SMSGroups { get; }
        Table<SMSItem> SMSItems { get; }
        Table<SMSList> SMSLists { get; }
        Table<SMSNumber> SMSNumbers { get; }
        Table<StateLookup> StateLookups { get; }
        Table<StreetType> StreetTypes { get; }
        Table<SubRequest> SubRequests { get; }
        Table<Tag> Tags { get; }
        Table<TagPerson> TagPeople { get; }
        Table<TagShare> TagShares { get; }
        Table<TagType> TagTypes { get; }
        Table<Task> Tasks { get; }
        Table<TaskList> TaskLists { get; }
        Table<TaskListOwner> TaskListOwners { get; }
        Table<TaskStatus> TaskStatuses { get; }
        Table<Transaction> Transactions { get; }
        Table<TransactionPerson> TransactionPeople { get; }
        Table<UploadPeopleRun> UploadPeopleRuns { get; }
        Table<UserRole> UserRoles { get; }
        Table<User> Users { get; }
        Table<VolApplicationStatus> VolApplicationStatuses { get; }
        Table<VolInterestCode> VolInterestCodes { get; }
        Table<VolInterestInterestCode> VolInterestInterestCodes { get; }
        Table<VolRequest> VolRequests { get; }
        Table<Volunteer> Volunteers { get; }
        Table<VolunteerCode> VolunteerCodes { get; }
        Table<VolunteerForm> VolunteerForms { get; }
        Table<VoluteerApprovalId> VoluteerApprovalIds { get; }
        Table<Word> Words { get; }
        Table<ZipCode> ZipCodes { get; }
        Table<Zip> Zips { get; }
        CMSDataContext Create(string connStr, string host);
        void OnCreated();
        void SubmitChanges(System.Data.Linq.ConflictMode failureMode);
        void ClearCache2();
        int GetMaxLength(string dbType);
        int GetMemberValueLength(object value);
        Person LoadPersonById(int id);
        Family LoadFamilyByPersonId(int id);
        Organization LoadOrganizationById(int? id);
        Contact LoadContactById(int? id);
        OrgFilter OrgFilter(Guid? id);
        bool OrgIdOk(int? id);
        bool PeopleIdOk(int? id);
        Meeting LoadMeetingById(int? id);
        Organization LoadOrganizationByName(string name);
        Organization LoadOrganizationByName(string name, int divid);
        string FetchExtra(int pid, string field);
        DateTime? FetchExtraDate(int pid, string field);
        IQueryable<Person> PeopleQueryLast();
        IQueryable<Person> PeopleQuery(Guid id);
        IQueryable<Person> PeopleQuery2(object query, bool fromDirectory = false);
        List<int> PeopleQueryIds(object query);
        IQueryable<Person> PeopleQuery2(string query, bool fromDirectory = false);
        IQueryable<Person> PeopleQueryCode(string code, bool fromDirectory = false);
        IQueryable<Person> PeopleQueryCondition(Condition c);
        IQueryable<Person> PersonQueryParents(IQueryable<Person> q);
        IQueryable<Person> PersonQueryPlusParents(IQueryable<Person> q);
        IQueryable<Person> PersonQueryFirstPersonSameEmail(IQueryable<Person> q);
        IQueryable<Person> ReturnPrimaryAdults(IQueryable<Person> q);
        void TagAll(IQueryable<Person> list);
        void TagAll(IQueryable<Person> list, Tag tag);
        void TagAll2(IQueryable<Person> list, Tag tag);
        string GetWhereClause(IQueryable<Person> list);
        void TagAll(IEnumerable<int> list, Tag tag);
        void UnTagAll(IQueryable<Person> list);
        void UnTagAll(List<int> list, Tag tag);
        void TagAll(List<int> list, Tag tag);
        Tag PopulateSpecialTag(Guid QueryId, int TagTypeId);
        Tag PopulateSpecialTag(IQueryable<Person> q, int TagTypeId);
        Tag NewTemporaryTag();
        Tag PopulateTemporaryTag(IQueryable<int> q);
        Tag PopulateTempTag(IEnumerable<int> a);
        void ClearTag(Tag tag);
        int PopulateSpecialTag(IQueryable<Person> q, string tagname, int tagTypeId);
        void DePopulateSpecialTag(IQueryable<Person> q, int TagTypeId);
        Tag TagById(int id);
        Tag TagCurrent();
        IEnumerable<Person> GetNewPeopleManagers();
        void GetCurrentUser();
        string[] CurrentRoles();
        int[] CurrentRoleIds();
        Tag OrgLeadersOnlyTag2();
        Tag FetchOrCreateTag(string tagname, int? ownerId, int tagtypeid);
        Tag FetchTag(string tagname, int? ownerId, int tagtypeid);
        int[] GetLeaderOrgIds(int? me);
        int[] GetParentChildOrgIds(int? parent);
        void SetOrgLeadersOnly();
        int AddAbsents([Parameter(DbType = "Int")] int? meetid, [Parameter(DbType = "Int")] int? userid);
        int UpdateAttendStr([Parameter(DbType = "Int")] int? orgid, [Parameter(DbType = "Int")] int? pid);
        int UpdatePastAttendStr([Parameter(DbType = "Int")] int? orgid, [Parameter(DbType = "Int")] int? pid);
        IMultipleResults AttendMeetingInfo(int MeetingId, int PeopleId);
        AttendMeetingInfo1 AttendMeetingInfo0(int MeetingId, int PeopleId);
        IMultipleResults RecordAttend([Parameter(DbType = "Int")] int? meetingId, [Parameter(DbType = "Int")] int? peopleId, [Parameter(DbType = "Bit")] bool? present);
        IMultipleResults RecordAttendance(
            [Parameter(DbType = "Int")] int? orgId,
            [Parameter(DbType = "Int")] int? peopleId,
            [Parameter(DbType = "DateTime")] DateTime meetingDate,
            [Parameter(DbType = "bit")] bool present,
            [Parameter(DbType = "Varchar(50)")] string location,
            [Parameter(DbType = "Int")] int? userId);
        string RecordAttendance(int? orgId, int? peopleId, DateTime meetingDate, bool present, string location);

        string RecordAttendance(int MeetingId, int PeopleId, bool present);

        bool UserPreference(string pref, bool def = false);

        string UserPreference(string pref, string defaultValue);


        void ToggleUserPreference(string pref);
        void SetUserPreference(string pref, object value);
        void SetUserPreference(int id, string pref, object value);
        int LinkEnrollmentTransaction([Parameter(DbType = "Int")] int? tid, [Parameter(DbType = "DateTime")] DateTime? trandt, [Parameter(DbType = "Int")] int? typeid, [Parameter(DbType = "Int")] int? orgid, [Parameter(DbType = "Int")] int? pid);
        int FlagOddTransaction([Parameter(DbType = "Int")] int? pid, [Parameter(DbType = "Int")] int? orgid);
        int PurgePerson([Parameter(DbType = "Int")] int? pid);
        int PurgeOrganization([Parameter(DbType = "Int")] int? oid);
        int UpdateMainFellowship([Parameter(DbType = "Int")] int? orgid);
        int UpdateMeetingCounters([Parameter(DbType = "Int")] int? mid);
        int DeletePeopleExtras([Parameter(DbType = "varchar(50)")] string field);
        int DeleteSpecialTags([Parameter(DbType = "Int")] int? pid);
        int UpdateResCodes();
        int PurgeAllPeopleInCampus([Parameter(DbType = "Int")] int? cid);
        int PopulateComputedEnrollmentTransactions([Parameter(DbType = "Int")] int? orgid);
        int RepairTransactions([Parameter(DbType = "Int")] int? orgid);
        int RepairTransactionsOrgs([Parameter(DbType = "Int")] int? orgid);
        int UpdateSchoolGrade([Parameter(DbType = "Int")] int? pid);
        int UpdateLastActivity([Parameter(DbType = "Int")] int? userid);
        int PurgeUser([Parameter(DbType = "Int")] int? uid);
        int DeleteQBTree([Parameter(DbType = "Int")] int? qid);
        int SetMainDivision([Parameter(DbType = "Int")] int? orgid, [Parameter(DbType = "Int")] int? divid);
        int DeleteQueryBitTags();
        int CreateMeeting([Parameter(DbType = "Int")] int oid, [Parameter(DbType = "DateTime")] DateTime? mdt);
        int InsertDuplicate([Parameter(DbType = "Int")] int i1, [Parameter(DbType = "Int")] int i2);
        int NoEmailDupsInTag([Parameter(DbType = "Int")] int tagid);
        int AttendUpdateN([Parameter(DbType = "Int")] int pid, [Parameter(DbType = "Int")] int max);
        int TrackOpen([Parameter(DbType = "UniqueIdentifier")] Guid guid);

        int TrackClick([Parameter(DbType = "VarChar(50)")] string hash, [Parameter(DbType = "VarChar(2000)")] ref string link);
        int SpamReporterRemove([Parameter(DbType = "VARCHAR(100)")] string email);
        int DropOrgMember([Parameter(DbType = "Int")] int oid, [Parameter(DbType = "Int")] int pid);
        int FastDrop([Parameter(DbType = "Int")] int oid, [Parameter(DbType = "Int")] int pid, [Parameter(DbType = "DateTime")] DateTime dropdate, [Parameter(DbType = "NVARCHAR(150)")] string orgname);
        int DeleteEnrollmentTransaction([Parameter(DbType = "Int")] int id);
        int RepairEnrollmentTransaction([Parameter(DbType = "Int")] int oid, [Parameter(DbType = "Int")] int pid);
        int PopulateTempTag([Parameter(DbType = "Int")] int id, [Parameter(DbType = "VARCHAR(MAX)")] string list);
        int AddTag1ToTag2([Parameter(DbType = "Int")] int t1, [Parameter(DbType = "Int")] int t2);
        int UpdateQuestion(
            [Parameter(DbType = "Int")] int? oid,
            [Parameter(DbType = "Int")] int? pid,
            [Parameter(DbType = "Int")] int? n,
            [Parameter(DbType = "VARCHAR(1000)")] string answer);
        int ArchiveContent([Parameter(DbType = "Int")] int? id);

        IQueryable<View.OrgPerson> OrgPeople(int? oid, string sgfilter);

        IQueryable<View.OrgPerson> OrgPeople(
             int? oid,
             string first,
             string last,
             string sgfilter,
             bool filterchecked,
             bool filtertag
            );
        IQueryable<View.OrgPerson> OrgPeople(
             int? oid,
             string grouptype,
             string first,
             string last,
             string sgfilter,
             bool showhidden,
             bool filterchecked,
             bool filtertag
            );
        OrganizationMember LoadOrgMember(int PeopleId, string OrgName, bool orgmustexist);
        IEnumerable<string[]> StatusFlags();
        IEnumerable<string[]> QueryStatClauses();
        Content Content(string name);
        string Content(string name, string defaultValue);
        Content ContentOfTypeHtml(string name);
        string ContentOfTypeSql(string name);
        Content ContentOfTypeSavedDraft(string name);
        string ContentOfTypePythonScript(string name);
        string ContentOfTypeText(string name);
        Content Content(string name, string defaultValue, int contentTypeId);

        Content Content(string name, int contentTypeId);
        string Content2(string name, string defaultValue, int contentTypeId);
        string ContentHtml(string name, string defaultValue);
        string ContentText(string name, string defaultValue);
        string ContentSql(string name, string defaultValue);
        void SetNoLock();
        int FetchOrCreateCampusId(string name);
        int FetchOrCreatePositionId(string name);
        int FetchOrCreateRoleId(string name);

        int FetchOrCreateOrgTypeId(string name);


        ContributionFund FetchOrCreateFund(string Description);

        EntryPoint FetchOrCreateEntryPoint(string type);
        ContributionFund FetchOrCreateFund(int FundId, string Description, bool? NonTax = null);
        int ActiveRecords();
        int ActiveRecords2();
        int ActiveRecordsdt(DateTime dt);
        int ActiveRecords2dt(DateTime dt);
        bool FromActiveRecords { get; set; }
        bool FromBatch { get; set; }

        IGateway Gateway(bool testing = false, string usegateway = null);

        Registration.Settings CreateRegistrationSettings(int orgId);
        Registration.Settings CreateRegistrationSettings(string s, int orgId);
        void UpdateStatusFlags();
        int TagRecentStartAttend(
            [Parameter(DbType = "Int")] int progid,
            [Parameter(DbType = "Int")] int divid,
            [Parameter(DbType = "Int")] int org,
            [Parameter(DbType = "Int")] int orgtype,
            [Parameter(DbType = "Int")] int days0,
            [Parameter(DbType = "Int")] int days,
            [Parameter(DbType = "Int")] int tagid);
        int AddExtraValueData(
            [Parameter(DbType = "Int")] int? pid,
            [Parameter(DbType = "varchar(150)")] string field,
            [Parameter(DbType = "varchar(200)")] string strvalue,
            [Parameter(DbType = "datetime")] DateTime? datevalue,
            [Parameter(DbType = "varchar(max)")] string text,
            [Parameter(DbType = "Int")] int? intvalue,
            [Parameter(DbType = "Bit")] bool? bitvalue);
        int TryIpWarmup();
        DbConnection ReadonlyConnection();
        void Log2Content(string file, string data);
        int? InsertIpLog([Parameter(DbType = "varchar(50)")] string ip, [Parameter(DbType = "varchar(50)")] string id);
        
    }
}
